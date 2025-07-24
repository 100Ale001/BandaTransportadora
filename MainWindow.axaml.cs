using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace BandaTransportadora
{
    public partial class MainWindow : Window
    {
        public class Motor
        {
            public bool Encendido { get; private set; }
            public void Encender() => Encendido = true;
            public void Apagar() => Encendido = false;
        }

        public class Valvula
        {
            public bool Abierta { get; private set; }
            public void Abrir() => Abierta = true;
            public void Cerrar() => Abierta = false;
        }

        private Motor motor = new Motor();
        private Valvula valvula = new Valvula();

        private double startX = 10;
        private double midX = 250;
        private double endX = 490;

        private bool cubetaLlena = false;
        private bool cubetaEnFinal = false;

        public MainWindow()
        {
            InitializeComponent();

            BtnIniciarMotor.Click += BtnIniciarMotor_Click;
            BtnActivarValvula.Click += BtnActivarValvula_Click;
            BtnFinalizar.Click += BtnFinalizar_Click;
            BtnRetirarCubeta.Click += BtnRetirarCubeta_Click;

            ResetBotePosition();
            ActualizarIndicadorMotor();
            ActualizarEstadoCubeta();
            ActualizarCubetaEnFinal();
        }

        private void ResetBotePosition()
        {
            Canvas.SetLeft(Bote, startX);
            Canvas.SetTop(Bote, 25); // Centrado verticalmente en Canvas de 100 alto
            cubetaLlena = false;
            cubetaEnFinal = false;
            ActualizarEstadoCubeta();
            ActualizarCubetaEnFinal();
        }

        private void ActualizarIndicadorMotor()
        {
            MotorIndicador.Background = motor.Encendido ? Brushes.LimeGreen : Brushes.Red;
        }

        private void ActualizarEstadoCubeta()
        {
            LblEstadoCubeta.Text = cubetaLlena ? "Cubeta: Llena" : "Cubeta: Vacía";
        }

        private void ActualizarCubetaEnFinal()
        {
            LblCubetaEnFinal.Text = cubetaEnFinal ? "Cubeta en final: Sí" : "Cubeta en final: No";
            BtnRetirarCubeta.IsEnabled = cubetaEnFinal;
        }

        private void BtnIniciarMotor_Click(object? sender, RoutedEventArgs e)
        {
            motor.Encender();
            LblMotorEstado.Text = "Motor encendido";
            ActualizarIndicadorMotor();

            BtnIniciarMotor.IsEnabled = false;
            BtnActivarValvula.IsEnabled = true;
            BtnFinalizar.IsEnabled = false;

            ResetBotePosition();
        }

        private async void BtnActivarValvula_Click(object? sender, RoutedEventArgs e)
        {
            if (!motor.Encendido)
            {
                await ShowMessage("Error", "Primero debes encender el motor.");
                return;
            }

            BtnActivarValvula.IsEnabled = false;
            LblValvulaEstado.Text = "Válvula abierta";
            valvula.Abrir();

            // Mover bote de inicio a medio
            await MoverBote(startX, midX, 2000);

            // Esperar 5 segundos con válvula abierta (simula llenado)
            await Task.Delay(5000);

            cubetaLlena = true;
            ActualizarEstadoCubeta();

            LblValvulaEstado.Text = "Válvula cerrada";
            valvula.Cerrar();

            // Mover bote de medio a final
            await MoverBote(midX, endX, 2000);

            cubetaEnFinal = true;
            ActualizarCubetaEnFinal();

            // Pausar mecanismo (no activar BtnFinalizar hasta retirar cubeta)
            BtnFinalizar.IsEnabled = false;
            BtnRetirarCubeta.IsEnabled = true;
        }

        private void BtnRetirarCubeta_Click(object? sender, RoutedEventArgs e)
        {
            // Simula retirar cubeta del final
            cubetaEnFinal = false;
            cubetaLlena = false;
            ActualizarCubetaEnFinal();
            ActualizarEstadoCubeta();

            BtnRetirarCubeta.IsEnabled = false;
            BtnFinalizar.IsEnabled = true;
        }

        private void BtnFinalizar_Click(object? sender, RoutedEventArgs e)
        {
            motor.Apagar();
            LblMotorEstado.Text = "Motor apagado";
            ActualizarIndicadorMotor();

            LblValvulaEstado.Text = "Válvula cerrada";

            BtnFinalizar.IsEnabled = false;
            BtnIniciarMotor.IsEnabled = true;
            BtnActivarValvula.IsEnabled = false;

            ResetBotePosition();
        }

        private Task ShowMessage(string title, string message)
        {
            var dlg = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                Content = new TextBlock
                {
                    Text = message,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            };

            return dlg.ShowDialog(this);
        }

        private Task MoverBote(double from, double to, int durationMs)
        {
            var tcs = new TaskCompletionSource<bool>();

            var startTime = DateTime.Now;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(15); // ~60fps

            timer.Tick += (_, __) =>
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                var progress = Math.Min(elapsed / durationMs, 1.0);

                double easedProgress = CubicEaseInOut(progress);

                double currentX = from + (to - from) * easedProgress;
                Canvas.SetLeft(Bote, currentX);

                if (progress >= 1.0)
                {
                    timer.Stop();
                    tcs.SetResult(true);
                }
            };

            timer.Start();
            return tcs.Task;
        }

        private double CubicEaseInOut(double t)
        {
            return t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        }
    }
}
