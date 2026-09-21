# 詳細設計書 - ユーザー・チーム管理

## 1. ユーザー登録 (G002)

### 1.1 画面レイアウト
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| ユーザーID | UserAccount.UserAccountID | Text | Yes | 一意制約あり |
| 名前 | UserAccount.UserAccountName | Text | Yes | |
| パスワード | UserAccount.Password | Password | Yes | |
| 確認用パスワード | UserAccount.ConfirmPassword | Password | Yes | パスワードと一致すること |
| メールアドレス | UserAccount.EmailAddress | Email | Yes | ID・パスワードを忘れた際の復旧に使用するため必須 |
| チームID | UserAccount.TeamID | Select | No | 既存チームに参加する場合に選択 |
| チームパスワード | UserAccount.TeamPassword | Password | No | チームID選択時は必須 |
| 利用規約同意 | - | Checkbox | Yes | |
| プライバシーポリシー同意 | - | Checkbox | Yes | |

### 1.2 処理ロジック
1. **入力チェック**:
    - 必須項目、型チェック、パスワード一致チェック。
    - ユーザーIDの重複チェック (`UserAccounts` テーブル)。
    - メールアドレスの必須・書式チェック。
    - チームIDが指定されている場合、チームパスワードの照合 (`Teams` テーブル)。
2. **データ保存**:
    - パスワードをハッシュ化して `UserAccount` オブジェクトを作成。
    - `UserAccounts` テーブルに INSERT。
3. **セッション設定**:
    - 管理者でない場合、`UserAccountID` と `TeamID` をセッションに保存。
4. **画面遷移**:
    - チーム未所属の場合: チーム登録画面 (`/Team/Create`) へリダイレクト。
    - チーム所属済みの場合: トップページ (`/Top/Index`) へリダイレクト。

## 2. チーム登録 (G012)

### 2.1 画面レイアウト
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| チームID | Team.TeamID | Text | Yes | 一意制約、50文字以内 |
| チーム名 | Team.TeamName | Text | Yes | 50文字以内 |
| チーム略名 | Team.TeamAbbreviation | Text | Yes | 10文字以内 |
| パスワード | Team.TeamPassword | Password | Yes | |
| 確認用パスワード | Team.ConfirmTeamPassword | Password | Yes | |
| 公開フラグ | Team.PublicFLG | Checkbox | Yes | デフォルトON |
| 代表者名 | Team.RepresentativeName | Text | No | |
| カテゴリ | Team.TeamCategoryClass | Select | No | 草野球、少年野球など |
| 使用球 | Team.UseBallClass | Select | No | 軟式M号、硬式など |
| 活動拠点 | Team.ActivityBase | Text | No | |
| チーム人数 | Team.TeamNumber | Number | No | |
| メールアドレス | Team.TeamEmailAddress | Email | No | チームパスワード再設定に使用。未登録の場合は問い合わせ対応となる |
| メッセージ | Team.MessageDetail | TextArea | No | |

### 2.2 処理ロジック
1. **入力チェック**:
    - 必須項目、文字数、パスワード一致チェック。
    - チームIDの重複チェック (`Teams` テーブル)。
2. **データ保存**:
    - パスワードをハッシュ化。
    - `Teams` テーブルに INSERT。
    - ログインユーザーの `UserAccount` レコードを取得し、`TeamID` を更新。
3. **セッション設定**:
    - `TeamID` をセッションに保存。
4. **画面遷移**:
    - トップページ (`/Top/Index`) へリダイレクト。

## 3. ログイン (G001)
※トップページ等から遷移

### 3.1 処理ロジック
1. **認証**:
    - ユーザーIDとパスワードで `UserAccounts` テーブルを検索。
    - パスワードはハッシュ化して比較。
2. **セッション設定**:
    - 認証成功時、`UserAccountID` と `TeamID` をセッションに保存。

## 4. アカウント復旧

ユーザID・パスワードを忘れた場合の復旧手段。メールアドレスの登録状況に応じて2つの経路を用意する。

```
[ログイン画面]
   ├─ パスワードをお忘れの場合 ───→ 4.1 ForgotPassword ──(メールのURL)──→ 4.2 ResetPassword
   ├─ ユーザIDをお忘れの場合 ─────→ 4.3 ForgotUserAccountID
   └─(4.1から)メール未登録の場合 ──→ 4.4 RecoverByTeam
                                        └─ チームパスワード不明 ─→ 4.5 ForgotTeamPassword ──(メールのURL)──→ 4.6 ResetTeamPassword
```

