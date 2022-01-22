using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutomateBrowser.Controllers
{
    static class Browsers
    {
        public const string Chrome = "chrome";
        public const string Firefox = "firefox";

        public static string ChromeLocation = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
        public static string FirefoxLocation = "C:\\Program Files\\Mozilla Firefox\\firefox.exe";

        public static string FireFoxDatabase = @"C:\Users\PANKAJ\AppData\Roaming\Mozilla\Firefox\Profiles\tc3timr4.default-release\places.sqlite";
    }
    [ApiController]
    public class BrowserAutomate : ControllerBase
    {
        public BrowserAutomate()
        {
        }

        [HttpGet]
        [Route("start")]
        public string Start(string browser, string url)
        {
            switch (browser)
            {
                case Browsers.Firefox:
                    Process.Start(Browsers.FirefoxLocation, url);
                    break;
                case Browsers.Chrome:
                    Process.Start(Browsers.ChromeLocation, url);
                    break;
                default:
                    return "Invalid input";
            }
            return $"Success";

        }

        [HttpGet]
        [Route("stop")]
        public bool Stop(string broswer)
        {
            switch (broswer)
            {
                case Browsers.Firefox:
                case Browsers.Chrome:
                    var processes = Process.GetProcessesByName(broswer);
                    processes.ToList().ForEach(p => p.Kill());
                    break;
                default:
                    return false;
            }
            return true;
        }

        [HttpGet]
        [Route("cleanup")]
        public async Task<bool> Cleanup(string browser)
        {
            switch (browser)
            {
                case Browsers.Firefox:
                    var processes = Process.GetProcessesByName(browser);
                    processes.ToList().ForEach(p => p.Kill());
                    await Task.Delay(1000);

                    Directory.Delete(@"C:\Users\PANKAJ\AppData\Roaming\Mozilla\Firefox\Profiles", true);

                    break;
                default:
                    break;
            }
            return true;
        }

        [HttpGet]
        [Route("geturl")]
        public string GetUrl(string browser)
        {
            switch (browser)
            {
                case Browsers.Chrome:
                   
                    break;

                case Browsers.Firefox:

                    var visited = new List<Visit>();
                    using (var connection = new SqliteConnection($"Data Source={Browsers.FireFoxDatabase}"))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText =
                                            @"
                                SELECT datetime(moz_historyvisits.visit_date/1000000,'unixepoch') AS visittime, moz_places.url AS url
                                FROM moz_places, moz_historyvisits 
                                WHERE moz_places.id = moz_historyvisits.place_id
                                ORDER BY visittime DESC
                                LIMIT 1
                        ";

                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            visited.Add(new Visit((Convert.ToDateTime(reader["visittime"])), reader["url"].ToString()));
                        }
                    }

                    var lastVisited = visited.OrderByDescending(c => c.timeVisited).FirstOrDefault();
                    return lastVisited != null ? lastVisited.url : "";
                default:
                    break;
            }
            return "";
        }

        record Visit (DateTime timeVisited, string url);
    }
}
