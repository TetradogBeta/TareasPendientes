using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat;
using Gabriel.Cat.Extension;
using System.Xml;
using Gabriel.Cat.Wpf;

namespace Tareas_Pendientes_v2
{
	public delegate void TareaEventHandler(Tarea tarea);
	public class Lista : IClauUnicaPerObjecte, IEnumerable<Tarea>, IComparable, IComparable<Lista>
	{
		enum NodoLista
		{
			Nombre,
			Id,
			Categorias,
			TareasLista,
			TareasHechas,
			Herencia,
			TareasOcultas
		}
		static LlistaOrdenada<long, Lista> listasGuardadas;
		#region Atributos y eventos
		long idUnico;
		string nombre;
		Llista<Lista> herencia;
		#endregion
		static Lista()
		{
			listasGuardadas = new LlistaOrdenada<long, Lista>();
		}
		#region Constructores
		public Lista(string nombreLista)
			: this(nombreLista, DateTime.Now.Ticks)
		{
		}
		public Lista(string nombreLista, long idUnico)
		{
			this.Nombre= nombreLista;
			this.idUnico = idUnico;
			herencia = new Llista<Lista>();
            Tarea.AñadirLista(this);
		}
		public Lista()
			: this("")
		{
		}
		private Lista(XmlNode nodo)
			: this(nodo.ChildNodes[(int)NodoLista.Nombre].InnerText.DescaparCaracteresXML(), Convert.ToInt64(nodo.ChildNodes[(int)NodoLista.Id].InnerText))
		{
			for (int i = 0; i < nodo.ChildNodes[(int)NodoLista.Categorias].ChildNodes.Count; i++) {//añado la lista en su categoria
				Categoria.Añadir(Convert.ToInt64(nodo.ChildNodes[(int)NodoLista.Categorias].ChildNodes[i].InnerText), this);
			}
			
			for (int i = 0; i < nodo.ChildNodes[(int)NodoLista.TareasLista].ChildNodes.Count; i++) {//añado las tareas de la Lista
				new Tarea(this, nodo.ChildNodes[(int)NodoLista.TareasLista].ChildNodes[i]);
			}
			//la herencia ,las tareas ocultas y hechas  fuera

		}
		#endregion

		public string Nombre {
			get {
				return nombre;
			}

			set
            {
               nombre = value;
			}
		}
        public bool EsTemporal {
			get { return !listasGuardadas.ContainsKey(IdUnico); }
			set {
				if (value) {
					if (listasGuardadas.ContainsKey(IdUnico)) {
						listasGuardadas.Remove(IdUnico);
						QuitarHerederos(this);
					}
				} else {
					if (!listasGuardadas.ContainsKey(IdUnico)) {
						listasGuardadas.Add(IdUnico, this);
					}
				}
			}
		}
		public long IdUnico {
			get {
				return idUnico;
			}
		}
        public bool ActualizarTextoTareas()
        {
            bool hayCambios = false;
            foreach (Tarea tarea in this)
                if (tarea.ActualizarTexto() && !hayCambios)
                    hayCambios = true;
            return  hayCambios;
        }
		#region Herencia Obj
		public bool TieneDescendencia {
			get {
				bool tieneHijos = false;
				listasGuardadas.WhileEach((MetodoWhileEach<KeyValuePair<long,Lista>>)((hijoList) => {
				                          	if (this != hijoList.Value)
				                          		tieneHijos = hijoList.Value.herencia.Contains(this);
				                          	return !tieneHijos;
				                          }));
				return tieneHijos;
			}
		}



		public Lista[] Herencia()
		{
			return herencia.ToArray();
		}

		public void Heredar(Lista lista)
		{
			if (!herencia.Contains(lista) && EsHeredable(lista))
				herencia.Add(lista);
		}

		public bool EsHeredable(Lista lista)
		{
			return ((IList<Lista>)ListasHeredables(this)).Contains(lista);
		}

