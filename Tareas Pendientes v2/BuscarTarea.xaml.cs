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
namespace Tareas_Pendientes_v2
{
    /// <summary>
    /// Lógica de interacción para BuscarTarea.xaml
    /// </summary>
    public partial class BuscarTarea : Window
    {
        private Lista listaActual;
        private MainWindow main;
        //filtra las tareas por: fecha,texto contenido,hecho o no hecho,herencia(muestra cmb con las posibilidades)

        public BuscarTarea(Lista listaActual,MainWindow main)
        {
        	this.main=main;
            this.listaActual = listaActual;
            InitializeComponent();
            if(listaActual.ToArray().Length==0)
            	throw new Exception("Cerrar");
        }
		void TxtBxTextoHaBuscar_TextChanged(object sender, TextChangedEventArgs e)
		{
            string texto = txtBxTextoHaBuscar.Text.ToLowerInvariant();
            stkTareasEncontradas.Children.Clear();
			stkTareasEncontradas.Children.AddRange(ListaTareaHeredada.ToLista(listaActual.Filtra((tarea)=>{
                return tarea.ContenidoSinFormato().ToLowerInvariant().Contains(texto);
            }),listaActual,false).ToObjViewerArray(VisualizaLista));
		}

		void VisualizaLista(Gabriel.Cat.Wpf.ObjViewer visor)
		{
			main.PonLista(visor.Object as Lista);
		}
    }
}
