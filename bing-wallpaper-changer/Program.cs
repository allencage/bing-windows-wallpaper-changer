using Microsoft.Win32;
using RestSharp;
using System.IO;
using System.Runtime.InteropServices;

namespace bing_wallpaper_changer
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RestClient("http://www.bing.com/");
            var request = new RestRequest("HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US", Method.GET);
            var response = client.Execute<dynamic>(request);
            string imageUrl = response.Data["images"][0]["url"];

            var imageRequest = new RestRequest(imageUrl, Method.GET);
            var imageBytes = client.DownloadData(imageRequest);

            string filePath = "MyImage.jpg";
            File.WriteAllBytes(filePath, imageBytes);
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            Wallpaper.Set(imagePath, Wallpaper.Style.Stretched);
        }
    }

    public sealed class Wallpaper
    {
        Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void Set(string path, Style style)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
