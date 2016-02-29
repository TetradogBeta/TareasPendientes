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
    public class Tarea : IClauUnicaPerObjecte,IComparable<Tarea>,IComparable
    {

        enum TareaXml
        {
            Descripcion,
            IdUnico
        }
        static LlistaOrdenada<long, Tarea> todasLasTareas;
        static LlistaOrdenada<Lista,Llista<Tarea>> tareasPorLista;//de la lista
        static Tarea()
        {
            tareasPorLista = new LlistaOrdenada<Lista, Llista<Tarea>>();
            todasLasTareas = new LlistaOrdenada<long, Tarea>();
        }
        long idUnico;
        string contenido;
        LlistaOrdenada<Lista,DateTime> listasTareaHecha;
        Llista<Lista> listasTareaOculta;//solo si esa lista tiene la tarea por herencia sino la elimina
       
        public event TareaEventHandler TareaHecha;
        public event TareaEventHandler TareaNoHecha;


        public Tarea(string contenido)
            : this(contenido, DateTime.Now.Ticks)
        {
        }
        public Tarea(string contenido, long idUnico)
        {
            this.contenido = contenido;
            this.idUnico = idUnico;
            listasTareaHecha = new LlistaOrdenada<Lista, DateTime>();
            listasTareaOculta = new Llista<Lista>();
        }
        public Tarea(XmlNode nodo)
            : this(nodo.ChildNodes[(int)TareaXml.Descripcion].InnerText.DescaparCaracteresXML(), Convert.ToInt64(nodo.ChildNodes[(int)TareaXml.IdUnico].InnerText))
        {
        }

        public Tarea()
            : this("")
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
        public bool EstaHecha(Lista lista)
        {
            return listasTareaHecha.Existeix(lista);
        }
        public bool EstaDisponible(Lista lista)
        {
            return tareasPorLista[lista].Existeix(this)||!listasTareaOculta.Existeix(lista);
        }
        public void Añadir(Lista lista)
        {
            if (!tareasPorLista.Existeix(lista))
                tareasPorLista.Afegir(lista, new Llista<Tarea>());
            if(!tareasPorLista[lista].Existeix(lista))
                 tareasPorLista[lista].Afegir(this);
        }
       public void AñadirHecho(Lista lista,DateTime fechaHecho)
        {
            listasTareaHecha.AfegirORemplaçar(lista, fechaHecho);
        }
        public void QuitarHecho(Lista lista)
        {
            listasTareaHecha.Elimina(lista);
        }
        public void Quitar(Lista lista)
        {
            if (!tareasPorLista.Existeix(lista))
                tareasPorLista.Afegir(lista, new Llista<Tarea>());
            if (tareasPorLista[lista].Existeix(this))
                tareasPorLista[lista].Elimina(this);
            else  if(!listasTareaOculta.Existeix(lista))
                listasTareaOculta.Afegir(lista);
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


        public static Tarea[] TareasLista(Lista lista)
        {
            Tarea[] tareasLista = { };
            if (tareasPorLista.Existeix(lista))
                tareasLista = tareasPorLista[lista].ToArray();
            return tareasLista;
        }

        public static Tarea[] TareasHechas(Lista lista)
        {
            Tarea[] tareasHechas;
            if (!tareasPorLista.Existeix(lista))
                tareasHechas = new Tarea[0]; 
            else
               tareasHechas= tareasPorLista[lista].Filtra((tarea) => { return tarea.EstaHecha(lista); }).ToArray();
            return tareasHechas;
        }

        public DateTime FechaHecho(Lista lista)
        {
            return listasTareaHecha[lista];
        }

        public static Tarea[] TareasOcultas(Lista lista)
        {
            Tarea[] tareasOcultas;
            if (!tareasPorLista.Existeix(lista))
                tareasOcultas = new Tarea[0];
            else
                tareasOcultas = tareasPorLista[lista].Filtra((tarea) => { return !tarea.EstaDisponible(lista); }).ToArray();
            return tareasOcultas;
        }
        public static Tarea Obtener(long idTarea)
        {
          return  todasLasTareas[idTarea];
        }

        public static Tarea[] TareasVisibles(Lista lista)
        {
            Llista<Tarea> tareasVisibles = new Llista<Tarea>(Tarea.TareasLista(lista));
            Lista[] herencia = lista.Herencia();
            for (int i = 0; i < herencia.Length; i++)
                tareasVisibles.AfegirMolts(TareasVisibles(herencia[i]));
            tareasVisibles.Elimina(TareasOcultas(lista));
            return tareasVisibles.ToArray();
        }
    }
}
