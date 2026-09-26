/*
 * チームの公開範囲（選手名・代表者名）の列を追加する（issues.md P-12）
 *
 * 背景：
 *   以前は公開すると選手の氏名・代表者名まで誰でも見られ、出したくないチームは非公開を選ぶしかなかった。
 *   「選手名を背番号で表示する」「代表者名を公開しない」を選べるようにする。
 *
 * 既存のチームは 0（氏名・代表者名を表示する。列を追加する前と同じ見え方）。
 * ※アプリの起動時にも DbInitializer が同じ列を追加するため、実行順序を誤ってもアプリは動作する（実行しなくてもよい）。
 * 再実行しても安全。GO は使用していない。
 */
IF COL_LENGTH('dbo.Team', 'MemberNameHiddenFLG') IS NULL
    ALTER TABLE dbo.Team ADD MemberNameHiddenFLG bit NOT NULL CONSTRAINT DF_Team_MemberNameHiddenFLG DEFAULT 0;

IF COL_LENGTH('dbo.Team', 'RepresentativeNameHiddenFLG') IS NULL
    ALTER TABLE dbo.Team ADD RepresentativeNameHiddenFLG bit NOT NULL CONSTRAINT DF_Team_RepresentativeNameHiddenFLG DEFAULT 0;

-- 確認
SELECT 選手名を背番号で表示 = SUM(CASE WHEN MemberNameHiddenFLG = 1 THEN 1 ELSE 0 END)
     , 代表者名を公開しない = SUM(CASE WHEN RepresentativeNameHiddenFLG = 1 THEN 1 ELSE 0 END)
     , 全チーム = COUNT(*)
  FROM dbo.Team;
