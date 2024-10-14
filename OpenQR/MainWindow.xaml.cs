using OpenQR.Models;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace OpenQR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            /*
            int modulePixelSize = 30;
            Assembly assembly = Assembly.GetExecutingAssembly();
            string fullResourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(res => res.EndsWith("youtube.png"));
            string outputFilePath = "images/myqrcode2.png";

            using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                Bitmap Logo = new Bitmap(stream);
                QR_Text text = new QR_Text("Это революционный прорыв, Джонни!", "#C81590", "#F8AB2A", "#000000", Logo);
                outputFilePath = "images/myqrcode.png";
                QrCodeGenerator.GenerateAndSaveQRCodeImage(text, outputFilePath, modulePixelSize, false);
            }
            

            fullResourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(res => res.EndsWith("twitter.png"));

            using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                Bitmap Logo = new Bitmap(stream);
                QR_Link link = new QR_Link("x.com", "#000000", "#16161A", "#25D366", Logo);
                outputFilePath = "images/myqrcode2.png";
                QrCodeGenerator.GenerateAndSaveQRCodeImage(link, outputFilePath, modulePixelSize, true);
            }
            */
        }

        private void ApplicationExit(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // Сохраняем данные...
                Application.Current.Shutdown();
            }
        }
    }
}