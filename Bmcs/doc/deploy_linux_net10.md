# 本番移行手順：Linux（B1）・.NET 10・独自ドメイン（bmcs.app）

課題 D-11（.NET 10 への更新）・P-6（独自ドメイン）の本番作業の手順。
アプリ側の対応（.NET 10 への更新、正規ホスト名への転送）はコミット済みで、ローカルで全画面・試合入力〜確定の流れを確認済み。

## 全体の流れ

| 段階 | 作業 | 利用者への影響 |
| --- | --- | --- |
| 1 | Linux の App Service（B1）を新しく作り、.NET 10 版を発行する | なし（旧サイトはそのまま動いている） |
| 2 | 新しいアプリを `○○.azurewebsites.net` で確認する | なし |
| 3 | Cloudflare の DNS を設定し、新しいアプリに `bmcs.app` を割り当てる（証明書も） | なし（新ドメインが使えるようになるだけ） |
| 4 | 新しいアプリに正規ホスト名（`bmcs.app`）を設定する | なし |
| 5 | 旧アプリ（Windows）を、`bmcs.app` へ転送するだけの状態にする | **ここで切り替わる**（旧URLのアクセスが新ドメインへ移る） |
| 6 | 後片付け（Clarity・Search Console・README など） | なし |

**段階5までは、いつでも旧サイトに戻せる**（旧アプリは触っていないため）。問題があれば段階5をやり直す（旧アプリの設定を戻す）だけでよい。

---

## 段階1：Linux の App Service を作る

### 1-1. App Service プラン
Azure ポータル →「App Service プラン」→「作成」
- リソースグループ：`ResourceGroup`（今と同じ）
- 名前：例 `ASP-Bmcs-Linux`
- **オペレーティングシステム：Linux**
- 地域：**Japan East**（DB と同じ地域にする。別の地域だと DB へのアクセスが遅くなる）
- 価格プラン：**Basic B1**（約2,200円/月）

### 1-2. Web アプリ
「Web アプリ」→「作成」
- リソースグループ：`ResourceGroup`
- 名前：例 `bmcs-web`（`bmcs` は旧アプリが使用中のため別名にする。既定のURLは `bmcs-web.azurewebsites.net` のようになる）
- 公開：コード
- **ランタイムスタック：.NET 10 (LTS)**
- オペレーティングシステム：**Linux**
- 地域：Japan East
- App Service プラン：1-1 で作ったもの

### 1-3. 構成（旧アプリの設定を移す）
旧アプリの「環境変数」（または「構成」）を開き、同じ値を新しいアプリに登録する。

> **重要：Linux では、設定名の区切りに「:」が使えない。「__」（アンダースコア2つ）で登録する。**
> 旧アプリが `EmailSettings:BaseUrl` の形で登録していても、新しいアプリでは `EmailSettings__BaseUrl` とする。

**アプリ設定**
| 名前 | 値 |
| --- | --- |
| `EmailSettings__MailServer` | 旧アプリと同じ |
| `EmailSettings__MailPort` | 旧アプリと同じ |
| `EmailSettings__SenderName` | 旧アプリと同じ |
| `EmailSettings__SenderEmail` | 旧アプリと同じ |
| `EmailSettings__Password` | 旧アプリと同じ |
| `EmailSettings__AdminEmail` | 旧アプリと同じ |
| `EmailSettings__BaseUrl` | **`https://bmcs.app`**（再設定メールのリンク、canonical・OGP の URL に使う。段階3が終わるまでは、このURLは開けない点に注意） |
| `Analytics__ClarityProjectID` | 旧アプリと同じ（`ylpd49tvn7`） |
| `Site__OgImage` | 旧アプリと同じ（未設定なら不要） |
| `Site__CanonicalHost` | **ここではまだ登録しない**（段階4で登録する。先に登録すると、確認用の azurewebsites.net の URL が開けなくなる） |

**接続文字列**
| 名前 | 値 | 種類 |
| --- | --- | --- |
| `AzureDatabaseConnectionString` | 旧アプリと同じ | SQLAzure |

※旧アプリの設定一覧に上記以外の項目があれば、それも同じように移す。

### 1-4. 全般設定
「構成」→「全般設定」
- **常時接続（Always On）：オン**（アクセスが途絶えた後の初回表示で待たされないようにする。B1 以上で使える）
- **HTTPS のみ：オン**
- 最小 TLS バージョン：1.2

### 1-5. データベースのファイアウォール
SQL サーバー（`bmcs`）→「ネットワーク」
- 「Azure サービスおよびリソースにこのサーバーへのアクセスを許可する」がオンなら、そのままでよい
- オフで個別のIPを許可している場合は、新しいアプリの「送信 IP アドレス」（Web アプリ →「プロパティ」）を追加する

### 1-6. 発行
新しいアプリの「発行プロファイルのダウンロード」で取得したプロファイルを使って発行する。
- ターゲット フレームワーク：**net10.0**
- 配置モード：フレームワーク依存
- ターゲット ランタイム：ポータブル（または linux-x64）

> Visual Studio が .NET 10 に対応していない場合は、コマンドで発行して zip で配置する。
> ```
> dotnet publish Bmcs/Bmcs.csproj -c Release -o publish
> （publish フォルダの中身を zip にして、ポータルの「高度なツール（Kudu）」→ Zip Push Deploy、
>   または Azure CLI の `az webapp deploy --src-path publish.zip --type zip` で配置する）
> ```

---

## 段階2：新しいアプリを確認する
`https://bmcs-web.azurewebsites.net`（1-2 で付けた名前）で確認する。**本番のDBにつながっているので、データを作る操作は検証用に留める。**

