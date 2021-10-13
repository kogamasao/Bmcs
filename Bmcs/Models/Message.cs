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
    public class Message : DataModelBase
    {
        [Key]
        [Display(Name = "メッセージID")]
        public int MessageID { get; set; }

        [Display(Name = "ユーザID")]
        public string UserAccountID { get; set; }
        
        [ForeignKey(nameof(Team)), Column(Order = 0)]
        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [ForeignKey(nameof(PrivateTeam)), Column(Order = 1)]
        [StringLength(50)]
        [Display(Name = "送信先チーム")]
        public string PrivateTeamID { get; set; }

        [Display(Name = "親メッセージID")]
        public int? ParentMessageID { get; set; }

        [Display(Name = "メッセージ区分")]
        public MessageClass? MessageClass { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "メッセージ")]
        public string MessageDetail { get; set; }

        [DefaultValue(true)]
        [Display(Name = "公開フラグ")]
        public bool PublicFLG { get; set; }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        [NotMapped]
        [Display(Name = "返信件数")]
        public int? ReplyCount { get; set; }

        [ForeignKey("TeamID")]
        public virtual Team Team { get; set; }

        [ForeignKey("PrivateTeamID")]
        public virtual Team PrivateTeam { get; set; }

        public UserAccount UserAccount { get; set; }
    }
}
