using OpenQR.Models;
using System.Drawing;
using System.Windows;

namespace OpenQR.Services
{
    // Сервис для генерации QR-кодов.
    internal class QrCodeService : IQrCodeService
    {
        // Данные QR-кода.
        private IQR_CodeData _code;
        private int _moduleSize = 80;
        public int ModuleSize
        {
            get => _moduleSize; 
            set
            {
                _moduleSize = value;
                // Генерация изображения QR-кода.
                generatedCode = QrCodeGenerator.GenerateQRCodeImage(code, _moduleSize);
                // Вызов события обновления QR-кода.
                QrCodeUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        // Свойство для доступа к данным QR-кода. При изменении данных генерируется новый QR-код.
        public IQR_CodeData code
        {
            get => _code;
            set
            {
                _code = value;
                try
                {
                    // Генерация изображения QR-кода.
                    generatedCode = QrCodeGenerator.GenerateQRCodeImage(code, ModuleSize);
                    // Вызов события обновления QR-кода.
                    QrCodeUpdated?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    // Отображение сообщения об ошибке, если произошла ошибка при генерации.
                    MessageBoxResult error = MessageBox.Show("Произошла ошибка при генерации QR кода. Вероятно, вы ввели слишком большой текст", "Ошибка", MessageBoxButton.OK);
                }
            }
        }

        // Сгенерированное изображение QR-кода.
        public Bitmap generatedCode { get; private set; }

        // Событие обновления QR-кода.
        public event EventHandler QrCodeUpdated;
    }
}