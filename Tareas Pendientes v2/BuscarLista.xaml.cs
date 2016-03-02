using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gabriel.Cat.Extension;
using Gabriel.Cat.Wpf;

namespace Tareas_Pendientes_v2
{
    /// <summary>
    /// Lógica de interacción para BuscarLista.xaml
    /// </summary>
    public partial class BuscarLista : Window
    {

        private MainWindow mainWindow;
        public BuscarLista(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;//para poder seleccionar la lista             mainWindow.PonLista(lista);

        }

        void TxtBxTextoHaBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tarea[] tareas;
            Lista[] listas;
            stkListasEncontradas.Children.Clear();
            if (rdBtnDescripcionTarea.IsChecked.Value)
            {
                //busco las tareas que contienen esa descripcion
                tareas = Tarea.Obtener(txtBxTextoHaBuscar.Text);
                //pongo las listas
                for (int i = 0; i < tareas.Length; i++)
                    stkListasEncontradas.Children.AddRange(ListaTareaHeredada.ListasTarea(tareas[i]).ToObjViewerArray(VisualizaLista));
            }
            else {
                //busco las listas que contienen ese nombre
                listas = Lista.Obtener(txtBxTextoHaBuscar.Text);
                stkListasEncontradas.Children.AddRange(listas.ToObjViewerArray(VisualizaLista));
            }
        }
        private void VisualizaLista(ObjViewer visor)
        {
            if (visor.Object is ListaTareaHeredada)
                mainWindow.PonLista(((ListaTareaHeredada)visor.Object).Lista);
            else
                mainWindow.PonLista(visor.Object as Lista);
        }
        void RdBtnNombreLista_Checked(object sender, RoutedEventArgs e)
        {
            if (mainWindow != null)
                TxtBxTextoHaBuscar_TextChanged(null, null);
        }

    }
    public class ListaTareaHeredada
    {
        Lista lista;
        Tarea tarea;
        bool mostrarLista;
        public ListaTareaHeredada(Lista lista, Tarea tarea, bool mostrarLista)
        {
            this.Lista = lista;
            this.Tarea = tarea;
            this.mostrarLista = mostrarLista;
        }

        public Lista Lista
        {
            get
            {
                return lista;
            }

            set
            {
                lista = value;
            }
        }

        public Tarea Tarea
        {
            get
            {
                return tarea;
            }

            set
            {
                tarea = value;
            }
        }
        public override string ToString()
        {
            string toString = lista != null && mostrarLista ? lista.ToString() + "->" : "";
            toString += tarea != null ? tarea.ToString() : "";
            if (lista != null && tarea != null)
                toString += !Tarea.TareasLista(lista).Contains(tarea) ? " #heredada" : "";
            return toString;
        }
        public static ListaTareaHeredada[] ListasTarea(Tarea tarea, bool mostrarLista = true)
        {
            Lista[] listas;
            List<ListaTareaHeredada> listasTarea = new List<ListaTareaHeredada>();
            if (tarea.Lista != null && !tarea.Lista.EsTemporal)
            {
                listas = Lista.Herederos(tarea.Lista);
                for (int i = 0; i < listas.Length; i++)
                    listasTarea.Add(new ListaTareaHeredada(listas[i], tarea, mostrarLista));
                listasTarea.Add(new ListaTareaHeredada(tarea.Lista, tarea, mostrarLista));
            }
            return listasTarea.ToArray();
        }

        public static IEnumerable<ListaTareaHeredada> ToLista(IEnumerable<Tarea> tareas, Lista lista, bool mostrarLista = true)
        {
            List<ListaTareaHeredada> listaTareas = new List<ListaTareaHeredada>();
            foreach (Tarea tarea in tareas)
            {

                listaTareas.Add(new ListaTareaHeredada(lista, tarea, mostrarLista));
            }
            return listaTareas;
        }
    }
}
