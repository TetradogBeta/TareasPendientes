using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat;
using Gabriel.Cat.Extension;
namespace Tareas_Pendientes_v2
{
    delegate void TareaEventHandler(Tarea tarea);
    class Lista:IClauUnicaPerObjecte,IEnumerable<Tarea>
    {
        static LlistaOrdenada<string, LlistaOrdenada<long,Lista>> listasPorCategoria;
        static Llista<Lista> todasLasListas;

        long idUnico;
        string nombreLista;
        LlistaOrdenada<string, string> categorias;
        LlistaOrdenada<long, Lista> herencia;
        LlistaOrdenada<long, Tarea> todasLasTareasLista;//estan las de la herencia asi no se tienen que ir poniendo siempre
        LlistaOrdenada<long, Tarea> tareasLista;
        LlistaOrdenada<long, Tarea> tareasHechas;
        public event TareaEventHandler TareaNueva;
        public event TareaEventHandler TareaEliminada;

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
        public Tarea[] Filtra(string contenido,bool? estanHechas,DateTime? fecha)
        {
            //por testear
            Llista<Tarea> tareas = new Llista<Tarea>();
            Llista<Tarea> tareasQueNoCoinciden = new Llista<Tarea>();
            string fechaString=fecha.HasValue? fecha.Value.ToShortDateString():"";
            estanHechas = !estanHechas.HasValue ? fecha.HasValue : false;
            if (contenido == null)
                contenido = "";
            else
                contenido = contenido.ToLower();
            foreach(Tarea tarea in this)
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
            if(herencia.Existeix(listaHaHerededar.idUnico))
                throw new Exception("Ya existe la herencia");
            if (!ContieneHerencia(listaHaHerededar))
                throw new Exception("Recursividad infinita, la lista no puede contener a sus hijos");

            //tener en cuenta si añaden o quitan tareas de las listas heredadas
            herencia.Afegir(listaHaHerededar.idUnico, listaHaHerededar);
            todasLasTareasLista.AfegirMolts(listaHaHerededar.todasLasTareasLista);
            listaHaHerededar.TareaEliminada+=QuitarTareaHeredada;
            listaHaHerededar.TareaNueva += AñadirTareaHeredada;
        }

        private void QuitarTareaHeredada(Tarea tareaHaQuitar)
        {
             if (todasLasTareasLista.Existeix(tareaHaQuitar.IdUnico))
                todasLasTareasLista.Elimina(tareaHaQuitar.IdUnico); 
        }
        private void AñadirTareaHeredada(Tarea tareaHaAñadir)
        {
            if (!todasLasTareasLista.Existeix(tareaHaAñadir.IdUnico))
                todasLasTareasLista.Afegir(tareaHaAñadir.IdUnico,tareaHaAñadir);
        }
        public void EliminarHerencia(Lista listaHaDesHeredar)
        {
            if (!herencia.Existeix(listaHaDesHeredar.idUnico))
                throw new Exception("No se hereda de esta lista");
            todasLasTareasLista.Elimina(listaHaDesHeredar.todasLasTareasLista.KeysToArray());
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
            bool contiene = listaHaHerededar.todasLasTareasLista.Count != 0;
            if(contiene)
            contiene= todasLasTareasLista.Existeix(listaHaHerededar.todasLasTareasLista.ElementAt(0).Key);
            return contiene;
        }

        public IComparable Clau()
        {
            return idUnico;
        }
        public IEnumerator<Tarea> GetEnumerator()
        {
            return todasLasTareasLista.ValuesToArray().ObtieneEnumerador();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            Llista<Lista> listas = new Llista<Lista>() ;
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
        public static void CambiarNombreCategoria(string nombreAnt,string nombreNuevo)
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
