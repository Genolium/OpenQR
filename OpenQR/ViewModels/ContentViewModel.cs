using OpenQR.Models;
using OpenQR.Services;
using System.ComponentModel;

namespace OpenQR.ViewModels
{
    // Модель представления для содержимого QR-кода.
    internal class ContentViewModel : BindableBase, INotifyPropertyChanged
    {
        // Сервис для работы с QR-кодом.
        private readonly IQrCodeService _qrCodeService;
        // Данные QR-кода.
        private IQR_CodeData _qr;

        // Конструктор.
        public ContentViewModel(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
            _qr = _qrCodeService.code;
        }

        // Содержимое QR-кода. При изменении содержимого обновляется QR-код.
        public string Content
        {
            get { return _qr.Content; }
            set
            {
                _qr.Content = value;
                _qrCodeService.code = _qr;
            }
        }

        // Выбранный тип кодирования.
        private EncodingType _selectedEncodingType;

        // Свойство для доступа к выбранному типу кодирования.
        // При изменении типа кодирования создается новый объект данных QR-кода 
        // соответствующего типа и обновляется QR-код.
        public EncodingType SelectedEncodingType
        {
            get { return _selectedEncodingType; }
            set
            {
                // Проверка на изменение значения.
                if (_selectedEncodingType != value)
                {
                    _selectedEncodingType = value;
                    // Создание объекта данных QR-кода в зависимости от выбранного типа кодирования.
                    switch (_selectedEncodingType)
                    {
                        case EncodingType.ТЕКСТ:
                            _qrCodeService.code = new QR_Text(_qr.Content, _qr.ForegroundColor_Top, _qr.ForegroundColor_Bottom, _qr.BackgroundColor, _qr.Logo, _qr.ModuleShape);
                            break;
                        case EncodingType.ССЫЛКА:
                            _qrCodeService.code = new QR_Link("https://vk.com", _qr.ForegroundColor_Top, _qr.ForegroundColor_Bottom, _qr.BackgroundColor, _qr.Logo, "http://", _qr.ModuleShape);
                            break;
                        case EncodingType.ТЕЛЕФОН:
                            _qrCodeService.code = new QR_Link("+79515554433", _qr.ForegroundColor_Top, _qr.ForegroundColor_Bottom, _qr.BackgroundColor, _qr.Logo, "tel:", _qr.ModuleShape);
                            break;
                        case EncodingType.ПОЧТА:
                            _qrCodeService.code = new QR_Text("your_mail@gmail.com", _qr.ForegroundColor_Top, _qr.ForegroundColor_Bottom, _qr.BackgroundColor, _qr.Logo, _qr.ModuleShape);
                            break;
                        case EncodingType.WhatsApp:
                            _qrCodeService.code = new QR_Link("+79515554433", _qr.ForegroundColor_Top, _qr.ForegroundColor_Bottom, _qr.BackgroundColor, _qr.Logo, "https://api.whatsapp.com/send?phone=", _qr.ModuleShape);
                            break;
                    }
                    // Обновление данных QR-кода и уведомление об изменении свойств.
                    _qr = _qrCodeService.code;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Content)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedEncodingType)));
                }
            }
        }
    }
}