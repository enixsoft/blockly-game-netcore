using BlocklyGame.Models.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlocklyGame.Models
{
    public class GameDataModel
    {
        public string category { get; set; }

        public string level { get; set; }

        public string xmlToolbox { get; set; }

        public string xmlStartBlocks { get; set; } 

        public Dictionary<string, string> savedGame { get; set; }

        public string jsonTasks { get; set; }

        public string jsonModals { get; set; }        

        public string jsonRatings { get; set; } 
    }
}
