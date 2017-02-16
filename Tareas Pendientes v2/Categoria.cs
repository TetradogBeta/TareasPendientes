using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat;
using System.Xml;
using Gabriel.Cat.Extension;
using System.Collections;

namespace Tareas_Pendientes_v2
{
	public class Categoria : IClauUnicaPerObjecte, IComparable, IComparable<Categoria>
	{
		enum NodoXmlCategoria
		{
			Nombre,
			IdUnico
		}
		static ListaUnica<Categoria> categorias = new ListaUnica<Categoria>();

		long idUnico;
		string nombre;
		Llista<Lista> listasDeLaCategoria;
		public Categoria(string nombre)
			: this(DateTime.Now.Ticks, nombre)
		{
		}
		public Categoria(long idUnico, string nombre)
		{
			this.IdUnico = idUnico;
			this.Nombre = nombre;
			listasDeLaCategoria = new Llista<Lista>();
			categorias.Add(this);
		}



		public Categoria(XmlNode nodo)
			: this(Convert.ToInt64(nodo.ChildNodes[(int)NodoXmlCategoria.IdUnico].InnerText), nodo.ChildNodes[(int)NodoXmlCategoria.Nombre].InnerText.DescaparCaracteresXML())
		{
		}

		public long IdUnico {
			get {
				return idUnico;
			}

			private set {
				idUnico = value;
			}
		}

		public string Nombre {
			get {
				return nombre;
			}

			set {
				nombre = value;
			}
		}
		public static ListaUnica<Categoria> CategoriasLista {
			get{ return categorias; }
		}

		public bool EstaDentro(Lista listaActual)
		{
			return listasDeLaCategoria.Contains(listaActual);
		}
		public void AñadirExistentes(Lista[] listas)
		{
			if (listas != null)
				for (int i = 0; i < listas.Length; i++)
					if (!listasDeLaCategoria.Contains(listas[i]))
						Añadir(listas[i]);
		}
		public void Añadir(Lista lista)
		{
			if (!listasDeLaCategoria.Contains(lista))
				listasDeLaCategoria.Add(lista);
		}

		public void Quitar(Lista lista)
		{
			listasDeLaCategoria.Remove(lista);
		}

		public Lista[] Listas()
		{
			return listasDeLaCategoria.Filtra((listaValida) => {
				return !listaValida.EsTemporal;
			}).ToArray();
		}
		public XmlNode ToXml()
		{
			XmlDocument nodo = new XmlDocument();
			StringBuilder nodeText =new StringBuilder( "<Categoria>");
            nodeText.Append("<Nombre>");
            nodeText.Append(Nombre.EscaparCaracteresXML());
            nodeText.Append("</Nombre>");
            nodeText.Append("<IdUnico>");
            nodeText.Append(IdUnico);
            nodeText.Append("</IdUnico>");
            nodeText.Append("</Categoria>");
			nodo.LoadXml(nodeText.ToString());
			return nodo.FirstChild;

		}
		public IComparable Clau
		{
            get
            {
                return IdUnico;
            }
		}

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
			return nombre;
		}
		public static Categoria ObtenerCategoria(long idCategoria)
		{
			return categorias[idCategoria];
		}
		public static Categoria ObtenerCategoria(string nombre)
		{
			Categoria categoria = null;
			categorias.WhileEach((categoriaHaMirar) => {
				if (categoriaHaMirar.Nombre == nombre)
					categoria = categoriaHaMirar;
				return categoria == null;
			});
			return categoria;
		}

		public static bool ExisteCategoria(string nombre)
		{
			return ObtenerCategoria(nombre) != null;
		}
		public static bool ExisteCategoria(long idCategoria)
		{
			return categorias.Contains(idCategoria);
		}
		public static Categoria[] Categorias()
		{
			return categorias.ToArray();
		}
		public static Categoria[] Categorias(Lista lista)
		{
			Llista<Categoria> categoriasLista = new Llista<Categoria>();
			foreach (Categoria categoria in categorias)
				if (categoria.listasDeLaCategoria.Contains(lista))
					categoriasLista.Add(categoria);
			return categoriasLista.ToArray();

		}
		public static bool JuntarSiCoincideNombreNuevo(Categoria categoria,string nombreNuevo)
		{
			Categoria old=ObtenerCategoria(nombreNuevo);
			bool coincide = categorias.Contains(old.idUnico);
			if (coincide) {
				old.AñadirExistentes(categoria.Listas());
			}
			return coincide;
		}
		public static void Añadir(Categoria categoria, IList<Lista> listas)
		{
			for(int i=0;i<listas.Count;i++)
				Añadir(categoria.IdUnico, listas[i]);
		}
		public static void Añadir(long idCategoria, Lista lista)
		{
			if (categorias.Contains(idCategoria) && !categorias[idCategoria].listasDeLaCategoria.Contains(lista))
				categorias[idCategoria].Añadir(lista);
		}
		public static void Quitar(long idCategoria, Lista lista)
		{
			if (categorias.Contains(idCategoria) && categorias[idCategoria].listasDeLaCategoria.Contains(lista))
				categorias[idCategoria].Quitar(lista);
		}
		public static XmlNode SaveXmlNodo()
		{
			XmlDocument xmldoc = new XmlDocument();
			StringBuilder strNodo =new StringBuilder( "<Categorias>");
			foreach (Categoria categoria in categorias)
				strNodo.Append(categoria.ToXml().OuterXml);
            strNodo.Append("</Categorias>");
			xmldoc.LoadXml(strNodo.ToString());
			xmldoc.Normalize();
			return xmldoc.FirstChild;
		}
		public static void LoadXmlNodo(XmlNode nodoCategorias)
		{
			Categoria categoria;
			for (int i = 0; i < nodoCategorias.ChildNodes.Count; i++) {
				categoria = new Categoria(nodoCategorias.ChildNodes[i]);
			}
		}
		public static void Eliminar(Categoria categoria)
		{
			if (categoria != null) {
				for (int i = 0; i < categoria.listasDeLaCategoria.Count; i++)
					Quitar(categoria.IdUnico, categoria.listasDeLaCategoria[i]);
				CategoriasLista.Remove(categoria);
			}
		}

	}
}
