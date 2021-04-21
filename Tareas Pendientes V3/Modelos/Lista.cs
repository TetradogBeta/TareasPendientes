using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Gabriel.Cat.S.Binaris;
using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Utilitats;

namespace Tareas_Pendientes_V3
{
    public delegate void TareaEventHandler(Tarea tarea);
    public class Lista : IClauUnicaPerObjecte, IEnumerable<Tarea>, IComparable, IComparable<Lista>, IElementoBinarioComplejo
    {
        public static readonly ElementoBinario Serializador;
        [IgnoreSerialitzer]
        public static LlistaOrdenada<long, Lista> Todas { get; set; }
        #region Atributos y eventos

        #endregion
        static Lista()
        {
            Serializador = ElementoBinario.GetSerializador<Lista>();
            Todas = new LlistaOrdenada<long, Lista>();
        }
        #region Constructores
        public Lista(string nombreLista)
            : this(nombreLista, DateTime.Now.Ticks)
        {
        }
        public Lista(string nombreLista, long idUnico)
        {
            Nombre = nombreLista;
            IdUnico = idUnico;
            Herencia = new SortedList<long, long>();
        }
        public Lista()
            : this("")
        {
        }
        #endregion

        public string Nombre { get; set; }
        public bool EsTemporal
        {
            get { return !Todas.ContainsKey(IdUnico); }
            set
            {
                if (value)
                {
                    if (Todas.ContainsKey(IdUnico))
                    {
                        Todas.Remove(IdUnico);
                        QuitarHerederos(this);
                    }
                }
                else
                {
                    if (!Todas.ContainsKey(IdUnico))
                    {
                        Todas.Add(IdUnico, this);
                    }
                }
            }
        }
        public long IdUnico { get; private set; }

        #region Herencia Obj
        public bool TieneDescendencia
        {
            get
            {
                bool tieneHijos = false;
                Todas.WhileEach((MetodoWhileEach<KeyValuePair<long, Lista>>)((hijoList) =>
                {
                    tieneHijos = !Equals(this, hijoList.Value) && hijoList.Value.Herencia.ContainsKey(IdUnico);
                    return !tieneHijos;
                }));
                return tieneHijos;
            }
        }

        public SortedList<long, long> Herencia { get; set; }



        public void Heredar(Lista lista)
        {
            if (!Herencia.ContainsKey(lista.IdUnico) && EsHeredable(lista))
                Herencia.Add(lista.IdUnico, lista.IdUnico);
        }

        public bool EsHeredable(Lista lista)
        {
            return ((IList<Lista>)ListasHeredables(this)).Contains(lista);
        }

        public void Desheredar(Lista lista)
        {
            if (Herencia.ContainsKey(lista.IdUnico))
            {
                Herencia.Remove(lista.IdUnico);

                foreach(var tarea in Tarea.Todas)
                {
                    tarea.Value.ListasTareaHecha.Remove(lista.IdUnico);
                    tarea.Value.ListasTareaOculta.Remove(lista.IdUnico);
                }
            }
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
            if (other != null)
            {
                compareTo = IdUnico.CompareTo(other.IdUnico);
            }
            else
                compareTo = -1;
            return compareTo;
        }
        public IComparable Clau => IdUnico;

        ElementoBinario IElementoBinarioComplejo.Serialitzer => Serializador;

        public IEnumerable<Tarea> GetVisibles() => GetTodas().Filtra(t=>!t.ListasTareaOculta.ContainsKey(IdUnico));

        public IEnumerable<Tarea> GetTodas()
        {
            Dictionary<long, long> todasListasLista =Herederos(this).ToDictionary((l)=>l);
            todasListasLista.Add(IdUnico, IdUnico);
            return Tarea.Todas.Filtra(t =>todasListasLista.ContainsKey(t.Value.IdListaParent)).Select(t=>t.Value);
        }
        IEnumerator<Tarea> IEnumerable<Tarea>.GetEnumerator()
        {
            return GetTodas().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetTodas().GetEnumerator();
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
        public static Lista[] GetListasWith(string text)
        {
            text = text.ToLowerInvariant();
            return Todas.Filtra((lista) =>
            {
                return lista.Value.Nombre.ToLowerInvariant().Contains(text);
            }).Select(l => l.Value).ToArray();
        }

        #region Herencia clase
        public static Lista[] ListasHeredables(Lista listaActual)
        {
            LlistaOrdenada<long,Lista> listasHeredables = new LlistaOrdenada<long,Lista>();
            listasHeredables.AddRange(Todas);
            listasHeredables.RemoveRange(Herencias(listaActual).ToArray());
            listasHeredables.RemoveRange(Herederos(listaActual).ToArray());
            listasHeredables.Remove(listaActual.IdUnico);

            return listasHeredables.Values.ToArray();
        }

        public static IEnumerable<long> Herencias(Lista listaActual)
        {
            return IHerencias(listaActual,false);
        }

        private static IEnumerable<long> IHerencias(Lista listaActual, bool includeOwnList = true)
        {
            foreach (var l in listaActual.Herencia)
                yield return l.Value;
            if (includeOwnList)
                yield return listaActual.IdUnico;
        }

        public static void QuitarHerederos(Lista listaActual)
        {

            foreach(long heredero in HerederosDirectos(listaActual.IdUnico))
            {
                listaActual.Desheredar(Todas[heredero]);
            }
 
        }

        public static IEnumerable<long> HerederosDirectos(long lista)
        {
            return Todas.Filtra((listaHeredera) =>
                                          listaHeredera.Value.Herencia.ContainsKey(lista)
                                         ).Select(l => l.Key);
        }
        public static IEnumerable<long> Herederos(Lista lista)
        {
           return IHerederos(lista.IdUnico);
        }

        static IEnumerable<long> IHerederos(long lista)
        {
            foreach (long herederoHijo in HerederosDirectos(lista))
            {
                yield return herederoHijo;

                foreach (long herederoNieto in IHerederos(herederoHijo))
                    yield return herederoNieto;
            }
                


        }



        #endregion


        #endregion
    }
}
