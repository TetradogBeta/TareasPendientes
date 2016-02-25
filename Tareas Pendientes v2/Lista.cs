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
    delegate void TareaEventHandler(Tarea tarea);
    class Lista : IClauUnicaPerObjecte, IEnumerable<Tarea>
    {
        static LlistaOrdenada<string, LlistaOrdenada<long, Lista>> listasPorCategoria;
        static Llista<Lista> todasLasListas;

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
        static Lista()
        {
            listasPorCategoria = new LlistaOrdenada<string, LlistaOrdenada<long, Lista>>();
            todasLasListas = new Llista<Lista>();
        }

        public Lista(string nombreLista)
        {
            this.nombreLista = nombreLista;
            idUnico = DateTime.Now.ToBinary();
            categorias = new LlistaOrdenada<string, string>();
            herencia = new LlistaOrdenada<long, Lista>();
            todasLasTareasVisiblesLista = new LlistaOrdenada<long, Tarea>();
            tareasLista = new LlistaOrdenada<long, Tarea>();
            tareasHechas = new LlistaOrdenada<long, Tarea>();
            tareasOcultas = new LlistaOrdenada<long, Tarea>();

        }
        private Lista(XmlNode nodo) : this(nodo.FirstChild.InnerText)
        {
            idUnico = Convert.ToInt64(nodo.ChildNodes[1].InnerText);
            for(int i=0;i<nodo.ChildNodes[2].ChildNodes.Count;i++)//añado la lista en su categoria
            {
                AñadirACategoria(nodo.ChildNodes[2].ChildNodes[i].InnerText.DescaparCaracteresXML());
            }
            //la herencia la hago fuera
            //añado las tareas de la Lista
            for (int i = 0; i < nodo.ChildNodes[4].ChildNodes.Count; i++)//añado la lista en su categoria
            {
                AñadirTarea(new Tarea(nodo.ChildNodes[4].ChildNodes[i]));//se añaden a sus listas
            }
            //las tareas ocultas y hechas las pongo cuando ya estan todas
        }
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
        public bool EstaEnLaCategoria(string categoria)
        {
            return categorias.Existeix(categoria);
        }
        public void AñadirACategoria(string categoria)
        {
            if (!listasPorCategoria.Existeix(categoria))
                throw new Exception("La categoria no existe");
            if (this.categorias.Existeix(categoria))
                throw new Exception("La lista ya esta en la categoria");
            listasPorCategoria[categoria].Afegir(this.idUnico, this);
            this.categorias.Afegir(categoria, categoria);

        }
        public void EliminarDeCategoria(string categoria)
        {
            if (!listasPorCategoria.Existeix(categoria))
                throw new Exception("La categoria no existe");
            if (!this.categorias.Existeix(categoria))
                throw new Exception("La lista no esta en la categoria");
            listasPorCategoria[categoria].Elimina(this.idUnico);
            this.categorias.Elimina(categoria);


        }
        public string[] Categorias()
        {
            return categorias.ValuesToArray();
        }
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
            if (tareasLista.Existeix(tarea.IdUnico))
            {
                tareasLista.Elimina(tarea.IdUnico);

            }
            if (todasLasTareasVisiblesLista.Existeix(tarea.IdUnico))
            {
                //la oculto
                todasLasTareasVisiblesLista.Elimina(tarea.IdUnico);
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
            if((todasLasTareasVisiblesLista.Existeix(tarea.IdUnico)||tareasOcultas.Existeix(tarea.IdUnico))&&!tareasHechas.Existeix(tarea.IdUnico)&&tarea.Hecho)
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
        public void AñadirHerencia(Lista listaHaHerededar)
        {
            if (herencia.Existeix(listaHaHerededar.idUnico))
                throw new Exception("Ya existe la herencia");
            if (!ContieneHerencia(listaHaHerededar))
                throw new Exception("Recursividad infinita, la lista no puede contener a sus herederos");

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
        }
        /// <summary>
        /// Mira que no se contenga la herencia la lista actual y las listas de su herencia
        /// </summary>
        /// <param name="listaHaHerededar"></param>
        /// <returns></returns>
        public bool ContieneHerencia(Lista listaHaHerededar)
        {
            bool contiene = listaHaHerededar.todasLasTareasVisiblesLista.Count != 0;
            if (contiene)
                contiene = todasLasTareasVisiblesLista.Existeix(listaHaHerededar.todasLasTareasVisiblesLista.ElementAt(0).Key);
            return contiene;
        }
        public Lista[] Herencia()
        {
            return herencia.ValuesToArray();
        }
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
            return xml.ParentNode;//mirar si coge el nodo principal
        }
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
        public static Lista[] LoadXml(XmlDocument xml)
        {
            LlistaOrdenada<long, Lista> listas = new LlistaOrdenada<long, Lista>();
            Lista listaCargada;
            //Categorias
            //Listas
            for (int i = 0; i < xml.FirstChild.ChildNodes.Count; i++)
                AñadirCategoria(xml.FirstChild.ChildNodes[i].InnerText.DescaparCaracteresXML());
            for (int i = 0; i < xml.LastChild.ChildNodes.Count; i++)
            {
                listaCargada = new Lista(xml.LastChild.ChildNodes[i]);
                listas.Afegir(listaCargada.idUnico, listaCargada);
            }
            //pongo herencia,tareas hechas y ocultas
            for (int i = 0; i < xml.LastChild.ChildNodes.Count; i++)
            {

                listaCargada = listas[Convert.ToInt64(xml.LastChild.ChildNodes[i].ChildNodes[1].InnerText)];//cojo la lista con el id
                for (int j = 0; i < xml.LastChild.ChildNodes[i].ChildNodes[3].ChildNodes.Count; j++)//pongo la herencia
                {
                    listaCargada.AñadirHerencia(listas[Convert.ToInt64(xml.LastChild.ChildNodes[i].ChildNodes[3].ChildNodes[j].InnerText)]);//hay solo el id de la lista de la que hereda
                }
                for (int j = 0; i < xml.LastChild.ChildNodes[i].ChildNodes[5].ChildNodes.Count; j++)//pongo las tareas ocultas
                {
                    listaCargada.EliminarTarea(Convert.ToInt64(xml.LastChild.ChildNodes[i].ChildNodes[5].ChildNodes[j].InnerText));//hay solo el id de la lista de la que hereda
                }
                for (int j = 0; i < xml.LastChild.ChildNodes[i].ChildNodes[6].ChildNodes.Count; j++)//pongo las tareas ocultas
                {
                    listaCargada.TareaHecha(Convert.ToInt64(xml.LastChild.ChildNodes[i].ChildNodes[6].ChildNodes[j].InnerText));//hay solo el id de la lista de la que hereda
                }
            }
            return listas.ValuesToArray();
        }
        public static XmlDocument ToXml(Lista listaTemporal)
        {
            XmlDocument xml = new XmlDocument();
            text nodo = "<TareasPendientes>";
            string[] categorias = TodasLasCategorias();
            //categorias
            nodo &= "<Categorias>";
            for(int i=0;i<categorias.Length;i++)
                nodo &= "<Categoria>"+categorias[i].EscaparCaracteresXML()+ "</Categoria>";
            nodo &= "</Categorias>";
            //lista temporal
            nodo &= "<ListaTemporal>";
            nodo &= listaTemporal != null ? listaTemporal.ToXml().OuterXml : "";
            nodo &= "</ListaTemporal>";
            //listas
            nodo &= "<Listas>";
            for (int i = 0; i < categorias.Length; i++)
                nodo &= todasLasListas[i].ToXml().OuterXml;
            nodo &= "</Listas>";

            nodo &= "</TareasPendientes>";
            xml.LoadXml(nodo);
            xml.Normalize();
            return xml;
        }

      

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
        public static string[] TodasLasCategorias()
        {
            return listasPorCategoria.KeysToArray();
        }
        public static void CambiarNombreCategoria(string nombreAnt, string nombreNuevo)
        {
            listasPorCategoria.CanviClau(nombreAnt, nombreNuevo);
            foreach (KeyValuePair<long, Lista> lista in listasPorCategoria[nombreNuevo])
            {
                lista.Value.categorias.Elimina(nombreAnt);
                lista.Value.categorias.Afegir(nombreNuevo, nombreNuevo);
            }
        }
        public static void AñadirCategoria(string nombre)
        {
            if (listasPorCategoria.Existeix(nombre))
                throw new Exception("Ya existe la categoria");
            listasPorCategoria.Afegir(nombre, new LlistaOrdenada<long, Lista>());
        }
        public static void EliminarCategoria(string nombre)
        {
            if (!listasPorCategoria.Existeix(nombre))
                throw new Exception("No existe la categoria");
            listasPorCategoria.Elimina(nombre);
        }

        public static bool ExisteCategoria(string categoria)
        {
            return listasPorCategoria.Existeix(categoria);
        }


    }
}
