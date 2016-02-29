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
    /// Lógica de interacción para ModificarNombreCategorias.xaml
    /// </summary>
    public partial class ModificarNombreCategorias : Window
    {
        private MainWindow main;

        public ModificarNombreCategorias(MainWindow main)
        {
            TextBox txtCategoria;
            Categoria[] categorias;
            InitializeComponent();
            categorias = Categoria.Categorias();
            for(int i=0;i<categorias.Length;i++)
            {
                txtCategoria = new TextBox();
                txtCategoria.Text = categorias[i].Nombre;
                txtCategoria.Tag = categorias[i];//es la version sin modificar y que se remplaza cuando se aplica
                //no hay un evento para saber si esta acabdo de editar sino lo pondria y quitaria el boton   
                if (txtCategoria.Text == MainWindow.TODASLASLISTAS)
                    txtCategoria.IsReadOnly = true;//asi no lo modifican :)
                stkCategorias.Children.Add(txtCategoria);

            }      
            this.main = main;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            //comprueba que sean diferentes y luego lo aplica, sino avisa
            SortedList<Categoria, TextBox> listaDeCategorias = new SortedList<Categoria, TextBox>();
            Categoria categoria;
            TextBox txtBxCategoria;
            try {
                for (int i = 0; i < stkCategorias.Children.Count; i++) {
                    txtBxCategoria =(TextBox)stkCategorias.Children[i];
                    categoria = txtBxCategoria.Tag as Categoria;
                    listaDeCategorias.Add(categoria, txtBxCategoria);

                }
                foreach(KeyValuePair<Categoria,TextBox> txtCategoria in listaDeCategorias)
                {
                    txtCategoria.Key.Nombre = txtCategoria.Value.Text;
                }
                MessageBox.Show("Se ha guardado correctamente","Faena guardada",MessageBoxButton.OK,MessageBoxImage.Information);
                //Activa el temporizador para el autoGuardado
                main.ActivarTemporizadorAutoSave();
            }
            catch
            {
                MessageBox.Show("Hay categorias con el mismo nombre","No se ha podido guardar",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
    }
}
