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

        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "メールアドレスの書式で入力してください。")]
        [Display(Name = "メールアドレス")]
        public string TeamEmailAddress { get; set; }

        //※以前は「メッセージ」で、チーム情報の「メッセージを送る」ボタンの近くに出ると、届いたメッセージのように読めた
        [DataType(DataType.MultilineText)]
        [Display(Name = "チーム紹介")]
        public string MessageDetail { get; set; }

        /// <summary>
        /// 最終ログイン日時
        /// ※所属ユーザの誰かがログインした日時。チーム単位で使用状況を判別するために保持する。
        ///   （所属ユーザを1人ずつ調べずに、未使用チームを抽出できるようにするため）
        /// </summary>
        [Display(Name = "最終ログイン日時")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime? LastLoginDatetime { get; set; }

        [DefaultValue(true)]
        [Display(Name = "公開フラグ")]
        public bool PublicFLG { get; set; }

        /// <summary>
        /// 選手名を背番号で表示するか（公開範囲。issues.md P-12）
        /// ※true の場合、他チーム・未ログインの人には、選手名の代わりに「背番号10」のように表示する（自チームと管理者には氏名で表示）。
        /// 　既存のチームは false（氏名を公開する。この列を追加する前と同じ見え方）
        /// </summary>
        [DefaultValue(false)]
        [Display(Name = "選手名を背番号で表示する")]
        public bool MemberNameHiddenFLG { get; set; }

        /// <summary>
        /// 代表者名を公開しないか（公開範囲。issues.md P-12）
        /// ※true の場合、他チーム・未ログインの人には、チーム情報の代表者名を表示しない
        /// </summary>
        [DefaultValue(false)]
        [Display(Name = "代表者名を公開しない")]
        public bool RepresentativeNameHiddenFLG { get; set; }

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
