using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gabriel.Cat;
using System.Xml;

using Gabriel.Cat.S.Utilitats;
using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Binaris;

namespace Tareas_Pendientes_V3
{
    public class Tarea :IClauUnicaPerObjecte, IComparable<Tarea>, IComparable,IElementoBinarioComplejo
    {

        public static  ElementoBinario Serializador { get; private set; }
        [IgnoreSerialitzer]
        public static LlistaOrdenada<Tarea> Todas { get; private set; }
        [IgnoreSerialitzer]
        public static LlistaOrdenada<long, LlistaOrdenada<Tarea>> TodasAgrupadas { get; private set; }

        //de la lista
        static Tarea()
        {
            Serializador = ElementoBinario.GetSerializador<Tarea>();
            TodasAgrupadas = new LlistaOrdenada<long, LlistaOrdenada<Tarea>>();
            Todas = new LlistaOrdenada<Tarea>();
        }


        //solo si esa lista tiene la tarea por herencia sino la elimina

        public Tarea(long lista, string contenido)
            : this(lista, contenido, DateTime.Now.Ticks)
        {
        }
        public Tarea(long lista, string contenido, long idUnico)
        {
            this.ContenidoConFormato = contenido;
            this.IdUnico = idUnico;
            this.IdListaParent = lista;
            ListasTareaHecha = new LlistaOrdenada<long, DateTime>();
            ListasTareaOculta = new LlistaOrdenada<long,long>();
            Todas.Add(this);
            if (!TodasAgrupadas.ContainsKey(lista))
                TodasAgrupadas.Add(lista, new LlistaOrdenada<Tarea>());
            if (!TodasAgrupadas[lista].ContainsKey(this))
                TodasAgrupadas[lista].Add(this);
        }



        public Tarea(long lista)
            : this(lista, "")
        {

        }
        public long IdUnico { get; set; }
        public string ContenidoConFormato { get; set; }


        public long IdListaParent { get;  set; }
        public LlistaOrdenada<long, DateTime> ListasTareaHecha { get; set; }
        public LlistaOrdenada<long, long> ListasTareaOculta { get; set; }

        public Lista ListaParent => Lista.Todas[IdListaParent];

        ElementoBinario IElementoBinarioComplejo.Serialitzer => Serializador;

        IComparable IClauUnicaPerObjecte.Clau => IdUnico;

        public int CompareTo(object obj)
        {
            return ICompareTo(obj as Tarea);
        }
        public int CompareTo(Tarea other)
        {
            return ICompareTo(other);
        }
        private int ICompareTo(Tarea other)
        {
            return Equals(other, default) ? -1 : IdUnico.CompareTo(other.IdUnico);
        }


    }
}
