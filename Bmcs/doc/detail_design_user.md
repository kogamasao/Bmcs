# 詳細設計書 - ユーザー・チーム管理

## 1. ユーザー登録 (G002)

### 1.1 画面レイアウト
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| ユーザーID | UserAccount.UserAccountID | Text | Yes | 一意制約あり |
| 名前 | UserAccount.UserAccountName | Text | Yes | |
| パスワード | UserAccount.Password | Password | Yes | |
| 確認用パスワード | UserAccount.ConfirmPassword | Password | Yes | パスワードと一致すること |
| メールアドレス | UserAccount.EmailAddress | Email | No | |
| チームID | UserAccount.TeamID | Select | No | 既存チームに参加する場合に選択 |
| チームパスワード | UserAccount.TeamPassword | Password | No | チームID選択時は必須 |
| 利用規約同意 | - | Checkbox | Yes | |
| プライバシーポリシー同意 | - | Checkbox | Yes | |

### 1.2 処理ロジック
1. **入力チェック**:
    - 必須項目、型チェック、パスワード一致チェック。
    - ユーザーIDの重複チェック (`UserAccounts` テーブル)。
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
| メールアドレス | Team.TeamEmailAddress | Email | No | |
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
