using System.IO;
using System.Media;
using System.Net.Cache;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Serilog;
using Windows.Media.Core;
namespace WoWAHDataProject.Code
{
    static class Egg
    {
        // 音ミク失敗 = "Miku sounds"+"Failure" / Sound+Future
        public static List<Tuple<string, MediaPlayer>> 音ミク失敗 = new List<Tuple<string, MediaPlayer>>();
        // 音ミク首尾よく = "Miku sounds"+"Successfully" / Sound+Future
        public static List<Tuple<string, MediaPlayer>> 音ミク首尾よく = new List<Tuple<string, MediaPlayer>>();
        // 音 = "Sound"
        public static bool 音;
        // 無作為 = "Random"
        private static dynamic 無作為 = new Random();
        // 根 = "Root"
        private static string 根 = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        // えれくとりっく_えんじぇぅ = "Electric Angel"
        public static int えれくとりっく_えんじぇぅ = 無作為.Next(0, 100);
        // プロ生ちゃんNumber = "Pronama Number"
        public static async void プロ生ちゃんNumber()
        {
            // かわいい = "Cute"
            int かわいい = 無作為.Next(0,100);
            if (かわいい >= 0)
            {
                // Indicator that we enable sound
                音 = true;
                // プロ生ちゃん = "Pronama Time"
                Log.Information("プロ生ちゃん Time!");
                // Pet Sound Pronama eggs
                await Pet音プロ生ちゃんEggs();
            }
        }
        public static async Task Pet音プロ生ちゃんEggs()
        {
            // 卵 = "Egg"
            dynamic 卵;
            Log.Information("It's time to pet 音プロ生ちゃん eggs!");
            TimeSpan now = DateTime.Now.TimeOfDay;
            // If it rolled 1, we pet the special egg
            if (えれくとりっく_えんじぇぅ == 1)
            {
                Log.Information("We pet our precious special egg!");
                string えれくとりっく_えんじぇぅMB = @$"{根}\卵\芸術プロ生ちゃん\中心\miku\えれくとりっく_えんじぇぅmubo.卵";
                // Hatch egg, indicate if it shall get played or not, indicate if it's a failure sound egg
                await Hatch音プロ生ちゃんEgg(えれくとりっく_えんじぇぅMB, "初", false);
            }
            // If it's morning, we pet the お早う(Ohayou|Good morning) eggs
            else if (now > new TimeSpan(6, 0, 0) && now < new TimeSpan(12, 0, 0))
            {
                Log.Information("Now we pet お早う eggs!");
                卵 = Directory.GetFiles(@$"{根}\卵\こんにちは\お早う");
                string お早う = 卵[無作為.Next(卵.Length)];
                await Hatch音プロ生ちゃんEgg(お早う, "初", false);
            }
            // If it's afternoon, we pet the こんにちは(Kon'nichiwa|Hello) eggs
            else if (now > new TimeSpan(12, 0, 0) && now < new TimeSpan(18, 0, 0))
            {
                Log.Information("Now we pet こんにちは eggs!");
                卵 = Directory.GetFiles(@$"{根}\卵\こんにちは");
                string こんにちは = 卵[無作為.Next(卵.Length)];
                await Hatch音プロ生ちゃんEgg(こんにちは, "初", false);
            }
            // If it's evening/night, we pet the こんばんは(Konbanwa|Good evening) eggs
            else
            {
                Log.Information("Now we pet こんばんは eggs!");
                卵 = Directory.GetFiles(@$"{根}\卵\こんにちは\こんばんは");
                string こんばんは = 卵[無作為.Next(卵.Length)];
                await Hatch音プロ生ちゃんEgg(こんばんは, "初", false);
            }
            // Pet and hatch the 首尾よく(Shubi yoku|Successfully) sound eggs
            Log.Information("Now we pet 首尾よく eggs!");
            卵 = Directory.GetFiles(@$"{根}\卵\首尾よく");
            string 首尾よく = 卵[無作為.Next(卵.Length)];
            await Hatch音プロ生ちゃんEgg(首尾よく, "ミク", false);
            // Pet and hatch the 失敗(Shippai|Failure) sound eggs
            Log.Information("Now we pet the 失敗 eggs!");
            卵 = Directory.GetFiles(@$"{根}\卵\失敗");
            string 失敗 = 卵[無作為.Next(卵.Length)];
            await Hatch音プロ生ちゃんEgg(失敗, "ミク", true);
            Log.Information("We just petted all the 音プロ生ちゃん eggs!");
            Log.Information("What adorable eggs!");
        }
        // Hatch the 音プロ生ちゃん Sound-Pronama eggs
        public static async Task Hatch音プロ生ちゃんEgg(string 卵, string 音, bool 失敗)
        {
            await semaphore.WaitAsync();
            try
            {
                Log.Information($"Now we hatch that egg: {卵}");
                // 卵の名前wav = "Egg name wav"
                string 卵の名前wav = 卵.Replace(".卵", ".wav");
                // Hatch the egg
                File.Move(卵, 卵の名前wav);
                Uri 初音Uri = new(卵の名前wav, UriKind.Absolute);
                // 初音 = "Hatsune"
                MediaPlayer 初音 = new();
                if (音 == "初")
                {
                    // If it's the special egg, we play it from memory with SoundPlayer
                    if(卵.Contains("えれくとりっく_えんじぇぅmubo"))
                    {
                        SoundPlayer sp = Playえれくとりっく_えんじぇぅ(卵の名前wav);
                        await Task.Run(() => sp.Play());
                        sp.Dispose();
                    }
                    // Otherwise we play the regular eggs with MediaPlayer
                    else
                    {
                        Play音楽ゲーム(初音, 卵の名前wav);
                    }
                }
                // If it's a failure sound egg, we add it to the failure list
                else if (音 == "ミク" && 失敗)
                {
                    Egg.音ミク失敗.Add(new Tuple<string, MediaPlayer>(卵の名前wav, 初音));
                }
                // If it's a successful sound egg, we add it to the successful list
                else
                {

                    Egg.音ミク首尾よく.Add(new Tuple<string, MediaPlayer>(卵の名前wav, 初音));
                }
            }
            finally
            {
                Log.Information("We just hatched the egg!");
                semaphore.Release();
            }
        }
        // Hatch the 芸術プロ生ちゃん Art-Pronama eggs
        public static async Task Hatch芸術プロ生ちゃんEgg(dynamic callWindow)
        {
            Log.Information("Look! There are even more eggs to hatch! 芸術プロ生ちゃん eggs!");
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // 芸術卵 = "Art egg"
                dynamic 芸術卵;
                // 芸術 = "Art"
                string 芸術;
                // Only on bigger windows
                if (callWindow.FindName("centerRightImage") != null)
                {
                    // ミク = "Miku"
                    bool ミク = false;
                    // If it rolled 1, we apply the special egg
                    if(えれくとりっく_えんじぇぅ == 1)
                    {
                        芸術 = @$"{根}\卵\芸術プロ生ちゃん\中心\miku\えれくとりっく_えんじぇぅ.卵";
                        ミク = true;
                    }
                    // Otherwise we apply the regular eggs
                    else
                    {
                        芸術卵 = Directory.GetFiles(@$"{根}\卵\芸術プロ生ちゃん\中心");
                        芸術 = 芸術卵[無作為.Next(芸術卵.Length)];
                    }
                    // Hatch egg, load it to memory, seal it, apply it
                    File.Move(芸術, 芸術.Replace(".卵", ".png"));
                    BitmapImage stream = LoadImageToMemory(芸術.Replace(".卵", ".png"), true, ミク);
                    File.Move(芸術.Replace(".卵", ".png"), 芸術.Replace(".png", ".卵"));
                    callWindow.centerRightImage.Source = stream;
                }
                // On all windows
                if (callWindow.FindName("bottomLeftImage") != null)
                {
                    // If the window is too small, we make it bigger
                    if (callWindow.Height < 375)
                    {
                        callWindow.Height = 375;
                    }
                    // Get egg
                    芸術卵 = Directory.GetFiles(@$"{根}\卵\芸術プロ生ちゃん\角");
                    芸術 = 芸術卵[無作為.Next(芸術卵.Length)];
                    // Hatch egg, load it to memory, seal it, apply it
                    File.Move(芸術, 芸術.Replace(".卵", ".png"));
                    BitmapImage stream = LoadImageToMemory(芸術.Replace(".卵", ".png"), false, false);
                    File.Move(芸術.Replace(".卵", ".png"), 芸術.Replace(".png", ".卵"));
                    callWindow.bottomLeftImage.Source = stream;
                }
            });
        }
        // 封印Egg = Seal egg | ニコニコ動画 = Nico Nico Douga -> Nico Nico Video
        public static async Task 封印Egg(string 卵, MediaPlayer ニコニコ動画)
        {
            await Task.Run(() => File.Move(卵, 卵.Replace(".wav", ".卵")));
            // Close media player
            ニコニコ動画.Close();
        }
        // Play音楽ゲーム = Play Music Game | キャッチザウェーブ = Catch the wave
        public static async Task Play音楽ゲーム(MediaPlayer キャッチザウェーブ, string 卵)
        {
            if (キャッチザウェーブ.Source == null)
            {
                Uri 初音Uri = new(卵, UriKind.Absolute);
                キャッチザウェーブ.Open(初音Uri);
            }
            // Set volume and play
            キャッチザウェーブ.Volume = 0.2;
            await キャッチザウェーブ.Dispatcher.InvokeAsync(() => キャッチザウェーブ.Play());
            // When the media ends, seal the egg
            キャッチザウェーブ.MediaEnded += async (sender, e) => await 封印Egg(卵, キャッチザウェーブ);
        }
        // Playえれくとりっく_えんじぇぅ = Play Electric Angel
        public static SoundPlayer Playえれくとりっく_えんじぇぅ(string uri)
        {
            // SoundPlayer with memory stream as source
            byte[] result = System.IO.File.ReadAllBytes(uri);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(result);
            SoundPlayer sp = new SoundPlayer(ms);
            File.Move(uri, uri.Replace(".wav", ".卵"));
            return sp;
        }
        public static BitmapImage LoadImageToMemory(string path, bool centerImage, bool ミク)
        {
            Log.Information("Look at the time! Now it's time to pet 芸術 eggs!"); // "Geijutsu"|"Art"
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            System.IO.FileStream stream = System.IO.File.Open(path, System.IO.FileMode.Open);
            // Load image to memory
            image.StreamSource = new System.IO.MemoryStream();
            // Set image properties based on weither its a corner image or if its special image
            if (!centerImage)
            {
                image.DecodePixelHeight = 200;
            }
            if (ミク)
            {
                image.DecodePixelWidth = 225;
            }
            stream.CopyTo(image.StreamSource);
            image.EndInit();
            stream.Close();
            stream.Dispose();
            image.StreamSource.Close();
            image.StreamSource.Dispose();
            return image;
        }
    }
}
