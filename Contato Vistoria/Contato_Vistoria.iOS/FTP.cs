using System;
using System.Net;
using System.IO;

using Foundation;
using UIKit;
using Contato_Vistoria.iOS;
using System.Collections.Generic;
using System.Json;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(FTP))]
namespace Contato_Vistoria.iOS
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
                string PureFileName = new FileInfo(fileName).Name;

                String uploadUrl = String.Format("{0}{1}/{2}", FtpUrl, UploadDirectory, PureFileName);
                FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(uploadUrl);
                req.Proxy = null;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.Credentials = new NetworkCredential(userName, password);
                req.UseBinary = true;
                req.UsePassive = true;
                req.KeepAlive = false;

                byte[] data = File.ReadAllBytes(fileName);
                req.ContentLength = data.Length;

                Stream stream = req.GetRequestStream();
                stream.Flush();
                stream.Write(data, 0, data.Length);
                stream.Close();


                FtpWebResponse res = (FtpWebResponse)req.GetResponse();
                string resp = res.StatusDescription;
                res.Close();

                return true;


            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> uploadAsync(string FtpUrl, string fileName, string userName, string password, string UploadDirectory = "")
        {
            try
            {
                string PureFileName = new FileInfo(fileName).Name;

                String uploadUrl = String.Format("{0}{1}/{2}", FtpUrl, UploadDirectory, PureFileName);
                FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(uploadUrl);
                req.Proxy = null;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.Credentials = new NetworkCredential(userName, password);
                req.UseBinary = true;
                req.UsePassive = true;
                req.KeepAlive = false;

                byte[] data = File.ReadAllBytes(fileName);
                req.ContentLength = data.Length;

                Stream stream = await req.GetRequestStreamAsync();
                await stream.FlushAsync();
                await stream.WriteAsync(data, 0, data.Length);
                stream.Close();


                FtpWebResponse res = (FtpWebResponse)await req.GetResponseAsync();
                string resp = res.StatusDescription;

                res.Close();

                if (res.StatusCode == FtpStatusCode.ClosingControl || res.StatusCode == FtpStatusCode.ClosingData)
                    return true;
                else
                    return false;
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
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(FtpUrl + Directory));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.KeepAlive = false;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(userName, password);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                ftpStream.Close();

                //System.Diagnostics.Debug.WriteLine(response.StatusCode);
                response.Close();

                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable || response.StatusCode == FtpStatusCode.ClosingControl)
                {
                    response.Close();
                    return true;
                }
                else
                {
                    response.Close();
                    return false;
                }

            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    response.Close();
                    return true;
                }
                else
                {
                    response.Close();
                    return false;
                }
            }

        }
        /*
        public async Task<bool> createDirAsync(string FtpUrl, string userName, string password, string Directory)
        {   
            try
            {
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(FtpUrl + Directory));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.KeepAlive = false;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(userName, password);
                FtpWebResponse response = (FtpWebResponse) await reqFTP.GetResponseAsync();
                Stream ftpStream = response.GetResponseStream();
                ftpStream.Close();
                if (response.StatusCode == FtpStatusCode.CommandOK || response.StatusCode == FtpStatusCode.FileActionOK)
                {
                    response.Close();
                    return true;
                }
                else
                {
                    response.Close();
                    return false;
                }

            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    response.Close();
                    return false;
                }
                return false;
            }

        }
        */
        public string getIpExtern()
        {
            string url = "https://api.myjson.com/bins/123g9p";

            JsonValue json = FetchIP(url);

            return json["ip"];
        }

        private JsonValue FetchIP(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            // Send the request to the server and wait for the response:
            using (WebResponse response = request.GetResponse())
            {
                // Get a stream representation of the HTTP web response:
                using (Stream stream = response.GetResponseStream())
                {
                    // Use this stream to build a JSON document object:
                    JsonValue jsonDoc = Task.Run(() => JsonObject.Load(stream)).Result;
                    Console.Out.WriteLine("Response: {0}", jsonDoc.ToString());

                    // Return the JSON document:
                    return jsonDoc;
                }
            }
        }


    }
}