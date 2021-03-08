using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using TaskBeeJee.ApiConnector;

namespace TaskBeeJee
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void LoginBtnClicked(object sender, EventArgs e)
        {
            //Validation
            if (loginEntry.Text.Length == 0)
            {
                await DisplayAlert("Validation error", "Empty login", "Ok");
                return;
            }
            if (passwordEntry.Text.Length == 0)
            {
                await DisplayAlert("Validation error", "Empty pasword", "Ok");
                return;
            }

            try
            {
                if (!await Main.AuthAsync(loginEntry.Text, passwordEntry.Text))
                {
                    await DisplayAlert("Error", "Bad credentials", "Ok");
                    return;
                }
            } catch
            {
                await DisplayAlert("Error", "Something went wrong", "Ok");
                return;
            }

            await Navigation.PopAsync();
        }
    }
}