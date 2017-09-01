using System;
using System.IO;
using System.Net;
using Contato_Vistoria.UWP;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking;

[assembly: Xamarin.Forms.Dependency(typeof(FTP))]
namespace Contato_Vistoria.UWP
{
    class FTP : IFtpWebRequest
    {
        public FTP()
        {
        }

        /// Upload File to Specified FTP Url with username and password and Upload Directory if need to upload in sub folders
        ///Base FtpUrl of FTP Server
        ///Local Filename to Upload
        ///Username of FTP Server
        ///Password of FTP Server
        ///[Optional]Specify sub Folder if any
        /// Status String from Server
        public bool upload(string FtpUrl, string fileName, string userName, string password, string UploadDirectory = "")
        {
            try
            {

                Uri uri = new Uri(FtpUrl);

                FtpClient client = new FtpClient();

                Task t = Task.Run(async () =>
                {

                    await client.ConnectAsync(new HostName(uri.Host), "21", userName, password);
                    string PureFileName = new FileInfo(fileName).Name;

                    byte[] data = File.ReadAllBytes(fileName);

                    await client.UploadAsync(String.Format("{0}/{1}", UploadDirectory, PureFileName), data);

                    await client.QuitAsync();
                });
                t.Wait();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool createDir(string FtpUrl, string userName, string password, string Directory)
        {
            try
            {
                Uri uri = new Uri(FtpUrl);

                FtpClient client = new FtpClient();

                Task t = Task.Run(async () =>
                {
                    await client.ConnectAsync(new HostName(uri.Host), "21", userName, password);
                    await client.MkdAsync(Directory);
                    await client.QuitAsync();
                });
                t.Wait();

                return true;
            }
            catch
            {
                return false;
            }

        }


        public string getIpExtern()
        {
            //string url = "https://api.myjson.com/bins/123g9p";

            Uri geturi = new Uri("https://api.myjson.com/bins/123g9p"); //replace your url  
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage responseGet = Task.Run(async () => await client.GetAsync(geturi)).Result;
            string ipJs = Task.Run(async () => await responseGet.Content.ReadAsStringAsync()).Result;

            JsonObject js = JsonObject.Parse(ipJs);
            return js.GetNamedString("ip");
        }


    }
}