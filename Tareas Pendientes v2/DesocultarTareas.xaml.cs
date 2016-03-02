/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 02/03/2016
 * Hora: 13:07
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Gabriel.Cat.Extension;
namespace Tareas_Pendientes_v2
{
	/// <summary>
	/// Interaction logic for DesocultarTareas.xaml
	/// </summary>
	public partial class DesocultarTareas : Window
	{
		Lista lista;
		MainWindow main;
		public DesocultarTareas(Lista lista,MainWindow main)
		{
			this.lista=lista;
			this.main=main;
			InitializeComponent();
			stkTareasOcultas.Children.AddRange(Tarea.TareasOcultas(lista).ToObjViewerArray(Desocultar));
			if(stkTareasOcultas.Children.Count==0)
				throw new Exception("Cerrar");
		}

		void Desocultar(Gabriel.Cat.Wpf.ObjViewer visor)
		{
			(visor.Object as Tarea).Desocultar(lista);
			stkTareasOcultas.Children.Remove(visor);
			main.PonLista(lista);
			main.ActivarTemporizadorAutoSave();
			if(stkTareasOcultas.Children.Count==0)
			{
				MessageBox.Show("No hay mas tareas ocultas de la herencia","Cerrando ventana",MessageBoxButton.OK,MessageBoxImage.Information);
				this.Close();
			}
		}
	}
}