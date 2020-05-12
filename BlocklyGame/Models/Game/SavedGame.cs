using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlocklyGame.Models.Game
{
    public class SavedGame : BaseEntity
    {
        public int Id { get; set; }  
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public int Category { get; set; }
        public int Level { get; set; }
        public int Progress { get; set; }
        [JsonPropertyNameAttribute("save")]
        public string Json { get; set; }
    }
}
