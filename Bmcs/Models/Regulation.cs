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
    public class Regulation
    {
        [Display(Name = "規定試合数")]
        public int RegulationGames { get; set; }

        [Display(Name = "規定投球回")]
        public int RegulationInnings { get; set; }

        [Display(Name = "規定打席")]
        public int RegulationAtBatting { get; set; }
    }
}
