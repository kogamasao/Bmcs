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
    public class Member
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MemberID { get; set; }
        public string TeamID { get; set; }
        public string MemberName { get; set; }
        public MemberClass? MemberClass { get; set; }
        public BatClass? BatClass { get; set; }
        public ThrowClass? ThrowClass { get; set; }
        public PositionClass? PositionClass { get; set; }
        public string UniformNumber { get; set; }
        public string Remarks { get; set; }
        [DefaultValue(false)]
        public bool DeleteFLG { get; set; }
        public DateTime? EntryDatetime { get; set; }
        public DateTime? UpdateDatetime { get; set; }

        public Team Team { get; set; }
    }
}
