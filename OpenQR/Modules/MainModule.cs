using OpenQR.Views;

namespace OpenQR.Modules
{
    // Модуль главного окна приложения.
    class MainModule : IModule
    {
        // Вызывается после инициализации контейнера зависимостей.
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // Получение менеджера регионов.
            var region = containerProvider.Resolve<IRegionManager>();
            // Регистрация представления ContentView в регионе MainContent.
            region.RegisterViewWithRegion("MainContent", typeof(ContentView));
        }

        // Регистрация типов в контейнере зависимостей.
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Регистрация представлений для навигации.
            containerRegistry.RegisterForNavigation<ContentView>();
            containerRegistry.RegisterForNavigation<StylesView>();
            containerRegistry.RegisterForNavigation<DesignView>();
            containerRegistry.RegisterForNavigation<LogoView>();
            containerRegistry.RegisterForNavigation<ExportView>();
            containerRegistry.RegisterForNavigation<FaqView>();
        }
    }
}