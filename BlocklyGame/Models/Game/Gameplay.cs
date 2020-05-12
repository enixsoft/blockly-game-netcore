using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlocklyGame.Models.Game
{
    public class Gameplay : BaseEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public int Category { get; set; }

        public int Level { get; set; }

        [JsonPropertyNameAttribute("level_start")]
        public string LevelStart { get; set; }

        public int Task { get; set; }

        [JsonPropertyNameAttribute("task_start")]
        public string TaskStart { get; set; }

        [JsonPropertyNameAttribute("task_end")]
        public string TaskEnd { get; set; }

        [JsonPropertyNameAttribute("task_elapsed_time")]
        public int TaskElapsedTime { get; set; }

        public int Rating { get; set; }

        public string Code { get; set; }

        public string Result { get; set; }
    }
}
