using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using Gabriel.Cat.Extension;
namespace Tareas_Pendientes_v2
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //cada x tiempo se guarda si hay cambios :) asi si hay algun problema cuando cierra no se perdera mucho :)//guarda las listas temporales 
        //al abrir mira si hay un autoGuardado si lo hay mira la fecha de editado y si es mas reciente que la del guardadoNormal pregunta si quiere carga esa si dice que si la pone sino la elimina.
        string NOMBREARCHIVO = "tareasPendientes.xml";
        public const string TODASLASLISTAS = "Todas las listas";
        Lista listaActual;
        Temporizador temporizadorAutoSave;//cuando deje de hacer si hay cosas sin guardar lo guardo
        const int TIEMPOAUTOSAVE = 5 * 1000;
        bool guardado;

        public MainWindow()
        {
            guardado = true;
            InitializeComponent();
            Load();
            if (listaActual == null)
                CreaListaNueva();
            temporizadorAutoSave = new Temporizador(TIEMPOAUTOSAVE);
            temporizadorAutoSave.Elapsed += (temp) => { Save(); };
        }

        private void CreaListaNueva()
        {
            listaActual = new Lista(txboxNombreLista.Text);

        }

        private void Load()
        {
            Action act;
            if (File.Exists(NOMBREARCHIVO))
            {
                XmlDocument xmlTareas = new XmlDocument();
                xmlTareas.Load(NOMBREARCHIVO);
                listaActual = Lista.LoadXml(xmlTareas);
                if (listaActual != null)
                {
                    act = () =>
                    {
                        txboxNombreLista.Text = listaActual.NombreLista;
                        PonTareasLista();
                    };
                    Dispatcher.BeginInvoke(act);
                }
                PonCategoriasCmb();
            }
            else {

                cmbCategorias.Items.Add(TODASLASLISTAS);
                Lista.AñadirCategoria(TODASLASLISTAS);
                cmbCategorias.SelectedIndex = 0;
            }
        }

        private void Save(object sender, EventArgs e)
        {
            try
            {
                Lista.ToXml(listaActual.EsTemporal ? listaActual : null).Save(NOMBREARCHIVO);
                guardado = true;
                temporizadorAutoSave.StopAndAbort();
            }
            catch { }
        }
        private void Save()
        {
            if (!guardado)
                Save(null, null);
        }
        public void ActivarTemporizadorAutoSave()
        {
            guardado = false;
            temporizadorAutoSave.StopAndAbort();
            temporizadorAutoSave.Start();
        }
        private void HerenciasLista_Click(object sender, RoutedEventArgs e)
        {
            //abre una ventana para poder gestinar las herencias que posee la lista actual
            new EditorHerenciaLista(listaActual, this).ShowDialog();
        }



        private void CategoriasLista_Click(object sender, RoutedEventArgs e)
        {
            //abre una ventana para poder gestinar las categorias en las que se encuentra la lista actual
            new EditorCategoriasLista(listaActual, this).ShowDialog();
            cmbCategorias_SelectionChanged(null, null);//asi si la lista esta en la actualmente en la categoria puesta entonces se vera

            //Errores
            //a veces no lo quita de la lista actual...
            //a veces cambiado de lista el objeto cambia por otro...pero mantiene el nombre...por mirar...
        }

        private void LimpiarCamposLista_Click(object sender, RoutedEventArgs e)
        {
            if (EsListaActualGuardable())
            {
                if (!listaActual.EsTemporal || MessageBox.Show("La lista no esta guardada, quieres borrar sus datos?", "Atencion", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    //limpia los campos
                    LimpiarCampos();
                    CreaListaNueva();
                    lstListasPendientes.SelectedIndex = -1;
                    //Activa el temporizador para el autoGuardado
                    ActivarTemporizadorAutoSave();
                }
            }

        }

        private bool EsListaActualGuardable()
        {
            return listaActual != null && (!listaActual.EsTemporal || listaActual.EsTemporal && (listaActual.Count != 0 || listaActual.NombreLista != ""));
        }

        private void AñadirLista_Click(object sender, RoutedEventArgs e)
        {
            //añade la lista creada al monton de listas (hasta entonces era temporal, y avisa que se van a perder los datos si se va a cambiar de lista o cerrar)
            string categoria = cmbCategorias.SelectedItem.ToString();
            if (EsListaActualGuardable())
            {
                if (listaActual.EsTemporal)
                {
                    if (!listaActual.EstaEnLaCategoria(TODASLASLISTAS))
                        listaActual.AñadirHaCategoria(TODASLASLISTAS);

                    if (categoria != TODASLASLISTAS && !listaActual.EstaEnLaCategoria(categoria))
                        listaActual.AñadirHaCategoria(categoria);
                    listaActual.EsTemporal = false;
                    if (listaActual.EstaEnLaCategoria(categoria))
                    {
                        lstListasPendientes.Items.Clear();
                        lstListasPendientes.Items.AddRange(Lista.FiltraPorCategoria(categoria));
                    }
                    Lista.AñadirListaHaCategoria(listaActual, TODASLASLISTAS);
                    Lista.AñadirListaHaCategoria(listaActual, categoria);
                    //Activa el temporizador para el autoGuardado
                    ActivarTemporizadorAutoSave();
                }
                else
                {
                    MessageBox.Show("La lista ya esta añadida.", "Atencion", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

        }
        private void EliminarLista_Click(object sender, RoutedEventArgs e)
        {
            Lista aux;
            //eliminia la lista, si tiene "descendencia" preguntara si esta seguro y si lo esta se quita la herencia de todas sus descendientes.

            if (EsListaActualGuardable() && (!listaActual.TieneDescendencia || MessageBox.Show("Esta lista esta siendo usada por otras como herencia, sigues queriendo borrarla?", "Se requiere su atencion", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes))
            {
                if (listaActual.TieneDescendencia)
                {
                    //quito la descendencia
                    Lista.QuitarHerederos(listaActual);
                }
                aux = listaActual;
                LimpiarCamposLista_Click(null, null);//limpio los campos
                aux.EsTemporal = true;

                cmbCategorias_SelectionChanged(null, null);

                //Activa el temporizador para el autoGuardado
                ActivarTemporizadorAutoSave();

            }
        }

        private void CategoriasManager_Click(object sender, RoutedEventArgs e)
        {
            //se abre una ventana para añadir, quitar(si tiene elementos se les quita esa categoria), si es la actual la que se quita o se modifica al cerrar la ventana se actualiza
            new CategoriasManager(this).ShowDialog();
            PonCategoriasCmb();

        }

        private void PonCategoriasCmb()
        {
            cmbCategorias.Items.Clear();
            cmbCategorias.Items.AddRange(Lista.TodasLasCategorias());
            cmbCategorias.SelectedIndex = 0;
        }

        private void BuscarLista_Click(object sender, RoutedEventArgs e)
        {
            //sale una ventana con un comboBox con todas las listas, y otro combobox con todas las categorias(solo sirve para filtrar las que se ven)
            //tiene un boton que pone View y si lo pulsas se pone como lista actual.¿pongo la categoria que es si no esta en la actual?
            new BuscarLista(this).ShowDialog();
        }
        #region Tareas
        private void EliminarElementoLista_Click(object sender, RoutedEventArgs e)
        {
            //abre una ventana para editar el contenido de la lista, los elementos de la herencia se ocultaran al "eliminarse de la lista"
            new EliminarTareas(listaActual, this).ShowDialog();
        }



        private void AñadirElementoLista_Click(object sender, RoutedEventArgs e)
        {
            //añade al final un elemento
            VisorTarea visor = new VisorTarea();
            stkTareas.Children.Add(visor);
            listaActual.AñadirTarea(visor.Tarea);
            //Activa el temporizador para el autoGuardado
            ActivarTemporizadorAutoSave();
        }
        private void BuscarElementoLista_Click(object sender, RoutedEventArgs e)
        {
            //sale una ventana con un comboBox con todas las tareas
            //tiene un boton que pone View. si le da lo posiciona aunque de momento no se mover la lista...
            new BuscarTarea().Show();
        }
        #endregion
        private void cmbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //cambia los elementos de la lista por los que toquen.
            if (cmbCategorias.SelectedItem != null)
            {
                lstListasPendientes.Items.Clear();
                lstListasPendientes.Items.AddRange(Lista.FiltraPorCategoria(cmbCategorias.SelectedItem.ToString()));
            }
        }

        private void lstListasPendientes_Selected(object sender, RoutedEventArgs e)
        {

            PonLista(lstListasPendientes.SelectedItem as Lista);

        }

        public void PonLista(Lista lista)
        {
            if (lista != null)
            {
                //cuando seleccionan una lista se visualiza a no ser que haya una lista temporal luego pregunto antes de hacer nada
                if (listaActual.EsTemporal && (listaActual.Count != 0 || listaActual.NombreLista != "") && MessageBox.Show("Hay una lista sin guardar que se va a perder, deseas Guardarla?", "Se requiere tu atencion", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    listaActual.EsTemporal = false;
                }
                listaActual = lista;
                txboxNombreLista.Text = listaActual.NombreLista;
                PonTareasLista();

            }
        }

        private void LimpiarCampos()
        {
            listaActual = null;
            txboxNombreLista.Text = "";
            CreaListaNueva();
            stkTareas.Children.Clear();
        }

        public void PonTareasLista()
        {
            VisorTarea visor;
            stkTareas.Children.Clear();
            foreach (Tarea tarea in listaActual)
            {
                visor = new VisorTarea(tarea);
                visor.TareaEditada += (sender, e) => { ActivarTemporizadorAutoSave(); };
                stkTareas.Children.Add(visor);
            }
        }
        private void txboxNombreLista_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (listaActual != null)
            {
                //se a modificado el nombre de la lista.
                listaActual.NombreLista = txboxNombreLista.Text;
                //cambia de la lista
                if (!listaActual.EsTemporal)
                    lstListasPendientes.Items.Refresh();
                //activa el temporizador para el auto guardado
                ActivarTemporizadorAutoSave();
            }
        }
    }
}
