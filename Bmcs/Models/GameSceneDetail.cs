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
    public class GameSceneDetail: DataModelBase
    {
        [Key]
        [Display(Name = "試合シーン詳細ID")]
        public int GameSceneDetailID { get; set; }

        [Display(Name = "試合シーンID")]
        public int? GameSceneID { get; set; }
        
        [Display(Name = "試合ID")]
        public int? GameID { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Display(Name = "メンバーID")]
        public int? MemberID { get; set; }

        [Display(Name = "シーン結果区分")]
        public SceneResultClass? SceneResultClass { get; set; }

        [Display(Name = "結果")]
        public DetailResultClass? DetailResultClass { get; set; }

        public Game Game { get; set; }

        public GameScene GameScene { get; set; }

        public Team Team { get; set; }

        public Member Member { get; set; }
    }
}
