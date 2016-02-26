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
    public class Tarea:IClauUnicaPerObjecte
    {
        string contenido;
        DateTime fechaHecho;
        long idUnico;
        public Tarea(string contenido) : this(contenido, default(DateTime),DateTime.Now.ToBinary())
        { }
        public Tarea(string contenido, DateTime fechaHecho,long idUnico)
        {
            this.contenido = contenido;
            this.fechaHecho = fechaHecho;
            this.idUnico = idUnico;
        }
        public Tarea(XmlNode nodo):this(nodo.FirstChild.InnerText.DescaparCaracteresXML(),new DateTime(Convert.ToInt64(nodo.ChildNodes[1].InnerText)), Convert.ToInt64(nodo.LastChild.InnerText)) { }
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
                    fechaHecho = DateTime.Now;
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
            nodeText &= "<FechaHecho>" + FechaHecho.ToBinary() + "</FechaHecho>";
            nodeText &= "<IdUnico>" + IdUnico + "</IdUnico></Tarea>";
            nodo.LoadXml(nodeText);
            return nodo.ParentNode;//mirar si coge el nodo principal

        }
        public override string ToString()
        {
            return Contenido;
        }
    }
}
