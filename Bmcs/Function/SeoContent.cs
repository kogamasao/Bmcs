using Bmcs.Enum;
using Bmcs.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bmcs.Function
{
    /// <summary>
    /// 検索エンジン向けのタイトル・説明文・構造化データ（JSON-LD）の組み立て
    /// ※同じチーム・試合を複数の画面（試合結果・イニング詳細など）で表示するため、文言をここにまとめる
    /// </summary>
    public static class SeoContent
    {
        /// <summary>
        /// 説明文の上限（検索結果で省略されずに表示されるのは、日本語でおおむね120文字程度）
        /// </summary>
        private const int DescriptionMaxLength = 120;

        /// <summary>
        /// サービス全体の説明文（トップと、説明文を設定していない画面で使う）
        /// ※検索される語（草野球・ソフトボール・スコア・成績・無料）を自然に含める
        /// </summary>
        public const string SiteDescription = "草野球・ソフトボールのスコアを入力すると、チーム成績と個人成績（打率・防御率など）を自動で集計します。他チームの成績の閲覧やメッセージのやり取りもできます。無料・ブラウザだけで使えます。";

        /// <summary>
        /// チームの見出し（例：東京ベアーズ（東京都・社会人・軟式））
        /// </summary>
        public static string TeamTitle(Team team)
        {
            var attributeList = TeamAttributeList(team);

            return attributeList.Count > 0
                   ? team.TeamName + "（" + string.Join("・", attributeList) + "）"
                   : team.TeamName;
        }

        /// <summary>
        /// チーム情報の説明文
        /// </summary>
        public static string TeamDescription(Team team)
        {
            var description = TeamTitle(team) + "のチーム情報と、試合結果・成績。";

            if (!string.IsNullOrWhiteSpace(team.MessageDetail))
            {
                description += OneLine(team.MessageDetail);
            }

            return Truncate(description);
        }

        /// <summary>
        /// 試合の見出し（例：2026/09/20 東京ベアーズ vs 横浜ドルフィンズ）
        /// </summary>
        public static string GameTitle(Game game)
        {
            return game.GameDateFormat + " " + game.Team.TeamName + " vs " + OpponentName(game);
        }

        /// <summary>
        /// 試合結果の説明文
        /// </summary>
        public static string GameDescription(Game game)
        {
            var attributeList = new List<string>();

            if (game.GameClass != null)
            {
                attributeList.Add(game.GameClassName);
            }

            if (!string.IsNullOrWhiteSpace(game.StadiumName))
            {
                attributeList.Add(game.StadiumName.Trim());
            }

            var description = game.GameDate.ToString("yyyy年M月d日") + "の" + game.Team.TeamName + " 対 " + OpponentName(game)
                              + (attributeList.Count > 0 ? "（" + string.Join("・", attributeList) + "）" : string.Empty)
                              + "の試合結果。";

            if (game.Score != null && game.OpponentTeamScore != null)
            {
                description += game.Score + "対" + game.OpponentTeamScore + WinLoseText(game.WinLoseClass) + "。";
            }

            description += "スコアボードと投手・打撃成績を掲載しています。";

            return Truncate(description);
        }

        /// <summary>
        /// 構造化データ：チーム（SportsTeam）
        /// </summary>
        public static Dictionary<string, object> SportsTeamData(Team team, string url)
        {
            var data = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "SportsTeam",
                ["name"] = team.TeamName,
                ["sport"] = SportName(team.UseBallClass),
                ["url"] = url,
            };

            if (!string.IsNullOrWhiteSpace(team.TeamAbbreviation))
            {
                data["alternateName"] = team.TeamAbbreviation.Trim();
            }

            if (!string.IsNullOrWhiteSpace(team.MessageDetail))
            {
                data["description"] = Truncate(OneLine(team.MessageDetail));
            }

            if (!string.IsNullOrWhiteSpace(team.ActivityBase))
            {
                data["location"] = new Dictionary<string, object>
                {
                    ["@type"] = "Place",
                    ["name"] = team.ActivityBase.Trim(),
                };
            }

            return data;
        }

        /// <summary>
        /// 構造化データ：試合（SportsEvent）
        /// ※先攻をアウェイ、後攻をホームとする（野球の慣例）。先攻後攻が未登録の場合は、区別せず competitor とする
        /// </summary>
        public static Dictionary<string, object> SportsEventData(Game game, string url, string teamUrl)
        {
            var team = new Dictionary<string, object>
            {
                ["@type"] = "SportsTeam",
                ["name"] = game.Team.TeamName,
                ["url"] = teamUrl,
            };

            var opponentTeam = new Dictionary<string, object>
            {
                ["@type"] = "SportsTeam",
                ["name"] = OpponentName(game),
            };

            var data = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "SportsEvent",
                ["name"] = game.Team.TeamName + " 対 " + OpponentName(game),
                ["sport"] = SportName(game.Team.UseBallClass),
                ["startDate"] = game.GameDate.ToString("yyyy-MM-dd"),
                ["url"] = url,
                ["description"] = GameDescription(game),
            };

            if (game.BatFirstBatSecondClass == BatFirstBatSecondClass.First)
            {
                data["awayTeam"] = team;
                data["homeTeam"] = opponentTeam;
            }
            else if (game.BatFirstBatSecondClass == BatFirstBatSecondClass.Second)
            {
                data["homeTeam"] = team;
                data["awayTeam"] = opponentTeam;
            }
            else
            {
                data["competitor"] = new List<object> { team, opponentTeam };
            }

            if (!string.IsNullOrWhiteSpace(game.StadiumName))
            {
                data["location"] = new Dictionary<string, object>
                {
                    ["@type"] = "Place",
                    ["name"] = game.StadiumName.Trim(),
                };
            }

            return data;
        }

        /// <summary>
        /// 構造化データ：サイト全体（トップのみ）
        /// </summary>
        public static List<object> SiteData(string baseUrl, string description)
        {
            return new List<object>
            {
                new Dictionary<string, object>
                {
                    ["@context"] = "https://schema.org",
                    ["@type"] = "WebSite",
                    ["name"] = "Bmcs",
                    ["alternateName"] = "草野球・ソフトボールのスコア管理 Bmcs",
                    ["url"] = baseUrl + "/",
                    ["inLanguage"] = "ja",
                },
                new Dictionary<string, object>
                {
                    ["@context"] = "https://schema.org",
                    ["@type"] = "WebApplication",
                    ["name"] = "Bmcs",
                    ["url"] = baseUrl + "/",
                    ["description"] = description,
                    ["applicationCategory"] = "SportsApplication",
                    ["operatingSystem"] = "Web",
                    ["browserRequirements"] = "ブラウザのみで利用できます（アプリのインストールは不要）",
                    ["inLanguage"] = "ja",
                    ["offers"] = new Dictionary<string, object>
                    {
                        ["@type"] = "Offer",
                        ["price"] = "0",
                        ["priceCurrency"] = "JPY",
                    },
                },
            };
        }

        /// <summary>
        /// 競技名（schema.org の sport）
        /// </summary>
        private static string SportName(UseBallClass? useBallClass)
        {
            return useBallClass == UseBallClass.SoftBall ? "Softball" : "Baseball";
        }

        /// <summary>
        /// チームの属性（活動拠点・カテゴリ・使用球）
        /// </summary>
        private static List<string> TeamAttributeList(Team team)
        {
            var attributeList = new List<string>();

            if (!string.IsNullOrWhiteSpace(team.ActivityBase))
            {
                attributeList.Add(OneLine(team.ActivityBase));
            }

            //「全て」は絞り込み用の値のため表示しない
            if (team.TeamCategoryClass != null && team.TeamCategoryClass != TeamCategoryClass.All)
            {
                attributeList.Add(team.TeamCategoryClassName);
            }

            if (team.UseBallClass != null && team.UseBallClass != UseBallClass.All)
            {
                attributeList.Add(team.UseBallClassName);
            }

            return attributeList.Where(r => !string.IsNullOrEmpty(r)).ToList();
        }

        private static string OpponentName(Game game)
        {
            return string.IsNullOrWhiteSpace(game.OpponentTeamName) ? "相手チーム" : game.OpponentTeamName.Trim();
        }

        private static string WinLoseText(WinLoseClass? winLoseClass)
        {
            switch (winLoseClass)
            {
                case WinLoseClass.Win:
                    return "で勝利";
                case WinLoseClass.Lose:
                    return "で敗戦";
                case WinLoseClass.Draw:
                    return "で引き分け";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 改行・連続する空白を1つの空白にする（説明文は1行で表示されるため）
        /// </summary>
        private static string OneLine(string text)
        {
            return Regex.Replace(text ?? string.Empty, @"\s+", " ").Trim();
        }

        /// <summary>
        /// 説明文の長さに切り詰める
        /// </summary>
        public static string Truncate(string text)
        {
            if (text.Length <= DescriptionMaxLength)
            {
                return text;
            }

            var length = DescriptionMaxLength - 1;

            //絵文字などのサロゲートペアの途中で切らない
            if (char.IsHighSurrogate(text[length - 1]))
            {
                length--;
            }

            return text.Substring(0, length) + "…";
        }
    }
}
