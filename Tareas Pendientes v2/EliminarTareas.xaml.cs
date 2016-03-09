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
using Gabriel.Cat;
using Gabriel.Cat.Extension;
using Gabriel.Cat.Wpf;
using System.Threading;

namespace Tareas_Pendientes_v2
{
    /// <summary>
    /// Lógica de interacción para EliminarTareas.xaml
    /// </summary>
    public partial class EliminarTareas : Window
    {
        MainWindow main;
        Lista listaHaEditar;
        ListaUnica<Tarea> listaTareas;
        public EliminarTareas(Lista listaHaEditar, MainWindow main)
        {

            InitializeComponent();
            this.main = main;
            this.listaHaEditar = listaHaEditar;
            listaTareas = Tarea.TareasLista(listaHaEditar).ToListaUnica();
            txblNombreLista.Text = "Lista: " + listaHaEditar.Nombre;
            ckOmitirPregunta.IsChecked = false;
            stkTareasLista.Children.AddRange(listaHaEditar.ToObjViewerArray(TareaHaEliminar));
            if (stkTareasLista.Children.Count == 0)
                throw new Exception("Cerrar");
        }

        private void CerrarSiNoHayTareasHaEliminar()
        {

            if (stkTareasLista.Children.Count == 0)
            {
                MessageBox.Show("No hay tareas ha eliminar", "Cerrando ventana", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        private void TareaHaEliminar(ObjViewer visor)
        {
            Tarea tarea = visor.Object as Tarea;
            bool hacer = false,esHeredada= !listaTareas.ExisteObjeto(tarea);
            if (ckOmitirPregunta.IsChecked.Value  || !esHeredada&&MessageBox.Show("Se va a borrar de forma permanente, estas seguro?", "se requiere su atención", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes ||esHeredada&&MessageBox.Show("Se va ocultar", "Informacion", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                if (esHeredada)
                {
                    hacer = Lista.Herederos(listaHaEditar).Length == 0;
                    if (ckOmitirPregunta.IsChecked.Value || !hacer && MessageBox.Show("Esta lista tiene herederos  y se quitara la tarea  tambien de ellos (si esta hecha o oculta), estas conforme?", "se requiere su atención", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                    {
                        hacer = true;
                    }

                }
                else
                {
                    hacer = true;
                }

                if (hacer)
                {
                    try
                    {
                        tarea.Ocultar(listaHaEditar);
                    }
                    catch
                    {
                        Tarea.Eliminar(tarea);
                    }

                    main.ActivarTemporizadorAutoSave();
                    main.PonTareasLista();
                    stkTareasLista.Children.Remove(visor);
                    CerrarSiNoHayTareasHaEliminar();
                }
            }
        }
    }
}
