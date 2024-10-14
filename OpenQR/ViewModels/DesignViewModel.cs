using OpenQR.Models;
using OpenQR.Services;
using System.ComponentModel;
using System.Windows.Input;

namespace OpenQR.ViewModels
{
    // Модель представления для настройки дизайна QR-кода.
    internal class DesignViewModel : BindableBase, INotifyPropertyChanged
    {
        // Выбранный тип формы модуля QR-кода.
        private ShapeType _selectedShapeType;

        // Свойство для доступа к выбранному типу формы модуля.
        public ShapeType SelectedShapeType
        {
            get => _selectedShapeType;
            set => SetProperty(ref _selectedShapeType, value);
        }

        // Сервис для работы с QR-кодом.
        private readonly IQrCodeService _qrCodeService;

        // Команда для изменения типа формы модуля.
        public ICommand ChangeShapeTypeCommand { get; }

        // Конструктор.
        public DesignViewModel(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
            // Инициализация команды.
            ChangeShapeTypeCommand = new DelegateCommand<ShapeType?>(ChangeShapeType);
        }

        // Метод для изменения типа формы модуля.
        public void ChangeShapeType(ShapeType? s)
        {
            // Проверка на наличие значения.
            if (s.HasValue)
            {
                // Установка выбранного типа формы.
                SelectedShapeType = s.Value;
                // Получение данных QR-кода.
                IQR_CodeData qr = _qrCodeService.code;
                // Установка типа формы модуля в данных QR-кода.
                qr.ModuleShape = s.Value;
                // Обновление данных QR-кода в сервисе.
                _qrCodeService.code = qr;
            }
        }
    }
}