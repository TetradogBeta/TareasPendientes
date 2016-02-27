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
    /// Lógica de interacción para VisorTarea.xaml
    /// </summary>
    public partial class VisorTarea : UserControl
    {
        Tarea tarea;
        public VisorTarea()
        {
            InitializeComponent();
            Tarea = new Tarea();
        }
        public VisorTarea(Tarea tarea):this()
        { Tarea = tarea; }
        public Tarea Tarea {
            get {
                if (tarea == null)
                    tarea = new Tarea(txtBxDescripcionTarea.Text);
                return tarea;
            }
            set {
                tarea = value;
                if(tarea!=null)
                {

                    txtBlFechaHecho.Text =tarea.Hecho?tarea.FechaHecho.ToString():"";
                    txtBxDescripcionTarea.Text = tarea.Contenido;
                    ckHecho.IsChecked = tarea.Hecho;
                }else
                {
                    txtBlFechaHecho.Text = "";
                    txtBxDescripcionTarea.Text = "";
                    ckHecho.IsChecked = false;
                }
            } }

        private void ckHecho_Checked(object sender, RoutedEventArgs e)
        {
            if (txtBlFechaHecho != null && tarea != null)
            {
                txtBlFechaHecho.Text = DateTime.Now.ToString();
                tarea.Hecho = true;
            }
        }

        private void ckHecho_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtBlFechaHecho != null && tarea != null)
            {
                tarea.Hecho = false;
                txtBlFechaHecho.Text = "";
            }
        }

        private void txtBxDescripcionTarea_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBxDescripcionTarea != null && tarea != null)
            {
                tarea.Contenido = txtBxDescripcionTarea.Text;
            }
        }
    }
}
