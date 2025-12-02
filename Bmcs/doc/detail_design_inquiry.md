# 詳細設計書 - 問い合わせ

## 1. 問い合わせ登録 (G100)

### 1.1 画面レイアウト
| 項目名 | 物理名 | 型 | 必須 | 備考 |
| --- | --- | --- | --- | --- |
| メールアドレス | Inquiry.EmailAddress | Text | Yes | ログイン時は自動入力 |
| タイトル | Inquiry.InquiryTitle | Text | Yes | |
| 内容 | Inquiry.InquiryDetail | TextArea | Yes | |
| 登録ボタン | - | Button | | |

### 1.2 処理ロジック
1.  **初期表示 (`OnGetAsync`)**:
    - ログインしている場合、ユーザーアカウントのメールアドレスを初期値として設定。
2.  **登録処理 (`OnPostAsync`)**:
    - 入力値を `Inquirys` テーブルに保存。
    - `ReplyFLG` (返信済フラグ) と `CompleteFLG` (完了フラグ) は `false` で初期化。
    - 登録完了後、トップページへリダイレクト。

## 2. 問い合わせ一覧 (G101 - 管理者用)

### 2.1 画面レイアウト
| 項目名 | 物理名 | 表示内容 | 備考 |
| --- | --- | --- | --- |
| メールアドレス | Inquiry.EmailAddress | Text | |
| タイトル | Inquiry.InquiryTitle | Text | |
| 内容 | Inquiry.InquiryDetail | Text | |
| 返信済 | Inquiry.ReplyFLG | Checkbox | |
| 完了 | Inquiry.CompleteFLG | Checkbox | |
| 登録者ID | Inquiry.EntryUserID | Text | |
| 登録日時 | Inquiry.EntryDatetime | Text | |
| 操作 | - | Button | 詳細、削除 |

### 2.2 処理ロジック
1.  **権限チェック**:
    - 管理者 (`IsAdmin`) でない場合、404エラーまたはアクセス拒否。
2.  **データ取得**:
    - 全ての問い合わせデータを取得し、一覧表示。

## 3. 問い合わせ詳細・削除 (G102, G103 - 管理者用)

### 3.1 詳細画面
- 問い合わせ内容の詳細を表示。
- 返信フラグ、完了フラグの更新機能（※現状のソースコードでは詳細画面での更新ロジックは確認できず、表示のみの可能性あり。必要に応じて実装検討）。

### 3.2 削除画面
- 問い合わせデータを物理削除。
