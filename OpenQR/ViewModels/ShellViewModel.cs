using System.Windows.Input;
using System.Windows;
using OpenQR.Models;
using OpenQR.Services;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.ComponentModel;

namespace OpenQR.ViewModels
{
    // Модель представления для главного окна приложения.
    public class ShellViewModel : BindableBase, INotifyPropertyChanged
    {
        // Менеджер регионов для навигации.
        private readonly IRegionManager _regionManager;
        // Сервис для работы с QR-кодом.
        private readonly IQrCodeService _qrCodeService;

        // Команда для выхода из приложения.
        public ICommand ApplicationExitCommand { get; }

        // Команды для навигации между представлениями.
        public ICommand NavigateToContentCommand { get; }
        public ICommand NavigateToStylesCommand { get; }
        public ICommand NavigateToDesignCommand { get; }
        public ICommand NavigateToLogoCommand { get; }
        public ICommand NavigateToExportCommand { get; }
        public ICommand NavigateToFaqCommand { get; }

        // Название текущего представления.
        private string _currentView = "ContentView";
        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // Конструктор.
        public ShellViewModel(IRegionManager regionManager, IQrCodeService qrCodeService)
        {
            _regionManager = regionManager;
            _qrCodeService = qrCodeService;
            // Подписка на событие обновления QR-кода.
            _qrCodeService.QrCodeUpdated += OnQrCodeUpdated;

            // Инициализация команд.
            ApplicationExitCommand = new DelegateCommand(ApplicationExit);

            NavigateToContentCommand = new DelegateCommand(() => Navigate("ContentView"));
            NavigateToStylesCommand = new DelegateCommand(() => Navigate("StylesView"));
            NavigateToDesignCommand = new DelegateCommand(() => Navigate("DesignView"));
            NavigateToLogoCommand = new DelegateCommand(() => Navigate("LogoView"));
            NavigateToExportCommand = new DelegateCommand(() => Navigate("ExportView"));
            NavigateToFaqCommand = new DelegateCommand(() => Navigate("FaqView"));

            // Создание QR-кода по умолчанию.
            _qrCodeService.code = new QR_Text("текст для кодирования");
            // Получение изображения сгенерированного QR-кода.
            GeneratedCodeImage = GetGeneratedCodeImage(_qrCodeService.generatedCode);
        }

        // Выход из приложения.
        private void ApplicationExit()
        {
            // Запрос подтверждения выхода.
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        // Переход к указанному представлению.
        private void Navigate(string viewName)
        {
            // Запрос навигации к представлению в регионе MainContent.
            _regionManager.RequestNavigate("MainContent", viewName);
            // Обновление названия текущего представления.
            CurrentView = viewName;
            // Уведомление об изменении свойства CurrentView.
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentView)));
        }

        // Обработчик события обновления QR-кода.
        private void OnQrCodeUpdated(object sender, EventArgs e)
        {
            // Обновление изображения сгенерированного QR-кода.
            GeneratedCodeImage = GetGeneratedCodeImage(_qrCodeService.generatedCode);
        }

        // Изображение сгенерированного QR-кода.
        private BitmapImage _generatedCodeImage;
        public BitmapImage GeneratedCodeImage
        {
            get => _generatedCodeImage;
            private set
            {
                _generatedCodeImage = value;
                // Уведомление об изменении свойства GeneratedCodeImage.
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(GeneratedCodeImage)));
            }
        }

        // Преобразует изображение Bitmap в BitmapImage.
        public BitmapImage GetGeneratedCodeImage(Bitmap code)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                code.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
    }
}