using CsvHelper;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using RandomUserAgent;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VerapSurveyChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("VerapSurveyChecker by Gabs");
            Console.WriteLine("==========================");
            Console.Write("How many votes do you want to cast? ");
            string vc = Console.ReadLine();
            int voteCast = string.IsNullOrWhiteSpace(vc) ? 2000000000 : int.Parse(vc);
            Console.Write("Do you want to watch the browser while casting an inorganic vote? y/n: ");
            string watch = Console.ReadLine();
            watch = string.IsNullOrWhiteSpace(watch) ? "n" : "y";
            Console.Write("Go incognito? y/n: ");
            string inc = Console.ReadLine();
            inc = string.IsNullOrWhiteSpace(inc) ? "n" : "y";
            Console.WriteLine("DON'T CLOSE THIS WINDOW!");
            Console.WriteLine("Casting vote for Sara Duterte...\n");

            InitLists();
            MainAsync("https://pilipinas2022.ph/", voteCast, watch, inc).Wait();
        }

        public static List<FirstName> firstnames = new List<FirstName>();
        public static List<LastName> lastnames = new List<LastName>();
        public static int totalFirstnames = 0;
        public static int totalLastnames = 0;

        public static List<Barangay> barangays = new List<Barangay>();
        public static List<CityMun> cityMuns = new List<CityMun>();
        public static List<Province> provinces = new List<Province>();
        public static int totalBarangay = 0;
        public static string selectedProvince;


        public static Random rnd = new Random();

        public static async Task MainAsync(string pageUrl, int voteCast = 10, string watch = "n", string inc = "n")
        {
            var lo = new LaunchOptions
            {
                ExecutablePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                Headless = true,
                Args = new string[] { "--disable-web-security", "--disable-features=site-per-process" } // "--disable-features=IsolateOrigins,site-per-process", 
            };

            if (watch.ToLower() == "y")
                lo.Headless = false;

            var browser = await Puppeteer.LaunchAsync(lo);                           
           
            for(int i = 0; i < voteCast; i++)
            {
                try
                {
                    Page page;
                    if (inc.ToLower() == "y")
                    {
                        var context = await browser.CreateIncognitoBrowserContextAsync();
                        page = await context.NewPageAsync();
                    }
                    else
                        page = await browser.NewPageAsync();
                        
                    await page.SetCacheEnabledAsync(false);
                    page.DefaultTimeout = 180000;
                    page.DefaultNavigationTimeout = 180000;
                    await page.SetViewportAsync(new ViewPortOptions
                    {
                        Width = 1280,
                        Height = 720,
                        DeviceScaleFactor = 0.75
                    });

                    string userAgent = RandomUa.RandomUserAgent;
                    await page.SetUserAgentAsync(userAgent);

                    await page.GoToAsync(pageUrl);

                    await page.WaitForSelectorAsync("#newbtnsaraduterte");
                    await page.ClickAsync("#newbtnsaraduterte", new ClickOptions { Delay = HumanDelay(1, 2) });

                    await page.WaitForSelectorAsync("#FullName");
                    await page.FocusAsync("#FullName");
                    var fullName = GetRandomName();
                    await page.Keyboard.TypeAsync(fullName);

                    var address = GetRandomAddress();
                    await page.SelectAsync("#Province", selectedProvince);

                    await page.FocusAsync("#Address");
                    await page.Keyboard.TypeAsync(address);

                    var mobile = GetRandomPhoneNum();
                    await page.FocusAsync("#Mobile");
                    await page.Keyboard.TypeAsync(mobile);

                    await page.ClickAsync("input[name='vote']", new ClickOptions { Delay = HumanDelay(1, 2) });

                    await page.WaitForSelectorAsync("#newbtnsaraduterte");
                    //await page.WaitForNavigationAsync(new NavigationOptions
                    //{
                    //    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                    //});
                    Thread.Sleep(50);
                    await page.CloseAsync();
                    Console.WriteLine($"{fullName} | {address} | {mobile}");
                    File.AppendAllText("log.csv",$"{fullName},{address},{mobile}\n");
                }
                catch { }
            }

            Console.WriteLine("Casting of inorganic vote done! Haha!");

            Process process = new Process(); 
            process.StartInfo.UseShellExecute = true; 
            process.StartInfo.FileName = "chrome"; 
            process.StartInfo.Arguments = pageUrl; 
            process.Start();
        }

        public static string GetRandomName()
        {
            var fn = firstnames[rnd.Next(0, totalFirstnames)].Firstname;
            var ln = lastnames[rnd.Next(0, totalLastnames)].Surname;
            return $"{fn} {ln}";
        }

        public static string GetRandomAddress()
        {
            string address;
            string comma = "";
            do
            {
                var idx = rnd.Next(0, totalBarangay);
                var brgy = barangays[idx];
                var cm = cityMuns.FirstOrDefault(x => x.citymunCode == brgy.citymunCode);
                var prov = provinces.FirstOrDefault(x => x.provCode == brgy.provCode);
                if(prov.regCode == 13 || string.IsNullOrWhiteSpace(prov.provDesc))
                {
                    selectedProvince = "National Capital Region (NCR)";
                }
                else
                {
                    selectedProvince = prov.provDesc;
                    comma = ", ";
                }
                address = $"{brgy.brgyDesc}, {cm.citymunDesc}{comma}{prov.provDesc}";
            } while (string.IsNullOrWhiteSpace(address));

            return address;
        }

        public static string GetRandomPhoneNum()
        {
            int[] prefix = { 0991, 0992, 0993, 0994, 0895, 0896, 0897, 0898, 0813, 0918, 0940, 0970, 
                0907, 0919, 0946, 0981, 0908, 0920, 0947, 0989, 0909, 0921, 0948, 0992, 0910, 0928, 
                0949, 0998, 0911, 0929, 0950, 0999, 0912, 0930, 0951, 0963, 0913, 0938, 0961, 0914, 
                0939, 0968, 0817, 0927, 0955, 0977, 0905, 0935, 0956, 0978, 0906, 0936, 0965, 0979, 
                0915, 0937, 0966, 0994, 0916, 0945, 0967, 0995, 0917, 0953, 0973, 0996, 0926, 0954, 
                0975, 0997, 0922, 0933, 0944, 0923, 0934, 0973, 0924, 0940, 0974, 0925, 0941, 0931, 0942, 0932, 0943 };

            string last6 = rnd.Next(0, 10000000).ToString("D7");
            return $"0{prefix[rnd.Next(0,prefix.Length)]}{last6}";
        }

        private static void InitLists()
        {
            using (var reader = new StreamReader("babynames-clean.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                firstnames = csv.GetRecords<FirstName>().ToList();
                totalFirstnames = firstnames.Count();
            }

            using (var reader = new StreamReader("surnames_freq_ge_100.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                lastnames = csv.GetRecords<LastName>().ToList();
                totalLastnames = lastnames.Count();
            }

            using (var reader = new StreamReader("refprovince.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                provinces = csv.GetRecords<Province>().ToList();
            }

            using (var reader = new StreamReader("refcitymun.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                cityMuns = csv.GetRecords<CityMun>().ToList();
            }

            using (var reader = new StreamReader("refbrgy.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                barangays = csv.GetRecords<Barangay>().ToList();
                totalBarangay = barangays.Count();
            }
        }

        private static int HumanDelay(int a, int b)
        {            
            return rnd.Next(a, b) * 1000;
        }

        #region Entities

        public class FirstName
        {
            public string Firstname { get; set; }
        }

        public class LastName
        {
            public string Surname { get; set; }
        }


        public class Barangay
        {
            public int id { get; set; }
            public string brgyDesc { get; set; }
            public int citymunCode { get; set; }
            public int provCode { get; set; }
        }

        public class CityMun
        {
            public int citymunCode { get; set; }
            public string citymunDesc { get; set; }
        }

        public class Province
        {
            public int regCode { get; set; }
            public int provCode { get; set; }
            public string provDesc { get; set; }
        }

        #endregion
    }



}
