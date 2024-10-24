using OpenQR.Services;
using OpenQR.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OpenQR.ViewModels
{
    // Модель представления для выбора стилей QR-кода.
    internal class StylesViewModel : BindableBase
    {
        // Коллекции стилей для каждой строки в представлении.
        public ObservableCollection<ButtonStyle> StylesRow1 { get; } = new();
        public ObservableCollection<ButtonStyle> StylesRow2 { get; } = new();

        // Команда для выбора стиля.
        public ICommand SelectStyleCommand { get; }

        // Сервис для работы с QR-кодом.
        private readonly IQrCodeService _qrCodeService;

        // Конструктор.
        public StylesViewModel(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;

            // Добавление стилей в коллекции.
            StylesRow1.Add(new ButtonStyle { IsSelected = true, ForegroundColor_Top = "#000000", ForegroundColor_Bottom = "#000000", BackgroundColor = "#FFFFFF", IconSource = "Static/Images/qr-1.png" });
            StylesRow1.Add(new ButtonStyle { ForegroundColor_Top = "#FCF6BA", ForegroundColor_Bottom = "#BF953F", BackgroundColor = "#0A0A0A", IconSource = "Static/Images/qr-2.png" });
            StylesRow1.Add(new ButtonStyle { ForegroundColor_Top = "#C81590", ForegroundColor_Bottom = "#F8AB2A", BackgroundColor = "#000000", IconSource = "Static/Images/qr-3.png" });

            StylesRow2.Add(new ButtonStyle { ForegroundColor_Top = "#000000", ForegroundColor_Bottom = "#000000", BackgroundColor = "#25D366", IconSource = "Static/Images/qr-4.png" });
            StylesRow2.Add(new ButtonStyle { ForegroundColor_Top = "#0866FF", ForegroundColor_Bottom = "#0866FF", BackgroundColor = "#FFFFFF", IconSource = "Static/Images/qr-5.png" });
            StylesRow2.Add(new ButtonStyle { ForegroundColor_Top = "#07F8F2", ForegroundColor_Bottom = "#E61946", BackgroundColor = "#000000", IconSource = "Static/Images/qr-6.png" });

            // Инициализация команды выбора стиля.
            SelectStyleCommand = new DelegateCommand<ButtonStyle>(selectedStyle =>
            {
                // Сброс выделения у всех стилей.
                foreach (var style in StylesRow1) { style.IsSelected = false; }
                foreach (var style in StylesRow2) { style.IsSelected = false; }

                // Выделение выбранного стиля.
                selectedStyle.IsSelected = true;

                // Обновление цветов QR-кода.
                ForegroundColor_Top = selectedStyle.ForegroundColor_Top;
                ForegroundColor_Bottom = selectedStyle.ForegroundColor_Bottom;
                BackgroundColor = selectedStyle.BackgroundColor;
            });
        }

        private string _foregroundColorTop = "#000000";
        public string ForegroundColor_Top
        {
            get { return _foregroundColorTop; }
            set
            {
                SetProperty(ref _foregroundColorTop, value);
                UpdateSelectedStyle();
            }
        }

        private string _foregroundColorBottom = "#000000";
        public string ForegroundColor_Bottom
        {
            get { return _foregroundColorBottom; }
            set
            {
                SetProperty(ref _foregroundColorBottom, value);
                UpdateSelectedStyle();
            }
        }

        private string _backgroundColor = "#FFFFFF";
        public string BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                SetProperty(ref _backgroundColor, value);
                UpdateSelectedStyle();
            }
        }

        private void UpdateSelectedStyle()
        {
            ButtonStyle selectedStyle = null;

            foreach (var style in StylesRow1)
            {
                if (style.IsSelected)
                {
                    selectedStyle = style;
                    break;
                }
            }

            if (selectedStyle == null)
            {
                foreach (var style in StylesRow2)
                {
                    if (style.IsSelected)
                    {
                        selectedStyle = style;
                        break;
                    }
                }
            }

            if (selectedStyle != null)
            {
                if (_qrCodeService.code != null)
                {
                    IQR_CodeData qr = _qrCodeService.code;
                    qr.ForegroundColor_Top = ForegroundColor_Top;
                    qr.ForegroundColor_Bottom = ForegroundColor_Bottom;
                    qr.BackgroundColor = BackgroundColor;
                    qr.FromLeftToRightCorner = IsVerticalGradient;
                    _qrCodeService.code = qr;
                }
            }
        }

        private bool _isVerticalGradient = true;
        public bool IsVerticalGradient
        {
            get => _isVerticalGradient;
            set
            {
                SetProperty(ref _isVerticalGradient, value);
                UpdateSelectedStyle();
            }
        }
    }

    // Модель для представления стиля кнопки.
    public class ButtonStyle : BindableBase
    {
        // Флаг выделения стиля.
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        // Цвет фона.
        private string _background;
        public string BackgroundColor
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }

        // Цвет переднего плана
        private string _darkColor;
        public string ForegroundColor_Top
        {
            get { return _darkColor; }
            set { SetProperty(ref _darkColor, value); }
        }

        // Цвет переднего плана
        private string _lightColor;
        public string ForegroundColor_Bottom
        {
            get { return _lightColor; }
            set { SetProperty(ref _lightColor, value); }
        }

        // Путь к файлу с изображением иконки стиля.
        private string _iconSource;
        public string IconSource
        {
            get => _iconSource;
            set { SetProperty(ref _iconSource, value); }
        }
    }
}