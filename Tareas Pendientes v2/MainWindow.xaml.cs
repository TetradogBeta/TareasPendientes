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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tareas_Pendientes_v2
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //cada x tiempo se guarda si hay cambios :) asi si hay algun problema cuando cierra no se perdera mucho :)//guarda las listas temporales 
        //al abrir mira si hay un autoGuardado si lo hay mira la fecha de editado y si es mas reciente que la del guardadoNormal pregunta si quiere carga esa si dice que si la pone sino la elimina.

        Lista listaActual; 
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HerenciasLista_Click(object sender, RoutedEventArgs e)
        {
            //abre una ventana para poder gestinar las herencias que posee la lista actual
            //Activa el temporizador para el autoGuardado
        }
        private void CategoriasLista_Click(object sender, RoutedEventArgs e)
        {
            //abre una ventana para poder gestinar las categorias en las que se encuentra la lista actual
            //Activa el temporizador para el autoGuardado
        }

        private void LimpiarCamposLista_Click(object sender, RoutedEventArgs e)
        {
            //limpia los campos
            txboxNombreLista.Text = "";
            stkTareas.Children.Clear();
            //Activa el temporizador para el autoGuardado
        }
        private void AñadirLista_Click(object sender, RoutedEventArgs e)
        {
            //añade la lista creada al monton de listas (hasta entonces era temporal, y avisa que se van a perder los datos si se va a cambiar de lista o cerrar)
            //Activa el temporizador para el autoGuardado
        }
        private void EliminarLista_Click(object sender, RoutedEventArgs e)
        {
            //eliminia la lista, si tiene "descendencia" preguntara si esta seguro y si lo esta se quita la herencia de todas sus descendientes.
            //limpio los campos
            LimpiarCamposLista_Click(null,null);
            //Activa el temporizador para el autoGuardado
        }

        private void CategoriasManager_Click(object sender, RoutedEventArgs e)
        {
            //se abre una ventana para añadir, quitar(si tiene elementos se les quita esa categoria), si es la actual la que se quita o se modifica al cerrar la ventana se actualiza
            //Activa el temporizador para el autoGuardado
        }

        private void BuscarLista_Click(object sender, RoutedEventArgs e)
        {
            //sale una ventana con un comboBox con todas las listas, y otro combobox con todas las categorias(solo sirve para filtrar las que se ven)
            //tiene un boton que pone View y si lo pulsas se pone como lista actual.¿pongo la categoria que es si no esta en la actual?
        }
        private void EliminarElementoLista_Click(object sender, RoutedEventArgs e)
        {
            //abre una ventana para editar el contenido de la lista, los elementos de la herencia se ocultaran al "eliminarse de la lista"
            //Activa el temporizador para el autoGuardado
        }
        private void AñadirElementoLista_Click(object sender, RoutedEventArgs e)
        {
            //añade al final un elemento
            stkTareas.Children.Add(new VisorTarea());
            //Activa el temporizador para el autoGuardado
        }
        private void BuscarElementoLista_Click(object sender, RoutedEventArgs e)
        {
            //sale una ventana con un comboBox con todas las tareas
            //tiene un boton que pone View. si le da lo posiciona aunque de momento no se mover la lista...
        }
        
        private void cmbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //cambia los elementos de la lista por los que toquen.
        }

        private void lstListasPendientes_Selected(object sender, RoutedEventArgs e)
        {
            //cuando seleccionan una lista se visualiza a no ser que haya una lista temporal luego pregunto antes de hacer nada
        }

        private void txboxNombreLista_TextChanged(object sender, TextChangedEventArgs e)
        {
            //se a modificado el nombre de la lista.
              //activa el temporizador para el auto guardado
              //cambia de la lista
                lstListasPendientes.Items.Refresh();
        }
    }
}
