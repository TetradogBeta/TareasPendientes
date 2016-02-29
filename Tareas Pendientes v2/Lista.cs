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
    public class Lista : IClauUnicaPerObjecte, IEnumerable<Tarea>
    {
        enum NodoLista
        {
            Nombre, Id, Categoria, Herencia, TareasLista, TareasOcultas, TareasHechas
        }
        static LlistaOrdenada<string, LlistaOrdenada<long, Lista>> listasPorCategoria;
        static Llista<Lista> todasLasListas;
        #region Atributos y eventos
        long idUnico;
        string nombreLista;
        LlistaOrdenada<string, string> categorias;
        LlistaOrdenada<long, Lista> herencia;
        LlistaOrdenada<long, Tarea> todasLasTareasVisiblesLista;//estan las de la herencia asi no se tienen que ir poniendo siempre
        LlistaOrdenada<long, Tarea> tareasLista;
        LlistaOrdenada<long, Tarea> tareasHechas;
        LlistaOrdenada<long, Tarea> tareasOcultas;//son las tareas que no tienen que ser visibles asi que las quito de todasLasTareasLista

        public event TareaEventHandler TareaNueva;
        public event TareaEventHandler TareaEliminada;
        #endregion
        static Lista()
        {
            listasPorCategoria = new LlistaOrdenada<string, LlistaOrdenada<long, Lista>>();
            todasLasListas = new Llista<Lista>();
        }
        #region Constructores
        public Lista(string nombreLista)
        {
            this.nombreLista = nombreLista;
            idUnico = DateTime.Now.Ticks;
            categorias = new LlistaOrdenada<string, string>();
            herencia = new LlistaOrdenada<long, Lista>();
            todasLasTareasVisiblesLista = new LlistaOrdenada<long, Tarea>();
            tareasLista = new LlistaOrdenada<long, Tarea>();
            tareasHechas = new LlistaOrdenada<long, Tarea>();
            tareasOcultas = new LlistaOrdenada<long, Tarea>();

        }
        public Lista():this(""){}
        private Lista(XmlNode nodo) : this(nodo.ChildNodes[(int)NodoLista.Nombre].InnerText)
        {
            string categoria;
            idUnico = Convert.ToInt64(nodo.ChildNodes[(int)NodoLista.Id].InnerText);
            for (int i = 0; i < nodo.ChildNodes[(int)NodoLista.Categoria].ChildNodes.Count; i++)//añado la lista en su categoria
            {
                categoria = nodo.ChildNodes[(int)NodoLista.Categoria].ChildNodes[i].InnerText.DescaparCaracteresXML();
                AñadirHaCategoria(categoria);
            }
            //la herencia la hago fuera
            //añado las tareas de la Lista
            for (int i = 0; i < nodo.ChildNodes[(int)NodoLista.TareasLista].ChildNodes.Count; i++)//añado la lista en su categoria
            {
                AñadirTarea(new Tarea(nodo.ChildNodes[(int)NodoLista.TareasLista].ChildNodes[i]));//se añaden a sus listas
            }
            //las tareas ocultas y hechas las pongo cuando ya estan todas
        }
        #endregion
        public string NombreLista
        {
            get
            {
                return nombreLista;
            }

            set
            {
                nombreLista = value;
            }
        }
        public int Count
        { get { return todasLasTareasVisiblesLista.Count; } }
        public bool EsTemporal
        {
            get { return !todasLasListas.Existeix(this); }
            set
            {
                string[] categorias;
                if (value)
                {
                    categorias = this.categorias.ValuesToArray();
                    for (int i = 0; i < categorias.Length; i++)
                        if (listasPorCategoria[categorias[i]].Existeix(idUnico))
                            listasPorCategoria[categorias[i]].Elimina(idUnico);
                    todasLasListas.Elimina(this);

                }
                else
                {
                    if (!todasLasListas.Existeix(this))
                    {
                        categorias = this.categorias.ValuesToArray();
                        for (int i = 0; i < categorias.Length; i++)
                            if (!listasPorCategoria[categorias[i]].Existeix(idUnico))
                                listasPorCategoria[categorias[i]].Afegir(idUnico, this);
                        todasLasListas.Afegir(this);
                    }
                }
            }
        }

        public bool TieneDescendencia { get { return TareaNueva != null; } }
        #region Categoria
        public bool EstaEnLaCategoria(string categoria)
        {
            return categorias.Existeix(categoria);
        }

        public static void QuitarHerederos(Lista listaActual)
        {
            for (int i = 0; i < todasLasListas.Count; i++)
                if (todasLasListas[i].ContieneHerencia(listaActual))
                    todasLasListas[i].EliminarHerencia(listaActual);
        }

        public void AñadirHaCategoria(string categoria)
        {
            if (!listasPorCategoria.Existeix(categoria))
                throw new Exception("La categoria no existe");
            if (this.categorias.Existeix(categoria))
                throw new Exception("La lista ya esta en la categoria");
            this.categorias.Afegir(categoria, categoria);

        }

        public void EliminarDeCategoria(string categoria)
        {
            if (!listasPorCategoria.Existeix(categoria))
                throw new Exception("La categoria no existe");
            if (!this.categorias.Existeix(categoria))
                throw new Exception("La lista no esta en la categoria");
            this.categorias.Elimina(categoria);


        }
        public string[] Categorias()
        {
            return categorias.ValuesToArray();
        }
        #endregion
        #region Tarea
        public void AñadirTarea(Tarea tarea)
        {
            if (tareasOcultas.Existeix(tarea.IdUnico))
            {
                tareasOcultas.Elimina(tarea.IdUnico);
            }
            if (!todasLasTareasVisiblesLista.Existeix(tarea.IdUnico))
            {
                tareasLista.Afegir(tarea.IdUnico, tarea);
                todasLasTareasVisiblesLista.Afegir(tarea.IdUnico, tarea);
                tarea.TareaHecha += TareaHecha;
                tarea.TareaNoHecha += TareaNoHecha;

            }
            else
            {
                throw new Exception("La tarea ya existe");
            }
            if (TareaNueva != null)//Aviso a mis herederos
                TareaNueva(tarea);
            TareaHecha(tarea);

        }
        public void EliminarTarea(Tarea tarea)
        {
            bool esPropia = false;
            if (tareasLista.Existeix(tarea.IdUnico))
            {
                tareasLista.Elimina(tarea.IdUnico);
                esPropia = true;
                tarea.TareaHecha -= TareaHecha;
                tarea.TareaNoHecha -= TareaNoHecha;
            }
            if (todasLasTareasVisiblesLista.Existeix(tarea.IdUnico))
            {
                //la oculto
                todasLasTareasVisiblesLista.Elimina(tarea.IdUnico);
                if(!esPropia)
                   tareasOcultas.Afegir(tarea.IdUnico, tarea);

            }
            else
            {
                throw new Exception("La tarea a quitar no existe");
            }
            if (TareaEliminada != null)//Aviso a mis herederos
                TareaEliminada(tarea);
            TareaNoHecha(tarea);
        }
        public void EliminarTarea(long idTarea)
        {
            if (!todasLasTareasVisiblesLista.Existeix(idTarea))
                throw new Exception("No esta la tarea para eliminar");
            EliminarTarea(todasLasTareasVisiblesLista[idTarea]);
        }
        public void TareaHecha(long idTarea)
        {
            if (!todasLasTareasVisiblesLista.Existeix(idTarea) && !tareasOcultas.Existeix(idTarea))
                throw new Exception("No se encuentra la tarea hecha");
            if (todasLasTareasVisiblesLista.Existeix(idTarea))
                TareaHecha(todasLasTareasVisiblesLista[idTarea]);
            else
                TareaHecha(tareasOcultas[idTarea]);
        }
        public void TareaHecha(Tarea tarea)
        {
            if ((todasLasTareasVisiblesLista.Existeix(tarea.IdUnico) || tareasOcultas.Existeix(tarea.IdUnico)) && !tareasHechas.Existeix(tarea.IdUnico) && tarea.Hecho)
            {
                tareasHechas.Afegir(tarea.IdUnico, tarea);
            }
        }
        public void TareaNoHecha(Tarea tarea)
        {
            if ((todasLasTareasVisiblesLista.Existeix(tarea.IdUnico) || tareasOcultas.Existeix(tarea.IdUnico)) && tareasHechas.Existeix(tarea.IdUnico) && !tarea.Hecho)
            {
                tareasHechas.Elimina(tarea.IdUnico);
            }
        }
        public Tarea[] Filtra(string contenido, bool? estanHechas, DateTime? fecha)
        {
            //por testear
            Llista<Tarea> tareas = new Llista<Tarea>();
            Llista<Tarea> tareasQueNoCoinciden = new Llista<Tarea>();
            string fechaString = fecha.HasValue ? fecha.Value.ToShortDateString() : "";
            estanHechas = !estanHechas.HasValue ? fecha.HasValue : false;
            if (contenido == null)
                contenido = "";
            else
                contenido = contenido.ToLower();
            foreach (Tarea tarea in this)
            {
                if (tarea.Contenido.ToLower().Contains(contenido))
                    tareas.Afegir(tarea);
            }
            for (int i = 0; i < tareas.Count; i++)
            {
                if (estanHechas.Value)
                {
                    if (!fechaString.Equals(tareas[i].FechaHecho.ToShortDateString()))
                    {
                        tareasQueNoCoinciden.Afegir(tareas[i]);
                    }
                }
            }
            tareas.Elimina(tareasQueNoCoinciden);
            return tareas.ToArray();
        }
        #endregion
        #region Herencia
        public void AñadirHerencia(Lista listaHaHerededar)
        {
            if (herencia.Existeix(listaHaHerededar.idUnico))
                throw new Exception("Ya existe la herencia");
            if (ContieneHerencia(listaHaHerededar))
                throw new Exception("la lista ha heredar no puede coincidir en ninguna tarea que ya este en la lista");

            //tener en cuenta si añaden o quitan tareas de las listas heredadas
            herencia.Afegir(listaHaHerededar.idUnico, listaHaHerededar);
            todasLasTareasVisiblesLista.AfegirMolts(listaHaHerededar.todasLasTareasVisiblesLista);
            listaHaHerededar.TareaEliminada += QuitarTareaHeredada;
            listaHaHerededar.TareaNueva += AñadirTareaHeredada;
        }

        private void QuitarTareaHeredada(Tarea tareaHaQuitar)
        {
            if (todasLasTareasVisiblesLista.Existeix(tareaHaQuitar.IdUnico))
                todasLasTareasVisiblesLista.Elimina(tareaHaQuitar.IdUnico);
            if (TareaEliminada != null)
                TareaEliminada(tareaHaQuitar);
        }
        private void AñadirTareaHeredada(Tarea tareaHaAñadir)
        {
            if (!todasLasTareasVisiblesLista.Existeix(tareaHaAñadir.IdUnico))
                todasLasTareasVisiblesLista.Afegir(tareaHaAñadir.IdUnico, tareaHaAñadir);
            if (TareaNueva != null)
                TareaNueva(tareaHaAñadir);
        }
        public void EliminarHerencia(Lista listaHaDesHeredar)
        {
            if (!herencia.Existeix(listaHaDesHeredar.idUnico))
                throw new Exception("No se hereda de esta lista directamente");
            todasLasTareasVisiblesLista.Elimina(listaHaDesHeredar.todasLasTareasVisiblesLista.KeysToArray());
            listaHaDesHeredar.TareaEliminada -= QuitarTareaHeredada;
            listaHaDesHeredar.TareaNueva -= AñadirTareaHeredada;
            herencia.Elimina(listaHaDesHeredar.idUnico);
        }
        /// <summary>
        /// Mira que no se contenga la herencia la lista actual y las listas de su herencia
        /// </summary>
        /// <param name="listaHaHerededar"></param>
        /// <returns></returns>
        public bool ContieneHerencia(Lista listaHaHerededar)
        {
            bool contiene = listaHaHerededar.todasLasTareasVisiblesLista.Count == 0;
            for(int i=0;i<listaHaHerededar.todasLasTareasVisiblesLista.Count&&!contiene;i++)
                contiene = todasLasTareasVisiblesLista.Existeix(listaHaHerededar.todasLasTareasVisiblesLista.ElementAt(i).Key);
            return contiene;
        }
        public Lista[] Herencia()
        {
            return herencia.ValuesToArray();
        }
        #endregion
        #region Xml
        public XmlNode ToXml()
        {//por testear 
            XmlDocument xml = new XmlDocument();
            text nodo = "<Lista><Nombre>" + NombreLista.EscaparCaracteresXML() + "</Nombre>";//nombre
            //id
            nodo &= "<IdUnico>" + idUnico + "</IdUnico>";
            //categorias
            nodo &= "<Categorias>";
            foreach (KeyValuePair<string, string> categoria in categorias)
                nodo &= "<Categoria>" + categoria.Value.EscaparCaracteresXML() + "</Categoria>";
            nodo &= "</Categorias>";
            //herencia solo ids
            nodo &= "<Herencias>";
            foreach (KeyValuePair<long, Lista> herenciaId in herencia)
                nodo &= "<Herencia>" + herenciaId.Key + "</Herencia>";
            nodo &= "</Herencias>";
            //tareas ocultas solo ids
            nodo &= "<TareasLista>";
            foreach (KeyValuePair<long, Tarea> tareaLista in tareasLista)
                nodo &=  tareaLista.Value.ToXml().OuterXml;
            nodo &= "</TareasLista>";
            //tareas ocultas solo ids
            nodo &= "<TareasOcultas>";
            foreach (KeyValuePair<long, Tarea> tareaOcultaId in tareasOcultas)
                nodo &= "<TareasOculta>" + tareaOcultaId.Key + "</TareasOculta>";
            nodo &= "</TareasOcultas>";
            //tareas hechas solo ids
            nodo &= "<TareasHechas>";
            foreach (KeyValuePair<long, Tarea> tareaHechaId in tareasHechas)
                nodo &= "<TareasHecha>" + tareaHechaId.Key + "</TareasHecha>";
            nodo &= "</TareasHechas>";

            nodo &= "</Lista>";
            xml.LoadXml(nodo);
            xml.Normalize();
            return xml.FirstChild;//mirar si coge el nodo principal
        }
        #endregion
        #region Interficies
        public IComparable Clau()
        {
            return idUnico;
        }
        public IEnumerator<Tarea> GetEnumerator()
        {
            return todasLasTareasVisiblesLista.ValuesToArray().ObtieneEnumerador();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #region Override
        public override string ToString()
        {
            return NombreLista != "" ? NombreLista : "'Sin nombre'";
        }
        #endregion
        #region Xml Clase
        /// <summary>
        /// Carga las listas y las categorias
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>devuelve la lista temporal si no hay es null</returns>
        public static Lista LoadXml(XmlDocument xml)
        {
            LlistaOrdenada<long, Lista> listas = new LlistaOrdenada<long, Lista>();
            Lista listaCargada;
            XmlNode xmlNodePrincipal = xml.FirstChild, xmlNode;
            //Categorias
            for (int i = 0; i < xmlNodePrincipal.FirstChild.ChildNodes.Count; i++)
                AñadirCategoria(xmlNodePrincipal.FirstChild.ChildNodes[i].InnerText.DescaparCaracteresXML());
            //Listas
            for (int i = 0; i < xmlNodePrincipal.LastChild.ChildNodes.Count; i++)
            {
                listaCargada = new Lista(xmlNodePrincipal.LastChild.ChildNodes[i]);
                listas.Afegir(listaCargada.idUnico, listaCargada);
                todasLasListas.Afegir(listaCargada);
                Lista.AñadirListaHaSusCategorias(listaCargada);
            }
            //pongo herencia,tareas hechas y ocultas
            for (int i = 0; i < xmlNodePrincipal.LastChild.ChildNodes.Count; i++)
            {
                xmlNode = xmlNodePrincipal.LastChild.ChildNodes[i];
                AcabarDeCargarLista(listas, xmlNode);
            }
            listaCargada = null;
            //lista temporal
            if (xmlNodePrincipal.ChildNodes[1].HasChildNodes)
            {
                xmlNode = xmlNodePrincipal.ChildNodes[1].FirstChild;//cojo el nodo de la lista temporal
                listaCargada = new Lista(xmlNode);//lo creo
                listas.Afegir(listaCargada.idUnico, listaCargada);//lo añado para usar el metodo
                AcabarDeCargarLista(listas, xmlNode);//lo acabo de cargar
                listaCargada.EsTemporal = true;//lo pongo como temporal
            }
            return listaCargada;
        }
     
        public static void AñadirListaHaSusCategorias(Lista listaCargada)
        {
            string[] categorias = listaCargada.Categorias();
            for (int i = 0; i < categorias.Length; i++)
                AñadirListaHaCategoria(listaCargada, categorias[i]);
        }

        public static void AñadirCategoria(string categoria)
        {
            if (listasPorCategoria.Existeix(categoria))
                throw new Exception("Ya existe la categoria");
            listasPorCategoria.Afegir(categoria, new LlistaOrdenada<long, Lista>());
        }
        public static void AñadirListaHaCategoria(Lista lista, string categoria)
        {
            if(!listasPorCategoria[categoria].Existeix(lista.idUnico))
               listasPorCategoria[categoria].Afegir(lista.idUnico, lista);
        }
        public static void QuitarListaDeCategoria(Lista lista, string categoria)
        {
            listasPorCategoria[categoria].Elimina(lista.idUnico);
        }
        private static void AcabarDeCargarLista(LlistaOrdenada<long, Lista> listas, XmlNode xmlNode)
        {
            Lista listaCargada;
            listaCargada = listas[Convert.ToInt64(xmlNode.ChildNodes[(int)NodoLista.Id].InnerText)];//cojo la lista con el id
            for (int j = 0; j < xmlNode.ChildNodes[(int)NodoLista.Herencia].ChildNodes.Count; j++)//pongo la herencia
            {
                listaCargada.AñadirHerencia(listas[Convert.ToInt64(xmlNode.ChildNodes[(int)NodoLista.Herencia].ChildNodes[j].InnerText)]);//hay solo el id de la lista de la que hereda
            }
            for (int j = 0; j < xmlNode.ChildNodes[(int)NodoLista.TareasOcultas].ChildNodes.Count; j++)//pongo las tareas ocultas
            {
                listaCargada.EliminarTarea(Convert.ToInt64(xmlNode.ChildNodes[(int)NodoLista.TareasOcultas].ChildNodes[j].InnerText));//hay solo el id de la lista de la que hereda
            }
            for (int j = 0; j < xmlNode.ChildNodes[(int)NodoLista.TareasHechas].ChildNodes.Count; j++)//pongo las tareas hechas
            {
                listaCargada.TareaHecha(Convert.ToInt64(xmlNode.ChildNodes[(int)NodoLista.TareasHechas].ChildNodes[j].InnerText));//hay solo el id de la lista de la que hereda
            }

        }

        public static XmlDocument ToXml(Lista listaTemporal = null)
        {
            XmlDocument xml = new XmlDocument();
            text nodo = "<TareasPendientes>";
            string[] categorias = TodasLasCategorias();
            //categorias
            nodo &= "<Categorias>";
            for (int i = 0; i < categorias.Length; i++)
                nodo &= "<Categoria>" + categorias[i].EscaparCaracteresXML() + "</Categoria>";
            nodo &= "</Categorias>";
            //lista temporal
            nodo &= "<ListaTemporal>";
            nodo &= listaTemporal != null && listaTemporal.EsTemporal ? listaTemporal.ToXml().OuterXml : "";
            nodo &= "</ListaTemporal>";
            //listas
            nodo &= "<Listas>";
            for (int i = 0; i < todasLasListas.Count; i++)
                nodo &= todasLasListas[i].ToXml().OuterXml;
            nodo &= "</Listas>";

            nodo &= "</TareasPendientes>";
            xml.LoadXml(nodo);
            xml.Normalize();
            return xml;
        }

        #endregion
        #region Filtrar
        public static Lista[] FiltraPorCategoria(string categoria)
        {
            LlistaOrdenada<long, Lista> listasCategoria = listasPorCategoria[categoria];
            Lista[] listas = null;
            if (listasCategoria != null)
                listas = listasCategoria.ValuesToArray();
            return listas;
        }
        public static Lista[] FiltraPorContenido(string contenido)
        {
            Llista<Lista> listas = new Llista<Lista>();
            contenido = contenido.ToLower();
            for (int i = 0; i < todasLasListas.Count; i++)
                if (todasLasListas[i].nombreLista.ToLower().Contains(contenido))
                    listas.Afegir(todasLasListas[i]);
            return listas.ToArray();
        }
        #endregion
        #region Categorias
        public static string[] TodasLasCategorias()
        {
            return listasPorCategoria.KeysToArray();
        }
        public static void CambiarNombreCategoria(string nombreAnt, string nombreNuevo)
        {
            if (nombreAnt != nombreNuevo)
            {
                if (listasPorCategoria.Existeix(nombreNuevo))
                    throw new Exception("El nombre para la categoria ya esta en uso");
                listasPorCategoria.CanviClau(nombreAnt, nombreNuevo);
                foreach (KeyValuePair<long, Lista> lista in listasPorCategoria[nombreNuevo])
                {
                    lista.Value.categorias.Elimina(nombreAnt);
                    lista.Value.categorias.Afegir(nombreNuevo, nombreNuevo);
                }
            }
        }

        public static void EliminarCategoria(string categoria)
        {
            if (!listasPorCategoria.Existeix(categoria))
                throw new Exception("No existe la categoria");
            foreach (KeyValuePair<long, Lista> listaHaQuitar in listasPorCategoria[categoria])
                listaHaQuitar.Value.EliminarDeCategoria(categoria);

            listasPorCategoria.Elimina(categoria);
        }

        public static bool ExisteCategoria(string categoria)
        {
            return listasPorCategoria.Existeix(categoria);
        }
        #endregion

        #region Herencia
        public static Lista[] HerenciaPosible(Lista listaActual)
        {
            List<Lista> listas = new List<Lista>();
            bool existeHerencia = todasLasListas.Existeix(listaActual);
            if (existeHerencia)
                todasLasListas.Elimina(listaActual);
            for (int i = 0; i < todasLasListas.Count; i++)
                if (!listaActual.ContieneHerencia(todasLasListas[i]))
                    listas.Add(todasLasListas[i]);
            if (existeHerencia)
                todasLasListas.Afegir(listaActual);//asi no tengo que comprovar de no añadir la misma
            return listas.ToArray();
        }
        #endregion
    }
}
