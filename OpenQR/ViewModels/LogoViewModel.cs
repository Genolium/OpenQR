﻿using Microsoft.Win32;
using OpenQR.Models;
using OpenQR.Services;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace OpenQR.ViewModels
{
    // Модель представления для выбора логотипа QR-кода.
    class LogoViewModel : BindableBase
    {
        // Коллекции логотипов для каждой строки в представлении.
        public ObservableCollection<Logo> StylesRow1 { get; } = new();
        public ObservableCollection<Logo> StylesRow2 { get; } = new();
        public ObservableCollection<Logo> StylesRow3 { get; } = new();

        // Сервис для работы с QR-кодом.
        private readonly IQrCodeService _qrCodeService;

        public ICommand SelectLogoCommand { get; }
        public ICommand LoadLogoCommand { get; }


        // Конструктор.
        public LogoViewModel(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;

            // Добавление логотипов в коллекции.
            StylesRow1.Add(new Logo { IsSelected = true }); // Пустой логотип по умолчанию
            StylesRow1.Add(new Logo { IconSource = "/Static/Images/social_media1.png" });
            StylesRow1.Add(new Logo { IconSource = "/Static/Images/social_media2.png" });
            StylesRow1.Add(new Logo { IconSource = "/Static/Images/social_media3.png" });

            StylesRow2.Add(new Logo { IconSource = "/Static/Images/social_media4.png" });
            StylesRow2.Add(new Logo { IconSource = "/Static/Images/social_media5.png" });
            StylesRow2.Add(new Logo { IconSource = "/Static/Images/social_media6.png" });
            StylesRow2.Add(new Logo { IconSource = "/Static/Images/social_media7.png" });

            StylesRow3.Add(new Logo { IconSource = "/Static/Images/social_media8.png" });
            StylesRow3.Add(new Logo { IconSource = "/Static/Images/social_media9.png" });
            StylesRow3.Add(new Logo { IconSource = "/Static/Images/social_media10.png" });
            StylesRow3.Add(new Logo { IconSource = "/Static/Images/social_media11.png" });

            // Инициализация команды выбора логотипа.
            SelectLogoCommand = new DelegateCommand<Logo>(selectedLogo =>
            {
                // Сброс выделения у всех логотипов.
                foreach (var style in StylesRow1) { style.IsSelected = false; }
                foreach (var style in StylesRow2) { style.IsSelected = false; }
                foreach (var style in StylesRow3) { style.IsSelected = false; }

                // Выделение выбранного логотипа.
                selectedLogo.IsSelected = true;

                // Обновление логотипа в данных QR-кода.
                if (_qrCodeService.code != null)
                {
                    IQR_CodeData qr = _qrCodeService.code;
                    if (selectedLogo.IconSource != null)
                    {
                        // Загрузка изображения логотипа из файла.
                        qr.Logo = new Bitmap(selectedLogo.IconSource.Replace("/Static", "Static"));
                    }
                    else
                    {
                        // Сброс логотипа.
                        qr.Logo = null;
                    }
                    // Обновление QR-кода в сервисе.
                    _qrCodeService.code = qr;
                }
            });

            LoadLogoCommand = new DelegateCommand(LoadLogo);

        }

        private void LoadLogo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;

                    foreach (var logo in StylesRow1) { logo.IsSelected = false; }
                    foreach (var logo in StylesRow2) { logo.IsSelected = false; }
                    foreach (var logo in StylesRow3) { logo.IsSelected = false; }

                    if (_qrCodeService.code != null)
                    {
                        IQR_CodeData qr = _qrCodeService.code;
                        qr.Logo = new Bitmap(filePath);
                        _qrCodeService.code = qr;
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxResult result = MessageBox.Show("При открытии изображения произошла ошибка! Проверьте целостность и формат файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    // Модель для представления логотипа.
    public class Logo : BindableBase
    {
        // Флаг выделения логотипа.
        private bool _isSelected;
        // Свойство для доступа к флагу выделения.
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        // Путь к файлу с изображением логотипа.
        private string _iconSource;
        // Свойство для доступа к пути к файлу с изображением.
        public string IconSource
        {
            get => _iconSource;
            set { SetProperty(ref _iconSource, value); }
        }
    }
}