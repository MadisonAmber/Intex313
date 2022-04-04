using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Intex313.Models
{
    public class Accident
    {

        [Key]
        [Required]
        public int AccidentID { get; set; }

    }
}
