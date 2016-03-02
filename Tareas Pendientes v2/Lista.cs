using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat;
using Gabriel.Cat.Extension;
using System.Xml;

namespace Tareas_Pendientes_v2
{
    public delegate void TareaEventHandler(Tarea tarea);
    public class Lista : IClauUnicaPerObjecte, IEnumerable<Tarea>, IComparable, IComparable<Lista>
    {
        enum NodoLista
        {
             Nombre,Id, Categorias, TareasLista, TareasHechas, Herencia, TareasOcultas
        }
        static LlistaOrdenada<long, Lista> listasGuardadas;
        #region Atributos y eventos
        long idUnico;
        string nombre;
        Llista<Lista> herencia;
        #endregion
        static Lista()
        {
            listasGuardadas = new LlistaOrdenada<long, Lista>();
        }
        #region Constructores
        public Lista(string nombreLista) : this(nombreLista, DateTime.Now.Ticks)
        { }
        public Lista(string nombreLista, long idUnico)
        {
            this.nombre = nombreLista;
            this.idUnico = idUnico;
            herencia = new Llista<Lista>();
        }
        public Lista() : this("") { }
        private Lista(XmlNode nodo) : this(nodo.ChildNodes[(int)NodoLista.Nombre].InnerText.DescaparCaracteresXML(), Convert.ToInt64(nodo.ChildNodes[(int)NodoLista.Id].InnerText))
        {
            for (int i = 0; i < nodo.ChildNodes[(int)NodoLista.Categorias].ChildNodes.Count; i++)//añado la lista en su categoria
            {
                Categoria.Añadir(Convert.ToInt64(nodo.ChildNodes[(int)NodoLista.Categorias].ChildNodes[i].InnerText), this);
            }
            
            for (int i = 0; i < nodo.ChildNodes[(int)NodoLista.TareasLista].ChildNodes.Count; i++)//añado las tareas de la Lista
            {
                new Tarea(this,nodo.ChildNodes[(int)NodoLista.TareasLista].ChildNodes[i]);
            }
            //la herencia ,las tareas ocultas y hechas  fuera

        }
        #endregion

        public string Nombre
        {
            get
            {
                return nombre;
            }

            set
            {
                nombre = value;
            }
        }
        public bool EsTemporal
        {
            get { return !listasGuardadas.Existeix(IdUnico); }
            set
            {
                if (value)
                {
                    if (listasGuardadas.Existeix(IdUnico))
                    {
                        listasGuardadas.Elimina(IdUnico);
                        QuitarHerederos(this);
                    }
                }
                else
                {
                    if (!listasGuardadas.Existeix(IdUnico))
                    {
                        listasGuardadas.Afegir(IdUnico, this);
                    }
                }
            }
        }
        public long IdUnico
        {
            get
            {
                return idUnico;
            }
        }
             public static Lista[] Obtener(string text)
        {
        	text=text.ToLowerInvariant();
        	return listasGuardadas.Filtra((lista) => { return lista.Value.Nombre.ToLowerInvariant().Contains(text); }).ValuesToArray();
        } 
        #region Herencia Obj
        public bool TieneDescendencia
        {
            get {
                bool tieneHijos = false;
                listasGuardadas.WhileEach((hijoList) => { if (this != hijoList.Value) tieneHijos = hijoList.Value.herencia.Existeix(this); return !tieneHijos; });
                return tieneHijos;
            }
        }



        public Lista[] Herencia()
        {
            return herencia.ToArray();
        }

        public void Heredar(Lista lista)
        {
            if (!herencia.Existeix(lista) && EsHeredable(lista))
                herencia.Afegir(lista);
        }

        public bool EsHeredable(Lista lista)
        {
            bool esHeredable = !Equals(lista)&&!herencia.Existeix(lista);
            //solo no es heredable si ya se esta heredando de el en cualquier linea
            for (int i = 0; i < herencia.Count && esHeredable; i++)
            {
                esHeredable = herencia[i].EsHeredable(lista);
            }
            return esHeredable;
        }

