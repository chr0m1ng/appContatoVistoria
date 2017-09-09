using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Contato_Vistoria
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public string server;
        public string user;
        public string pass;

        public SettingsPage()
        {
            InitializeComponent();
            server = entryServer.Text;
            user = entryUser.Text;
            pass = entryPass.Text;
        }

        protected async void btSalvarClicked(object sender, EventArgs e)
        {
            server = entryServer.Text.Trim();
            user = entryUser.Text.Trim();
            pass = entryPass.Text;

            await DisplayAlert("Opções", "Salvo com Sucesso!", "Ok");
            await Navigation.PopAsync();
        }
    }
}
