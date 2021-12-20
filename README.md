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
- Asp.Net core 3.1 Razor
- Entity Framework(O/R Mapper)
- C#
- Html
- CSS
- Bootstrap
- Javascript
- JQuery

### データベース
- SQL Server(for Local)
- Azure Database(for Production)

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

## その他
- サービス紹介記事
  https://alivetodayblog.com/bmcs/
- 作成期間
  Ver1.0.0まで約3か月
- 改善したいこと
　画面デザイン全般
　チーム、メンバーにアイコンを設定したい
　ユーザ側でグループを作成して、その中で成績を見たり、メッセージを送る機能
