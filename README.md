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
- ASP.NET Core 8.0 Razor Pages
- Entity Framework(O/R Mapper)
- C#
- Html
- CSS
- Bootstrap 4（既存画面。順次 Tailwind CSS へ移行中）
- Tailwind CSS 4（新規・移行済み画面）
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
- Visual Studioをインストールする(2019以上)

　https://docs.microsoft.com/ja-jp/visualstudio/install/install-visual-studio?view=vs-2019
 
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
| `Analytics:ClarityProjectID` | Microsoft Clarity のプロジェクトID。**未設定の場合は計測タグを出力しない** |

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

※移行SQLは、アプリのデプロイ「前」に実行してください。

## その他
- サービス紹介記事
  https://alivetodayblog.com/bmcs/
- 作成期間
  Ver1.0.0まで約3か月
- 改善したいこと
　画面デザイン全般
　チーム、メンバーにアイコンを設定したい
　ユーザ側でグループを作成して、その中で成績を見たり、メッセージを送る機能
