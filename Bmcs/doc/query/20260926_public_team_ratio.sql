/*
 * 公開チーム・非公開チームの件数（参照のみ）
 *
 * 目的：公開チームへの誘導（issues.md P-10〜P-12）の前後で、公開の割合がどう変わったかを比べる。
 *       施策を入れる前に1回実行し、結果を課題一覧に記録しておく。
 *
 * ※システムデータ（SystemDataFLG）・削除済みのチームは除く。
 * ※「最近使っている」は、最終ログイン日時（Team.LastLoginDatetime）が90日以内のチーム。
 *   2026-09-22 より前のデータは推定値（20260922_session_lastlogin.sql）のため、目安として見る。
 */

-- 1) 全体と、最近使っているチーム
SELECT 対象 = N'全チーム'
     , 公開 = SUM(CASE WHEN PublicFLG = 1 THEN 1 ELSE 0 END)
     , 非公開 = SUM(CASE WHEN PublicFLG = 0 THEN 1 ELSE 0 END)
     , 公開の割合 = CAST(100.0 * SUM(CASE WHEN PublicFLG = 1 THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0) AS DECIMAL(5, 1))
  FROM dbo.Team
 WHERE SystemDataFLG = 0 AND DeleteFLG = 0
UNION ALL
SELECT N'最近90日に使っているチーム'
     , SUM(CASE WHEN PublicFLG = 1 THEN 1 ELSE 0 END)
     , SUM(CASE WHEN PublicFLG = 0 THEN 1 ELSE 0 END)
     , CAST(100.0 * SUM(CASE WHEN PublicFLG = 1 THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0) AS DECIMAL(5, 1))
  FROM dbo.Team
 WHERE SystemDataFLG = 0 AND DeleteFLG = 0
   AND LastLoginDatetime >= DATEADD(DAY, -90, SYSDATETIME())
UNION ALL
SELECT N'確定済みの試合が1試合以上あるチーム'
     , SUM(CASE WHEN t.PublicFLG = 1 THEN 1 ELSE 0 END)
     , SUM(CASE WHEN t.PublicFLG = 0 THEN 1 ELSE 0 END)
     , CAST(100.0 * SUM(CASE WHEN t.PublicFLG = 1 THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0) AS DECIMAL(5, 1))
  FROM dbo.Team t
 WHERE t.SystemDataFLG = 0 AND t.DeleteFLG = 0
   AND EXISTS (SELECT 1 FROM dbo.Game g WHERE g.TeamID = t.TeamID AND g.DeleteFLG = 0 AND g.StatusClass IN (9, 10));

-- 2) カテゴリ別（小学生などのチームで非公開が多いか。P-12 の既定値の判断に使う）
--    カテゴリ：1=小学生 2=中学生 3=高校生 4=大学生 5=社会人 6=独立リーグ 7=プロ 9=その他（NULL=未設定）
SELECT カテゴリ = TeamCategoryClass
     , 公開 = SUM(CASE WHEN PublicFLG = 1 THEN 1 ELSE 0 END)
     , 非公開 = SUM(CASE WHEN PublicFLG = 0 THEN 1 ELSE 0 END)
  FROM dbo.Team
 WHERE SystemDataFLG = 0 AND DeleteFLG = 0
 GROUP BY TeamCategoryClass
 ORDER BY TeamCategoryClass;