        public void Desheredar(Lista lista)
        {
            herencia.Elimina(lista);
        }
        #endregion
        #region Xml NodoLista
        public XmlNode ToXml()
        {//por testear  Nombre,Id, Categorias, TareasLista, TareasHechas, Herencia, TareasOcultas
            XmlDocument xml = new XmlDocument();
            Categoria[] categorias = Categoria.Categorias(this);
            Tarea[] tareasLista = Tarea.TareasLista(this);
            Tarea[] tareasHechas = Tarea.TareasHechas(this);
            Tarea[] tareasOcultas = Tarea.TareasOcultas(this);
            text nodo = "<Lista>";

            //nombre
            nodo &= "<Nombre>" + Nombre.EscaparCaracteresXML() + "</Nombre>";
            //id
            nodo &= "<IdUnico>" + idUnico + "</IdUnico>";
            //categorias
            nodo &= "<Categorias>";
            for (int i = 0; i < categorias.Length; i++)
                nodo &= "<IdCategoria>" + categorias[i].IdUnico + "</IdCategoria>";
            nodo &= "</Categorias>";
            //tareas lista todo el nodo 
            nodo &= "<TareasLista>";
            for (int i = 0; i < tareasLista.Length; i++)
                nodo &= tareasLista[i].ToXml().OuterXml;
            nodo &= "</TareasLista>";
            //tareas hechas solo ids y fecha
            nodo &= "<TareasHechas>";
            for (int i = 0; i < tareasHechas.Length; i++)
                nodo &= "<TareaHecha><IdTareaHecha>" + tareasHechas[i].IdUnico + "</IdTareaHecha><FechaHecho>" + tareasHechas[i].FechaHecho(this).Ticks + "</FechaHecho></TareaHecha>";
            nodo &= "</TareasHechas>";
            //herencia solo ids
            nodo &= "<Herencias>";
            for (int i = 0; i < herencia.Count; i++)
                nodo &= "<IdHerencia>" + herencia[i].IdUnico + "</IdHerencia>";
            nodo &= "</Herencias>";
            //tareas ocultas solo ids
            nodo &= "<TareasOcultas>";
            for (int i = 0; i < tareasOcultas.Length; i++)
                nodo &= "<TareaOculta>" + tareasOcultas[i].IdUnico + "</TareaOculta>";
            nodo &= "</TareasOcultas>";
            nodo &= "</Lista>";
            xml.LoadXml(nodo);
            xml.Normalize();
            return xml.FirstChild;//mirar si coge el nodo principal
        }
        #endregion
        #region Interficies
        public int CompareTo(object obj)
        {
            return CompareTo(obj as Lista);
        }
        public int CompareTo(Lista other)
        {
            int compareTo;
            if (other != null)
            {
                compareTo = IdUnico.CompareTo(other.IdUnico);
            }
            else
                compareTo = -1;
            return compareTo;
        }
        public IComparable Clau()
        {
            return idUnico;
        }
        public IEnumerator<Tarea> GetEnumerator()
        {
            return Tarea.TareasVisibles(this).ObtieneEnumerador();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #endregion
        public override string ToString()
        {
            return String.IsNullOrEmpty(nombre)|| String.IsNullOrWhiteSpace(nombre) ? "'Sin nombre'":Nombre;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Lista);
        }
        public bool Equals(Lista other)
        {
            bool equals=other!=null;
            if (equals)
                equals = IdUnico.Equals(other.IdUnico);
            return equals;     
        }
        #region Clase 
        #region Xml Clase
        //saveXml
        public static XmlNode SaveNodoXml(Lista listaTemporal = null)
        {
            XmlDocument xmldoc =new XmlDocument();
            text txtNodo = "<Listas>";
            txtNodo &= "<ListaTemporal>";
            if(listaTemporal!=null)
               txtNodo &= listaTemporal.ToXml().OuterXml;
            txtNodo &= "</ListaTemporal>";
            txtNodo &= "<ListasGuardadas>";
           foreach(KeyValuePair<long,Lista> lista in listasGuardadas)
                txtNodo &= lista.Value.ToXml().OuterXml;
            txtNodo &= "</ListasGuardadas>";
            txtNodo &= "</Listas>";
            xmldoc.LoadXml(txtNodo);
            xmldoc.Normalize();
            return xmldoc.FirstChild;
        }
        //loadXml
        public static Lista LoadNodoXml(XmlNode nodoListas)
        {
            XmlNode nodo, nodoListaTemporal = nodoListas.ChildNodes[0].FirstChild;
            Lista lista;
            long idLista;
            listasGuardadas.Buida();
            //pongo las listas guardadas
            nodoListas = nodoListas.ChildNodes[1];
            //creo las listas
            for (int i = 0; i < nodoListas.ChildNodes.Count; i++)
            {
                lista = new Lista(nodoListas.ChildNodes[i]);
                listasGuardadas.Afegir(lista.IdUnico, lista);
            }
            //pongo la herencia
            for (int i = 0; i < nodoListas.ChildNodes.Count; i++)
            {
                idLista = Convert.ToInt64(nodoListas.ChildNodes[i].ChildNodes[(int)NodoLista.Id].InnerText);
                nodo = nodoListas.ChildNodes[i].ChildNodes[(int)NodoLista.Herencia];
                PonHerenciaXml(nodo, listasGuardadas[idLista]);

            }
            //añado tareas ocultas y hechas
            for (int j = 0; j < nodoListas.ChildNodes.Count; j++)
            {
                nodo = nodoListas.ChildNodes[j];
                PonTareasHechasYOclutasXml(nodo, listasGuardadas[Convert.ToInt64(nodo.ChildNodes[(int)NodoLista.Id].InnerText)]);
            }
            if (nodoListaTemporal!=null&&nodoListaTemporal.HasChildNodes)
            {
                lista = new Lista(nodoListaTemporal);
                PonHerenciaXml(nodoListaTemporal.ChildNodes[(int)NodoLista.Herencia], lista);
                PonTareasHechasYOclutasXml(nodoListaTemporal, lista);
            }
            else
            {
                lista = null;
            }
            return lista;
        }
        private static void PonHerenciaXml(XmlNode subNodoHerencia, Lista lista)
        {
            for (int j = 0; j < subNodoHerencia.ChildNodes.Count; j++)//añado la herencia que tenga
                listasGuardadas[lista.IdUnico].herencia.Afegir(listasGuardadas[Convert.ToInt64(subNodoHerencia.ChildNodes[j].InnerText)]);
        }
        private static void PonTareasHechasYOclutasXml(XmlNode nodoListaTemporal, Lista lista)
        {
            for (int i = 0; i < nodoListaTemporal.ChildNodes[(int)NodoLista.TareasOcultas].ChildNodes.Count; i++)
            {
                Tarea.Obtener(Convert.ToInt64(nodoListaTemporal.ChildNodes[(int)NodoLista.TareasOcultas].ChildNodes[i].InnerText)).Ocultar(lista);
            }
            for (int i = 0; i < nodoListaTemporal.ChildNodes[(int)NodoLista.TareasHechas].ChildNodes.Count; i++)
            {
                Tarea.Obtener(Convert.ToInt64(nodoListaTemporal.ChildNodes[(int)NodoLista.TareasHechas].ChildNodes[i].ChildNodes[0].InnerText)).AñadirHecho(lista, new DateTime(Convert.ToInt64(nodoListaTemporal.ChildNodes[(int)NodoLista.TareasHechas].ChildNodes[i].ChildNodes[1].InnerText)));
            }
        }
        #endregion
        #region Herencia clase
        public static Lista[] ListasHeredables(Lista listaActual)
        {
            Llista<Lista> listasHeredables = new Llista<Lista>();
            foreach (KeyValuePair<long,Lista> listaHaHeredar in listasGuardadas)
                if (listaActual.EsHeredable(listaHaHeredar.Value))
                    listasHeredables.Afegir(listaHaHeredar.Value);
            return listasHeredables.ToArray();
        }
        public static void QuitarHerederos(Lista listaActual)
        {
            Lista[] herederos = Herederos(listaActual);
            Tarea[] tareasLista = Tarea.TareasLista(listaActual);
            DateTime fechaHecho;
            for (int i = 0; i < herederos.Length; i++)
            {
                herederos[i].herencia.Elimina(listaActual);

            }
            for (int i = 0; i < tareasLista.Length; i++)
            {
                fechaHecho = default(DateTime);
                if (tareasLista[i].EstaHecha(listaActual))
                {
                    fechaHecho = tareasLista[i].FechaHecho(listaActual);
                }
                tareasLista[i].VaciarListaHechosYOcultos();
                if (!fechaHecho.Equals(default(DateTime)))
                    tareasLista[i].AñadirHecho(listaActual, fechaHecho);
            }
        }

        public static Lista[] Herederos(Lista lista)
        {
            return listasGuardadas.ValuesToArray().Filtra((listaActual) => { return listaActual.herencia.Existeix(lista); }).ToArray();
        }
        #endregion
        /// <summary>
        /// Quita de Categoria,Tarea y la lista de listas guardadas y de se quita de sus heredereos (si esta guardada) 
        /// </summary>
        /// <param name="lista">lista a quitar</param>
        public static void Elimina(Lista lista)
        { 
            Tarea[] tareasLista= Tarea.TareasLista(lista);
            Categoria[] categorias = Categoria.Categorias(lista);
            lista.EsTemporal = true;
            for (int i = 0; i < tareasLista.Length; i++)
                Tarea.Eliminar(tareasLista[i]);        
            for (int i = 0; i < categorias.Length; i++)
                categorias[i].Quitar(lista);
        }
        #endregion
    }
}
