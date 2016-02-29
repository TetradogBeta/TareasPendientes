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
    /// Lógica de interacción para EditorHerencia.xaml
    /// </summary>
    public partial class EditorHerenciaLista : Window
    {
        private Lista listaActual;
        private MainWindow main;
        public EditorHerenciaLista(Lista listaActual,MainWindow main)
        {
            this.main = main;
            InitializeComponent();    
            this.listaActual = listaActual;
            stkHerencia.Children.AddRange(listaActual.Herencia().ToObjViewerArray(QuitarHerencia));
            PonHerenciaValidaAlCmb();
        }

        private void QuitarHerencia(ObjViewer visor)
        {
            if (ckOmitirPregunta.IsChecked.Value || MessageBox.Show("Se va a borrar de forma permanente todas las tareas hechas y ocultas de esa herencia de la lista actual, estas seguro?", "se requiere su atención", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                listaActual.Desheredar(visor.Object as Lista);
                stkHerencia.Children.Remove(visor);
                //Activa el temporizador para el autoGuardado
                main.ActivarTemporizadorAutoSave();
                main.PonTareasLista();
                PonHerenciaValidaAlCmb();
            }
        }

        private void PonHerenciaValidaAlCmb()
        {
            cmbHerenciaPosible.Items.Clear();
            cmbHerenciaPosible.Items.AddRange(Lista.ListasHeredables(listaActual));
            if (cmbHerenciaPosible.Items.Count > 0)
                cmbHerenciaPosible.SelectedIndex = 0;
        }

        private void btnAñadirHerencia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listaActual.Heredar(cmbHerenciaPosible.SelectedItem as Lista);
                stkHerencia.Children.Add(cmbHerenciaPosible.SelectedItem.ToObjViewer(QuitarHerencia));
                PonHerenciaValidaAlCmb();
                //Activa el temporizador para el autoGuardado
                main.ActivarTemporizadorAutoSave();
                main.PonTareasLista();
            }
            catch(Exception m)
            {
                MessageBox.Show(m.Message,"Atención",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
    }
}
