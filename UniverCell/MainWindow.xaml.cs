﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace UniverCell
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /* Solución temporal al error de renderizado causado por el re-login en windows
         * Se cambió el método de renderizado de hardware a software
         * https://github.com/MahApps/MahApps.Metro/issues/2734#issuecomment-260839524
         */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var w = (Window)sender;
            var src = PresentationSource.FromVisual(w) as System.Windows.Interop.HwndSource;
            var target = src.CompositionTarget;
            target.RenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
        }

        /// <summary>
        /// Inicializar Ventana
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            /*
             *Preparar la ventana principal  
             */
            //Cargar los datos del usuario
            ActualizarDatosTienda();
            MostrarDatosUsuario();
            CargarAvatar();
            //Cargar tablas
            ActualizarTablaVentas();
            EstadisticasRecargas();
            Actualizar_Tabla_Inventario();
            ActualizarTablaRecargas();
            ActualizarTablaReparaciones();

            //Cargar Controles
            LeerMonedas();
        }

        private void Label_Usuario(object sender, RoutedEventArgs e)
        {
            // ... Get label.
            var label = sender as Label;
            // ... Set date in content.
            label.Content = Sesion.nomb_completo;
        }

        private void Label_Puesto(object sender, RoutedEventArgs e)
        {
            // ... Get label.
            var label = sender as Label;
            // ... Set date in content.
            label.Content = Sesion.puest;
        }


        //Los tiles dirigen a un tab
        private void tile_ventas_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl.SelectedIndex = 1;
        }

        private void tile_recargas_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl.SelectedIndex = 2;
        }

        private void tile_reparaciones_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl.SelectedIndex = 3;
        }

        private void tile_inventario_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl.SelectedIndex = 4;
        }

        private void tile_estadisticas_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl.SelectedIndex = 5;
        }

        private void tile_caja_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl.SelectedIndex = 6;
        }

        private void atras_btn_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl.SelectedIndex > 0)
            {
                tabControl.SelectedIndex -= 1;
            }
            else
                atras_btn.IsEnabled = false;
        }

        /// <summary>
        /// Cargar controles específicos al cambiar de tab
        /// </summary>
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            atras_btn.IsEnabled = true;
            if (tabControl.SelectedIndex == 1)
            {
                combo_bx_Moneda.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Actualizar los datos de la tienda
        /// </summary>
        public void ActualizarDatosTienda()
        {
            //Leer el fichero de correspondiente al logotipo de la empresa
            string ruta        = AppDomain.CurrentDomain.BaseDirectory + "Images\\logo.jpg";
            FileStream stream  = new FileStream(ruta, FileMode.Open, FileAccess.Read);
            logo_tienda.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            stream.Close();

            //Leer desde la base de datos los datos del negocio
            try
            {
                Conexion.conect.Open();
                string Comando = "Select*From ajustes;";
                MySqlCommand cmd = new MySqlCommand(Comando, Conexion.conect);
                MySqlDataReader Reader = cmd.ExecuteReader();

                while (Reader.Read())
                {
                    string nomb   = Reader.GetString("nombre_negocio");
                    string tel   = Reader.GetString("telefono");
                    string dir      = Reader.GetString("direccion");
                    string email = Reader.GetString("email");
                    Tienda.id_moneda = Reader.GetInt32("id");

                    lbl_tienda_nombre.Content = nomb;
                    lbl_tel_tienda.Content = tel;
                    lbl_dir_tienda.Text = dir;
                    lbl_email_tienda.Content = email;
                }
                Conexion.conect.Close();
                Tienda.nombre_moneda = Moneda.Nombre(Tienda.id_moneda);
                Tienda.signo_moneda = Moneda.Signo(Tienda.id_moneda);


                lbl_moneda_tienda.Content = Tienda.signo_moneda + "," + Tienda.nombre_moneda;
            }
            catch
            {
                MessageBox.Show("Error al leer los datos de la tienda","Error");
            }
            
        }

        ///Cargar el avatar del usuario
        private void btn_cambiar_foto_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToInt32(Sesion.lvl) <= 2)
            {
                Archivos.CargarImagen("Usuario", "Avatar_" + Sesion.nomb_usuario, 0);
                CargarAvatar();
            }
            else
            {
                MessageBox.Show("Solo el administrador puede hacer cambios", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Stop);
            }

        }
        private void CargarAvatar()
        {
            string ruta = AppDomain.CurrentDomain.BaseDirectory + "Images\\Avatar_" + Sesion.nomb_usuario + ".jpg";
            FileStream stream = new FileStream(ruta, FileMode.Open, FileAccess.Read);
            Avatar.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            stream.Close();
        }

        //Abrir los ajustes de la tienda
        private void tienda_ajustes_Click(object sender, RoutedEventArgs e)
        {
            if(Convert.ToInt32(Sesion.lvl) <= 2)
            {
                Ajustes aj = new Ajustes(this);
                aj.Show();
            }
            else
            {
                MessageBox.Show("Solo el administrador puede hacer cambios","Advertencia",MessageBoxButton.OK,MessageBoxImage.Stop);
            }
        }

        void LeerMonedas()
        {
            Moneda.CargarMonedas();

            foreach(var monedas in Tienda.ListaMonedas)
            {
                combo_bx_Moneda.Items.Add(monedas);
                art_combo_moneda.Items.Add(monedas);
            }
        }

        private void BTN_Inv_Proveedores_Click(object sender, RoutedEventArgs e)
        {
            Proveedores Prov = new Proveedores();
            Prov.ShowDialog();
        }
    }
}
