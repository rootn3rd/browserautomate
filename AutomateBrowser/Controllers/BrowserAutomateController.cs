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
        public static string ChromeDatabase = @"C:\Users\PANKAJ\AppData\Local\Google\Chrome\User Data\Default\History";

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

                    var query = @"DELETE FROM moz_places";

                    using (var connection = new SqliteConnection($"Data Source={Browsers.FireFoxDatabase}"))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText = query;

                        command.ExecuteNonQuery();
                    }
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

                    //HACK- need to copy
                    var tempFile = @"C:\Users\PANKAJ\AppData\Local\Temp\chrome-history";
                    if (!System.IO.File.Exists(tempFile))
                    {
                        System.IO.File.Delete(tempFile);
                    }
                    System.IO.File.Copy(Browsers.ChromeDatabase, tempFile, true);

                    var query = "select url from urls order by last_visit_time desc LIMIT 1";

                    var chromeResults = f_GetVisit(tempFile, query, f_Transfomer);

                    return chromeResults.FirstOrDefault() ?? "";

                case Browsers.Firefox:

                    Func<SqliteDataReader, string> ffxformer = (t) => t["url"].ToString();

                    var fxquery = @"
                                SELECT datetime(moz_historyvisits.visit_date/1000000,'unixepoch') AS visittime, moz_places.url AS url
                                FROM moz_places, moz_historyvisits 
                                WHERE moz_places.id = moz_historyvisits.place_id
                                ORDER BY visittime DESC
                                LIMIT 1
                        ";

                    var firefoxResults = f_GetVisit(Browsers.FireFoxDatabase, fxquery, f_Transfomer);
                    return firefoxResults.FirstOrDefault() ?? "";

                default:
                    break;
            }
            return "";


            IEnumerable<string> f_GetVisit(string source, string query, Func<SqliteDataReader, string> transformer)
            {
                using var connection = new SqliteConnection($"Data Source={source}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = query;

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return transformer(reader);
                }

            }

            string f_Transfomer(SqliteDataReader r)
            {
                return r["url"].ToString();
            }
        }

        record Visit(DateTime timeVisited, string url);
    }
}
