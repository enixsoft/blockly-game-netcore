using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BlocklyGame.Models.Game
{
    public class BaseEntity
    {
        public DateTime CreatedDate { get; set; }
       
        public DateTime UpdatedDate { get; set; }   
    }
}
