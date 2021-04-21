using Gabriel.Cat.S.Binaris;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tareas_Pendientes_V3
{
    public class Data:IElementoBinarioComplejo
    {
        public const string BDNAME = "bdTareasPendientes";
        public static readonly ElementoBinario Serializador = ElementoBinario.GetSerializador<Data>();
        public SortedList<long, Categoria> Categorias { get; set; } = new SortedList<long, Categoria>();
        public SortedList<long, Lista> Listas { get; set; } = new SortedList<long, Lista>();
        public SortedList<long, Tarea> Tareas { get; set; } = new SortedList<long, Tarea>();

        ElementoBinario IElementoBinarioComplejo.Serialitzer => Serializador;
        public byte[] GetBytes() => Serializador.GetBytes(this);


        public static Data Get(byte[] dataBytes) => (Data)Serializador.GetObject(dataBytes);
        public static Data Load()
        {
            Data data;
            if (false)
            {

            }
            else
            {
                data = new Data();
            }
            return data;
        }
        public static void Save(Data bd)
        {

        }
      
    }
}