		public void Desheredar(Lista lista)
		{
            Tarea[] tareas;
            if (herencia.Contains(lista))
            {
                herencia.Remove(lista);
                tareas = Tarea.TareasLista(lista);
                //quitar tareas lista
                for(int i=0;i<tareas.Length;i++)
                {
                    tareas[i].QuitarHecho(this);
                    tareas[i].Desocultar(this);
                }
            }
		}
		#endregion
		#region Xml NodoLista
		public XmlNode ToXml()
		{//por testear  Nombre,Id, Categorias, TareasLista, TareasHechas, Herencia, TareasOcultas
			XmlDocument xml = new XmlDocument();
			Categoria[] categorias = Categoria.Categorias(this);
			Tarea[] tareasLista = Tarea.TareasLista(this);
			Tarea[] tareasHechas = Tarea.TareasHechas(this);
			Tarea[] tareasOcultas = Tarea.TareasOcultas(this);
			StringBuilder strNodo =new StringBuilder( "<Lista>");

            //nombre
            strNodo.Append("<Nombre>");
            strNodo.Append(nombre.EscaparCaracteresXML());
            strNodo.Append("</Nombre>");
            //id
            strNodo.Append("<IdUnico>");
            strNodo.Append(idUnico);
            strNodo.Append("</IdUnico>");
            //categorias
            strNodo.Append("<Categorias>");
            for (int i = 0; i < categorias.Length; i++)
            {
                strNodo.Append("<IdCategoria>");
                strNodo.Append(categorias[i].IdUnico);
                strNodo.Append("</IdCategoria>");
            }
            strNodo.Append("</Categorias>");
            //tareas lista todo el nodo
            strNodo.Append("<TareasLista>");
			for (int i = 0; i < tareasLista.Length; i++)
                strNodo.Append(tareasLista[i].ToXml().OuterXml);
            strNodo.Append("</TareasLista>");
            //tareas hechas solo ids y fecha
            strNodo.Append("<TareasHechas>");
            for (int i = 0; i < tareasHechas.Length; i++)
            {
                strNodo.Append("<TareaHecha><IdTareaHecha>");
                strNodo.Append(tareasHechas[i].IdUnico);
                strNodo.Append("</IdTareaHecha><FechaHecho>");
                strNodo.Append(tareasHechas[i].FechaHecho(this).Ticks);
                strNodo.Append("</FechaHecho></TareaHecha>");
            }
            strNodo.Append("</TareasHechas>");
            //herencia solo ids
            strNodo.Append("<Herencias>");
            for (int i = 0; i < herencia.Count; i++)
            {
                strNodo.Append("<IdHerencia>");
                strNodo.Append(herencia[i].IdUnico);
                strNodo.Append("</IdHerencia>");
            }
            strNodo.Append("</Herencias>");
            //tareas ocultas solo ids
            strNodo.Append("<TareasOcultas>");
            for (int i = 0; i < tareasOcultas.Length; i++)
            {
                strNodo.Append("<TareaOculta>");
                strNodo.Append(tareasOcultas[i].IdUnico);
                strNodo.Append("</TareaOculta>");
            }
            strNodo.Append("</TareasOcultas>");
            strNodo.Append("</Lista>");
			xml.LoadXml(strNodo.ToString());
			xml.Normalize();
			return xml.FirstChild;//mirar si coge el nodo principal
		}
		#endregion
		#region Interficies
		public int CompareTo(object obj)
		{
			return CompareTo(obj as Lista);
		}
		public int CompareTo(Lista other)
		{
			int compareTo;
			if (other != null) {
				compareTo = IdUnico.CompareTo(other.IdUnico);
			} else
				compareTo = -1;
			return compareTo;
		}
		public IComparable Clau
		{
            get
            {
                return idUnico;
            }
		}
		public IEnumerator<Tarea> GetEnumerator()
		{
			return Tarea.TareasVisibles(this).ObtieneEnumerador();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		#endregion
		public override string ToString()
		{
			return String.IsNullOrEmpty(Nombre) || String.IsNullOrWhiteSpace(Nombre) ? "'Sin nombre'" : Nombre;
		}
		public override bool Equals(object obj)
		{
			return Equals(obj as Lista);
		}
		public bool Equals(Lista other)
		{
			bool equals = other != null;
			if (equals)
				equals = IdUnico.Equals(other.IdUnico);
			return equals;
		}
		#region Clase
		public static Lista[] Obtener(string text)
		{
			text = text.ToLowerInvariant();
			return listasGuardadas.Filtra((lista) => {
			                              	return lista.Value.Nombre.ToLowerInvariant().Contains(text);
			                              }).ValuesToArray();
		}
		#region Xml Clase
		//saveXml
		public static XmlNode SaveNodoXml(Lista listaTemporal = null)
		{
			XmlDocument xmldoc = new XmlDocument();
			StringBuilder strNodo = new StringBuilder("<Listas>");
			strNodo.Append( "<ListaTemporal>");
			if (listaTemporal != null)
                strNodo.Append(listaTemporal.ToXml().OuterXml);
            strNodo.Append("</ListaTemporal>");
            strNodo.Append("<ListasGuardadas>");
			foreach (KeyValuePair<long,Lista> lista in listasGuardadas)
                strNodo.Append(lista.Value.ToXml().OuterXml);
            strNodo.Append("</ListasGuardadas>");
            strNodo.Append("</Listas>");
			xmldoc.LoadXml(strNodo.ToString());
			xmldoc.Normalize();
			return xmldoc.FirstChild;
		}
		//loadXml
		public static Lista LoadNodoXml(XmlNode nodoListas)
		{
			XmlNode nodo, nodoListaTemporal = nodoListas.ChildNodes[0].FirstChild;
			Lista lista;
			long idLista;
			listasGuardadas.Clear();
			//pongo las listas guardadas
			nodoListas = nodoListas.ChildNodes[1];
			//creo las listas
			for (int i = 0; i < nodoListas.ChildNodes.Count; i++) {
				lista = new Lista(nodoListas.ChildNodes[i]);
				listasGuardadas.Add(lista.IdUnico, lista);
			}
			//pongo la herencia
			for (int i = 0; i < nodoListas.ChildNodes.Count; i++) {
				idLista = Convert.ToInt64(nodoListas.ChildNodes[i].ChildNodes[(int)NodoLista.Id].InnerText);
				nodo = nodoListas.ChildNodes[i].ChildNodes[(int)NodoLista.Herencia];
				PonHerenciaXml(nodo, listasGuardadas[idLista]);

			}
			//añado tareas ocultas y hechas
			for (int j = 0; j < nodoListas.ChildNodes.Count; j++) {
				nodo = nodoListas.ChildNodes[j];
				PonTareasHechasYOclutasXml(nodo, listasGuardadas[Convert.ToInt64(nodo.ChildNodes[(int)NodoLista.Id].InnerText)]);
			}
			if (nodoListaTemporal != null && nodoListaTemporal.HasChildNodes) {
				lista = new Lista(nodoListaTemporal);
				PonHerenciaXml(nodoListaTemporal.ChildNodes[(int)NodoLista.Herencia], lista);
				PonTareasHechasYOclutasXml(nodoListaTemporal, lista);
			} else {
				lista = null;
			}
			return lista;
		}
		private static void PonHerenciaXml(XmlNode subNodoHerencia, Lista lista)
		{
			for (int j = 0; j < subNodoHerencia.ChildNodes.Count; j++)//añado la herencia que tenga
				listasGuardadas[lista.IdUnico].herencia.Add(listasGuardadas[Convert.ToInt64(subNodoHerencia.ChildNodes[j].InnerText)]);
		}
		private static void PonTareasHechasYOclutasXml(XmlNode nodoListaTemporal, Lista lista)
		{
			for (int i = 0; i < nodoListaTemporal.ChildNodes[(int)NodoLista.TareasOcultas].ChildNodes.Count; i++) {
				Tarea.Obtener(Convert.ToInt64(nodoListaTemporal.ChildNodes[(int)NodoLista.TareasOcultas].ChildNodes[i].InnerText)).Ocultar(lista);
			}
			for (int i = 0; i < nodoListaTemporal.ChildNodes[(int)NodoLista.TareasHechas].ChildNodes.Count; i++) {
				Tarea.Obtener(Convert.ToInt64(nodoListaTemporal.ChildNodes[(int)NodoLista.TareasHechas].ChildNodes[i].ChildNodes[0].InnerText)).AñadirHecho(lista, new DateTime(Convert.ToInt64(nodoListaTemporal.ChildNodes[(int)NodoLista.TareasHechas].ChildNodes[i].ChildNodes[1].InnerText)));
			}
		}
		#endregion
		#region Herencia clase
		public static Lista[] ListasHeredables(Lista listaActual)
		{
			ListaUnica<Lista> listasHeredables = new ListaUnica<Lista>();
			listasHeredables.AddRange(listasGuardadas.ValuesToArray());
			listasHeredables.RemoveRange(Lista.Herencias(listaActual));
			listasHeredables.RemoveRange(Lista.Herederos(listaActual));
			listasHeredables.Remove(listaActual);
			
			return listasHeredables.ToArray();
		}

		public static Lista[] Herencias(Lista listaActual)
		{
			return IHerencias(listaActual).Except(new Lista[]{listaActual}).ToArray();
		}

		private static IEnumerable<Lista> IHerencias(Lista listaActual)
		{
			List<Lista> herencia = new List<Lista>();
			Lista[] listaHerencia = listaActual.Herencia();
			herencia.Add(listaActual);
			for (int i = 0; i < listaHerencia.Length; i++)
				herencia.AddRange(IHerencias(listaHerencia[i]));
			return herencia;
		}

		public static void QuitarHerederos(Lista listaActual)
		{
			Lista[] herederos = HerederosDirectos(listaActual);
			Tarea[] tareasLista = Tarea.TareasLista(listaActual);
			DateTime fechaHecho;
			for (int i = 0; i < herederos.Length; i++) {
				herederos[i].herencia.Remove(listaActual);

			}
			for (int i = 0; i < tareasLista.Length; i++) {
				fechaHecho = default(DateTime);
				if (tareasLista[i].EstaHecha(listaActual)) {
					fechaHecho = tareasLista[i].FechaHecho(listaActual);
				}
				tareasLista[i].VaciarListaHechosYOcultos();
				if (!fechaHecho.Equals(default(DateTime)))
					tareasLista[i].AñadirHecho(listaActual, fechaHecho);
			}
		}

		public static Lista[] HerederosDirectos(Lista lista)
		{
			return listasGuardadas.Filtra((listaHeredera) =>
			                              listaHeredera.Value.herencia.Contains(lista)
			                             ).ValuesToArray();
		}
		public static Lista[] Herederos(Lista lista)
		{
			Lista[] herederos= IHerederos(lista).Except(new Lista[] { lista }).ToArray();
			return herederos;
		}

		static IEnumerable<Lista> IHerederos(Lista lista)//
		{
			List<Lista> todosLosHerederos = new List<Lista>();
			Lista[] herederos = Lista.HerederosDirectos(lista);
			todosLosHerederos.Add(lista);
			for (int i = 0; i < herederos.Length; i++)
				todosLosHerederos.AddRange(IHerederos(herederos[i]));
			return todosLosHerederos;

		}

		#endregion
		/// <summary>
		/// Quita de Categoria,Tarea y la lista de listas guardadas y de se quita de sus heredereos (si esta guardada)
		/// </summary>
		/// <param name="lista">lista a quitar</param>
		public static void Elimina(Lista lista)
		{
			Tarea[] tareasLista = Tarea.TareasLista(lista);
			Categoria[] categorias = Categoria.Categorias(lista);
			lista.EsTemporal = true;
			for (int i = 0; i < tareasLista.Length; i++)
				Tarea.Eliminar(tareasLista[i]);
			for (int i = 0; i < categorias.Length; i++)
				categorias[i].Quitar(lista);
		}
		#endregion
	}
}
