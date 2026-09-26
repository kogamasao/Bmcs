# Bmcsとは？

野球、ソフトボールのスコア管理、及び他チームの成績やメッセージのやり取りができるWEBサービスです。

本リポジトリはそのWEBサービスのソース一式です。

本番環境のURLは以下です。主な機能や操作方法はこちらに記載しています。
サンプルチームにログインして各機能を使用できます。

(ID:YGUser パスワード：1)

https://bmcs.azurewebsites.net/

どなたでも無料で使用できますので、野球、ソフトボール関連の方は気軽にご使用ください。

## 使用技術
### 言語、フレームワーク等
- ASP.NET Core 10.0 Razor Pages（.NET 10）
- Entity Framework(O/R Mapper)
- C#
- Html
- CSS
- Tailwind CSS 4（2026-09 に Bootstrap 4 から全画面を移行）
- Javascript
- JQuery

### データベース
- SQL Server(for Local)
- Azure Database(for Production)

### メール送信
- MailKit(SMTP)
  - ユーザID・パスワードの再設定メール、問い合わせの管理者通知に使用

### インフラ
- Azure App Service

## ローカル環境の実行方法(for Windows)
- .NET 10 SDK と、.NET 10 に対応した Visual Studio をインストールする

　https://learn.microsoft.com/ja-jp/visualstudio/install/install-visual-studio
 
　Visual Studioにて「Bmcs.sln」を起動

- SQL Server Express LocalDBを準備する
　下記内容を記載した「Bmcs\appsettings.json」ファイルを作成する
```json
{
  "ConnectionStrings": {
    "SqlServerConnectionString": "Server=(localdb)\\mssqllocaldb;Database=Bmcs;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```
- ソリューションを実行する
　F5実行すると「Bmcs\Data\DbInitializer.cs」が実行され、DBの作成及びサンプルデータが投入されます。

- メール送信を確認する場合は「Bmcs\appsettings.json」に「EmailSettings」を設定する
　設定例は「Bmcs\appsettings.sample.json」を参照してください。
　BaseUrlは再設定メールに記載するURLの基点です（未設定時はリクエストのホストを使用）。
　AdminEmailは問い合わせ通知の宛先です（未設定時はSenderEmailへ送信）。

## 設定値(appsettings.json)
`Bmcs/appsettings.sample.json` を `appsettings.json` にコピーして使用する(git管理外)。
本番(Azure App Service)では「構成」→「アプリケーション設定」に登録する。
階層は `__`(アンダースコア2つ)で区切る。例: `Analytics__ClarityProjectID`

| キー | 用途 |
| --- | --- |
| `EmailSettings:*` | メール送信(SMTP)、メール本文に載せる絶対URL |
| `Site:OgImage` | SNS共有時に表示する画像のパス。**未設定の場合は画像なしのカードになる** |
| `Site:CanonicalHost` | 正規のホスト名（例：`bmcs.app`）。それ以外のホスト名へのアクセスを301で転送し、canonical・サイトマップ・robots.txt のURLにも使う。**未設定の場合は転送せず、URLは `EmailSettings:BaseUrl` から組み立てる** |
| `Analytics:ClarityProjectID` | Microsoft Clarity のプロジェクトID。**未設定の場合は計測タグを出力しない** |
| `Analytics:GoogleAnalyticsMeasurementID` | Google アナリティクス(GA4)の測定ID(`G-` で始まる)。**未設定の場合は計測タグを出力しない**。パスワード再設定のURLに含まれるトークン(`token`)は、送信前に取り除く |

※計測タグを有効にする場合、プライバシーポリシー(`Pages/Privacy.cshtml` 第10条)の
　記載と実際の運用が一致しているか確認すること。

## CSS(Tailwind)のビルド方法
移行済み画面は Tailwind CSS を使用している。
`Bmcs/Styles/app.css` がソースで、`Bmcs/wwwroot/css/app.css` が生成物。

**生成物はリポジトリにコミットしているため、デプロイ時に Node.js もビルドも不要。**
CSSクラスを追加・変更したときだけ、以下を実行してコミットする。

```sh
./build-css.sh            # 1回だけビルド
./build-css.sh --watch    # 編集しながら確認する場合
```

初回のみ、CLI(単一バイナリ・Node.js不要)を取得する。
```sh
mkdir -p ~/.local/bin
curl -sL -o ~/.local/bin/tailwindcss \
  "https://github.com/tailwindlabs/tailwindcss/releases/download/v4.3.3/tailwindcss-linux-x64"
chmod +x ~/.local/bin/tailwindcss
```

### Bootstrap から Tailwind への移行方針
1ページ内でクラス名が衝突しないよう、**レイアウトごと分ける**。
- 未移行の画面 … `_Layout.cshtml`(Bootstrapのみ)
- 移行済みの画面 … `_LayoutTailwind.cshtml`(Tailwindのみ)。ページ側で `Layout` を指定する

移行が全画面完了したら、Bootstrap と `site.css` を削除する。
詳細は `Bmcs/doc/detail_design_ui.md` を参照。

## DBの変更を既存環境へ適用する方法
本プロジェクトはEF Migrationsではなく「DbInitializer.cs」のEnsureCreatedでDBを作成しています。
そのため、既に作成済みのDB（本番・開発）へテーブル追加等を反映する場合は、
「Bmcs\doc\migration」配下のSQLをSSMSまたはsqlcmdで実行してください。

- 20260920_password_reset.sql … アカウント復旧機能（ResetTokenテーブル、ヘルプ文言の更新）
- 20260921_survey.sql … アンケート機能（Survey関連5テーブル、第1回アンケートの設問）
- 20260921_help_message.sql … ヘルプ本文の修正（ユーザ作成のチーム選択→チームID入力、チームIDの説明追加、誤字）
- 20260922_session_lastlogin.sql … セッションのSQL Server保持（SessionCacheテーブル）、最終ログイン日時の追加
- 20260926_member_bulk_help.sql … メンバー追加（まとめて登録）のヘルプ本文の更新。本文だけの変更のため、デプロイの前後どちらに実行してもよい
- 20260926_uniform_number_halfwidth.sql … 全角数字・前後の空白が入った背番号を半角に直す。先に `doc/query/20260926_uniform_number_check.sql` で件数を確認する
  ※**既存データへの推定値設定のみ手動実行が必要。** テーブル作成と列追加は
  `DbInitializer` が起動時に行うため、実行順序を誤ってもアプリは動作する

※移行SQLは、アプリのデプロイ「前」に実行してください。

## 検索エンジン対策(SEO)
検索結果に出すページは各画面で `SetIndex` を呼んで指定し、それ以外の画面は `noindex` になる。
`/sitemap.xml`(公開チーム・試合結果などのURLを自動生成)と `/robots.txt` はアプリが出力する。
詳細は `Bmcs/doc/detail_design_seo.md` を参照。

## アップデート情報
利用者から見える変更（新機能・改善・規約の改定など）をデプロイするときは、
`Bmcs/Data/UpdateHistoryList.cs` の先頭に追加する(`/Updates` とトップページに表示される)。
運営側で見つけて直した不具合や内部の改修は書かない。詳細は `Bmcs/doc/detail_design_other.md` の3章を参照。

## その他
- サービス紹介記事
  https://alivetodayblog.com/bmcs/
- 作成期間
  Ver1.0.0まで約3か月
- 改善したいこと
　画面デザイン全般
　チーム、メンバーにアイコンを設定したい
　ユーザ側でグループを作成して、その中で成績を見たり、メッセージを送る機能
