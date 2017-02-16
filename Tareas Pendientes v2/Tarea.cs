using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gabriel.Cat;
using System.Xml;
using Gabriel.Cat.Extension;
using Gabriel.Cat.Wpf;
using System.Windows;
using System.Windows.Threading;

namespace Tareas_Pendientes_v2
{
    public class Tarea :IClauUnicaPerObjecte, IComparable<Tarea>, IComparable
    {

        enum TareaXml
        {
            Descripcion,
            IdUnico
        }
        public static Dispatcher Dispatcher;
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
        RicoTextBox rtbContenido;
        string contenidoConFormato;
        string contenidoSinFormato;
        ulong lastSavedTime;
        LlistaOrdenada<Lista, DateTime> listasTareaHecha;
        ListaUnica<Lista> listasTareaOculta;
        //solo si esa lista tiene la tarea por herencia sino la elimina

        public Tarea(Lista lista, string contenido)
            : this(lista, contenido, DateTime.Now.Ticks)
        {
        }
        public Tarea(Lista lista, string contenido, long idUnico)
        {
            this.contenidoConFormato = contenido;
            this.idUnico = idUnico;
            this.Lista = lista;
            listasTareaHecha = new LlistaOrdenada<Lista, DateTime>();
            listasTareaOculta = new ListaUnica<Lista>();
            todasLasTareas.Add(this);
            if (!tareasPorLista.ContainsKey(lista))
                tareasPorLista.Add(lista, new ListaUnica<Tarea>());
            if (!tareasPorLista[lista].Contains(this))
                tareasPorLista[lista].Add(this);
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
        public string ContenidoConFormato
        {
            get
            {
                return contenidoConFormato;
            }

            set
            {
                contenidoConFormato = value;
                if(rtbContenido!=null)
                   RtbContenido.TextWithFormat = value;
            }
        }
        
        public RicoTextBox RtbContenido
        {
            get {
                Action act;
                if (rtbContenido == null)
                {
                    act = () =>
                    {
                        rtbContenido = new RicoTextBox();
                        try
                        {
                            rtbContenido.TextWithFormat = contenidoConFormato;
                        }
                        catch {
                            rtbContenido.Text = contenidoSinFormato;
                            contenidoConFormato = rtbContenido.TextWithFormat;
                        }
                    };
                    Dispatcher.BeginInvoke(act).Wait();
                }
                return rtbContenido; }
            private set { rtbContenido = value; }
        }

        public string ContenidoSinFormato
        {
            get
            {
                if (contenidoSinFormato == null)
                {
                    contenidoSinFormato = RtbContenido.Text;
                }
                return contenidoSinFormato;
            }
            set
            {
                if (value == null) value = "";
                contenidoSinFormato = value;
                if (rtbContenido != null)
                    RtbContenido.Text= value;
            }
        }
        public bool Actualizable {
            get {
                bool actualizable = false;
                if(rtbContenido!=null)
                actualizable= lastSavedTime < RtbContenido.TextChangedTimes;
                return actualizable;
            }
        }
        public bool ActualizarTexto()
        {
            bool actualizado = false;
            if (Actualizable)
            {
                contenidoSinFormato = RtbContenido.Text;
                contenidoConFormato = RtbContenido.TextWithFormat;
                lastSavedTime = RtbContenido.TextChangedTimes;
                actualizado = true;
            }
            return actualizado;
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
            return listasTareaHecha.ContainsKey(lista);
        }
        public bool EstaVisible(Lista lista)
        {
            return tareasPorLista.ContainsKey(lista)&&(tareasPorLista[lista].Contains(this) || !listasTareaOculta.Contains(lista));
        }

        public DateTime FechaHecho(Lista lista)
        {
            return listasTareaHecha[lista];
        }

        public void AñadirHecho(Lista lista, DateTime fechaHecho)
        {
            listasTareaHecha.AddOrReplace(lista, fechaHecho);
        }
        public void QuitarHecho(Lista lista)
        {
            listasTareaHecha.Remove(lista);
        }
        public void Ocultar(Lista lista)
        {
            if (this.lista.Equals(lista))
                throw new Exception("No se puede ocultar de la propia lista");
            //quitar herederos hechos y ocultos
            Lista[] herederos = Lista.Herederos(lista);
            for (int i = 0; i < herederos.Length; i++)
            {
                QuitarHecho(herederos[i]);
                Desocultar(herederos[i]);
            }
            listasTareaOculta.Add(lista);
            QuitarHecho(lista);
        }
        public void Desocultar(Lista lista)
        {
            listasTareaOculta.Remove(lista);
        }
        public XmlNode ToXml()
        {
            //por testear
            StringBuilder nodeText =new StringBuilder( "<Tarea>");
            XmlDocument nodo = new XmlDocument();
            ActualizarTexto();
            nodeText.Append("<Descripcion>");
            nodeText.Append(ContenidoConFormato.EscaparCaracteresXML());
            nodeText.Append( "</Descripcion>");
            nodeText.Append("<IdUnico>");
            nodeText.Append(IdUnico);
            nodeText.Append("</IdUnico></Tarea>");
            nodo.LoadXml(nodeText.ToString());
            return nodo.FirstChild;//mirar si coge el nodo principal

        }
        public void VaciarListaHechosYOcultos()
        {
            this.listasTareaHecha.Clear();
            listasTareaOculta.Clear();
        }
        public IComparable Clau
        {
            get
            {
                return idUnico;
            }
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
            string contenido = ContenidoSinFormato;
            string toString = contenido == "" ? "'Sin ContenidoConFormato'" : contenido;
            return toString;
        }

        public static Tarea[] Obtener(string text)
        {
            text = text.ToLowerInvariant();
            return todasLasTareas.Filtra((tarea) =>
            {
                tarea.ActualizarTexto();
                return tarea.ContenidoSinFormato.ToLowerInvariant().Contains(text);
            }).ToArray();
        }

        public static Tarea[] TareasLista(Lista lista)
        {
            Tarea[] tareasLista = { };
            if (tareasPorLista.ContainsKey(lista))
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
            if (!tareasPorLista.ContainsKey(lista))
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
            if (!tareasPorLista.ContainsKey(lista))
                tareasPorLista.Add(lista, new ListaUnica<Tarea>());

        }
        public static Tarea[] TareasOcultas(Lista lista)
        {
            List<Tarea> tareasOcultas = new List<Tarea>();
            Tarea[] tareas = todasLasTareas.ToArray();
            for(int i=0;i<tareas.Length;i++)
                if (tareas[i].listasTareaOculta.Contains(lista))
                    tareasOcultas.Add(tareas[i]);
            return tareasOcultas.ToArray();
        }

        public static Tarea[] TareasVisibles(Lista lista)
        {
            ListaUnica<Tarea> tareasVisibles = new ListaUnica<Tarea>();
            Lista[] herencia = lista.Herencia();
            Tarea[] tareas;
            tareasVisibles.AddRange(Tarea.TareasLista(lista));
            for (int i = 0; i < herencia.Length; i++)
            {
                tareas = TareasVisibles(herencia[i]);
                for(int j=0;j<tareas.Length;j++)
                    if(!tareasVisibles.Contains(tareas[j]))
                       tareasVisibles.Add(tareas[j]);
            }
            tareasVisibles.RemoveRange(TareasOcultas(lista));
            
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
                todasLasTareas.Remove(tarea);
                tareasPorLista[tarea.lista].Remove(tarea);
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



        public static Tarea[] Todas()
        {
            return todasLasTareas.ToArray();
        }
    }
}
