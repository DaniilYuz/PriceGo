using PriceGo2.Viws;

namespace PriceGo2
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(StartPage), typeof(StartPage));
            Routing.RegisterRoute(nameof(mainPage), typeof(mainPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ChangeAvatarPage), typeof(ChangeAvatarPage));
        }
    }
}
