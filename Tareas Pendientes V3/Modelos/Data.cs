using Gabriel.Cat.S.Binaris;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tareas_Pendientes_V3
{
    public class Data:IElementoBinarioComplejo
    {
        class DataHelper
        {
            [Inject]
            public IJSRuntime JavaScript { get; set; }
        }
        public const string BDNAME = "bdTareasPendientes";
        public static readonly ElementoBinario Serializador = ElementoBinario.GetSerializador<Data>();
        [IgnoreSerialitzer]
        public static Data DataBase { get; set; }




        public SortedList<long, Categoria> Categorias { get; set; } = new SortedList<long, Categoria>();
        public SortedList<long, Lista> Listas { get; set; } = new SortedList<long, Lista>();
        public SortedList<long, Tarea> Tareas { get; set; } = new SortedList<long, Tarea>();

        ElementoBinario IElementoBinarioComplejo.Serialitzer => Serializador;
        public byte[] GetBytes() => Serializador.GetBytes(this);


        public static Data Get(byte[] dataBytes) => (Data)Serializador.GetObject(dataBytes);
        public static async Task Load()
        {
            DataHelper dataHelper = new DataHelper();
            if (await dataHelper.JavaScript.InvokeAsync<bool>("ExistBD",BDNAME))
            {

            }
            else
            {
                DataBase = new Data();
            }

        }
        public static void Save(Data bd)
        {

        }
      
    }
}
