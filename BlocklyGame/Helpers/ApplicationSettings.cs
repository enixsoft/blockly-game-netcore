using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlocklyGame.Helpers
{
    public class ApplicationSettings
    {
        public string AdminPassword { get; set; }

        public string BaseURL { get; set; }

        public bool MySQLDatabase { get; set; }

        public bool SeederEnabled { get; set; }

        public string GOOGLE_RECAPTCHA_KEY { get; set; }

        public string GOOGLE_RECAPTCHA_SECRET { get; set; }

        public Dictionary<string, string> CountryCodeLocalization { get; set; }
    }
}
