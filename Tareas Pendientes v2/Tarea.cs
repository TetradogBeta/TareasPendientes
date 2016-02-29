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
        static LlistaOrdenada<long,LlistaOrdenada<long,Tarea>> tareas=new LlistaOrdenada<long, LlistaOrdenada<long, Tarea>>();
        string contenido;
        LlistaOrdenada<Lista,DateTime> fechasHecho;
        long idUnico;
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
            fechasHecho = new LlistaOrdenada<Lista, DateTime>();
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

        public DateTime FechaHecho(Lista lista)
        {
            return fechasHecho[lista];
        }
        public bool Hecho(Lista lista)
        {
            return !FechaHecho(lista).Equals(default(DateTime));
        }
        public void AñadirTareaHecha(Lista lista,DateTime fechaHecho)
        {
            if(!fechasHecho.Existeix(lista))
            {
                fechasHecho.Afegir(lista, fechaHecho);
            }
        }
        public void AñadirTareaHecha(Lista lista)
        {
            AñadirTareaHecha(lista, DateTime.Now);
        }
        public void QuitarTareaHecha(Lista lista)
        {
            if (fechasHecho.Existeix(lista))
            {
                fechasHecho.Elimina(lista);
            }

        }


        public IComparable Clau()
        {
            return idUnico;
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
        public override string ToString()
        {
            string toString = Contenido == "" ? "'Sin Contenido'" : Contenido;
            return toString;
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
        public static void Hacer(Lista lista, long idTarea, DateTime dateTime)
        {
            if (!tareas.Existeix(lista.IdUnico))
                tareas.Afegir(lista.IdUnico, new LlistaOrdenada<long, Tarea>());
            if(tareas[lista.IdUnico].Existeix(idTarea))
            tareas[lista.IdUnico][idTarea].AñadirTareaHecha(lista, dateTime);
        }
        public static void Deshacer(Lista lista,long idTarea)
        {
            if (tareas.Existeix(lista.IdUnico))
                if (tareas[lista.IdUnico].Existeix(idTarea))
                tareas[lista.IdUnico][idTarea].QuitarTareaHecha(lista);
        }
        public static Tarea[] TareasHechas(Lista lista)
        {
            Tarea[] tareasHechas;
            if (!tareas.Existeix(lista.IdUnico))
                tareasHechas= new Tarea[0];
            else
                tareasHechas = tareas[lista.IdUnico].ValuesToArray().Filtra((tarea) => { return tarea.Hecho(lista); }).ToArray();
            return tareasHechas;
        }
    }
}
