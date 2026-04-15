using Microsoft.Extensions.DependencyInjection;

namespace Profile_assignment_5
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new View.ProfilePage();
        }

    }    
}