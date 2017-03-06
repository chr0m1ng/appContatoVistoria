using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Media;
using System.IO;
using System.Net;
using MvvmCross.Platform;

namespace Contato_Vistoria
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListCarImages : ContentPage
    {
        private int qtdOutros;
        private List<string> tiposImg;
        private string placa;
        public ListCarImages(string placa)
        {
            InitializeComponent();
            BindingContext = new ListCarImagesViewModel();
            qtdOutros = 1;
            tiposImg = new List<string>();
            tiposImg.AddRange(new string[] { "Chassi", "Motor", "Frente", "Traseira", "Etiquetas", "Vidro" , "Lacre", "Placa", "Hodometro", "Outros" });
            this.placa = placa;
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            await DisplayAlert("Selected", e.SelectedItem.ToString(), "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        protected async void btAddImgClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Tipo de Imagem", "Cancelar", null, tiposImg.ToArray());

            if (!action.Equals("Cancelar"))
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("Sem Camera", ":( Erro na Camera.", "OK");
                    return;
                }

                if (action.Equals("Outros") && qtdOutros > 1)
                {
                    action = action + " " + qtdOutros.ToString();
                    qtdOutros++;
                }

                else if (action.Equals("Outros"))
                    qtdOutros++;

                var arquivo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = placa,
                    Name = action + ".jpg"
                });

                await DisplayAlert("Salvo no Celular", "Imagem Salva no Celular", "Ok");

                if (arquivo == null)
                    return;

                ((ListCarImagesViewModel)BindingContext).Items.Add(new ListCarImagesViewModel.Item { Text = action, Image = arquivo.Path });

            }
            if (!action.Equals("Outros"))
                tiposImg.Remove(action);
        }

        protected async void btConcluidoClicked(object sender, EventArgs e)
        {
            /*
            foreach(var item in ((ListCarImagesViewModel)BindingContext).Items)
            {
                File img = new File(item.Image);
                img.Delete();

            }
            */

            await Navigation.PopAsync();
        }

        /*
        public void UploadFile(string pathFile)
        {
            FtpWebRequest webReq = (FtpWebRequest)WebRequest.Create("ftp://192.168.0.102");
            webReq.Credentials = new NetworkCredential("gabrielrsantoss@outlook.com", "32562033Ga");
            webReq.Method = "I don't know that to put in here, i have no WebRequestMethods class...";
            Stream
            
        }
        */
    }



    class ListCarImagesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Item> Items { get; }
        public ObservableCollection<Grouping<string, Item>> ItemsGrouped { get; }
        public ListCarImagesViewModel()
        {
            Items = new ObservableCollection<Item>();

            var sorted = from item in Items
                         orderby item.Text
                         group item by item.Text[0].ToString() into itemGroup
                         select new Grouping<string, Item>(itemGroup.Key, itemGroup);

            ItemsGrouped = new ObservableCollection<Grouping<string, Item>>(sorted);

            RefreshDataCommand = new Command(async () => await RefreshData());
        }

        public ICommand RefreshDataCommand { get; }

        async Task RefreshData()
        {
            IsBusy = true;
            //Load Data Here
            await Task.Delay(2000);

            IsBusy = false;
        }

        bool busy;
        public bool IsBusy
        {
            get { return busy; }
            set
            {
                busy = value;
                OnPropertyChanged();
                ((Command)RefreshDataCommand).ChangeCanExecute();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public class Item
        {
            public string Image { get; set; }
            public string Text { get; set; }

            public override string ToString() => Text;
        }

        public class Grouping<K, T> : ObservableCollection<T>
        {
            public K Key { get; private set; }

            public Grouping(K key, IEnumerable<T> items)
            {
                Key = key;
                foreach (var item in items)
                    this.Items.Add(item);
            }
        }
    }
}
