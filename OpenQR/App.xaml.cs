using System.Windows;
using OpenQR.Views;
using OpenQR.Modules;
using OpenQR.Services;

namespace OpenQR
{
    public partial class App 
    {   
        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IQrCodeService, QrCodeService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<MainModule>();
        }
    }

}
