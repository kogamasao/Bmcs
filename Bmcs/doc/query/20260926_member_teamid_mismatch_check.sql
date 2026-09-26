/*
 * 選手（Member）のチームIDと、チーム（Team）のチームIDの表記が違うデータの確認（参照のみ）
 *
 * 背景：
 *   DB の照合順序は大文字小文字・全角半角を区別しないため、「JB」のチームの選手が「jb」「ＪＢ」のチームIDで保存されていても外部キーは通る。
 *   公開範囲（選手名を背番号で表示する。issues.md P-12）の判定はアプリでも同じ基準で比べるようにしたが、
 *   自チームかどうかの判定（IsMyTeamData）は完全一致のため、表記が違う選手は自チームのユーザにも背番号で表示される（氏名が漏れる方向ではない）。
 *   2026-09-26 以降、メンバー追加は DB のチームIDで保存するよう修正した。本SQLは、それ以前のデータの有無を確認する。
 *
 * ※該当があっても、Member.TeamID だけを直すかは、関連テーブル（試合・成績など）の件数を見て決める（20260924_teamid_case_check.sql と同じ方針）。
 */
SELECT m.MemberID
     , 選手のTeamID = m.TeamID
     , チームのTeamID = t.TeamID
     , m.MemberName
     , m.EntryDatetime
  FROM dbo.Member m
 INNER JOIN dbo.Team t ON m.TeamID = t.TeamID
 WHERE m.TeamID COLLATE Latin1_General_BIN2 <> t.TeamID COLLATE Latin1_General_BIN2;
