using System;
using System.Collections.Generic;
using System.Linq;
using Bmcs.Enum;
using Bmcs.Models;

namespace Bmcs.Data
{
    /// <summary>
    /// アップデート情報（/Updates に表示する）
    ///
    /// 【追加のしかた】
    /// ・利用者から見える変更（新機能・画面や操作の改善・規約の改定・URLの変更など）をデプロイするときに、同じコミットでリストの先頭に追加する
    /// ・利用者の言葉で書く（画面名・ボタン名は画面の表記に合わせる。内部の仕組みや技術用語は書かない）
    /// ・運営側で見つけて直した不具合、内部の改修（基盤の更新・リファクタリング）は書かない
    /// ・同じ時期の細かい変更は1件にまとめる
    /// </summary>
    public static class UpdateHistoryList
    {
        /// <summary>
        /// 新しい順
        /// </summary>
        public static IReadOnlyList<UpdateHistory> All { get; } = new List<UpdateHistory>
        {
            new UpdateHistory
            {
                Date = new DateTime(2026, 9, 26),
                UpdateHistoryClass = UpdateHistoryClass.NewFeature,
                Title = "メンバー（選手）をまとめて登録できるようにしました",
                DetailList =
                {
                    "「メンバー追加」の画面で、1行に1人ずつ入力して、チーム全員を一度に登録できるようになりました。これまでは1人登録するたびにメンバー一覧へ戻っていました。",
                    "入力が必要なのは名前だけです。背番号・区分・投・打・ポジションは任意で、あとから変更できます。",
                    "「＋5人分の行を追加」で入力する行を増やせます（1回に50人まで）。",
                    "登録が終わると、登録した人数と、次にやること（試合の登録）をご案内します。",
                    "背番号は数字（3桁まで）で入力してください。全角で入力した数字は半角に直して登録します。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2026, 9, 26),
                UpdateHistoryClass = UpdateHistoryClass.NewFeature,
                Title = "「アップデート情報」のページを追加しました",
                DetailList =
                {
                    "いつ、どんな機能が追加・改善されたかを、このページでご確認いただけます。",
                    "最新のアップデートは、トップページにも表示されます。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2026, 9, 25),
                UpdateHistoryClass = UpdateHistoryClass.Notice,
                Title = "サイトのアドレスを「bmcs.app」に変更しました",
                DetailList =
                {
                    "新しいアドレスは https://bmcs.app です。",
                    "以前のアドレス（bmcs.azurewebsites.net）を開いた場合も、自動で新しいアドレスへ移動します。ブックマークはそのままでもご利用いただけますが、新しいアドレスへの登録し直しをおすすめします。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2026, 9, 25),
                UpdateHistoryClass = UpdateHistoryClass.Improvement,
                Title = "公開しているチームや試合結果を、検索で見つけやすくしました",
                DetailList =
                {
                    "公開チームのチーム情報・試合結果・成績のページが、チーム名や対戦相手、試合の日付で検索結果に表示されやすくなりました。",
                    "チーム情報・試合・成績の画面の上部に「チーム一覧 / チーム名 / 試合一覧」のように現在の位置を表示し、ひとつ上の画面へ戻りやすくしました。",
                    "非公開のチームや、メンバー（選手）の個別のページは、検索結果に表示されません。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2026, 9, 25),
                UpdateHistoryClass = UpdateHistoryClass.Notice,
                Title = "プライバシーポリシーを改定しました",
                DetailList =
                {
                    "サービス改善のために利用しているアクセス解析ツール（Microsoft Clarity・Google アナリティクス）について、取得する情報と、利用を停止する方法を記載しました。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2026, 9, 24),
                UpdateHistoryClass = UpdateHistoryClass.Improvement,
                Title = "全画面のデザインを新しくしました",
                DetailList =
                {
                    "スマートフォンでも見やすく、押しやすい画面にしました。",
                    "トップページ：はじめての方向けに、Bmcs でできることと「サンプルチームで体験する」を分かりやすくしました。",
                    "新規登録：チーム作成では必須の項目だけを先に表示し、チーム略名はチーム名から自動で入力するようにしました。ユーザ登録からチーム作成、選手の登録まで順番に進めます。",
                    "打順設定：チームのはじめての試合では、登録済みの選手を背番号順に自動で割り当てるようにしました。必要なところだけ変更してください。",
                    "スコア入力：「試合終了」を押したときに確認を表示し、3アウトになるときには案内を表示するようにしました。",
                    "試合結果：「確定する」を押すまで成績に集計されないことを、画面に表示するようにしました。成績の画面でも、確定前の試合がある場合にお知らせします。",
                    "公開チーム一覧：チームをカードで表示し、各チームの試合・成績を開きやすくしました。",
                    "チーム情報変更：チームIDを確認できるようにしました（メンバーにチームへ参加してもらうときに使います）。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2026, 9, 22),
                UpdateHistoryClass = UpdateHistoryClass.Improvement,
                Title = "システムの更新時にログアウトされないようにしました",
                DetailList =
                {
                    "これまでは、サービスの更新作業のたびにログアウトされ、スコア入力の途中でも再ログインが必要でした。今後は更新作業があってもログインしたまま使えます。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2026, 9, 21),
                UpdateHistoryClass = UpdateHistoryClass.NewFeature,
                Title = "ユーザID・パスワードを忘れたときに、ご自身で再設定できるようにしました",
                DetailList =
                {
                    "ログイン画面の「ユーザIDをお忘れの場合」「パスワードをお忘れの場合」から、登録したメールアドレスで確認・再設定できます。",
                    "メールアドレスを登録していない場合も、所属チームのチームIDとチームパスワードで復旧できます。",
                    "チームパスワードが分からない場合は、チームのメールアドレスで再設定できます。",
                    "これにあわせて、新規登録ではメールアドレスの入力が必須になりました。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2026, 9, 21),
                UpdateHistoryClass = UpdateHistoryClass.NewFeature,
                Title = "アンケート機能を追加しました",
                DetailList =
                {
                    "今後の改善のため、選択式のアンケート（1分程度）にご協力をお願いすることがあります。回答していないアンケートがあるときは、トップページに案内が表示されます。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2026, 6, 2),
                UpdateHistoryClass = UpdateHistoryClass.Improvement,
                Title = "成績の表を横にスクロールしても、選手名が見えたままになるようにしました",
                DetailList =
                {
                    "成績や試合結果の表は項目が多く横にスクロールします。スクロールしても左端の選手名の列が固定され、どの選手の成績か分かるようになりました。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2024, 11, 1),
                UpdateHistoryClass = UpdateHistoryClass.Improvement,
                Title = "試合一覧を新しい試合から表示するようにしました",
                DetailList =
                {
                    "試合一覧を日付の新しい順に並べ、最近の試合をすぐに開けるようにしました。",
                },
            },
            new UpdateHistory
            {
                Date = new DateTime(2021, 11, 1),
                IsMonthOnly = true,
                UpdateHistoryClass = UpdateHistoryClass.Notice,
                Title = "Bmcs の提供を開始しました",
                DetailList =
                {
                    "野球・ソフトボールのスコアを入力すると、チーム成績と個人成績（打率・防御率など）を自動で集計します。",
                    "スコアは1打席ごとに入力する方法と、試合結果だけを入力する方法から選べます。タイブレークなど、草野球・ソフトボールのルールにも対応しています。",
                    "チームやメンバーの管理、打順設定、イニングごとの経過の表示、他チームとのメッセージのやり取りができます。",
                    "チームの成績を公開すると、他のチームの成績と見比べることができます。",
                    "無料で、ブラウザだけで利用できます。",
                },
            },
        };

        /// <summary>
        /// 最新の数件（トップページ用）
        /// </summary>
        public static IEnumerable<UpdateHistory> Latest(int count)
        {
            return All.OrderByDescending(r => r.Date).Take(count);
        }
    }
}
