using System.Threading.Tasks;

namespace Contato_Vistoria
{
    public interface IFtpWebRequest
    {
        bool upload(string FtpUrl, string fileName, string userName, string password, string UploadDirectory = "");
        bool createDir(string FtpUrl, string userName, string password, string Directory);
    }
}
