using Gabriel.Cat.Extension;
using Gabriel.Cat.Wpf;
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

namespace Tareas_Pendientes_v2
{
    /// <summary>
    /// Lógica de interacción para CategoriasManager.xaml
    /// </summary>
    public partial class CategoriasManager : Window
    {
        //poder cambiar nombre de una categoria
        MainWindow main;
        public CategoriasManager(MainWindow main)
        {
            this.main = main;
            InitializeComponent();
            stkCategorias.Children.AddRange(Lista.TodasLasCategorias().ToObjViewerArray(EliminarCategoria));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ObjViewer categoriaNueva;
            //miro que no exista
            if (!Lista.ExisteCategoria(txtNombreCategoria.Text))
            {
                categoriaNueva = new ObjViewer(txtNombreCategoria.Text);
                //Añado
                Lista.AñadirCategoria(categoriaNueva.Object as string);
                stkCategorias.Children.Add(categoriaNueva);
                categoriaNueva.ObjSelected += EliminarCategoria;
                //Activa el temporizador para el autoGuardado
                main.ActivarTemporizadorAutoSave();
                txtNombreCategoria.Text = "";
            }
            else
            {
                //informo
                MessageBox.Show("La categoria ya existe", "Atencion", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EliminarCategoria(ObjViewer visor)
        {
            if (visor.Object.ToString() != MainWindow.TODASLASLISTAS)
                if (ckOmitirPregunta.IsChecked.Value || MessageBox.Show("Se va a borrar de forma permanente, estas seguro?", "se requiere su atención", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    Lista.EliminarCategoria(visor.Object as string);
                    stkCategorias.Children.Remove(visor);
                    //Activa el temporizador para el autoGuardado
                    main.ActivarTemporizadorAutoSave();
                }
        }
    }
}
