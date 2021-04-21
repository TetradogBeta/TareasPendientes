using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections;
using Gabriel.Cat.S.Utilitats;
using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Binaris;

namespace Tareas_Pendientes_V3
{
	public class Categoria : IClauUnicaPerObjecte, IComparable, IComparable<Categoria>,IElementoBinarioComplejo
	{
		public static readonly ElementoBinario Serializador = ElementoBinario.GetSerializador<Categoria>();
	
		[IgnoreSerialitzer]
		public static LlistaOrdenada<long,Categoria> TodasLasCateogiras { get; private set; } = new LlistaOrdenada<long,Categoria>();

		public Categoria(string nombre)
			: this(DateTime.Now.Ticks, nombre)
		{
		}
		public Categoria(long idUnico, string nombre)
		{
			this.IdUnico = idUnico;
			this.Nombre = nombre;
			TodasLasCateogiras.Add(IdUnico,this);
		}




        public long IdUnico { get; private set; }

        public string Nombre { get; set; }
		
		public SortedList<long,long> Listas { get; set; }

		public Lista[] GetListas() => Listas.Values.Convert((l) => Lista.Todas[l]);
    

        public IComparable Clau => IdUnico;

		ElementoBinario IElementoBinarioComplejo.Serialitzer => Serializador;

        public int CompareTo(object obj)
		{
			return CompareTo(obj as Categoria);
		}

		public int CompareTo(Categoria other)
		{
			int compareTo;
			if (other != null)
				compareTo = IdUnico.CompareTo(other.IdUnico);
			else
				compareTo = -1;
			return compareTo;
		}
		public override string ToString()
		{
			return Nombre;
		}
	
	}
}