### 共通仕様
- **アカウントの有無を画面に出さない**（4.1・4.3・4.5）：入力値が一致しない場合も一致した場合と同じ完了メッセージを表示する。
  ※4.4はチームパスワード認証のため、認証失敗をエラーとして表示する（チームIDの誤りとパスワードの誤りは区別しない）。
- **試行回数制限**（`Function/RateLimiter.cs`、メモリ保持）
    - チーム認証：IP単位・チーム単位で**失敗のみ**を計上し、10分間に10回で一時停止。認証成功時はリセット。
    - メール送信を伴う受付：IP単位で10分間に10回（成功・失敗を問わず計上）＋対象（ユーザID／メールアドレス／チームID）単位で60分間に3回。
    - 問い合わせ登録：IP単位で60分間に5回。
    - キーは `Trim().ToUpperInvariant()` で正規化する（DBの照合順序が大文字小文字・末尾空白を区別しないため、生値では回避されてしまう）。
- **メール本文**：ユーザ入力値は `Function/MailBody.Escape()` でHTMLエスケープしてから埋め込む。
- **絶対URL**：`EmailSettings:BaseUrl` を優先して生成する（未設定時はリクエストのホスト）。
- **アクセス元IP**：リバースプロキシ配下でも実IPを取得するため、`UseForwardedHeaders`（X-Forwarded-For）を有効にしている。
- **更新系POSTのログイン必須化**：`PageModelBase.OnPageHandlerExecuting` で、POST時はログインを必須とする。
  未ログインで使用する画面（ログイン・ユーザ作成・問い合わせ・本章の6画面）のみ `AllowAnonymousPost` を true にする。

### 制約事項（仕様として許容しているもの）
- **再設定の妨害**：対象単位の回数制限は、ユーザIDやメールアドレスの一致を判定する前に計上する。
  そのため第三者が対象のユーザIDを知っていれば、3回送信することで60分間は再設定の申請を妨害できる。
  （一致判定後に計上すると、存在しないユーザIDへの総当たりが無制限になるため、妨害耐性より大量送信防止を優先している）
- **試行回数制限の保持先**：アプリのメモリ上で管理するため、再起動でリセットされる。
  複数インスタンスへスケールアウトする場合はインスタンス毎の制限となる。

### 4.1 パスワード再設定の申請 (ForgotPassword)
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| ユーザID | UserAccountID | Text | Yes | |
| メールアドレス | EmailAddress | Email | Yes | ユーザに登録済みのものと一致すること |

処理ロジック
1. 入力チェック → 試行回数制限のチェック。
2. ユーザID・メールアドレスの両方が一致する有効なユーザを検索（`DeleteFLG = false`）。
3. 一致した場合、そのユーザの未使用トークンを使用済に更新し、新規トークンを発行（区分1、有効期限1時間）。
4. 再設定用URL（`/UserAccount/ResetPassword?token=...`）をメール送信。送信失敗はログのみ。
5. 一致・不一致に関わらず完了メッセージを表示。

### 4.2 パスワードの再設定 (ResetPassword)
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| トークン | Token | Hidden | Yes | クエリ文字列で受領 |
| 新しいパスワード | Password | Password | Yes | |
| 確認用パスワード | ConfirmPassword | Password | Yes | 新しいパスワードと一致すること |

処理ロジック
1. トークンの有効性を判定（区分1、未使用、期限内）。無効な場合は再申請を案内。
2. トークンの `TargetID` からユーザを取得（リクエスト側の入力は使用しない）。
3. パスワードをハッシュ化して更新し、トークンを使用済に更新（同一トランザクション）。
4. 競合（`DbUpdateConcurrencyException`）時は無効なURLとして扱う。
5. セッションをクリアし、本人へ変更完了を通知するメールを送信。

### 4.3 ユーザID確認 (ForgotUserAccountID)
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| メールアドレス | EmailAddress | Email | Yes | |

処理ロジック
1. 入力チェック → 試行回数制限のチェック。
2. 該当メールアドレスの有効なユーザを検索し、ユーザIDの一覧をメール送信（登録値宛）。
3. 該当なしの場合も同じ完了メッセージを表示。

### 4.4 チーム情報による復旧 (RecoverByTeam)
メールアドレス未登録のユーザ向けの経路。

| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| チームID | RecoverTeamID | Text | Yes | |
| チームパスワード | TeamPassword | Password | Yes | 一覧表示時・再設定時の両方で入力（HTMLに平文を残さないため） |
| 対象ユーザ | SelectedUserAccountID | Radio | Yes | 一覧から選択 |
| 新しいパスワード | Password | Password | Yes | |
| 確認用パスワード | ConfirmPassword | Password | Yes | |

処理ロジック
1. 入力チェック（入力ミスで試行回数を消費しないよう、制限判定より先に実施）。
2. チームID・チームパスワードを照合（`DeleteFLG = false`、`SystemDataFLG = false`）。失敗時のみ回数を計上。
3. 復旧対象のユーザ一覧を取得。除外条件は次の2つ。
    - 管理者アカウント（`SystemConstant.AdminUserAccountID`）… チームパスワードからの権限奪取を防ぐため
    - **メールアドレス登録済みのユーザ** … 4.1の経路があるため。チームパスワードを知る第三者による乗っ取りを防ぐ
4. 再設定時もチームパスワードを再照合し、選択ユーザが一覧に含まれることを確認したうえで更新。
5. 競合時はやり直しを促す。
6. チームにメールアドレスが登録されている場合、再設定した旨をチーム宛に通知する（第三者による不正な再設定を検知するため）。

制約事項
- チームパスワードはチーム内の共有情報のため、**同一チームのメールアドレス未登録ユーザ同士は互いにパスワードを再設定できる**。画面に警告を表示し、メールアドレスの登録を促す。

### 4.5 チームパスワード再設定の申請 (ForgotTeamPassword)
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| チームID | TeamID | Text | Yes | |
| チームのメールアドレス | TeamEmailAddress | Email | Yes | チームに登録済みのものと一致すること |

処理ロジック
1. 入力チェック → 試行回数制限のチェック。
2. チームID・メールアドレスの両方が一致するチームを検索（`DeleteFLG = false`、`SystemDataFLG = false`）。
3. 一致した場合、未使用トークンを使用済に更新し、新規トークンを発行（区分2、有効期限1時間）。
4. 再設定用URL（`/Team/ResetTeamPassword?token=...`）をメール送信。
    - **この時点ではチームパスワードを変更しない**（第三者の申請による一方的な変更を防ぐため）。
5. 一致・不一致に関わらず完了メッセージを表示。

補足
- チームにログインできるユーザがいる場合は、チーム情報編集画面（`/Team/Edit`）から変更できる。
- チームにメールアドレスが未登録の場合は問い合わせ対応となる。

### 4.6 チームパスワードの再設定 (ResetTeamPassword)
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| トークン | Token | Hidden | Yes | クエリ文字列で受領 |
| 新しいチームパスワード | TeamPassword | Password | Yes | |
| 確認用チームパスワード | ConfirmTeamPassword | Password | Yes | 新しいチームパスワードと一致すること |

処理ロジック
1. トークンの有効性を判定（区分2、未使用、期限内）。
2. トークンの `TargetID` からチームを取得し、チームパスワードを更新、トークンを使用済に更新。
3. 競合時は無効なURLとして扱う。
4. チームのメールアドレス宛に変更完了を通知。

## 5. ユーザー情報編集 (G095)

### 5.1 画面レイアウト
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| ユーザーID | UserAccount.UserAccountID | Text | - | 変更不可 |
| 名前 | UserAccount.UserAccountName | Text | Yes | |
| パスワード | UserAccount.Password | Password | No | 入力時のみ変更 |
| 確認用パスワード | UserAccount.ConfirmPassword | Password | No | パスワードと一致すること |
| メールアドレス | UserAccount.EmailAddress | Email | No | **登録済みの場合は空にできない**（変更は可能） |
| チームID | UserAccount.TeamID | Select | No | 変更時はチームパスワードの照合が必要 |
| チームパスワード | UserAccount.TeamPassword | Password | No | チーム変更時は必須 |

### 5.2 処理ロジック
1. **入力チェック**:
    - 必須項目、パスワード一致チェック、メールアドレスの書式チェック。
    - メールアドレスが登録済みの場合、空への更新を禁止する。
      ※未登録のまま利用している既存ユーザに影響を与えないため、新規登録のみ必須としている。
    - チームID変更時はチームパスワードの照合。
2. **データ保存**:
    - パスワードは入力があった場合のみハッシュ化して更新。
    - `UserAccounts` テーブルを UPDATE。
