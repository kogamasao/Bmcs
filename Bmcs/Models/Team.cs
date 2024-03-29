﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Enum;

namespace Bmcs.Models
{
    public class Team : DataModelBase, IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required(ErrorMessage ="{0}は必須です。")]
        [StringLength(50, ErrorMessage ="{0}は50桁以内で入力してください。" )]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [Display(Name = "チーム名")]
        [StringLength(50, ErrorMessage = "{0}は50桁以内で入力してください。")]
        public string TeamName { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [Display(Name = "チーム略名")]
        [StringLength(10, ErrorMessage = "{0}は10桁以内で入力してください。")]
        public string TeamAbbreviation { get; set; }

        [Display(Name = "代表者名")]
        public string RepresentativeName { get; set; }

        [Display(Name = "カテゴリ")]
        public TeamCategoryClass? TeamCategoryClass { get; set; }

        [NotMapped]
        [Display(Name = "カテゴリ")]
        public string TeamCategoryClassName 
        {
            get
            {
                return TeamCategoryClass.GetEnumName();
            }     
        }

        [Display(Name = "使用球")]
        public UseBallClass? UseBallClass { get; set; }

        [NotMapped]
        [Display(Name = "使用球")]
        public string UseBallClassName
        {
            get
            {
                return UseBallClass.GetEnumName();
            }
        }

        [Display(Name = "活動拠点")]
        [StringLength(50)]
        public string ActivityBase { get; set; }

        [Display(Name = "チーム人数")]
        public int? TeamNumber { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string TeamPassword { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "確認用パスワード")]
        public string ConfirmTeamPassword { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "メールアドレスの書式で入力してください。")]
        [Display(Name = "メールアドレス")]
        public string TeamEmailAddress { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "メッセージ")]
        public string MessageDetail { get; set; }

        [DefaultValue(true)]
        [Display(Name = "公開フラグ")]
        public bool PublicFLG { get; set; }

        [DefaultValue(false)]
        [Display(Name = "システムデータフラグ")]
        public bool SystemDataFLG { get; set; }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        [NotMapped]
        [Display(Name = "チーム")]
        public string TeamIDName {
            get
            {
                return TeamID + " " + TeamName;
            }
        }

        public ICollection<Member> Members { get; set; }

        public ICollection<Order> Orders { get; set; }

        public ICollection<Game> Games { get; set; }

        public ICollection<InningScore> InningScores { get; set; }

        public ICollection<GameScene> GameScenes { get; set; }

        public ICollection<GameSceneDetail> GameSceneDetails { get; set; }

        public ICollection<GameSceneRunner> GameSceneRunners { get; set; }

        public ICollection<GameScoreFielder> GameScoreFielders { get; set; }

        public ICollection<GameScorePitcher> GameScorePitchers { get; set; }

        [InverseProperty(nameof(Message.Team))]
        public ICollection<Message> Messages { get; set; }

        [InverseProperty(nameof(Message.PrivateTeam))]
        public ICollection<Message> ReplyMessages { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TeamPassword != ConfirmTeamPassword)
            {
                yield return new ValidationResult(
                    "パスワードが一致していません。",
                    new[] { nameof(TeamPassword), nameof(ConfirmTeamPassword) });
            }
        }
    }
}
