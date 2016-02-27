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
    /// Lógica de interacción para BuscarLista.xaml
    /// </summary>
    public partial class BuscarLista : Window
    {
        private MainWindow mainWindow;

        //poder buscar una lista por el contenido de una tarea
        public BuscarLista()
        {
            InitializeComponent();
        }

        public BuscarLista(MainWindow mainWindow):this()
        {
            this.mainWindow = mainWindow;//para poder seleccionar la lista             mainWindow.PonLista(lista);

        }
    }
}
