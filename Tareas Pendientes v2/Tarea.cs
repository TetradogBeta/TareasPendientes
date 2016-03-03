using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat;
using System.Xml;
using Gabriel.Cat.Extension;
namespace Tareas_Pendientes_v2
{
    public class Tarea : IClauUnicaPerObjecte, IComparable<Tarea>, IComparable
    {

        enum TareaXml
        {
            Descripcion,
            IdUnico
        }
        static ListaUnica<Tarea> todasLasTareas;
        static LlistaOrdenada<Lista, ListaUnica<Tarea>> tareasPorLista;
        //de la lista
        static Tarea()
        {
            tareasPorLista = new LlistaOrdenada<Lista, ListaUnica<Tarea>>();
            todasLasTareas = new ListaUnica<Tarea>();
        }
        Lista lista;
        long idUnico;
        string contenido;
        LlistaOrdenada<Lista, DateTime> listasTareaHecha;
        ListaUnica<Lista> listasTareaOculta;
        //solo si esa lista tiene la tarea por herencia sino la elimina

        public Tarea(Lista lista, string contenido)
            : this(lista, contenido, DateTime.Now.Ticks)
        {
        }
        public Tarea(Lista lista, string contenido, long idUnico)
        {
            this.contenido = contenido;
            this.idUnico = idUnico;
            this.Lista = lista;
            listasTareaHecha = new LlistaOrdenada<Lista, DateTime>();
            listasTareaOculta = new ListaUnica<Lista>();
            todasLasTareas.Añadir(this);
            if (!tareasPorLista.Existeix(lista))
                tareasPorLista.Afegir(lista, new ListaUnica<Tarea>());
            if (!tareasPorLista[lista].ExisteObjeto(this))
                tareasPorLista[lista].Añadir(this);
        }

        public Tarea(Lista lista, XmlNode nodo)
            : this(lista, nodo.ChildNodes[(int)TareaXml.Descripcion].InnerText.DescaparCaracteresXML(), Convert.ToInt64(nodo.ChildNodes[(int)TareaXml.IdUnico].InnerText))
        {
        }

        public Tarea(Lista lista)
            : this(lista, "")
        {

        }
        public long IdUnico
        {
            get
            {
                return idUnico;
            }

            set
            {
                idUnico = value;
            }
        }
        public string Contenido
        {
            get
            {
                return contenido;
            }

            set
            {
                contenido = value;
            }
        }

        public Lista Lista
        {
            get
            {
                return lista;
            }

            private set
            {
                lista = value;

            }
        }

        public bool EstaHecha(Lista lista)
        {
            return listasTareaHecha.Existeix(lista);
        }
        public bool EstaVisible(Lista lista)
        {
            return tareasPorLista.Existeix(lista)&&(tareasPorLista[lista].ExisteObjeto(this) || !listasTareaOculta.ExisteObjeto(lista));
        }

        public DateTime FechaHecho(Lista lista)
        {
            return listasTareaHecha[lista];
        }

        public void AñadirHecho(Lista lista, DateTime fechaHecho)
        {
            listasTareaHecha.AfegirORemplaçar(lista, fechaHecho);
        }
        public void QuitarHecho(Lista lista)
        {
            listasTareaHecha.Elimina(lista);
        }
        public void Ocultar(Lista lista)
        {
            if (this.lista.Equals(lista))
                throw new Exception("No se puede ocultar de la propia lista");

            listasTareaOculta.Añadir(lista);
        }
        public void Desocultar(Lista lista)
        {
            listasTareaOculta.EliminaObjeto(lista);
        }
        public XmlNode ToXml()
        {
            //por testear
            text nodeText = "<Tarea>";
            XmlDocument nodo = new XmlDocument();
            nodeText &= "<Descripcion>" + Contenido.EscaparCaracteresXML() + "</Descripcion>";
            nodeText &= "<IdUnico>" + IdUnico + "</IdUnico></Tarea>";
            nodo.LoadXml(nodeText);
            return nodo.FirstChild;//mirar si coge el nodo principal

        }
        public void VaciarListaHechosYOcultos()
        {
            this.listasTareaHecha.Buida();
            listasTareaOculta.Vaciar();
        }
        public IComparable Clau()
        {
            return idUnico;
        }


        public int CompareTo(Tarea other)
        {
            int compareTo;
            if (other != null)
            {
                compareTo = ToString().CompareTo(other.ToString());
            }
            else
                compareTo = -1;
            return compareTo;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Tarea);
        }

        public override string ToString()
        {
            string toString = Contenido == "" ? "'Sin Contenido'" : Contenido;
            return toString;
        }

        public static Tarea[] Obtener(string text)
        {
            text = text.ToLowerInvariant();
            return todasLasTareas.Filtra((tarea) =>
            {
                return tarea.Contenido.ToLowerInvariant().Contains(text);
            }).ToArray();
        }

        public static Tarea[] TareasLista(Lista lista)
        {
            Tarea[] tareasLista = { };
            if (tareasPorLista.Existeix(lista))
                tareasLista = tareasPorLista[lista].ToArray();
            return tareasLista; 
        }

        public static Tarea[] TareasHeredadas(Lista lista)
        {
            return lista.ToArray().Except(TareasLista(lista)).ToArray();
        }
        public static Tarea[] TareasHechas(Lista lista)
        {
            Tarea[] tareasHechas;
            if (!tareasPorLista.Existeix(lista))
                tareasHechas = new Tarea[0];
            else
                tareasHechas = todasLasTareas.Filtra((tarea) =>
                {
                    return tarea.EstaHecha(lista);
                }).ToArray();
            return tareasHechas;
        }

        public static void AñadirLista(Lista lista)
        {
            if (!tareasPorLista.Existeix(lista))
                tareasPorLista.Afegir(lista, new ListaUnica<Tarea>());

        }
        public static Tarea[] TareasOcultas(Lista lista)
        {
            List<Tarea> tareasOcultas = new List<Tarea>();
            foreach (Tarea tarea in todasLasTareas.ToArray())
                if (tarea.listasTareaOculta.ExisteObjeto(lista))
                    tareasOcultas.Add(tarea);
            return tareasOcultas.ToArray();
        }

        public static Tarea[] TareasVisibles(Lista lista)
        {
            ListaUnica<Tarea> tareasVisibles = new ListaUnica<Tarea>();
            Lista[] herencia = lista.Herencia();
            Tarea[] tareas;
            tareasVisibles.Añadir(Tarea.TareasLista(lista));
            for (int i = 0; i < herencia.Length; i++)
            {
                tareas = TareasVisibles(herencia[i]);
                for(int j=0;j<tareas.Length;j++)
                    if(!tareasVisibles.ExisteObjeto(tareas[j]))
                       tareasVisibles.Añadir(tareas[j]);
            }
            tareasVisibles.Elimina(TareasOcultas(lista));
            
            return tareasVisibles.ToArray();
        }

        public static Tarea Obtener(long idTarea)
        {
            return todasLasTareas[idTarea];
        }

        public static void Eliminar(Tarea tarea)
        {
            if (tarea.lista != null)
            {
                tarea.VaciarListaHechosYOcultos();
                todasLasTareas.Elimina(tarea);
                tareasPorLista[tarea.lista].Elimina(tarea);
                tarea.lista = null;
            }


        }
        public static int CompareTo(Tarea tarea1, Tarea tarea2)
        {
            int compareTo = tarea1 == null ? tarea2 != null ? -1 : 1 : tarea1 != null ? 1 : 0;
            if (compareTo == 0)
                compareTo = tarea1.CompareTo(tarea2);
            return compareTo;

        }


    }
}
