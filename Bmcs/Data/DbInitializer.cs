using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Enum;
using Bmcs.Function;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// セッション保存用テーブルを作成する（存在しない場合のみ）
        /// </summary>
        /// <param name="context"></param>
        private static void CreateSessionCacheTable(BmcsContext context)
        {
            context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID('dbo.SessionCache') IS NULL
BEGIN
    CREATE TABLE dbo.SessionCache
    (
        Id                         nvarchar(449)  NOT NULL
      , Value                      varbinary(MAX) NOT NULL
      , ExpiresAtTime              datetimeoffset NOT NULL
      , SlidingExpirationInSeconds bigint         NULL
      , AbsoluteExpiration         datetimeoffset NULL
      , CONSTRAINT PK_SessionCache PRIMARY KEY (Id)
    );

    CREATE NONCLUSTERED INDEX Index_ExpiresAtTime ON dbo.SessionCache (ExpiresAtTime);
END");
        }

        /// <summary>
        /// 最終ログイン日時の列を追加する（存在しない場合のみ）
        /// </summary>
        /// <param name="context"></param>
        private static void AddLastLoginDatetimeColumn(BmcsContext context)
        {
            context.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.UserAccount', 'LastLoginDatetime') IS NULL
    ALTER TABLE dbo.UserAccount ADD LastLoginDatetime datetime2(7) NULL;
IF COL_LENGTH('dbo.Team', 'LastLoginDatetime') IS NULL
    ALTER TABLE dbo.Team ADD LastLoginDatetime datetime2(7) NULL;");
        }

        /// <summary>
        /// 公開範囲（選手名・代表者名）の列を追加する（存在しない場合のみ。issues.md P-12）
        /// ※既存のチームは 0（氏名・代表者名を公開する。列を追加する前と同じ見え方）
        /// </summary>
        /// <param name="context"></param>
        private static void AddTeamPublicRangeColumns(BmcsContext context)
        {
            context.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.Team', 'MemberNameHiddenFLG') IS NULL
    ALTER TABLE dbo.Team ADD MemberNameHiddenFLG bit NOT NULL CONSTRAINT DF_Team_MemberNameHiddenFLG DEFAULT 0;
IF COL_LENGTH('dbo.Team', 'RepresentativeNameHiddenFLG') IS NULL
    ALTER TABLE dbo.Team ADD RepresentativeNameHiddenFLG bit NOT NULL CONSTRAINT DF_Team_RepresentativeNameHiddenFLG DEFAULT 0;");
        }

        public static void Initialize(BmcsContext context)
        {
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            //セッション保存用テーブル
            //※EnsureCreated() はモデルにない表を作らないため、ここで作成する。
            //  無い状態で起動すると、ログインPOSTは302を返すのにセッションが保存されず
            //  ログイン画面へ戻り続ける（500にならないため監視でも気づけない）。
            //  ここで作成しておけば、移行SQLの実行順序に依存しなくなる。
            //  列の定義は Microsoft.Extensions.Caching.SqlServer が要求する形式で固定。
            CreateSessionCacheTable(context);

            //最終ログイン日時
            //※EnsureCreated() は既存テーブルへの列追加を行わないため、ここで追加する。
            //  列が無い状態で起動すると UserAccount / Team の参照が全て失敗し、
            //  ログインすらできなくなる（移行SQLの実行順序に依存させない）。
            AddLastLoginDatetimeColumn(context);

            //公開範囲（選手名・代表者名）
            //※最終ログイン日時と同じく、列が無いと Team の参照が全て失敗するため、起動時に追加する
            AddTeamPublicRangeColumns(context);

            var isUpdate = false;

            //初期データ投入
            if (!context.SystemAdmins.ToList().Any())
            {
                isUpdate = true;

                var systemAdmins = new List<SystemAdmin>();

                foreach (var value in System.Enum.GetValues(typeof(SystemAdminClass)))
                {
                    var messageDetail = string.Empty;

                    if((int)value == (int)SystemAdminClass.TopInformation)
                    {
                        messageDetail = "基本機能使用可能です。ヘルプを作成中";
                    }
                    else if ((int)value == (int)SystemAdminClass.UserAccountCreate)
                    {
                        messageDetail = @"<p>ユーザを登録します（無料）。登録後、新しくチームを作る場合はチーム作成へ進みます。</p><ul class=""help-ul""><li>ユーザID(必須)<br>ログインに使用するIDです。登録後は変更できません。半角英数字がおすすめです。（既に使用されているIDはエラーとなります。）</li><li>ユーザ名(必須)<br>メッセージの投稿者として表示されます。</li><li>パスワード(必須)<br>ログインに使用します。「確認用パスワード」に同じパスワードを入力してください。</li><li>メールアドレス(必須)<br>ユーザIDやパスワードを忘れたときの復旧と、サイト管理者からのご連絡に使用します。</li><li>既にあるチームに参加する<br>チームに誘われた場合は、ここを開き、チームの代表者から聞いたチームIDとチームパスワードを入力します。<br>新しくチームを作る場合は空のままで構いません。登録後にチーム作成へ進みます。</li></ul><p>利用規約とプライバシーポリシーを確認し、2つに同意すると登録できます。</p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.UserAccountEdit)
                    {
                        messageDetail = @"<p>ログインしているユーザの情報を変更します。</p><ul class=""help-ul""><li>ユーザID<br>ログインに使用するIDです。変更はできません。</li><li>ユーザ名(必須)<br>メッセージの投稿者として表示されます。</li><li>パスワード<br>変更する場合のみ入力してください。空のままなら変わりません。<br>入力した場合は、「確認用パスワード」に同じパスワードを入力してください。</li><li>メールアドレス<br>ユーザIDやパスワードを忘れたときの復旧と、サイト管理者からのご連絡に使用します。<br>一度登録したメールアドレスは削除できません（変更はできます）。<br>登録がない場合、復旧はチームIDとチームパスワードによる方法のみ（チームに所属している場合のみ）になるため、登録をおすすめします。</li><li>所属チーム<br>チームを移る・チームに参加する場合は、「別のチームに移る」（所属していない場合は「チームに参加する」）を開き、チームの代表者から聞いたチームIDとチームパスワードを入力します。<br>移ると、今のチームのデータは編集できなくなります。<br>チームIDを空にして保存すると、チームから抜けます。</li></ul><p>※体験用のユーザは、ユーザ情報を変更できません。</p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.TeamIndex)
                    {
                        messageDetail = @"<p>公開しているチームの一覧です。<br>チーム名を押すと、チームの紹介やチーム成績を確認できます。</p><ul class=""help-ul""><li>試合<br>そのチームの試合一覧を表示します。</li><li>成績<br>そのチームの成績ページを表示します。</li><li>メンバー<br>そのチームのメンバー一覧を表示します。</li><li>メッセージを送る（公開チームに所属している場合のみ表示。自チームと体験用のユーザには表示されません）<br>そのチームへのダイレクトメッセージ（非公開）を送る画面を表示します。</li></ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.TeamCreate)
                    {
                        messageDetail = @"<p>チームを作成します。入力が必要なのは、チームID・チーム名・チーム略名・チームパスワード（確認用を含む）です。</p><ul class=""help-ul""><li>チーム名(必須)<br>公開チーム一覧や試合結果に表示されます。</li><li>チーム略名(必須)<br>スコアボードに表示されます。10文字以内で入力します。</li><li>チームID(必須)<br>チームメイトがこのチームに参加するときに入力するIDです。<br><strong>作成後は変更できません。</strong>半角英数字がおすすめです。（既に使用されているIDはエラーとなります。）</li><li>チームパスワード(必須)<br>チームメイトがこのチームに参加するときに入力します。ログインパスワードとは別のものです。<br>確認用の欄に同じパスワードを入力してください。作成後は「チーム情報変更」で変更できます。</li><li>チームを公開する<br>公開すると、成績・試合結果・メンバー・チーム情報（活動拠点・チーム紹介など）を<strong>誰でも（ログインしていない人も）</strong>見られるようになり、他チームとメッセージのやり取りができます。<br>非公開の場合、他チームとのメッセージのやり取りはできません。あとから変更できます。</li><li>公開する範囲<br>公開したときの、他のチームやログインしていない人への見え方を選びます。選手名は「氏名を表示する」か「背番号で表示する」（メンバーのメッセージとイニング詳細の備考も表示しません）、代表者名は「表示する」か「表示しない」を選べます。自チームのユーザには、いつも氏名で表示されます。チーム紹介など、チームで書き込む欄の内容はそのまま表示されます。</li></ul><p>以下は任意の項目です（チーム作成では「くわしい情報を入力する」を開くと表示されます）。メールアドレス以外は、公開チームにした場合に誰でも見られます（代表者名は「公開する範囲」で表示しないこともできます）。</p><ul class=""help-ul""><li>代表者名<br>チームの代表者名を入力します。</li><li>カテゴリ・使用球<br>カテゴリ・使用球ごとに、公開チームの成績を比較できます。</li><li>活動拠点・チーム人数<br>対戦相手を探すときの参考情報になります。</li><li>メールアドレス<br>チームパスワードを忘れたときの再設定と、サイト管理者からのご連絡に使用します。<strong>他チームには表示されません。</strong><br>登録がない場合、チームパスワードを忘れたときはお問い合わせでの対応となるため、登録をおすすめします。</li><li>チーム紹介<br>チームの紹介や募集内容などを入力します。公開チーム一覧に表示されます。</li></ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.TeamEdit)
                    {
                        messageDetail = @"<p>チーム情報を変更します。<br>チームIDは画面の上部で確認できます（変更はできません）。チームメイトがチームに参加するときは、チームIDとチームパスワードを伝えてください。</p><ul class=""help-ul""><li>チーム名(必須)<br>公開チーム一覧や試合結果に表示されます。</li><li>チーム略名(必須)<br>スコアボードに表示されます。10文字以内で入力します。</li><li>チームパスワード<br>変更する場合のみ入力してください。空のままなら変わりません。<br>入力した場合は、確認用の欄に同じパスワードを入力してください。</li><li>チームを公開する<br>公開すると、成績・試合結果・メンバー・チーム情報（活動拠点・チーム紹介など）を<strong>誰でも（ログインしていない人も）</strong>見られるようになり、他チームとメッセージのやり取りができます。<br>非公開の場合、他チームとのメッセージのやり取りはできません。あとから変更できます。</li><li>公開する範囲<br>公開したときの、他のチームやログインしていない人への見え方を選びます。選手名は「氏名を表示する」か「背番号で表示する」（メンバーのメッセージとイニング詳細の備考も表示しません）、代表者名は「表示する」か「表示しない」を選べます。自チームのユーザには、いつも氏名で表示されます。チーム紹介など、チームで書き込む欄の内容はそのまま表示されます。</li></ul><p>以下は任意の項目です（チーム作成では「くわしい情報を入力する」を開くと表示されます）。メールアドレス以外は、公開チームにした場合に誰でも見られます（代表者名は「公開する範囲」で表示しないこともできます）。</p><ul class=""help-ul""><li>代表者名<br>チームの代表者名を入力します。</li><li>カテゴリ・使用球<br>カテゴリ・使用球ごとに、公開チームの成績を比較できます。</li><li>活動拠点・チーム人数<br>対戦相手を探すときの参考情報になります。</li><li>メールアドレス<br>チームパスワードを忘れたときの再設定と、サイト管理者からのご連絡に使用します。<strong>他チームには表示されません。</strong><br>登録がない場合、チームパスワードを忘れたときはお問い合わせでの対応となるため、登録をおすすめします。</li><li>チーム紹介<br>チームの紹介や募集内容などを入力します。公開チーム一覧に表示されます。</li></ul><p>※体験用のチームは、チーム情報を変更できません。</p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.TeamDetails)
                    {
                        messageDetail = @"<p>
                                            登録されているチーム情報、年毎のチーム成績を確認できるページです。
                                            「メッセージを送る」からダイレクト(非公開)メッセージを送る画面に遷移します。
                                        </p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.MyTeamMemberIndex)
                    {
                        messageDetail = @"<p>チームのメンバー一覧を確認することができます。<br />
                                                    メンバーを追加する場合は「メンバー追加」よりメンバー作成ページへ遷移します。
                                            </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                編集<br>
                                                該当メンバーの情報を編集するページへ遷移します。<br>
                                            </li>
                                            <li>
                                                詳細<br>
                                                該当メンバーの詳細情報ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                削除<br>
                                                該当メンバーを削除するページへ遷移します。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.PublicMemberIndex)
                    {
                        messageDetail = @"<p>公開チームのメンバー一覧を確認することができます。<br />
                                            </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                詳細<br>
                                                該当メンバーの詳細情報ページへ遷移します。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.MemberCreate)
                    {
                        messageDetail = @"<p>試合に出る監督や選手を登録します。1行に1人ずつ入力し、チーム全員をまとめて登録できます。登録したメンバーは試合に出場できます。</p><ul class=""help-ul""><li>背番号<br>数字3桁以内で入力します。未定の場合は空のままで構いません。</li><li>名前(必須)<br>入力が必要なのは名前だけです。名前も背番号も空の行は登録されません。</li><li>区分<br>選手・監督などの区分です。はじめは「選手」になっています。監督やコーチでも試合に出場できます。</li><li>投・打・ポジション<br>任意です。登録していないポジションでも出場できます。</li><li>行を追加<br>「＋5人分の行を追加」で入力する行を増やせます。1回に50人まで登録できます。不要な行は右端の「×」で削除できます。</li></ul><p>メッセージ（紹介文など）は、登録後にメンバー一覧の「編集」から入力できます。<br>チームIDで参加したチームメイトは「ユーザ」としてスコア入力ができるようになりますが、選手としては登録されません。</p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.MemberEdit)
                    {
                        messageDetail = @"<p>
                                            チームに所属する監督や選手情報を編集します。
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                背番号<br>
                                                任意の背番号を入力します。<br>
                                            </li>
                                            <li>
                                                メンバー名(必須)<br>
                                                任意のメンバー名を入力します。<br>
                                            </li>
                                            <li>
                                                メンバー区分<br>
                                                メンバーの区分を選択します。<br>
                                                システム内での区別はなく、監督やコーチでも試合に出場は可能です。<br>
                                            </li>
                                            <li>
                                                投<br>
                                                利き投げを選択します。<br>
                                                システム内での区別はありません。<br>
                                            </li>
                                            <li>
                                                打<br>
                                                打席を選択します。<br>
                                                システム内での区別はありません。<br>
                                            </li>
                                            <li>
                                                ポジション<br>
                                                ポジションを選択します。<br>
                                                システム内での区別はなく、登録していないポジション以外でも出場は可能です。<br>
                                            </li>
                                            <li>
                                                メッセージ<br>
                                                自由項目です。<br>
                                                メンバーの紹介文などを入力してみましょう。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.MemberDetails)
                    {
                        messageDetail = @"<p>
                                            登録されているメンバー情報、年毎の投手成績、野手成績を確認できるページです。
                                        </p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.MemberDelete)
                    {
                        messageDetail = @"<p>
                                            登録されているメンバーを削除します。<br />
                                            削除を行うと、メンバーは試合で使用できなくなり、メンバー一覧や成績ページからも表示対象外になります。<br />
                                            ただし過去の試合履歴では表示対象です。削除した選手の成績はチーム成績の合計からは除外されません。<br />
                                            完全に削除したい場合は、お問い合わせください。<br />
                                        </p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.MyTeamGameIndex)
                    {
                        messageDetail = @"<p>チームの過去試合一覧を確認することができます。<br />
                                                    試合を開始する場合は「試合へ」より試合ページへ遷移します。
                                            </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                試合入力(試合途中のみ表示)<br>
                                                試合の入力ページに遷移します。<br>
                                                入力タイプが「試合結果のみ」の場合は表示されません。<br>
                                            </li>
                                            <li>
                                                試合結果編集(試合終了後に表示)<br>
                                                試合結果編集ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                試合結果(確定後に表示)<br>
                                                試合結果ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                試合情報編集<br>
                                                試合の基本情報編集ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                削除<br>
                                                試合を削除するページへ遷移します。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.PublicGameIndex)
                    {
                        messageDetail = @"<p>公開チームの過去試合一覧を確認することができます。
                                            </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                試合結果(確定後に表示)<br>
                                                試合結果ページへ遷移します。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.GameCreate)
                    {
                        messageDetail = @"<p>
                                            試合の基本情報を登録します。
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                日付(必須)<br>
                                                試合の日付を入力します。<br>
                                            </li>
                                            <li>
                                                試合種別<br>
                                                試合種別を選択します。<br>
                                                選択することで試合種別毎に成績を確認することができます。<br>
                                            </li>
                                            <li>
                                                相手チーム名<br>
                                                相手チーム名を入力します。<br>
                                            </li>
                                            <li>
                                                相手チーム略名<br>
                                                相手チーム略名を10桁以内で入力します。スコアボードでのチーム名表示などに使用されます。<br>
                                            </li>
                                            <li>
                                                球場<br>
                                                試合が行われる球場を入力します。<br>
                                                メモ項目であり、システム内制御で使用することはありません。<br>
                                            </li>
                                            <li>
                                                天候<br>
                                                試合当日の天候を入力します。<br>
                                                メモ項目であり、システム内制御で使用することはありません。<br>
                                            </li>
                                            <li>
                                                先攻後攻(必須)<br>
                                                先攻後攻を選択します。<br>
                                                試合プレー入力を1回でも行うと先攻後攻は変更できなくなります。<br>
                                            </li>
                                            <li>
                                                入力方式<br>
                                                入力方式を選択します。<br>
                                                <br>
                                                「試合結果のみ」は、１打席毎の入力は行わず、イニングスコア、各選手の投手結果、野手結果を手入力するタイプです。<br>
                                                チームではなく個人成績のみを管理し、メモのように使用したい場合や、試合中に都度入力できない場合に有効です。<br>
                                                しかし、プレー毎に入力しないためイニング毎の詳細を確認することができず、自動で成績を集計することもありません。<br>
                                                <br>
                                                「プレー毎」(※推奨)は、スコアブックを付けるように１打者毎の結果を試合の流れに沿って入力するタイプです。※１球毎の入力は行いません。<br>
                                                実際の試合を見ながら入力できる場合に有効です。<br>
                                                試合後は自動で結果を集計し、イニング毎の詳細、細かい成績まで確認することができます。<br>
                                                <br>
                                                どちらの入力タイプも相手チームの結果管理は行いません。<br>
                                                「プレー毎」を選択して、試合プレー入力を1回でも行うと入力方式は変更できなくなります。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.GameEdit)
                    {
                        messageDetail = @"<p>試合の基本情報を編集します。</p><ul class=""help-ul""><li>日付(必須)<br>試合の日付を入力します。</li><li>相手チーム名・相手チーム略名<br>相手チーム名は試合一覧や試合結果に、略名はスコアボードに表示されます（略名は10文字以内）。</li><li>球場・天候<br>試合一覧・試合情報に表示されます（公開チームの場合は誰でも見られます）。</li><li>試合種別<br>試合種別ごとに成績を確認できます。</li><li>先攻後攻・入力方式<br><strong>試合を進めると変更できません。</strong>プレー毎は1打席目を登録した時点、試合結果のみは試合結果を確定した時点で固定されます。<br>それ以降は、値だけが表示されます。入力を始めてから切り替える場合は、試合を作り直してください。<br>「プレー毎」は、1打席ずつ試合の流れに沿って入力します。イニングごとの詳細や細かい成績まで自動で集計されます。<br>「試合結果のみ」は、イニングスコアと選手ごとの成績をまとめて入力します。イニングごとの詳細は残りません。<br>どちらの入力方式も、相手チームの成績は管理しません。</li></ul><p>保存すると、試合前はプレー毎なら打順の設定へ、試合結果のみなら試合結果の入力へ進みます。試合中はスコア入力に戻り、それ以外は試合一覧に戻ります。</p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.GameDelete)
                    {
                        messageDetail = @"<p>
                                            試合データを削除します。<br />
                                            削除を行うと、対象試合のスコアは、チーム、各メンバー成績から集計対象外になります。<br />
                                        </p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.OrderBeforeGame)
                    {
                        messageDetail = @"<p>
                                            スターティングオーダーを作成します。<br />
                                            選手、守備位置を選択してください。(前回のスターティングオーダーが初期表示されます。)<br />
                                            あらゆるルールに対応するため、同一選手が複数の打順に登録、別々の選手が同じ守備位置を守る、10人以上出場、守備のみの選手の出場等を可能にしています。<br />
                                            また、試合中に打順に割込、打順から抜ける、打順を１回スキップする、再出場(リエントリー)等も対応しています。<br />
                                            ※公式戦の場合は選手の重複や、守備位置の重複等にご注意ください。<br />
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                削除<br>
                                                該当行の選手を削除します。<br>
                                                削除後は打順が自動で再表示されます。<br>
                                                8人以下としたい場合は選手行を削除してください。<br>
                                            </li>
                                            <li>
                                                オーダー追加<br>
                                                10人以上出場する場合はオーダー追加ボタンより新規選手行が追加されます。<br>
                                            </li>
                                            <li>
                                                守備のみ追加<br>
                                                指名打者制の場合の投手やFPの選手は、守備のみ追加ボタンより選手行を追加して選手を選択します。<br>
                                            </li>                                 
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.OrderDuringGame)
                    {
                        messageDetail = @"<p>
                                            選手を交代します。<br />
                                            交代したい選手を選択して、新しい選手と入れ替えてください。<br />
                                            守備位置を変更したい場合も新しい守備位置を選びなおしてください。<br />
                                            投手交代はプレー入力ページからでも可能ですが、ベンチ選手との交代限定になります。<br />
                                            代打、代走を除く、その他の選手交代はこちらのページで交代処理が必要です。<br />
                                            あらゆるルールに対応するため、同一選手が複数の打順に登録、別々の選手が同じ守備位置を守る、10人以上出場、守備のみの選手の出場等を可能にしています。<br />
                                            また、試合中に打順に割込、打順から抜ける、打順を１回スキップする、再出場(リエントリー)等も対応しています。<br />
                                            ※公式戦の場合は選手の重複や、守備位置の重複等にご注意ください。<br />
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                削除<br>
                                                試合途中に選手を離脱させたい場合は、削除します。<br>
                                                削除後は打順が空き打順になり、他の選手の打順はそのままです。<br>
                                                指名打者を解除し投手が打順に入る場合は、守備のみ欄の投手を削除してください。<br>
                                            </li>
                                            <li>
                                                オーダー追加<br>
                                                スターティングオーダー作成時と異なり、行追加されずに別画面が表示されます。<br>
                                                追加したい打順(小数の指定可能)を入力し、行追加を行ってください。<br>
                                            </li>
                                            <li>
                                                守備のみ追加<br>
                                                指名打者制の場合の投手やFPの選手は、守備のみ追加ボタンより選手行を追加して選手を選択します。<br>
                                            </li>                                 
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.GameScene)
                    {
                        messageDetail = @"<p>
                                            プレー入力結果を入力します。初めての方は本ヘルプをよくお読みください。<br />
                                            確認したい項目をクリックするとヘルプが表示されます。<br />
                                        </p>
                                        <div class=""accordion"" id=""help"">
                                            <div class=""card"">
                                                <div class=""card-header"" id=""button-area1"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-button-area1""
                                                        aria-expanded=""true"" aria-controls=""card-button-area1"">
                                                        戻る-次へ-最新へ-試合結果へ
                                                    </button>
                                                </div>
                                                <div id=""card-button-area1"" class=""collapse""
                                                    aria-labelledby=""button-area1"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                戻る(試合開始直後以外は表示)<br>
                                                                前打者のプレー入力に戻ります。<br>
                                                                入力内容の更新は行いません。<br>
                                                            </li>
                                                            <li>
                                                                次へ(修正モードのみ表示)<br>
                                                                次打者のプレー入力に進みます。<br>
                                                                入力内容の更新は行いません。<br>
                                                            </li>
                                                            <li>
                                                                最新へ(修正モードのみ表示)<br>
                                                                プレー入力の続きに戻ります。<br>
                                                                入力内容の更新は行いません。<br>
                                                            </li>       
                                                            <li>
                                                                試合結果へ(試合終了後のみ表示)<br>
                                                                試合結果編集ページに遷移します。<br>
                                                                入力内容の更新は行いません。<br>
                                                            </li>                                 
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class=""card"">
                                                <div class=""card-header"" id=""button-area2"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-button-area2""
                                                        aria-expanded=""true"" aria-controls=""card-button-area2"">
                                                        元に戻す-スキップ
                                                    </button>
                                                </div>
                                                <div id=""card-button-area2"" class=""collapse""
                                                    aria-labelledby=""button-area2"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                元に戻す<br>
                                                                現在の入力内容を破棄して、現在のプレー結果を初期化して再表示します。<br>
                                                                プレー結果修正時は一度「元に戻す」を実行してから修正することをオススメします。<br>
                                                                前打者の修正結果が現在プレーの状況に影響を与える修正であった場合、「元に戻す」を実行しないと前打者の結果が反映されません。<br>
                                                                現在のプレーに影響を与える前打者の修正例としては「アウトカウント」や「ランナー」、「選手交代」といった修正です。<br>
                                                                打者の結果のみの修正(例：「見三振」⇒「空三振」)の場合は後続に影響がないため、「元に戻す」を実行する必要はありません。<br>
                                                            </li>
                                                            <li>
                                                                スキップ<br>
                                                                草野球に対応した機能です。現在の打者を一度スキップして次の打者のプレーに進みます。<br>
                                                                一時的でなく試合に復帰しない場合は、守備時のオーダー変更で削除を行ってください。<br>
                                                            </li>                              
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class=""card"">
                                                <div class=""card-header"" id=""inning-score-area"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-inning-score-area""
                                                        aria-expanded=""true"" aria-controls=""card-inning-score-area"">
                                                        イニングスコア
                                                    </button>
                                                </div>
                                                <div id=""card-inning-score-area"" class=""collapse""
                                                    aria-labelledby=""inning-score-area"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                スコア<br>
                                                                対象のスコアを選択すると、対象イニングのイニング詳細ページに遷移します。<br>
                                                                プレーの修正を行いたい場合は、一度イニング詳細ページに遷移して、修正対象のプレーを選択してください。<br>
                                                                「戻る」「次へ」で修正対象のプレーに遷移することも可能です。<br>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class=""card"">
                                                <div class=""card-header"" id=""defense-area"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-defense-area""
                                                        aria-expanded=""true"" aria-controls=""card-defense-area"">
                                                        守備
                                                    </button>
                                                </div>
                                                <div id=""card-defense-area"" class=""collapse""
                                                    aria-labelledby=""defense-area"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <p>
                                                            守備側の選手交代を行うエリアです。<br />
                                                            守備側に変更がない場合は入力不要です。<br />
                                                        </p>
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                守備変更(マイチームが守備時のみ表示)<br>
                                                                選手交代ページに遷移します。<br>
                                                                攻撃時に代打、代走、割込出場等を行った場合は、イニングの先頭に必ず確認しましょう。<br>
                                                            </li>
                                                            <li>
                                                                投手交代<br>
                                                                現在の投手とベンチの投手と交代したい場合は、新しい投手を選択してください。<br>
                                                                現在の投手がベンチに退かない場合は、「守備変更」より守備位置の変更を行ってださい。<br>
                                                                相手チームが守備時はシステム側で用意している投手が表示されます。<br>
                                                                左右毎に投手を用意していますが、システム上で判断を行うことはありません。(メモ代わりです。)<br>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class=""card"">
                                                <div class=""card-header"" id=""scene-change-area"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-scene-change-area""
                                                        aria-expanded=""true"" aria-controls=""card-scene-change-area"">
                                                        打席中ランナー結果
                                                    </button>
                                                </div>
                                                <div id=""card-scene-change-area"" class=""collapse""
                                                    aria-labelledby=""scene-change-area"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <p>
                                                            ランナーが塁上にいる場合のみ表示されます。<br />
                                                            打席中に発生したプレー(盗塁、牽制死、WPなど)によるランナーの動きを入力します。<br />
                                                            代走の選手交代も本エリアで入力します。<br />
                                                            打席中にランナーの動きがない場合は入力不要です。<br />
                                                        </p>
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                対象選手<br>
                                                                発生したプレーに応じて対象選手を選択します。<br>
                                                                どの選手を選択するかどうかは、後述する「結果」によって変わります。<br>
                                                                対象選手が相手チームの場合は、成績管理に影響しないため任意の選択で問題ありません。<br>
                                                                対象選手を選択しないことも可能ですが、成績に反映されなくなります。<br>
                                                            </li>
                                                            <li>
                                                                結果<br>
                                                                発生したプレーを選択します。<br>
                                                                各プレーに「(投)」、「(捕)」、「(走)」、「(野)」と表記されていますが、対象選手にどの選手を選択すれば良いかを表しています。<br>
                                                                「盗塁(走)」であればランナー、「WP(投)」であれば投手を選択します。<br>
                                                                「盗塁死(走)」の場合、捕手に「盗塁阻止」が自動でカウントされるので捕手の入力は不要です。<br>
                                                            </li>
                                                            <li>
                                                                追加-削除<br>
                                                                複数プレーが発生した場合に行追加、行削除が可能です。<br>
                                                            </li>
                                                            <li>
                                                                ランナー<br>
                                                                塁上にいる選手が表示されます。<br>
                                                                代走を送る場合は、代走の選手を選択してください。<br>
                                                            </li>
                                                            <li>
                                                                ランナー結果<br>
                                                                発生したプレーにより、結果(アウト、進塁した塁、得点)を選択します。<br>
                                                                「得点」には４種類用意されており、「打」と「自」はそれぞれ「打点」と「自責点」を表しています。<br>
                                                                「得点」は打点も自責点も付く得点です。<br>
                                                                「得点(打自無)」は打点も自責点も付かない得点です。<br>
                                                                打席中ランナー結果エリアの場合は、打点が付かない得点と想定されるので「得点(打無)」か「得点(打自無)」を選択することになります。<br>
                                                                ※一般的に自責点が付かない得点は野手の失策が絡む場合ですが、詳細は他サイト等でご確認ください。<br>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class=""card"">
                                                <div class=""card-header"" id=""batter-area"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-batter-area""
                                                        aria-expanded=""true"" aria-controls=""card-batter-area"">
                                                        打者結果
                                                    </button>
                                                </div>
                                                <div id=""card-batter-area"" class=""collapse""
                                                    aria-labelledby=""batter-area"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <p>
                                                            打者の結果を入力します。<br />
                                                            代打の選手交代も本エリアで入力します。<br />
                                                            「空三振」が初期表示されるため、「空三振」の場合は入力不要です。<br />
                                                            また、割込代打出場も本エリアで入力します。<br />
                                                        </p>
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                割込<br>
                                                                草野球に対応した機能です。<br>
                                                                現在の打順の前に割り込んで代打として途中出場します。(指名打者扱い)<br>
                                                                チェックを付けると打順が自動計算され割込出場が可能です。(打者を選択し直してください。)<br>
                                                                現在の打者は次のプレーで打席に立ちます。<br>
                                                            </li>
                                                            <li>
                                                                打者<br>
                                                                現在の打者が表示されます。<br>
                                                                代打を送る場合は、代打の選手を選択してください。<br>
                                                            </li>
                                                            <li>
                                                                方向<br>
                                                                打球方向を選択します。<br>
                                                                三振、四球等、打球が飛ばない結果の場合は「無」を選択します。<br>
                                                            </li>
                                                            <li>
                                                                打球<br>
                                                                打球の種類を選択します。<br>
                                                                三振、四球等、打球が飛ばない結果の場合は「無」を選択します。<br>
                                                            </li>
                                                            <li>
                                                                打者結果<br>
                                                                打席結果を選択します。<br>
                                                                選択した打席結果によって後述する「打席後ランナー結果」のランナーが自動で進塁します。<br>
                                                                ただし進塁するのは「塁打」分になるため、塁打以上に進塁させたい場合は進塁先を指定する必要があります。<br>
                                                                ゴロで誰がアウトになったか、誰が進塁したかどうか、ケースによって進塁状況は様々であるため、ランナーの結果は必ず確認してください。<br>
                                                                また、「打席中ランナー結果」によってチェンジになった場合は「打席なし(チェンジ)」を選択します。次イニングは再び現在の打者から攻撃が始まります。<br>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class=""card"">
                                                <div class=""card-header"" id=""scene-change-area"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-scene-change-area""
                                                        aria-expanded=""true"" aria-controls=""card-scene-change-area"">
                                                        打席後ランナー結果
                                                    </button>
                                                </div>
                                                <div id=""card-scene-change-area"" class=""collapse""
                                                    aria-labelledby=""scene-change-area"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <p>
                                                            打者結果によって発生したプレー(補殺、失策)、及びランナーの結果を入力します。<br />
                                                            ランナーの結果は必ず確認してください。<br />
                                                            操作方法は「打席中ランナー結果」と変わりません。<br />
                                                        </p>
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                対象選手<br>
                                                                失策した野手、補殺した野手を選択します。<br>
                                                                「打者結果」にて「二ゴロ失策」と入力しても、二塁手に失策が記録されませんので、本エリアで改めて入力が必要です。(※野手が複数失策する場合を考慮)<br>
                                                                対象選手が相手チームの場合は、成績管理に影響しないため任意の選択で問題ありません。<br>
                                                                対象選手を選択しないことも可能ですが、成績に反映されなくなります。<br>
                                                            </li>
                                                            <li>
                                                                結果<br>
                                                                発生したプレーを選択します。<br>
                                                            </li>
                                                            <li>
                                                                追加-削除<br>
                                                                複数プレーが発生した場合に行追加、行削除が可能です。<br>
                                                            </li>
                                                            <li>
                                                                ランナー<br>
                                                                打者ランナーと塁上にいる選手が表示されます。<br>
                                                                打席中ランナー結果でアウトになったランナーは表示されません。<br>
                                                                本エリアでも代走を送ることは可能ですが、草野球においても想定されるケースはレアなので基本的に不要です。<br>
                                                            </li>
                                                            <li>
                                                                ランナー結果<br>
                                                                発生したプレーにより、結果(アウト、進塁した塁、得点)を選択します。<br>
                                                                「得点」には４種類用意されており、「打」と「自」はそれぞれ「打点」と「自責点」を表しています。<br>
                                                                「得点」は打点も自責点も付く得点です。<br>
                                                                「得点(打自無)」は打点も自責点も付かない得点です。<br>
                                                                ※打点が付かない場合、自責点が付かない場合についての詳細は他サイト等でご確認ください。<br>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class=""card"">
                                                <div class=""card-header"" id=""note-area"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-note-area""
                                                        aria-expanded=""true"" aria-controls=""card-note-area"">
                                                        メモ
                                                    </button>
                                                </div>
                                                <div id=""card-note-area"" class=""collapse""
                                                    aria-labelledby=""note-area"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                メモ<br>
                                                                入力したプレーのメモを残すことが可能です。<br>
                                                                使用方法は自由ですが、後で他の人に確認して、プレーデータの修正が必要な場合は、実際に起きたプレーを文字に残しておくと良いでしょう。<br>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class=""card"">
                                                <div class=""card-header"" id=""button-area3"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-button-area3""
                                                        aria-expanded=""true"" aria-controls=""card-button-area3"">
                                                        次の打者へ
                                                    </button>
                                                </div>
                                                <div id=""card-button-area3"" class=""collapse""
                                                    aria-labelledby=""button-area3"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                次の打者へ<br>
                                                                現在の入力内容を登録して、同イニングの次のプレーに進みます。<br>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class=""card"">
                                                <div class=""card-header"" id=""button-area4"">
                                                    <button class=""btn btn-link"" type=""button""
                                                        data-toggle=""collapse"" data-target=""#card-button-area4""
                                                        aria-expanded=""true"" aria-controls=""card-button-area4"">
                                                        チェンジ-試合終了-タイブレーク
                                                    </button>
                                                </div>
                                                <div id=""card-button-area4"" class=""collapse""
                                                    aria-labelledby=""button-area4"" data-parent=""#help"">
                                                    <div class=""card-body"">
                                                        <ul class=""help-ul"">
                                                            <li>
                                                                チェンジ<br>
                                                                現在の入力内容を登録して、攻守交代をします。<br>
                                                                ※修正モードの場合、同イニング次打者以降の結果は削除されます。<br>
                                                                ３アウトでチェンジするという縛りはなく、任意のタイミングでチェンジすることが可能です。<br>
                                                            </li>
                                                            <li>
                                                                試合終了<br>
                                                                現在の入力内容を登録して、試合終了とし、試合結果編集ページに遷移します。<br>
                                                                ※修正モードの場合、次打者以降の結果は削除されます。<br>
                                                                指定したイニングで試合終了となる縛りはなく、任意のタイミングで試合を終了することが可能です。<br>
                                                            </li>
                                                            <li>
                                                                タイブレーク(裏のイニングのみ表示)<br>
                                                                現在の入力内容を登録して、タイブレークに移ります。<br>
                                                                ※修正モードの場合、同イニング次打者以降の結果は削除されます。<br>
                                                                表示される画面よりタイブレークの開始打順、開始アウトカウント、開始ランナーを指定することが可能です。<br>
                                                                「前の回から継続打順」、「０アウト」、「一二塁」が初期表示されます。<br>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                       ";
                    }
                    else if ((int)value == (int)SystemAdminClass.MyTeamInningScore)
                    {
                        messageDetail = @"<p>試合の1打席ごとの記録を確認できます（入力方式が「プレー毎」の試合のみ）。<br>スコアボードの数字を押すと、そのイニングだけを表示します。</p><ul class=""help-ul""><li>スコア入力に戻る（試合中のみ表示）<br>スコア入力画面に戻ります。</li><li>全イニングを表示（イニングで絞り込んでいるときのみ表示）<br>すべてのイニングの表示に戻します。</li><li>試合結果編集（試合終了後に表示）<br>試合結果の入力・確定画面に移動します。</li><li>試合結果（確定後に表示）<br>試合結果画面に移動します。</li><li>修正（ロック済みの試合では表示されません）<br>その打席を、スコア入力画面の修正モードで開きます。</li></ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.PublicInningScore)
                    {
                        messageDetail = @"<p>試合の1打席ごとの記録を確認できます（入力方式が「プレー毎」の試合のみ）。<br>スコアボードの数字を押すと、そのイニングだけを表示します。</p><ul class=""help-ul""><li>全イニングを表示（イニングで絞り込んでいるときのみ表示）<br>すべてのイニングの表示に戻します。</li><li>試合結果（確定後に表示）<br>試合結果画面に移動します。</li></ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.GameScoreDetails)
                    {
                        messageDetail = @"<p>
                                            試合結果の詳細を確認できます。<br />
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                イニング詳細(プレー毎のみ表示)<br>
                                                イニング詳細ページ(全イニング)に遷移します。<br>
                                                指定イニングのみ表示させたい場合は、スコアボードの対象イニングを選択してください。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.GameScoreEdit)
                    {
                        messageDetail = @"<p>
                                            試合結果を入力します。(※PC上での操作を推奨)<br />
                                            <br />
                                            入力方式が「プレー毎」の場合は、全プレーのデータを集計した結果が初期表示されます。<br />
                                            投手の「勝敗HS」のみユーザ側で選択が必要です。<br />
                                            ※現在、リリーフ投手が出塁を許していないランナーが生還した場合も、リリーフ投手に失点が付く仕様になっています。交代前のランナーの出塁を許した投手に失点を付けたい場合は失点、自責点の調整を行ってください。(改善予定)<br />
                                            その他項目をご確認の上、確定ボタンを押してください。<br />
                                            ※その他の項目を直接修正することも可能ですが、各プレーデータと整合性が取れなくなってしまうため、「イニング詳細」⇒「修正」よりプレーデータから修正を行うことをオススメします。<br />
                                            <br />
                                            入力方式が「試合結果のみ」の場合は、イニングスコアと各選手の結果を入力する必要があります。<br />
                                            <br />
                                            確定をすると成績ページに結果が反映されます。
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                再集計する(プレー毎のみ表示)<br>
                                                全プレーデータを再集計して結果を再表示します。<br>
                                                プレー結果の修正を行った後は、必ず実行してください。<br>
                                            </li>
                                            <li>
                                                イニング詳細(プレー毎のみ表示)<br>
                                                イニング詳細ページに遷移します。<br>
                                            </li>   
                                            <li>
                                                イニング追加 イニング削除<br>
                                                イニングを追加削除します。入力方式が「プレー毎」でも使用できますが、通常「試合結果のみ」の場合にのみ使用します。<br>
                                                ※「プレー毎」の場合は、想定したスコアになるようにプレーデータを修正することをオススメします。<br>
                                            </li> 
                                            <li>
                                                追加 削除<br>
                                                選手行の追加削除します。入力方式が「プレー毎」でも使用できますが、通常「試合結果のみ」の場合にのみ使用します。<br>
                                                ※「プレー毎」の場合は、想定した選手が表示されるようにプレーデータを修正することをオススメします。<br>
                                            </li> 
                                            <li>
                                                各項目<br>
                                                空欄の場合は０として登録されます。<br>
                                                各項目の詳細については他サイト等でご確認頂きますようお願い致します。<br>
                                            </li> 
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.MyTeamTeamScore)
                    {
                        messageDetail = @"<p>
                                            チームの成績をチーム、投手、野手毎に確認できます。<br />
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                表示<br>
                                                ページ上部の条件にて画面を再表示します。<br>
                                                規定投球回は9イニング1回を基準として、各試合のイニングに応じて算出しています。<br>
                                                規定打席は9イニング3.1打席を基準として、各試合のイニングに応じて算出しています。<br>
                                            </li>
                                            <li>
                                                チーム成績詳細<br>
                                                チーム成績詳細ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                投手成績詳細<br>
                                                投手成績詳細ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                野手成績詳細<br>
                                                野手成績詳細ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                選手名<br>
                                                各選手の詳細ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                項目名(詳細ページのみ)<br>
                                                指定した項目で並び変えをすることができます。(チーム：勝率、投手：防御率、野手：打率がデフォルト)<br>
                                                各項目の詳細については他サイト等でご確認頂きますようお願い致します。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.PublicTeamScore)
                    {
                        messageDetail = @"<p>
                                            公開チームの成績をチーム、投手、野手毎に確認できます。<br />
                                            自チームと同じカテゴリのチームをチェックしてみましょう！<br />
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                表示<br>
                                                ページ上部の条件にて画面を再表示します。<br>
                                                カテゴリ、使用球はマイチームの設定値が初期表示されます。<br>
                                                規定試合数は表示対象チームの試合数を平均したものです。<br>
                                                規定投球回は9イニング1回を基準として、表示対象チーム各試合のイニングに応じて算出しています。<br>
                                                規定打席は9イニング3.1打席を基準として、表示対象チーム各試合のイニングに応じて算出しています。<br>
                                            </li>
                                            <li>
                                                チーム成績詳細<br>
                                                チーム成績詳細ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                投手成績詳細<br>
                                                投手成績詳細ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                野手成績詳細<br>
                                                野手成績詳細ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                チーム名<br>
                                                指定したチームのチーム成績トップページへ遷移します。<br>
                                            </li>
                                            <li>
                                                選手名<br>
                                                各選手の詳細ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                項目名(詳細ページのみ)<br>
                                                指定した項目で並び変えをすることができます。(チーム：勝率、投手：防御率、野手：打率がデフォルト)<br>
                                                各項目の詳細については他サイト等でご確認頂きますようお願い致します。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.PostMessage)
                    {
                        messageDetail = @"<p>チーム内の連絡や、他チームとのやり取りに使います。<br>ログインしていない場合は、公開メッセージの閲覧のみできます。</p><ul class=""help-ul""><li>表示の切り替え（ログイン時のみ表示）<br>「公開」…全チームの公開メッセージ<br>「自チームの投稿」…自チームが投稿した公開メッセージ<br>「自チームに関係」…自チームが送った・受け取ったメッセージ（公開・非公開）<br>「非公開」…チーム内のメッセージと、ダイレクトメッセージ</li><li>メッセージを投稿する<br>押すと投稿欄が開きます。タイトルとメッセージを入力して投稿します。<br>チーム一覧・チーム情報の「メッセージを送る」から来た場合は、投稿欄が開いた状態で、そのチームが送信先に選ばれています（見出しは「○○ へのダイレクトメッセージ」）。</li><li>送信先チーム<br>空のままにすると、全チームに向けた公開メッセージになります。<br>チームを選ぶと、そのチームと自チームだけに表示されるダイレクトメッセージ（非公開）になります。<br>自チームを選ぶと、チーム内だけのメッセージになります。<br>非公開チームと体験用のユーザは、チーム内のメッセージのみ送れます。</li><li>確認＆返信（ログインしていない場合は「確認」）<br>メッセージと返信の履歴を表示し、返信できます。</li></ul><p>投稿・返信すると、そのメッセージの画面を表示します。<br>一覧は、新しく投稿・返信があったものから順に表示されます。</p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.ReplyMessage)
                    {
                        messageDetail = @"<p>メッセージと返信の履歴を表示し、返信します。返信は投稿された順に表示されます。</p><ul class=""help-ul""><li>返信<br>返信は、元のメッセージと同じ範囲に表示されます（公開メッセージへの返信は、誰でも見られます）。<br>非公開チームと体験用のユーザは、チーム内のメッセージにのみ返信できます。<br>ログインしていない場合は、返信欄は表示されません。</li></ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.Inquiry)
                    {
                        messageDetail = @"<p>
                                            サイト管理者へお問い合わせをします。<br />
                                            ご質問、ご要望、不具合報告等は、こちらのページよりお問い合わせください。<br />
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                メールアドレス(必須)<br>
                                                入力したメールアドレス宛てに返信させて頂きます。<br>
                                                ユーザ情報に登録したメールアドレスが初期表示されます。<br>
                                            </li>
                                            <li>
                                                お問い合わせタイトル(必須)<br>
                                                お問い合わせタイトルを入力します。<br>
                                            </li>
                                            <li>
                                                お問い合わせ内容(必須)<br>
                                                お問い合わせ内容を入力します。<br>
                                            </li>

                                        </ul>";
                    }

                    systemAdmins.Add(new SystemAdmin
                    {
                        SystemAdminClass = (SystemAdminClass)value,
                        MessageTitle = value.GetEnumName(),
                        MessageDetail = messageDetail,
                        EntryDatetime = DateTime.Now,
                        EntryUserID = "ADMIN",
                        UpdateDatetime = DateTime.Now,
                        UpdateUserID = "ADMIN"
                    });
                }

                context.SystemAdmins.AddRange(systemAdmins);
            }

            //初期データ投入
            if (!context.UserAccounts.ToList().Any())
            {
                isUpdate = true;

                var userAccounts = new UserAccount[]
                {
                    new UserAccount
                    {
                          UserAccountID = "ADMIN"
                        , UserAccountName = "管理者"
                        , Password = "1".ChangeHashValue()
                        , TeamID = "JB"
                        , EmailAddress = "proud.of.y.d@gmail.com"
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new UserAccount
                    {
                          UserAccountID = "YGUser"
                        , UserAccountName = "ジャイアンツ管理者"
                        , Password = "1".ChangeHashValue()
                        , TeamID = "YG"
                        , EmailAddress = ""
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new UserAccount
                    {
                          UserAccountID = "HTUser"
                        , UserAccountName = "タイガース管理者"
                        , Password = "1".ChangeHashValue()
                        , TeamID = "HT"
                        , EmailAddress = ""
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new UserAccount
                    {
                          UserAccountID = "JBUser"
                        , UserAccountName = "JAPAN BRIDGE 管理者"
                        , Password = "1".ChangeHashValue()
                        , TeamID = "JB"
                        , EmailAddress = ""
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                };

                context.UserAccounts.AddRange(userAccounts);
            }

            //初期データ投入
            if (!context.Teams.ToList().Any())
            {
                isUpdate = true;

                var teams = new Team[]
                {
                     new Team
                    {
                          TeamID = "SYSTEM"
                        , TeamName = "システムチーム"
                        , TeamAbbreviation = "ST"
                        , RepresentativeName = "システム"
                        , TeamCategoryClass = TeamCategoryClass.Other
                        , UseBallClass = UseBallClass.Other
                        , ActivityBase = "システム"
                        , TeamNumber = 10
                        , TeamPassword = "SYSTEM".ChangeHashValue()
                        , TeamEmailAddress = ""
                        , MessageDetail= ""
                        , PublicFLG = false
                        , SystemDataFLG = true
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Team
                    {
                          TeamID = "YG"
                        , TeamName = "読売ジャイアンツ(サンプル)"
                        , TeamAbbreviation = "YG"
                        , RepresentativeName = "原　辰徳"
                        , TeamCategoryClass = TeamCategoryClass.Proffessional
                        , UseBallClass = UseBallClass.Hard
                        , ActivityBase = "東京"
                        , TeamNumber = 70
                        , TeamPassword = "1".ChangeHashValue()
                        , TeamEmailAddress = ""
                        , MessageDetail= "東京ドームが本拠地です。"
                        , PublicFLG = true
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Team
                    {
                          TeamID = "HT"
                        , TeamName = "阪神タイガース(サンプル)"
                        , TeamAbbreviation = "HT"
                        , RepresentativeName = "矢野　燿大"
                        , TeamCategoryClass = TeamCategoryClass.Proffessional
                        , UseBallClass = UseBallClass.Hard
                        , ActivityBase = "兵庫"
                        , TeamNumber = 70
                        , TeamPassword = "1".ChangeHashValue()
                        , TeamEmailAddress = ""
                        , MessageDetail= "甲子園が本拠地です。"
                        , PublicFLG = true
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Team
                    {
                          TeamID = "JB"
                        , TeamName = "JAPAN BRIDGE"
                        , TeamAbbreviation = "JB"
                        , RepresentativeName = "鈴木　雄三"
                        , TeamCategoryClass = TeamCategoryClass.Adult
                        , UseBallClass = UseBallClass.Rubber
                        , ActivityBase = "東京都中央区"
                        , TeamNumber = 12
                        , TeamPassword = "1".ChangeHashValue()
                        , TeamEmailAddress = ""
                        , MessageDetail= "30代のチームです。"
                        , PublicFLG = false
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                };

                context.Teams.AddRange(teams);
            }

            //初期データ投入
            if (!context.Members.ToList().Any())
            {
                isUpdate = true;

                var members = new Member[]
                {
                    new Member
                    {
                          TeamID = "SYSTEM"
                        , UniformNumber = ""
                        , MemberName = "相手投手"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = null
                        , BatClass = null
                        , PositionGroupClass =  PositionGroupClass.Pitcher
                        , MessageDetail = ""
                        , SystemDataFLG = true
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "SYSTEM"
                        , UniformNumber = ""
                        , MemberName = "相手投手(右)"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = null
                        , PositionGroupClass = PositionGroupClass.Pitcher
                        , MessageDetail = ""
                        , SystemDataFLG = true
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "SYSTEM"
                        , UniformNumber = ""
                        , MemberName = "相手投手(左)"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Left
                        , BatClass = null
                        , PositionGroupClass = PositionGroupClass.Pitcher
                        , MessageDetail = ""
                        , SystemDataFLG = true
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "SYSTEM"
                        , UniformNumber = ""
                        , MemberName = "相手野手"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = null
                        , BatClass = null
                        , PositionGroupClass = PositionGroupClass.Catcher
                        , MessageDetail = ""
                        , SystemDataFLG = true
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "SYSTEM"
                        , UniformNumber = ""
                        , MemberName = "相手野手(右)"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = null
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = true
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                     new Member
                    {
                          TeamID = "SYSTEM"
                        , UniformNumber = ""
                        , MemberName = "相手野手(左)"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = null
                        , BatClass = BatClass.Left
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = true
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "88"
                        , MemberName = "原　辰徳"
                        , MemberClass = Enum.MemberClass.Manager
                        , ThrowClass = null
                        , BatClass = null
                        , PositionGroupClass = null
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "6"
                        , MemberName = "坂本　勇人"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = "主将"
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "18"
                        , MemberName = "菅野　智之"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Pitcher
                        , MessageDetail = "投手主将"
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "8"
                        , MemberName = "丸　佳浩"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Left
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "29"
                        , MemberName = "吉川　尚輝"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Left
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "22"
                        , MemberName = "小林　誠司"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Catcher
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "25"
                        , MemberName = "岡本　和真"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "5"
                        , MemberName = "中島　宏之"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "9"
                        , MemberName = "亀井　善行"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Left
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "31"
                        , MemberName = "松原　聖弥"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Left
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "24"
                        , MemberName = "大城　卓三"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Left
                        , PositionGroupClass = PositionGroupClass.Catcher
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "48"
                        , MemberName = "ウィーラー"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "00"
                        , MemberName = "湯浅　大"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                     new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "0"
                        , MemberName = "増田　大輝"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "20"
                        , MemberName = "戸郷　翔征"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Pitcher
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "YG"
                        , UniformNumber = "49"
                        , MemberName = "ビエイラ"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Pitcher
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "HT"
                        , UniformNumber = "88"
                        , MemberName = "矢野　燿大"
                        , MemberClass = Enum.MemberClass.Manager
                        , ThrowClass = null
                        , BatClass = null
                        , PositionGroupClass = null
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "6"
                        , MemberName = "古賀"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Left
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "30"
                        , MemberName = "鈴木"
                        , MemberClass = Enum.MemberClass.PlayingManager
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Catcher
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "7"
                        , MemberName = "治下"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Pitcher
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "10"
                        , MemberName = "永田"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "1"
                        , MemberName = "中塚"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "3"
                        , MemberName = "山崎"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "8"
                        , MemberName = "髙橋"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "11"
                        , MemberName = "小越"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Infielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "5"
                        , MemberName = "杉田"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = ""
                        , MemberName = "飯島"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "4"
                        , MemberName = "榎本"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "14"
                        , MemberName = "田中"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = "13"
                        , MemberName = "安西"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                     new Member
                    {
                          TeamID = "JB"
                        , UniformNumber = ""
                        , MemberName = "川西"
                        , MemberClass = Enum.MemberClass.Player
                        , ThrowClass = ThrowClass.Right
                        , BatClass = BatClass.Right
                        , PositionGroupClass = PositionGroupClass.Outfielder
                        , MessageDetail = ""
                        , SystemDataFLG = false
                        , DeleteFLG = false
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                };

                context.Members.AddRange(members);
            }

            //初期データ投入
            if (!context.Messages.ToList().Any())
            {
                isUpdate = true;

                var messages = new Message[]
                {
                    new Message
                    {
                          UserAccountID = "JBUser"
                        , TeamID = "JB"
                        , PrivateTeamID = null
                        , ParentMessageID = null
                        , DeleteFLG = false
                        , PublicFLG = true
                        , MessageClass = MessageClass.Post
                        , MessageTitle = "JBからの公開投稿　※このメッセージはサンプルです。"
                        , MessageDetail = "一般の投稿です。対戦相手募集"
                        , ReplyCount = 0
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Message
                    {
                          UserAccountID = "JBUser"
                        , TeamID = "JB"
                        , PrivateTeamID = "JB"
                        , ParentMessageID = null
                        , DeleteFLG = false
                        , PublicFLG = false
                        , MessageClass = MessageClass.Post
                        , MessageTitle = "チーム内投稿　※このメッセージはサンプルです。"
                        , MessageDetail = "チーム内連絡です。"
                        , ReplyCount = 0
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Message
                    {
                          UserAccountID = "YGUser"
                        , TeamID = "YG"
                        , PrivateTeamID = null
                        , ParentMessageID = null
                        , DeleteFLG = false
                        , PublicFLG = true
                        , MessageClass = MessageClass.Post
                        , MessageTitle = "巨人よりお知らせ　※このメッセージはサンプルです。"
                        , MessageDetail = "巨人から公開メッセージ"
                        , ReplyCount = 0
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Message
                    {
                          UserAccountID = "YGUser"
                        , TeamID = "YG"
                        , PrivateTeamID = "JB"
                        , ParentMessageID = null
                        , DeleteFLG = false
                        , PublicFLG = false
                        , MessageClass = MessageClass.Post
                        , MessageTitle = "巨人からJB　※このメッセージはサンプルです。"
                        , MessageDetail = "巨人からJBへのダイレクトメッセージ"
                        , ReplyCount = 0
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                     new Message
                    {
                          UserAccountID = "HTUser"
                        , TeamID = "HT"
                        , PrivateTeamID = null
                        , ParentMessageID = null
                        , DeleteFLG = false
                        , PublicFLG = true
                        , MessageClass = MessageClass.Post
                        , MessageTitle = "阪神からお知らせ　※このメッセージはサンプルです。"
                        , MessageDetail = "阪神から公開メッセージ"
                        , ReplyCount = 0
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                    new Message
                    {
                          UserAccountID = "HTUser"
                        , TeamID = "HT"
                        , PrivateTeamID = "JB"
                        , ParentMessageID = null
                        , DeleteFLG = false
                        , PublicFLG = false
                        , MessageClass = MessageClass.Post
                        , MessageTitle = "阪神からJB　※このメッセージはサンプルです。"
                        , MessageDetail = "阪神からJBへのダイレクトメッセージ"
                        , ReplyCount = 0
                        , EntryDatetime = DateTime.Now
                        , EntryUserID = "ADMIN"
                        , UpdateDatetime = DateTime.Now
                        , UpdateUserID = "ADMIN"
                    },
                };

                context.Messages.AddRange(messages);
            }

            if (isUpdate)
            {
                context.SaveChanges();
            }
        }
    }
}
