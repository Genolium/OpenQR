using OpenQR.Services;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace OpenQR.ViewModels
{
    // Модель представления для экспорта QR-кода.
    class ExportViewModel : BindableBase, INotifyPropertyChanged
    {
        // Выбранный формат экспорта.
        private string _selectedFormat = ".png";

        // Свойство для доступа к выбранному формату экспорта.
        public string SelectedFormat
        {
            get => _selectedFormat;
            set
            {
                SetProperty(ref _selectedFormat, value);
            }
        }

        // Сервис для работы с QR-кодом.
        private readonly IQrCodeService _qrCodeService;

        // Конструктор.
        public ExportViewModel(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
            // Инициализация команды экспорта.
            ExportCommand = new DelegateCommand(() => {
                // Экспорт QR-кода в файл с указанным форматом и текущей датой и временем в имени.
                ExportQRCode($@"images/qr_{DateTime.Now:yyyyMMdd_HHmmss_fff}{SelectedFormat}");
            });

            // Инициализация команды выбора формата.
            SelectFormatCommand = new DelegateCommand<string>(format =>
            {
                // Установка выбранного формата.
                SelectedFormat = format;
            });
        }

        // Команда для экспорта QR-кода.
        public ICommand ExportCommand { get; }
        // Команда для выбора формата экспорта.
        public ICommand SelectFormatCommand { get; }

        // Получает информацию о кодеке изображения по MIME-типу.
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        // Экспортирует QR-код в файл с указанным именем.
        private void ExportQRCode(string filePath)
        {
            // Получение изображения QR-кода из сервиса.
            Bitmap qrCodeBitmap = _qrCodeService.generatedCode;

            // Проверка наличия изображения.
            if (qrCodeBitmap != null)
            {
                // Экспорт в зависимости от выбранного формата.
                switch (SelectedFormat)
                {
                    case ".png":
                        // Сохранение в формате PNG.
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            EncoderParameters encoderParameters = new EncoderParameters(1);
                            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                            ImageCodecInfo codecInfo = GetEncoderInfo("image/png");
                            qrCodeBitmap.Save(fs, codecInfo, encoderParameters);
                        }
                        break;
                    case ".jpeg":
                        // Сохранение в формате JPEG.
                        EncoderParameters jpegEncoderParameters = new EncoderParameters(1);
                        EncoderParameter jpegEncoderParameter = new EncoderParameter(Encoder.Quality, 85L);
                        jpegEncoderParameters.Param[0] = jpegEncoderParameter;
                        ImageCodecInfo jpegCodecInfo = GetEncoderInfo("image/jpeg");
                        qrCodeBitmap.Save(filePath, jpegCodecInfo, jpegEncoderParameters);
                        break;
                    case ".pdf":
                        // Экспорт в PDF.
                        ExportToPdf(qrCodeBitmap, filePath);
                        break;
                    default:
                        // Сообщение об ошибке, если формат не поддерживается.
                        MessageBox.Show("Unsupported file format.");
                        break;
                }

                // Отображение сообщения об успешном экспорте, если файл существует.
                if (File.Exists(filePath))
                {
                    MessageBox.Show("QR код успешно экспортирован", "OpenQR", MessageBoxButton.OK);
                }
            }
        }

        // Экспортирует QR-код в PDF-файл.
        private void ExportToPdf(Bitmap qrCodeBitmap, string filePath)
        {
            // Создание PDF-документа.
            using (var doc = new PdfDocument())
            {
                // Добавление страницы.
                var page = doc.AddPage();
                // Создание графического объекта для рисования на странице.
                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    // Получение размеров страницы в точках.
                    double pageWidthPt = page.Width.Point;
                    double pageHeightPt = page.Height.Point;

                    // Установка фиксированного размера QR-кода в точках.
                    double fixedSizePt = 350;

                    // Вычисление координат для центрирования QR-кода на странице.
                    double xPt = (pageWidthPt - fixedSizePt) / 2;
                    double yPt = (pageHeightPt - fixedSizePt) / 2;

                    // Преобразование Bitmap в XImage.
                    using (var ms = new MemoryStream())
                    {
                        qrCodeBitmap.Save(ms, ImageFormat.Png);
                        ms.Position = 0;
                        using (var xImage = XImage.FromStream(ms))
                        {
                            // Рисование изображения QR-кода на странице.
                            gfx.DrawImage(xImage, xPt, yPt, fixedSizePt, fixedSizePt);
                        }
                    }
                }

                // Сохранение PDF-документа в файл.
                doc.Save(filePath);
            }
        }
    }
}