- [ ] トップ・公開チーム・公開チーム成績・メッセージが表示される
- [ ] 「サンプルチームで体験する」でログインできる
- [ ] 自分のアカウントでログインし、試合一覧・メンバー一覧・成績が表示される
- [ ] 管理者アカウントで、ユーザ一覧・お問い合わせ一覧・アンケート集計が表示される（課題 M-13）
- [ ] ログイン状態が、アプリの再起動（ポータルの「再起動」）後も維持される（セッションが DB に保存されていること）
- [ ] 「この画面の使い方」（ヘルプ）が表示される
- [ ] スマホで表示が崩れない

> 旧アプリと新しいアプリが同じDBを使うため、この間に旧アプリで入力された内容も新しいアプリに表示される（問題ない）。

---

## 段階3：Cloudflare の DNS と独自ドメイン

### 3-1. App Service にドメインを追加する
新しいアプリ →「カスタム ドメイン」→「カスタム ドメインを追加」
- ドメイン プロバイダー：**その他のすべてのドメイン サービス**
- TLS/SSL 証明書：**App Service マネージド証明書**
- TLS/SSL の種類：SNI SSL
- ドメイン：`bmcs.app`

画面に、DNS に登録する値が表示される（控えておく）。
- **A レコード**：`@` → IPアドレス（例 `20.xx.xx.xx`）
- **TXT レコード**：`asuid` → 検証ID（長い文字列）

### 3-2. Cloudflare に DNS レコードを登録する
Cloudflare のダッシュボード → `bmcs.app` →「DNS」→「Records」→「Add record」

| Type | Name | Content | Proxy status |
| --- | --- | --- | --- |
| A | `@` | 3-1 のIPアドレス | **DNS only（灰色の雲）** |
| TXT | `asuid` | 3-1 の検証ID | （設定なし） |

> **Proxy status は必ず「DNS only」にする。** オレンジの雲（Proxied）だと、App Service が証明書を発行できない。

### 3-3. 検証と証明書
- App Service の画面に戻り「検証」→「追加」
- 数分〜数十分で証明書が発行され、`https://bmcs.app` が開けるようになる
- 「カスタム ドメイン」の一覧で、`bmcs.app` の状態が「セキュリティ保護済み」になっていることを確認する

### 3-4.（任意）www も使う場合
`www.bmcs.app` でも開けるようにするなら、同じ手順で追加する。
- Cloudflare：CNAME `www` → `bmcs-web.azurewebsites.net`（DNS only）、TXT `asuid.www` → 検証ID
- App Service：カスタムドメイン `www.bmcs.app` を追加（マネージド証明書）
- `www` へのアクセスは、段階4の設定で `bmcs.app` に自動で転送される

---

## 段階4：新しいアプリに正規ホスト名を設定する
新しいアプリの「環境変数」に追加する。

| 名前 | 値 |
| --- | --- |
| `Site__CanonicalHost` | `bmcs.app` |

これで、`bmcs-web.azurewebsites.net` や `www.bmcs.app` で来たアクセスは `https://bmcs.app` へ転送される（301）。

- [ ] `https://bmcs.app` が開く
- [ ] `https://bmcs-web.azurewebsites.net/Team` を開くと `https://bmcs.app/Team` に移る

---

## 段階5：旧アプリ（Windows）を転送専用にする
旧URL（`bmcs.azurewebsites.net`）へのアクセス・共有済みのリンク・検索エンジンの評価を、新ドメインへ引き継ぐ。

1. 旧アプリの「環境変数」に `Site:CanonicalHost` = `bmcs.app` を追加する（**Windows なので「:」のままでよい**。「__」でも可）
2. 旧アプリに、今回のコード（.NET 10 版）を発行する
   - 旧アプリの「構成」→「スタックの設定」で .NET のバージョンを **.NET 10** にする
   - 一覧に .NET 10 が無い場合は、**自己完結型**（配置モード：自己完結、ターゲット ランタイム：win-x86 または win-x64）で発行する
3. 確認
   - [ ] `https://bmcs.azurewebsites.net/` を開くと `https://bmcs.app/` に移る
   - [ ] `https://bmcs.azurewebsites.net/Score?scorePageClass=Index&isPublic=true` を開くと、同じパスで `https://bmcs.app/…` に移る

> **影響：ログイン中の利用者は、新ドメインで一度ログインし直しになる**（ログイン状態の Cookie はドメインごとのため）。
> 入力中のデータが失われないよう、利用の少ない時間帯（平日の昼など）に行う。
>
> 旧アプリは無料プラン（F1）のまま残してよい（転送だけなら CPU の制限にかからない）。
> 旧URLが検索結果から消えるまで（数か月）は削除しない。

**戻す場合**：旧アプリの `Site:CanonicalHost` を削除して再起動する（旧アプリが通常どおり動く）。

---

## 段階6：後片付け
- [ ] Microsoft Clarity：プロジェクトの設定でサイトのURLを `https://bmcs.app` に変更する
- [ ] Google Search Console に `bmcs.app` を登録する（Cloudflare の DNS に TXT を追加して所有権を確認する）。旧URLのプロパティがあれば「アドレス変更ツール」を使う
- [ ] README の本番URLを `https://bmcs.app/` に変える（コードの変更。依頼があれば対応する）
- [ ] 課題一覧の D-11・P-6 を対応済みにする
- [ ] 1か月ほど様子を見て問題が無ければ、旧アプリの App Service プランは無料のまま残す（削除はしない）

## 費用の見込み
| 項目 | 月額 |
| --- | --- |
| App Service（Linux B1、新） | 約2,200円 |
| App Service（Windows F1、旧・転送用） | 無料 |
| Azure SQL Database（Basic、変更なし） | 約850円 |
| ドメイン（bmcs.app、$14.20/年） | 約180円（年額の月割り） |
| **合計** | **約3,200円** |
