﻿using System;
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
namespace Tareas_Pendientes_v2
{

    /// <summary>
    /// Lógica de interacción para VisorTarea.xaml
    /// </summary>
    public partial class VisorTarea : UserControl,IComparable<VisorTarea>,IComparable
    {
        Tarea tarea;
        Lista lista;
        public event EventHandler TareaEditada;
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
                    tarea = new Tarea(lista,txtBxDescripcionTarea.TextWithFormat);
                return tarea;
            }
            set {
                tarea = value;
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
                    txtBxDescripcionTarea.TextWithFormat = tarea.Contenido;
                   
                }else
                {
                    txtBlFechaHecho.Text = "";
                    txtBxDescripcionTarea.Text="";
                    ckHecho.IsChecked = false;
                }
            } }

        private void ckHecho_Checked(object sender, RoutedEventArgs e)
        {
            if (txtBlFechaHecho != null && tarea != null)
            {
                txtBlFechaHecho.Text = DateTime.Now.ToString();
                Tarea.AñadirHecho(lista,DateTime.Now);
                if (TareaEditada != null)
                    TareaEditada(this, new EventArgs());
            }
        }

        private void ckHecho_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtBlFechaHecho != null && tarea != null)
            {
                Tarea.QuitarHecho(lista);
                txtBlFechaHecho.Text = "";
                if (TareaEditada != null)
                    TareaEditada(this, new EventArgs());
            }
        }

        private void txtBxDescripcionTarea_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBxDescripcionTarea != null && tarea != null)
            {
                Tarea.Contenido = txtBxDescripcionTarea.TextWithFormat;
                if (TareaEditada != null)
                    TareaEditada(this, new EventArgs());
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
