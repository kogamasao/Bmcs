/*
 * 所属チームIDの大文字小文字・全角半角が、チームの TeamID と違うユーザの確認（参照のみ）
 *
 * 背景：
 *   ユーザ登録のチーム参加をチームIDの入力にした時期（2026-09-21〜）は、入力値をそのまま UserAccount.TeamID に保存していた。
 *   DB の照合順序は大文字小文字・全角半角を区別しないため「yg」でも参加できるが、アプリは TeamID を完全一致で比較する箇所がある。
 *   2026-09-24 以降は DB の値で保存するよう修正した。本SQLは、それ以前に登録されたデータの有無を確認する。
 *
 * ※該当があっても、UserAccount.TeamID だけを直してはいけない。
 *   そのユーザが作成した試合・メンバー等にも同じ値（例「yg」）で保存されているため、片方だけ直すと自チームのデータを開けなくなる。
 *   該当があった場合は、関連テーブルの件数を確認したうえで、まとめて直す方針を別途決める。
 */
SELECT u.UserAccountID
     , ユーザのTeamID = u.TeamID
     , チームのTeamID = t.TeamID
     , u.EntryDatetime
  FROM dbo.UserAccount u
 INNER JOIN dbo.Team t ON u.TeamID = t.TeamID
 WHERE u.TeamID COLLATE Latin1_General_BIN2 <> t.TeamID COLLATE Latin1_General_BIN2;
