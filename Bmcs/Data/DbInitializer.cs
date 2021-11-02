using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Enum;

namespace Bmcs.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BmcsContext context)
        {
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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
                        messageDetail = @"<p>本サービスを使用するためのユーザを作成します。</p>
                                            <ul class=""help-ul"">
                                                <li>
                                                    ユーザID(必須)<br>
                                                    任意のIDを入力します。(既に使用されているIDはエラーをなります。)<br>
                                                </li>
                                                <li>
                                                    ユーザ名(必須)<br>
                                                    任意のユーザ名を入力します。ユーザ名は「メッセージ」機能に投稿者として表示されます。<br>
                                                </li>
                                                <li>
                                                    パスワード(必須)<br>
                                                    ログインするためのパスワードを入力します。使用する文字に特に制限はありません。<br>
                                                    「確認用パスワード」に同じパスワードを入力してください。<br>
                                                    パスワードをお忘れの際はお問い合わせください。<br>
                                                </li>
                                                <li>
                                                    メールアドレス<br>
                                                    ユーザのメールアドレスを入力します。<br>
                                                    現在の使用用途はサイト管理者からご連絡させて頂く以外に使用しません。<br>
                                                    必須ではありませんので、空欄でも構いません。<br>
                                                </li>
                                                <li>
                                                    チーム<br>
                                                    別ユーザにチームを既に作成している場合はチームを選択します。<br>
                                                    チームを作成したユーザよりパスワードを共有して頂き、チームパスワードを入力してください。<br>
                                                    チームを未作成の場合は空欄でご登録ください。ユーザ作成後に作成します。<br>
                                                </li>
                                            </ul>
                                            <p>利用規約、プライバシーポリシーをご確認後にチェックを入れ、ご登録ください。</p>";
                    }
                    else if ((int)value == (int)SystemAdminClass.UserAccountEdit)
                    {
                        messageDetail = @"<p>登録したユーザ情報を変更します。</p>
                                            <ul class=""help-ul"">
                                                <li>
                                                    ユーザ名(必須)<br>
                                                    任意のユーザ名を入力します。ユーザ名は「メッセージ」機能に投稿者として表示されます。<br>
                                                </li>
                                                <li>
                                                    パスワード<br>
                                                  パスワードを変更する場合のみご入力ください。<br>
                                                    パスワードを変更しない場合は空欄でご登録ください。<br>
                                                    パスワードを入力した場合に限り、「確認用パスワード」に同じパスワードを入力してください。<br>
                                                </li>
                                                <li>
                                                    メールアドレス<br>
                                                    ユーザのメールアドレスを入力します。<br>
                                                    現在の使用用途はサイト管理者からご連絡させて頂く以外に使用しません。<br>
                                                    必須ではありませんので、空欄でも構いません。<br>
                                                </li>
                                                <li>
                                                    チーム<br>
                                                    ユーザを別チームの所属に変更したい場合は、別のチームを選択します。<br>
                                                    その場合は、変更したチームのチームパスワードを入力してください。<br>
                                                    チームを変更しない場合はそのままご登録ください。<br>
                                                </li>
                                            </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.TeamIndex)
                    {
                        messageDetail = @"<p>公開設定のチーム一覧を確認することができます。<br />
                                                    気になるチームの情報を確認してみましょう。
                                            </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                試合<br>
                                                該当チームの試合一覧へ遷移します。<br>
                                            </li>
                                            <li>
                                                成績<br>
                                                該当チームの成績ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                詳細<br>
                                                該当チームの詳細情報ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                メンバー<br>
                                                該当チームのメンバー一覧ページへ遷移します。<br>
                                            </li>
                                            <li>
                                                メッセージを送る<br>
                                                該当チームへダイレクト(非公開)メッセージを送る画面に遷移します。<br>
                                            </li>
                                        </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.TeamCreate)
                    {
                        messageDetail = @"<p>
                                                ユーザが操作するチームを作成します。
                                            </p>
                                            <ul class=""help-ul"">
                                                <li>
                                                    チームID(必須)<br>
                                                    任意のIDを入力します。(既に使用されているIDはエラーをなります。)<br>
                                                </li>
                                                <li>
                                                    チーム名(必須)<br>
                                                    任意のチーム名を入力します。<br>
                                                </li>
                                                <li>
                                                    チーム略名(必須)<br>
                                                    任意のチーム略名を10桁以内で入力します。スコアボードでのチーム名表示などに使用されます。<br>
                                                </li>
                                                <li>
                                                    パスワード(必須)<br>
                                                    作成するチームに対して別ユーザーが操作するためのパスワードを入力します。使用する文字に特に制限はありません。<br>
                                                    設定したパスワードはユーザ作成画面で入力が必要です。(チーム作成者は入力不要です。)<br>
                                                    「確認用パスワード」に同じパスワードを入力してください。<br>
                                                    設定したパスワードはログイン後に変更できます。<br>
                                                </li>
                                                <li>
                                                    公開フラグ<br>
                                                    作成するチームを他チームに公開する場合はチェックを付けます。(インターネット上に公開されます。)<br>
                                                    チームを公開設定にすることで、他チームとメッセージ機能でやり取りを行ったり、成績を比較したり、試合結果やメンバーを確認してもらうことが可能です。<br>
                                                    他ユーザから参照されたくない場合はチェックをオフにしてください。<br>
                                                    公開にすることで機能が制限されないため、公開設定がオススメです。<br>
                                                </li>
                                            </ul>
                                            <p>
                                                以下の項目は、公開チーム設定とした場合に、他チームのユーザから確認できる項目です。<br />
                                                非公開チームの場合は、使用用途がありませんので入力不要です。
                                            </p>
                                            <ul class=""help-ul"">
                                                <li>
                                                    代表者名<br>
                                                    チームの代表者を入力します。<br>
                                                </li>
                                                <li>
                                                    カテゴリ<br>
                                                    チームの分類を入力します。チームカテゴリ毎に公開チームの成績を確認することができます。<br>
                                                </li>
                                                <li>
                                                    使用球<br>
                                                    チームの使用球を入力します。カテゴリ同様に使用球毎に公開チームの成績を確認することができます。<br>
                                                </li>
                                                <li>
                                                    活動拠点<br>
                                                    チームの活動拠点を入力します。メッセージ機能で対戦相手を募集する際の参考情報になります。<br>
                                                </li>
                                                <li>
                                                    チーム人数<br>
                                                    チームの人数を入力します。メッセージ機能で対戦相手を募集する際の参考情報になります。<br>
                                                </li>
                                                <li>
                                                    メールアドレス<br>
                                                    チーム代表者のメールアドレスを入力します。<br>
                                                    現在の使用用途はサイト管理者からご連絡させて頂いたり、メッセージ機能以外で他チームから連絡を取る際に使用します。<br>
                                                    必須ではありませんので、空欄でも構いません。<br>
                                                </li>
                                                <li>
                                                    メッセージ<br>
                                                    自由項目です。<br>
                                                    他チームに対してチーム紹介文を入力したりすることができます。<br>
                                                </li>
                                            </ul>";
                    }
                    else if ((int)value == (int)SystemAdminClass.TeamEdit)
                    {
                        messageDetail = @"<p>
                                            登録したチーム情報を変更します。
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                チーム名(必須)<br>
                                                任意のチーム名を入力します。<br>
                                            </li>
                                            <li>
                                                チーム略名<br>
                                                任意のチーム略名を10桁以内で入力します。スコアボードでのチーム名表示などに使用されます。<br>
                                            </li>
                                            <li>
                                                パスワード(必須)<br>
                                                作成するチームに対して別ユーザーが操作するためのパスワードを入力します。使用する文字に特に制限はありません。<br>
                                                パスワードを変更する場合のみご入力ください。<br>
                                                パスワードを変更しない場合は空欄でご登録ください。<br>
                                                設定したパスワードはユーザ作成画面で入力が必要です。(チーム作成者は入力不要です。)<br>
                                                「確認用パスワード」に同じパスワードを入力してください。<br>
                                            </li>
                                            <li>
                                                公開フラグ<br>
                                                作成するチームを他チームに公開する場合はチェックを付けます。(インターネット上に公開されます。)<br>
                                                チームを公開設定にすることで、他チームとメッセージ機能でやり取りを行ったり、成績を比較したり、試合結果やメンバーを確認してもらうことが可能です。<br>
                                                他ユーザから参照されたくなくない場合はチェックをオフにしてください。<br>
                                                公開にすることで機能が制限されないため、公開設定がオススメです。<br>
                                            </li>
                                        </ul>
                                        <p>
                                            以下の項目は、公開チーム設定とした場合に、他チームのユーザから確認できる項目です。<br />
                                            非公開チームの場合は、使用用途がありませんので入力不要です。
                                        </p>
                                        <ul class=""help-ul"">
                                            <li>
                                                代表者名<br>
                                                チームの代表者を入力します。<br>
                                            </li>
                                            <li>
                                                カテゴリ<br>
                                                チームの分類を入力します。チームカテゴリ毎に公開チームの成績を確認することができます。<br>
                                            </li>
                                            <li>
                                                使用球<br>
                                                チームの使用球を入力します。カテゴリ同様に使用球毎に公開チームの成績を確認することができます。<br>
                                            </li>
                                            <li>
                                                活動拠点<br>
                                                チームの活動拠点を入力します。メッセージ機能で対戦相手を募集する際の参考情報になります。<br>
                                            </li>
                                            <li>
                                                チーム人数<br>
                                                チームの人数を入力します。メッセージ機能で対戦相手を募集する際の参考情報になります。<br>
                                            </li>
                                            <li>
                                                メールアドレス<br>
                                                チーム代表者のメールアドレスを入力します。<br>
                                                現在の使用用途はサイト管理者からご連絡させて頂いたり、メッセージ機能以外で他チームから連絡を取る際に使用します。<br>
                                                必須ではありませんので、空欄でも構いません。<br>
                                            </li>
                                            <li>
                                                メッセージ<br>
                                                自由項目です。<br>
                                                他チームに対してチーム紹介文を入力したりすることができます。<br>
                                            </li>
                                        </ul>";
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
                        messageDetail = @"<p>
                                            チームに所属する監督や選手を登録します。<br />
                                            登録したメンバーは試合に出場が可能になります。
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
                                            削除を行うと、試合で使用できなくなり、成績ページからも表示対象外になります。<br />
                                            過去の試合履歴では表示対象です。削除した選手の成形はチーム成績の合計からは除外されません。<br />
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
                        , Password = "1"
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
                        , Password = "1"
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
                        , Password = "1"
                        , TeamID = "HT"
                        , EmailAddress = "proud.of.y.d@gmail.com"
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
                        , Password = "1"
                        , TeamID = "JB"
                        , EmailAddress = "proud.of.y.d@gmail.com"
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
                        , TeamPassword = "SYSTEM"
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
                        , TeamPassword = "1"
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
                        , TeamPassword = "1"
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
                        , TeamPassword = "1"
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
                        , UniformNumber = "11"
                        , MemberName = "投手(左右未設定)"
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
                        , UniformNumber = "12"
                        , MemberName = "投手(右投手)"
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
                        , UniformNumber = "13"
                        , MemberName = "投手(左投手)"
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
                        , UniformNumber = "1"
                        , MemberName = "野手(打席未設定)"
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
                        , UniformNumber = "2"
                        , MemberName = "野手(右打者)"
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
                        , UniformNumber = "3"
                        , MemberName = "野手(左打者)"
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
