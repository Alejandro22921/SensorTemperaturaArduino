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
using System.IO.Ports;
using System.Windows.Threading;
using System.Windows.Forms.DataVisualization.Charting;

namespace SensorTemperaturaArduino
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort puerto = new SerialPort("COM1", 9600, Parity.None, 8);
        DispatcherTimer tiempo = new DispatcherTimer();
        Chart figura = new Chart();
        string datosRecibidos = "";
        int contador = 0;

        public MainWindow()
        {
            InitializeComponent();
            tiempo.Interval = TimeSpan.FromMilliseconds(100);  /* --- CADA 100ms --- */
            tiempo.Tick += new EventHandler(tiempo_Tick);
        }

        void tiempo_Tick(object sender, EventArgs e)
        {
            if (puerto.IsOpen)
                puerto.Write("y");
        }

        private void btnConectar_Click(object sender, RoutedEventArgs e)
        {
            if (btnConectar.Content.ToString() == "ON")
            {
                try
                {
                    puerto.Open();
                    tiempo.Start();

                    btnConectar.Content = "OFF";
                    lblDatosRecibidos.Background = Brushes.LightGray;
                    puerto.DataReceived += new SerialDataReceivedEventHandler(puerto_DataReceived);
                }
                catch (Exception)
                {
                    MessageBox.Show("NO SE PUDO ABRIR EL PUERTO", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                puerto.Close();
                ellipseBlue.Fill = Brushes.Black;
                ellipseYellow.Fill = Brushes.Black;
                ellipseRed.Fill = Brushes.Black;
                lblDatosRecibidos.Content = "";
                lblDatosRecibidos.Background = Brushes.Black;
                btnConectar.Content = "ON";
                tiempo.Stop();
            }
        }

        delegate void ActualizaDatos();
        void puerto_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            datosRecibidos = puerto.ReadLine();
            Dispatcher.Invoke(new ActualizaDatos(actualiza));
        }

        void actualiza()
        {
            try
            {
                contador++;
                lblDatosRecibidos.Content = "" + datosRecibidos;
                long temperatura = Convert.ToInt64(datosRecibidos);
                figura.Series[0].Points.AddXY(contador, temperatura);

                if (temperatura < 15L)
                {
                    puerto.Write("0");
                    ellipseBlue.Fill = Brushes.Blue;
                    ellipseYellow.Fill = Brushes.Black;
                    ellipseRed.Fill = Brushes.Black;
                }
                else if (temperatura < 30L)
                {
                    puerto.Write("1");
                    ellipseBlue.Fill = Brushes.Black;
                    ellipseYellow.Fill = Brushes.Yellow;
                    ellipseRed.Fill = Brushes.Black;
                }
                else
                {
                    puerto.Write("2");
                    ellipseBlue.Fill = Brushes.Black;
                    ellipseYellow.Fill = Brushes.Black;
                    ellipseRed.Fill = Brushes.Red;
                }
            }
            catch (Exception)
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.figura.Titles.Add("Temperatura");
            this.figura.Series.Add("Temperatura");
            this.figura.Series["Temperatura"].ChartType = SeriesChartType.Spline;
            this.figura.Palette = ChartColorPalette.Fire;
            figura.ChartAreas.Add("");
            windowsHost.Child = figura;
        }
    }
}
