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
    /// Lógica de interacción para BuscarTarea.xaml
    /// </summary>
    public partial class BuscarTarea : Window
    {
        private Lista listaActual;

        //filtra las tareas por: fecha,texto contenido,hecho o no hecho,herencia(muestra cmb con las posibilidades)
        public BuscarTarea()
        {
            InitializeComponent();
        }

        public BuscarTarea(Lista listaActual)
        {
            this.listaActual = listaActual;
        }
    }
}
