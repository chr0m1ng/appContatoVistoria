using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Contato_Vistoria
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateFolderPage : ContentPage
    {
        public CreateFolderPage()
        {
            InitializeComponent();
        }

        protected async void btCriarPastaClicked(object sender, EventArgs e)
        {
            if (entryLetras.Text.Length == 3 && entryNumeros.Text.Length == 4)
                await Navigation.PushAsync(new ListCarImages(entryLetras.Text + "-" + entryNumeros.Text));
            else
                await DisplayAlert("Erro", "Informe a Placa Corretamente.", "Ok");
        }
    }
}
