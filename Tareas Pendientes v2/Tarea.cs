using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat;
namespace Tareas_Pendientes_v2
{
    class Tarea:IClauUnicaPerObjecte
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
    }
}
