using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Media;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Contato_Vistoria
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListCarImages : ContentPage
    {
        private string placa;
        private string server;
        private string user;
        private string pass;
        private string subDir;
        private List<bool> isUploading;

        public ListCarImages(string placa, string server, string user, string pass)
        {
            InitializeComponent();
            BindingContext = new ListCarImagesViewModel();
            this.placa = placa;
            this.server = server;
            this.user = user;
            this.pass = pass;


            isUploading = new List<bool>();

            DateTime data = DateTime.Today;
            var mesNome = DateTimeFormatInfo.CurrentInfo.GetMonthName(data.Month);

            subDir = "/" + data.Year + "/" + data.Month + "-" + mesNome + "/" + data.Day + "-" + data.Month + "-" + data.Year + "/" + placa;

            DependencyService.Get<IFtpWebRequest>().createDir(server, user, pass, "/" + data.Year);
            DependencyService.Get<IFtpWebRequest>().createDir(server, user, pass, "/" + data.Year + "/" + data.Month + "-" + mesNome);
            DependencyService.Get<IFtpWebRequest>().createDir(server, user, pass, "/" + data.Year + "/" + data.Month + "-" + mesNome + "/" + data.Day + "-" + data.Month + "-" + data.Year);
            DependencyService.Get<IFtpWebRequest>().createDir(server, user, pass, subDir);
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        protected async void btGaleriaClicked(object sender, EventArgs e)
        {
            try
            {
                var ListItems = ((ListCarImagesViewModel)BindingContext).Items;

                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert("Sem Galeria", ":( Erro na Galeria.", "OK");
                    return;
                }

                loading.IsRunning = true;
                loading.IsVisible = true;
                btTirarFoto.IsEnabled = false;
                btAbrirGaleria.IsEnabled = false;
                btConcluido.IsEnabled = false;
                var arquivo = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full
                });

                if (arquivo == null)
                {
                    loading.IsRunning = false;
                    loading.IsVisible = false;
                    btTirarFoto.IsEnabled = true;
                    btAbrirGaleria.IsEnabled = true;
                    btConcluido.IsEnabled = true;
                    return;
                }

                else
                {
                    String unixTimestamp = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Millisecond.ToString();
                    ListItems.Add(new ListCarImagesViewModel.Item { Text = unixTimestamp, Image = arquivo.Path });
                    int current = ListItems.Count - 1;
                    btTirarFoto.IsEnabled = true;
                    btAbrirGaleria.IsEnabled = true;
                    btConcluido.IsEnabled = true;
                    isUploading.Add(true);
                    await Task.Run(() => DependencyService.Get<IFtpWebRequest>().upload(server, ListItems[current].Image, user, pass, subDir));
                    isUploading[current] = false;
                }

                loading.IsRunning = false;
                loading.IsVisible = false;

            }
            catch(Exception err)
            {
                await DisplayAlert("Erro Adicionar Imagem",err.ToString(), "Ok");
            }
        }

        protected async void btTirarFotoClicked(object sender, EventArgs e)
        {
            try
            {
                var ListItems = ((ListCarImagesViewModel)BindingContext).Items;

                String unixTimestamp = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Millisecond.ToString();

                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("Sem Camera", ":( Erro na Camera.", "OK");
                    return;
                }

                if (!CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("Sem Camera", ":( Erro na Camera.", "OK");
                    return;
                }

                loading.IsRunning = true;
                loading.IsVisible = true;
                btTirarFoto.IsEnabled = false;
                btAbrirGaleria.IsEnabled = false;
                btConcluido.IsEnabled = false;
                var arquivo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    AllowCropping = true,
                    SaveToAlbum = true,
                    Directory = placa,
                    Name = unixTimestamp
                });

                if (arquivo == null)
                {
                    loading.IsRunning = false;
                    loading.IsVisible = false;
                    btTirarFoto.IsEnabled = true;
                    btAbrirGaleria.IsEnabled = true;
                    btConcluido.IsEnabled = true;
                    return;
                }
                else
                {

                    ListItems.Add(new ListCarImagesViewModel.Item { Text = unixTimestamp, Image = arquivo.Path});
                    int current = ListItems.Count - 1;
                    btTirarFoto.IsEnabled = true;
                    btAbrirGaleria.IsEnabled = true;
                    btConcluido.IsEnabled = true;
                    isUploading.Add(true);
                    await Task.Run(() => DependencyService.Get<IFtpWebRequest>().upload(server, ListItems[current].Image, user, pass, subDir));
                    isUploading[current] = false;
                }

                loading.IsRunning = false;
                loading.IsVisible = false;
                btTirarFoto.IsEnabled = true;
                btAbrirGaleria.IsEnabled = true;
                btConcluido.IsEnabled = true;
            }
            catch (Exception err)
            {
                await DisplayAlert("Erro Adicionar Imagem", err.ToString(), "Ok");
            }
        }

        protected async void btConcluidoClicked(object sender, EventArgs e)
        {
            try
            {
                var ListItems = ((ListCarImagesViewModel)BindingContext).Items;
                if (ListItems.Count > 0)
                {
                    loading.IsRunning = true;
                    loading.IsVisible = true;
                    btTirarFoto.IsEnabled = false;
                    btAbrirGaleria.IsEnabled = false;
                    btConcluido.IsEnabled = false;
                    await Task.Delay(1000);

                    while (!isUploading.All(x => x == false))
                        await Task.Delay(1000);

                    await DisplayAlert("Upload", "Upload de fotos realizado com sucesso! :) (Por Segurança, Favor Checar se já está no servidor)", "Ok");
                    loading.IsRunning = false;
                    loading.IsVisible = false;
                    btTirarFoto.IsEnabled = true;
                    btAbrirGaleria.IsEnabled = true;
                    btConcluido.IsEnabled = true;

                }
                await Navigation.PopAsync();
            }
            catch (Exception err)
            {
                await DisplayAlert("Erro Upload", err.ToString(), "Ok");
            }
        }
    }



    class ListCarImagesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Item> Items { get; }
        public ListCarImagesViewModel()
        {
            Items = new ObservableCollection<Item>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public class Item
        {
            public string Image { get; set; }
            public string Text { get; set; }

            public override string ToString() => Image;
        }
    }
}
