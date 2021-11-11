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
    public class Member : DataModelBase
    {
        [Key]
        [Display(Name = "メンバーID")]
        public int MemberID { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [StringLength(3)]
        [Display(Name = "背番号")]
        public string UniformNumber { get; set; }


        [NotMapped]
        [Display(Name = "背番号")]
        public string OrderUniformNumber
        {
            get
            {
                return Convert.ToInt32(string.IsNullOrEmpty(UniformNumber) ? "0" : UniformNumber).ToString("000");
            }
        }

        [Required(ErrorMessage = "{0}は必須です。")]
        [StringLength(50, ErrorMessage = "{0}は50桁以内で入力してください。")]
        [Display(Name = "名前")]
        public string MemberName { get; set; }

        [Display(Name = "メンバー区分")]
        public MemberClass? MemberClass { get; set; }

        [NotMapped]
        [Display(Name = "メンバー区分")]
        public string MemberClassName
        {
            get
            {
                return MemberClass.GetEnumName();
            }
        }

        [Display(Name = "投")]
        public ThrowClass? ThrowClass { get; set; }

        [NotMapped]
        [Display(Name = "投")]
        public string ThrowClassName
        {
            get
            {
                return ThrowClass.GetEnumName();
            }
        }

        [Display(Name = "打")]
        public BatClass? BatClass { get; set; }

        [NotMapped]
        [Display(Name = "打")]
        public string BatClassName
        {
            get
            {
                return BatClass.GetEnumName();
            }
        }

        [Display(Name = "ポジション")]
        public PositionGroupClass? PositionGroupClass { get; set; }

        [NotMapped]
        [Display(Name = "ポジション")]
        public string PositionGroupClassName
        {
            get
            {
                return PositionGroupClass.GetEnumName();
            }
        }

        [DataType(DataType.MultilineText)]
        [Display(Name = "メッセージ")]
        public string MessageDetail { get; set; }

        [DefaultValue(false)]
        [Display(Name = "システムデータフラグ")]
        public bool SystemDataFLG { get; set; }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        [NotMapped]
        [Display(Name = "選手名")]
        public string UniformNumberMemberName
        {
            get
            {
                return UniformNumber + " " + MemberName;
            }
        }

        public Team Team { get; set; }

        public ICollection<Order> Orders { get; set; }

        [InverseProperty(nameof(GameScene.PitcherMember))]
        public ICollection<GameScene> PitcherGameScenes { get; set; }

        [InverseProperty(nameof(GameScene.BatterMember))]
        public ICollection<GameScene> BatterGameScenes { get; set; }

        public ICollection<GameSceneDetail> GameSceneDetails { get; set; }

        public ICollection<GameSceneRunner> GameSceneRunners { get; set; }

        public ICollection<GameScorePitcher> GameScorePitchers { get; set; }

        public ICollection<GameScoreFielder> GameScoreFielders { get; set; }
    }
}
