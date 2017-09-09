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
        private List<bool> uploading;
        private bool erros = false;
        private int qtdErro = 0;

        public ListCarImages(string placa, string server, string user, string pass)
        {
            InitializeComponent();
            BindingContext = new ListCarImagesViewModel();
            this.placa = placa;
            this.server = server;
            this.user = user;
            this.pass = pass;

            uploading = new List<bool>();

            DateTime data = DateTime.Today;
            var mesNome = DateTimeFormatInfo.CurrentInfo.GetMonthName(data.Month);

            subDir = "/" + data.Year + "/" + data.Month + "-" + mesNome + "/" + data.Day + "-" + data.Month + "-" + data.Year + "/" + placa;

            if (!DependencyService.Get<IFtpWebRequest>().createDir(server, user, pass, "/" + data.Year))
            {
                Navigation.PopAsync();
                DisplayAlert("erro", "erro ao criar pasta", "Ok");
            }
            if(!DependencyService.Get<IFtpWebRequest>().createDir(server, user, pass, "/" + data.Year + "/" + data.Month + "-" + mesNome))
            {
                Navigation.PopAsync();
                DisplayAlert("erro", "erro ao criar pasta", "Ok");
            }
            if (!DependencyService.Get<IFtpWebRequest>().createDir(server, user, pass, "/" + data.Year + "/" + data.Month + "-" + mesNome + "/" + data.Day + "-" + data.Month + "-" + data.Year))
            {
                Navigation.PopAsync();
                DisplayAlert("erro", "erro ao criar pasta", "Ok");
            }
            if (!DependencyService.Get<IFtpWebRequest>().createDir(server, user, pass, subDir))
            {
                Navigation.PopAsync();
                DisplayAlert("erro", "erro ao criar pasta", "Ok");
            }
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;
            DisplayAlert("Ta upando?", ((ListCarImagesViewModel.Item)((ListView)sender).SelectedItem).Uploading.ToString(), "Ok");
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
                Device.BeginInvokeOnMainThread(() =>
                {
                    loading.IsRunning = true;
                    loading.IsVisible = true;
                    btTirarFoto.IsEnabled = false;
                    btAbrirGaleria.IsEnabled = false;
                    btConcluido.IsEnabled = false;
                });
                var arquivo = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full
                });

                Device.BeginInvokeOnMainThread(() => 
                {
                    btTirarFoto.IsEnabled = true;
                    btAbrirGaleria.IsEnabled = true;
                    btConcluido.IsEnabled = true;
                });

                if (arquivo == null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        loading.IsRunning = false;
                        loading.IsVisible = false;
                    });
                    return;
                }

                else
                {
                    String unixTimestamp = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Millisecond.ToString();
                    await Task.Run(() =>
                    {
                        ListItems.Add(new ListCarImagesViewModel.Item { Text = unixTimestamp, Image = arquivo.Path, Uploading = true });
                        int current = ListItems.Count - 1;

                        bool resp = DependencyService.Get<IFtpWebRequest>().uploadAsync(server, ListItems[current].Image, user, pass, subDir).Result;

                        Task.Delay(2000);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ListItems.ElementAt(current).Uploading = false;
                            loading.IsRunning = false;
                            loading.IsVisible = false;
                        });

                        if (!resp)
                        {
                            erros = true;
                            qtdErro++;
                        }
                        uploading.Add(resp);
                        return;
                    });
                }
            }
            catch (Exception err)
            {
                await DisplayAlert("Erro Adicionar Imagem", err.ToString(), "Ok");
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
                Device.BeginInvokeOnMainThread(() =>
                {
                    loading.IsRunning = true;
                    loading.IsVisible = true;
                    btTirarFoto.IsEnabled = false;
                    btAbrirGaleria.IsEnabled = false;
                    btConcluido.IsEnabled = false;
                });

                var arquivo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    AllowCropping = true,
                    SaveToAlbum = true,
                    Directory = placa,
                    Name = unixTimestamp
                });

                Device.BeginInvokeOnMainThread(() =>
                {
                    btTirarFoto.IsEnabled = true;
                    btAbrirGaleria.IsEnabled = true;
                    btConcluido.IsEnabled = true;
                });

                if (arquivo == null)
                {
                    Device.BeginInvokeOnMainThread(() => 
                    {
                        loading.IsRunning = false;
                        loading.IsVisible = false;
                    });
                    return;
                }
                else
                {

                    ListItems.Add(new ListCarImagesViewModel.Item { Text = unixTimestamp, Image = arquivo.Path, Uploading = true });;
                    await Task.Run(() =>
                    {
                        int current = ListItems.Count - 1;

                        bool resp = DependencyService.Get<IFtpWebRequest>().uploadAsync(server, ListItems[current].Image, user, pass, subDir).Result;

                        Task.Delay(2000);

                        Device.BeginInvokeOnMainThread(() => 
                        {
                            ListItems.ElementAt(current).Uploading = false;
                            loading.IsRunning = false;
                            loading.IsVisible = false;
                        });

                        if (!resp)
                        {
                            erros = true;
                            qtdErro++;
                        }
                        uploading.Add(resp);
                        return;
                    });

                }
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
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        loading.IsRunning = true;
                        loading.IsVisible = true;
                        btTirarFoto.IsEnabled = false;
                        btAbrirGaleria.IsEnabled = false;
                        btConcluido.IsEnabled = false;
                    });


                    while(!ListItems.All(x => x.Uploading == false))
                        await Task.Delay(1000);


                    if(uploading.All(x => x == true))
                        await DisplayAlert("Upload", "Upload de fotos realizado com sucesso! :) (Por Segurança, Favor Checar se já está no servidor)", "Ok");

                    if (erros)
                        await DisplayAlert("Erro no Upload", "Houve erro ao fazer o upload de " + qtdErro + " fotos. Favor checar no servidor.", "Ok");


                    Device.BeginInvokeOnMainThread(() =>
                    {
                        loading.IsRunning = false;
                        loading.IsVisible = false;
                        btTirarFoto.IsEnabled = true;
                        btAbrirGaleria.IsEnabled = true;
                        btConcluido.IsEnabled = true;
                    });

                }
                uploading.Clear();
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

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private ObservableCollection<Item> _Items;
        public ObservableCollection<Item> Items
        {
            get
            {
                return _Items;
            }
            set
            {
                _Items = value;
                OnPropertyChanged("Items");
            }
        }

        public ListCarImagesViewModel()
        {
            _Items = new ObservableCollection<Item>();
        }

        public class Item : INotifyPropertyChanged
        {

            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName]string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public string Image { get; set; }
            public string Text { get; set; }
            private bool _Uploading;
            public bool Uploading
            {
                get
                {
                    return _Uploading;
                }
                set
                {
                    _Uploading = value;
                    OnPropertyChanged("Uploading");
                }
            }

            public override string ToString() => Image;


        }
    }
}
