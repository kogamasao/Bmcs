# 詳細設計書 - メンバー管理

## 1. メンバー一覧 (G020)

### 1.1 画面レイアウト
| 項目名 | 物理名 | 表示内容 | 備考 |
| --- | --- | --- | --- |
| 背番号 | Member.UniformNumber | Text | |
| 名前 | Member.MemberName | Text | |
| メンバー区分 | Member.MemberClassName | Text | 選手、監督、コーチなど |
| 投 | Member.ThrowClassName | Text | 右投、左投 |
| 打 | Member.BatClassName | Text | 右打、左打 |
| ポジション | Member.PositionGroupClassName | Text | 投手、捕手、内野手、外野手 |
| メッセージ | Member.MessageDetail | Text | |
| 操作 | - | Button | 編集、詳細、削除（権限による） |

### 1.2 処理ロジック
1.  **データ取得**:
    - `teamID` パラメータで指定されたチームのメンバーを取得。
    - 削除フラグ (`DeleteFLG`) が false のレコードのみ。
    - 公開フラグ (`PublicFLG`) が true または自分のチームの場合のみ表示。
2.  **ソート順**:
    - メンバー区分 > ポジション > 背番号 の順で昇順。
3.  **ページネーション**:
    - 1ページあたり20件表示。
4.  **権限制御**:
    - 自分のチームまたは管理者の場合のみ、追加・編集・削除ボタンを表示。

## 2. メンバー登録 (G022)

### 2.1 画面レイアウト
| 項目名 | 物理名 | 型 | 必須 | 制約・備考 |
| --- | --- | --- | --- | --- |
| 背番号 | Member.UniformNumber | Text | No | 3文字以内 |
| 名前 | Member.MemberName | Text | Yes | 50文字以内 |
| メンバー区分 | Member.MemberClass | Select | No | |
| 投 | Member.ThrowClass | Select | No | |
| 打 | Member.BatClass | Select | No | |
| ポジション | Member.PositionGroupClass | Select | No | |
| メッセージ | Member.MessageDetail | TextArea | No | |

### 2.2 処理ロジック
1.  **権限チェック**:
    - ログイン済みであること。
    - 自分のチームまたは管理者であること。
2.  **データ保存**:
    - `Members` テーブルに INSERT。
    - `Teams` テーブルの `TeamNumber` (チーム人数) を更新 (+1)。
3.  **画面遷移**:
    - メンバー一覧画面 (`/Member/Index`) へリダイレクト。
