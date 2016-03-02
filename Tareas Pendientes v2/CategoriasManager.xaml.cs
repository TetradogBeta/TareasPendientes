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
            stkCategorias.Children.AddRange(Categoria.Categorias().ToObjViewerArray(EliminarCategoria));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ObjViewer categoriaNuevaView;
            Categoria categoriaNueva;
            //miro que no exista
            if (!Categoria.ExisteCategoria(txtNombreCategoria.Text))
            {
                categoriaNuevaView = new ObjViewer(txtNombreCategoria.Text);
                categoriaNueva = new Categoria(categoriaNuevaView.Object as string);
                categoriaNuevaView.Tag = categoriaNueva;
                stkCategorias.Children.Add(categoriaNuevaView);
                categoriaNuevaView.ObjSelected += EliminarCategoria;
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
                    Categoria.Eliminar((visor.Tag as Categoria));
                    stkCategorias.Children.Remove(visor);
                    //Activa el temporizador para el autoGuardado
                    main.ActivarTemporizadorAutoSave();
                }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new ModificarNombreCategorias(main).ShowDialog();
            stkCategorias.Children.Clear();
            stkCategorias.Children.AddRange(Categoria.Categorias().ToObjViewerArray(EliminarCategoria));
        }
    }
}
