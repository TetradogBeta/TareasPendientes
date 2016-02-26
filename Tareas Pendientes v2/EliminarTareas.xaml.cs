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
    /// Lógica de interacción para EliminarTareas.xaml
    /// </summary>
    public partial class EliminarTareas : Window
    {
        Lista listaHaEditar;
        public EliminarTareas(Lista listaHaEditar)
        {
            InitializeComponent();
            this.listaHaEditar = listaHaEditar;
            txblNombreLista.Text = listaHaEditar.NombreLista;
            stkTareasLista.Children.AddRange(listaHaEditar.ToObjViewerArray(TareaHaEliminar));
            ckOmitirPregunta.IsChecked = false;
        }

        private void TareaHaEliminar(ObjViewer visor)
        {
            if (ckOmitirPregunta.IsChecked.Value||MessageBox.Show("Se va a borrar de forma permanente, estas seguro?", "se requiere su atencion", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                listaHaEditar.EliminarTarea(visor.Object as Tarea);
                stkTareasLista.Children.Remove(visor);
            }
        }
    }
}
