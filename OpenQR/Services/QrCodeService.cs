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
                    generatedCode = QrCodeGenerator.GenerateQRCodeImage(code, 80, true);
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