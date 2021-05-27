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

namespace VerapSurveyChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("VerapSurveyChecker by Gabs");
            Console.WriteLine("==========================");
            Console.Write("How many votes do you want to cast? ");
            int voteCast = int.Parse(Console.ReadLine());
            InitLists();
            MainAsync("https://pilipinas2022.ph/", voteCast).Wait();
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

        public static async Task MainAsync(string pageUrl, int voteCast = 10)
        {            
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                Headless = false,
                Args = new string[] { "--disable-web-security", "--disable-features=site-per-process" } // "--disable-features=IsolateOrigins,site-per-process", 
            });
           
            for(int i = 0; i < voteCast; i++)
            {
                try
                {
                    Page page = await browser.NewPageAsync();
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
                    await page.Keyboard.TypeAsync(GetRandomName());

                    var address = GetRandomAddress();
                    await page.SelectAsync("#Province", selectedProvince);

                    await page.FocusAsync("#Address");
                    await page.Keyboard.TypeAsync(address);

                    await page.FocusAsync("#Mobile");
                    await page.Keyboard.TypeAsync(GetRandomPhoneNum());

                    await page.ClickAsync("input[name='vote']", new ClickOptions { Delay = HumanDelay(1, 2) });

                    await page.WaitForSelectorAsync("#newbtnsaraduterte");
                    //await page.WaitForNavigationAsync(new NavigationOptions
                    //{
                    //    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                    //});
                    Thread.Sleep(50);
                    await page.CloseAsync();
                }
                catch { }
            }

            Console.WriteLine("Casting of inorganic vote done! Haha!");
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
            do
            {
                var idx = rnd.Next(0, totalBarangay);
                var brgy = barangays[idx];
                var cm = cityMuns.FirstOrDefault(x => x.citymunCode == brgy.citymunCode);
                var prov = provinces.FirstOrDefault(x => x.provCode == brgy.provCode);
                selectedProvince = prov.provDesc;
                address = $"{brgy.brgyDesc}, {cm.citymunDesc}, {prov.provDesc}";
            } while (string.IsNullOrWhiteSpace(address));

            return address;
        }

        public static string GetRandomPhoneNum()
        {
            int[] prefix = { 904, 905, 906, 907, 908, 909, 910, 911, 912, 913, 914, 915, 
                916, 917, 918, 919, 920, 921, 922, 923, 924, 925, 926, 927, 928, 929, 930, 
                931, 932, 933, 934, 935, 936, 937, 938, 939, 941, 942, 943, 944, 945, 946, 
                947, 948, 949, 950, 955, 956, 961, 965, 966, 967, 970, 973, 975, 976, 977, 
                978, 979, 981, 989, 994, 995, 997, 998, 999 };

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
            // "id","psgcCode","provDesc","regCode","provCode"
            public int provCode { get; set; }
            public string provDesc { get; set; }
        }

        #endregion
    }



}
