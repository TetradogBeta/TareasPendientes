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
    public class Tarea : IClauUnicaPerObjecte
    {
        enum TareaXml
        {
            Descripcion,
            FechaHecho,
            IdUnico
        }
        string contenido;
        DateTime fechaHecho;
        long idUnico;
        public event TareaEventHandler TareaHecha;
        public event TareaEventHandler TareaNoHecha;
        public Tarea(string contenido)
            : this(contenido, default(DateTime), DateTime.Now.Ticks)
        {
        }
        public Tarea(string contenido, DateTime fechaHecho, long idUnico)
        {
            this.contenido = contenido;
            this.fechaHecho = fechaHecho;
            this.idUnico = idUnico;
        }
        public Tarea(XmlNode nodo)
            : this(nodo.ChildNodes[(int)TareaXml.Descripcion].InnerText.DescaparCaracteresXML(), new DateTime(Convert.ToInt64(nodo.ChildNodes[(int)TareaXml.FechaHecho].InnerText)), Convert.ToInt64(nodo.ChildNodes[(int)TareaXml.IdUnico].InnerText))
        {
        }

        public Tarea()
            : this("")
        {
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

        public DateTime FechaHecho
        {
            get
            {
                return fechaHecho;
            }

            set
            {
                fechaHecho = value;
            }
        }
        public bool Hecho
        {
            get { return !fechaHecho.Equals(default(DateTime)); }
            set
            {
                if (value)
                {
                    fechaHecho = DateTime.Now;
                    if (TareaHecha != null)
                        TareaHecha(this);
                }
                else {
                    fechaHecho = default(DateTime);
                    if (TareaNoHecha != null)
                        TareaNoHecha(this);
                }
            }
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
            nodeText &= "<FechaHecho>" + FechaHecho.Ticks + "</FechaHecho>";
            nodeText &= "<IdUnico>" + IdUnico + "</IdUnico></Tarea>";
            nodo.LoadXml(nodeText);
            return nodo.FirstChild;//mirar si coge el nodo principal

        }
        public override string ToString()
        {
            string toString = Contenido == "" ? "'Sin Contenido'" : Contenido;
            if (Hecho)
                toString += " -fecha " + fechaHecho.ToString() + "-";
            return toString;
        }
    }
}
