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
            Nombre, IdUnico
        }
        public static ListaUnica<Categoria> CategoriasLista { get; private set; }= new ListaUnica<Categoria>();

        long idUnico;
        string nombre;
        Llista<Lista> listasDeLaCategoria;
        public Categoria(string nombre) : this(DateTime.Now.Ticks, nombre) { }
        public Categoria(long idUnico, string nombre)
        {
            this.IdUnico = idUnico;
            this.Nombre = nombre;
            listasDeLaCategoria = new Llista<Lista>();
            CategoriasLista.Añadir(this);
        }



        public Categoria(XmlNode nodo) : this(Convert.ToInt64(nodo.ChildNodes[(int)NodoXmlCategoria.IdUnico].InnerText), nodo.ChildNodes[(int)NodoXmlCategoria.Nombre].InnerText.DescaparCaracteresXML())
        { }
        public long IdUnico
        {
            get
            {
                return idUnico;
            }

            private set
            {
                idUnico = value;
            }
        }

        public string Nombre
        {
            get
            {
                return nombre;
            }

            set
            {
                nombre = value;
            }
        }
        public void Añadir(Lista lista)
        {
            if (!listasDeLaCategoria.Existeix(lista))
                listasDeLaCategoria.Afegir(lista);
        }

        public void Quitar(Lista lista)
        {
            listasDeLaCategoria.Elimina(lista);
        }

        public Lista[] Listas()
        {
            return listasDeLaCategoria.Filtra((listaValida) => { return !listaValida.EsTemporal; }).ToArray();
        }
        public XmlNode ToXml()
        {
            text nodeText = "<Categoria>";
            XmlDocument nodo = new XmlDocument();
            nodeText &= "<Nombre>" + Nombre.EscaparCaracteresXML() + "</Nombre>";
            nodeText &= "<IdUnico>" + IdUnico + "</IdUnico></Categoria>";
            nodo.LoadXml(nodeText);
            return nodo.FirstChild;

        }
        public IComparable Clau()
        {
            return Nombre;//porque no quiero repetidos :)
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
            return CategoriasLista[idCategoria];
        }
        public static Categoria ObtenerCategoria(string nombre)
        {
            Categoria categoria=null;
            CategoriasLista.WhileEach((categoriaHaMirar) => {
                 if ( categoriaHaMirar.Nombre== nombre)
                     categoria =categoriaHaMirar;
                 return categoria == null;
             });
            return categoria;
        }

        public static bool ExisteCategoria(string nombre)
        {
            return ObtenerCategoria(nombre)!=null;
        }
        public static bool ExisteCategoria(long idCategoria)
        {
            return CategoriasLista.ExisteClave(idCategoria);
        }
        public static Categoria[] Categorias()
        {
            return CategoriasLista.ToArray();
        }
        public static Categoria[] Categorias(Lista lista)
        {
            Llista<Categoria> categoriasLista = new Llista<Categoria>();
           for(int i=0;i< CategoriasLista.Count;i++)
                if (CategoriasLista[i].listasDeLaCategoria.Existeix(lista))
                    categoriasLista.Afegir(CategoriasLista[i]);
            return categoriasLista.ToArray();

        }
        public static void Añadir(Categoria categoria, IEnumerable<Lista> listas)
        {
            foreach (Lista lista in listas)
                Añadir(categoria.IdUnico, lista);
        }
        public static void Añadir(long idCategoria, Lista lista)
        {
            if (!CategoriasLista.ExisteClave(idCategoria)&&!CategoriasLista[idCategoria].listasDeLaCategoria.Existeix(lista))
                CategoriasLista[idCategoria].Añadir(lista);
        }
        public static void Quitar(long idCategoria, Lista lista)
        {
            if (CategoriasLista.ExisteClave(idCategoria) && CategoriasLista[idCategoria].listasDeLaCategoria.Existeix(lista))
                CategoriasLista[idCategoria].Quitar(lista);
        }
        public static XmlNode SaveXmlNodo()
        {
            XmlDocument xmldoc = new XmlDocument();
            text txtNodo = "<Categorias>";
            for (int i = 0; i < CategoriasLista.Count; i++)
                txtNodo &= CategoriasLista[i].ToXml().OuterXml;
            txtNodo &= "</Categorias>";
            xmldoc.LoadXml(txtNodo);
            xmldoc.Normalize();
            return xmldoc.FirstChild;
        }
        public static void LoadXmlNodo(XmlNode nodoCategorias)
        {
            Categoria categoria;
            for (int i = 0; i < nodoCategorias.ChildNodes.Count; i++)
            {
                categoria = new Categoria(nodoCategorias.ChildNodes[i]);
            }
        }
        public static void Eliminar(Categoria categoria)
        {
            for (int i = 0; i < categoria.listasDeLaCategoria.Count; i++)
                Quitar(categoria.IdUnico, categoria.listasDeLaCategoria[i]);
            
        }

    }
}
