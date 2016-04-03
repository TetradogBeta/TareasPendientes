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
using Gabriel.Cat;
using Gabriel.Cat.Wpf;
namespace Tareas_Pendientes_v2
{

    /// <summary>
    /// Lógica de interacción para VisorTarea.xaml
    /// </summary>
    public partial class VisorTarea : UserControl,IComparable<VisorTarea>,IComparable
    {
        Tarea tarea;
        Lista lista;
        public VisorTarea(Tarea tarea)
        {
            InitializeComponent();
            if (tarea != null)
            {
                this.lista = tarea.Lista;
                Tarea = tarea;
            }


        }

        public VisorTarea(Lista lista):this(lista,null)
        {
        	Tarea=Tarea;
        }

        public VisorTarea(Lista lista, Tarea tarea)
        {
        	InitializeComponent();
        	this.lista = lista;
            this.Tarea = tarea;

        }


        public Tarea Tarea {
            get {
                if (tarea == null)
                {
                    tarea = new Tarea(lista, "");
                }
                return tarea;
            }
            set {
                tarea = value;
                gRtb.Children.Clear();
                if(Tarea.RtbContenido.Parent!=null)
                     (Tarea.RtbContenido.Parent as Grid).Children.Remove(Tarea.RtbContenido);
                gRtb.Children.Add(Tarea.RtbContenido);
                if(tarea!=null)
                {
                    DateTime temps = tarea.FechaHecho(lista);
                    ckHecho.IsChecked = tarea.EstaHecha(lista);
                    if(tarea.EstaHecha(lista))
                    {
                        tarea.QuitarHecho(lista);
                        tarea.AñadirHecho(lista, temps);
                    }
                    txtBlFechaHecho.Text = ckHecho.IsChecked.Value ? tarea.FechaHecho(lista).ToString():"";

                }else
                {
                    txtBlFechaHecho.Text = "";
                    ckHecho.IsChecked = false;
                }
            } }

        private void ckHecho_Checked(object sender, RoutedEventArgs e)
        {
            if (txtBlFechaHecho != null && tarea != null)
            {
                txtBlFechaHecho.Text = DateTime.Now.ToString();
                Tarea.AñadirHecho(lista,DateTime.Now);
             
            }
        }

        private void ckHecho_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtBlFechaHecho != null && tarea != null)
            {
                Tarea.QuitarHecho(lista);
                txtBlFechaHecho.Text = "";
            }
        }
        public int CompareTo(object obj)
        {
            return CompareTo(obj as VisorTarea);
        }

        public int CompareTo(VisorTarea other)
        {
            int compareTo;
            if(other!=null)
            {
                compareTo = Tarea.CompareTo(Tarea,other.Tarea);
            }
            else
            {
                compareTo = -1;
            }
            return compareTo;
        }


    }
}
