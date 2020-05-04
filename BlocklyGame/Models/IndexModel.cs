using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlocklyGame.Models
{
    public class IndexModel
    {
        public string CsrfToken { get; set; }

        public string Title { get; set; } = "Title";

        public string User { get; set; } = "null";

        public string Errors { get; set; } = "{}";

        public string Old { get; set; } = "{}";

        public string Lang { get; set; } = "{}";

        public string RecaptchaKey { get; set; } = "null";

        public string InGameProgress { get; set; } = "[]";

        public string GameData { get; set; } = "[]";

        public Dictionary<string, string> Cookies { get; set; } = new Dictionary<string, string>();   
    }
}
