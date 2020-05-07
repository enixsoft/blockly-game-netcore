using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BlocklyGame.Models.Game
{
    public class Progress : BaseEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public int Category { get; set; }
        public int Level { get; set; }
        public int Percentage { get; set; }
    }
}
