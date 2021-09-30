using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Enum;

namespace Bmcs.Models
{
    public class TotalingItem
    {
        [Display(Name = "年")]
        public int? Year { get; set; }

        [Display(Name = "試合種別")]
        public GameClass? GameClass { get; set; }
    }
}
