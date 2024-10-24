using OpenQR.Models;
using System.Drawing;

namespace OpenQR.Services
{
    // Интерфейс для сервиса генерации QR-кодов.
    public interface IQrCodeService
    {
        // Данные QR-кода.
        IQR_CodeData code { get; set; }
        int ModuleSize { get; set; }

        // Сгенерированное изображение QR-кода.
        Bitmap generatedCode { get; }

        // Событие обновления QR-кода.
        event EventHandler QrCodeUpdated;
    }
}