using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlocklyGame.Helpers
{
    public class ApplicationSettings
    {
        public bool SeederEnabled { get; set; }

        public string GOOGLE_RECAPTCHA_KEY { get; set; }

        public string GOOGLE_RECAPTCHA_SECRET { get; set; }
    }
}
