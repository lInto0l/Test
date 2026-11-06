using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Documents;

namespace CinemaStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler((sender, args) =>
            {
                if (sender is Window window)
                {
                    window.SetResourceReference(TextElement.ForegroundProperty, "MaterialDesignBody");
                    window.SetResourceReference(Window.BackgroundProperty, "MaterialDesignPaper");
                    window.SetValue(TextElement.FontWeightProperty, FontWeights.Medium);
                    window.SetResourceReference(Window.FontFamilyProperty, "MaterialDesignFont");
                }
            }));
        }
    }

}
