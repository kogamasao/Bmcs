# 詳細設計書 - メッセージ管理

## 1. メッセージ一覧・投稿 (G040)

### 1.1 画面レイアウト
| 項目名 | 物理名 | 表示内容 | 備考 |
| --- | --- | --- | --- |
| フィルタタブ | - | Link | 公開、チーム公開、関連チーム、非公開 |
| **投稿フォーム** | | | |
| タイトル | Message.MessageTitle | Text | 新規投稿時のみ |
| 本文 | Message.MessageDetail | TextArea | |
| 送信先チーム | Message.PrivateTeamID | Select | 指定すると非公開メッセージ |
| 送信ボタン | - | Button | 投稿 または 返信 |
| **メッセージ一覧** | | | |
| タイトル | Message.MessageTitle | Text | |
| チーム名 | Team.TeamName | Link | |
| 投稿者 | UserAccount.UserAccountName | Text | |
| 本文 | Message.MessageDetail | Text | HTML表示 |
| 更新日時 | Message.UpdateDatetime | Text | |
| 返信件数 | Message.ReplyCount | Text | |
| 操作 | - | Button | 確認＆返信 |

### 1.2 処理ロジック
1.  **データ取得**:
    - `messagePageClass` (フィルタ) に応じて取得するメッセージを切り替え。
        - **Public**: 全体の公開メッセージ。
        - **PublicTeam**: 自チームの公開メッセージ。
        - **RelatedTeam**: 自チームまたは送信先が自チームのメッセージ。
        - **Private**: 非公開メッセージ。
    - `messageID` が指定された場合（返信モード）、そのメッセージと関連する返信を表示。
2.  **投稿/返信処理**:
    - `OnPostAsync` で処理。
    - 新規投稿 (`MessageID` なし) または 返信 (`MessageID` あり) を判定。
    - 返信の場合、親メッセージの `ReplyCount` を更新 (+1)。
    - 送信先チームが指定された場合、`PrivateTeamID` を設定し、`PublicFLG` を false に設定。
3.  **権限制御**:
    - ログイン必須。
    - 非公開チームの場合、公開投稿や他チームへのメッセージ送信は不可。

## 2. メッセージ詳細・返信 (G041)
※機能的には「メッセージ一覧・投稿」画面内で `MessageID` を指定した状態として実装。

### 2.1 画面レイアウト
- **親メッセージ表示**: スレッドの親となるメッセージを表示。
- **返信一覧**: 時系列順に返信メッセージを表示。
- **返信フォーム**: タイトルは「返信」固定、本文入力のみ。

### 2.2 処理ロジック
- 親メッセージID (`ParentMessageID`) または自身のID (`MessageID`) が一致するレコードを取得。
- 投稿日時 (`EntryDatetime`) の昇順で表示。
