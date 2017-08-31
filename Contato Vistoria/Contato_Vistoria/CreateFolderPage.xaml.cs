using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Contato_Vistoria
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateFolderPage : ContentPage
    {
        private ListCarImages ListPage;
        private SettingsPage Settings;

        public CreateFolderPage()
        {
            try
            {
                InitializeComponent();
                this.Appearing += CreateFolderPage_Appearing;
                Settings = new SettingsPage();
            }
            catch(Exception err)
            {
                DisplayAlert("Erro", err.ToString(), "Ok");
            }
        }

        private void CreateFolderPage_Appearing(object sender, EventArgs e)
        {
            try
            {
                entryLetras.Text = "";
                entryNumeros.Text = "";
                switchMegaLaudo.IsToggled = false;

                if (ListPage != null)
                    ListPage = null;

                this.Appearing -= CreateFolderPage_Appearing;
            }
            catch(Exception err)
            {
                DisplayAlert("Erro", err.ToString(), "Ok");
            }
        }

        protected async void btCriarPastaClicked(object sender, EventArgs e)
        {
            try
            {
                if (entryLetras.Text.Length == 3 && entryNumeros.Text.Length == 4)
                {
                    if(switchMegaLaudo.IsToggled)
                    {
                        string myIp = DependencyService.Get<IFtpWebRequest>().getIpExtern();
                        await DisplayAlert("IP", myIp, "Ok");
                        ListPage = new ListCarImages(entryLetras.Text + "-" + entryNumeros.Text + " - MEGALAUDO", "ftp://" + myIp, Settings.user, Settings.pass);
                    }
                    else
                        ListPage = new ListCarImages(entryLetras.Text + "-" + entryNumeros.Text, Settings.server, Settings.user, Settings.pass);
                    entryLetras.Text = "";
                    entryNumeros.Text = "";
                    switchMegaLaudo.IsToggled = false;
                    await Navigation.PushAsync(ListPage);
                }
                else
                    await DisplayAlert("Erro", "Informe a Placa Corretamente.", "Ok");
            }
            catch(Exception err)
            {
                await DisplayAlert("Erro", err.ToString(), "Ok");
            }
        }

        protected async void btOpcoesClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(Settings);
            }
            catch(Exception err)
            {
                await DisplayAlert("Erro", err.ToString(), "Ok");
            }
        }

        private void entryLetras_Unfocused(object sender, FocusEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => 
            {
                entryNumeros.Focus();
            }); 
        }
    }
}
