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
    /// Lógica de interacción para EditorCategoriasLista.xaml
    /// </summary>
    public partial class EditorCategoriasLista : Window
    {
        private Lista listaActual;
        MainWindow main;
        private EditorCategoriasLista(MainWindow main)
        {
            this.main = main;
            InitializeComponent();
        }

        public EditorCategoriasLista(Lista listaActual, MainWindow main) : this(main)
        {
            Categoria[] categorias = Categoria.Categorias();
            List<CheckBox> chks = new List<CheckBox>();
            CheckBox chkAux;
            for (int i = 0; i < categorias.Length; i++)
            {
                chkAux = new CheckBox();
                chkAux.IsChecked = Categoria.CategoriasList.Existeix(categorias[i].IdUnico);
                chkAux.Content = categorias[i];
                chkAux.Tag = categorias[i];
                chkAux.Checked += AñadirCategoria;
                chkAux.Unchecked += QuitarCategoria;
                stkCategorias.Children.Add(chkAux);
            }
            this.listaActual = listaActual;
            txtNombre.Text ="Lista: "+ listaActual.Nombre;
        }

        private void QuitarCategoria(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (sender as CheckBox);
            Categoria categoria = chk.Tag as Categoria;
            if (categoria.Nombre != MainWindow.TODASLASLISTAS)
            {
                categoria.Quitar(listaActual);
                //Activa el temporizador para el autoGuardado
                main.ActivarTemporizadorAutoSave();
            }
        }

        private void AñadirCategoria(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (sender as CheckBox);
            Categoria categoria =chk.Tag as Categoria;
            categoria.Añadir(listaActual);
            //Activa el temporizador para el autoGuardado
            main.ActivarTemporizadorAutoSave();
        }

    }
}
