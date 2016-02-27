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
        public EliminarTareas(Lista listaHaEditar,MainWindow main)
        {
         
            InitializeComponent();
            this.main = main;
            this.listaHaEditar = listaHaEditar;
            txblNombreLista.Text ="Lista: "+ listaHaEditar.NombreLista;
            ckOmitirPregunta.IsChecked = false;
            stkTareasLista.Children.Clear();
            stkTareasLista.Children.AddRange(listaHaEditar.ToObjViewerArray(TareaHaEliminar));
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
            if (ckOmitirPregunta.IsChecked.Value || MessageBox.Show("Se va a borrar de forma permanente, estas seguro?", "se requiere su atención", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                listaHaEditar.EliminarTarea(visor.Object as Tarea);
                main.ActivarTemporizadorAutoSave();
                main.PonTareasLista();
                stkTareasLista.Children.Remove(visor);
                CerrarSiNoHayTareasHaEliminar();
            }
        }
    }
}
