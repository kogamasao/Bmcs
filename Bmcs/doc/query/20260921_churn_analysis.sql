/*
 * 離脱チームの「到達段階」を集計する（参照のみ。更新はしない）
 *
 * 目的：登録直後に離脱したチームが、どの画面で止まったのかを確定させる。
 *       この結果によって、次に直すべき画面が変わる。
 *
 * 実行方法：Azureポータルのクエリエディター、または SSMS で上から順に実行する。
 *          SELECT のみのため、本番データへの影響はない。
 *
 * 判定の考え方：
 *   チームの「最終活動日時」を、そのチームに紐づく全データの更新日時の最大値とする。
 *   最終活動日時 - チーム登録日時 が1日未満のチームを「1日で離脱」とみなす。
 */

--------------------------------------------------------------------------------
-- 1) 全体像：チームを「継続期間」で分類する
--------------------------------------------------------------------------------
WITH TeamActivity AS (
    SELECT t.TeamID
         , t.TeamName
         , t.TeamCategoryClass
         , t.EntryDatetime
         , LastActivity =
           (SELECT MAX(v) FROM (VALUES
                (t.UpdateDatetime)
              , ((SELECT MAX(m.UpdateDatetime) FROM dbo.Member    m WHERE m.TeamID = t.TeamID))
              , ((SELECT MAX(g.UpdateDatetime) FROM dbo.Game      g WHERE g.TeamID = t.TeamID))
              , ((SELECT MAX(o.UpdateDatetime) FROM dbo.[Order]   o WHERE o.TeamID = t.TeamID))
              , ((SELECT MAX(s.UpdateDatetime) FROM dbo.GameScene s WHERE s.TeamID = t.TeamID))
           ) AS x(v))
      FROM dbo.Team t
     WHERE t.DeleteFLG = 0
       AND t.SystemDataFLG = 0   -- サンプルチームを除外する
)
SELECT 継続期間 =
       CASE WHEN DATEDIFF(HOUR, EntryDatetime, LastActivity) < 24  THEN N'1) 1日未満'
            WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 7   THEN N'2) 1週間未満'
            WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 30  THEN N'3) 1ヶ月未満'
            WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 180 THEN N'4) 半年未満'
            ELSE                                                        N'5) 半年以上'
       END
     , チーム数 = COUNT(*)
  FROM TeamActivity
 GROUP BY CASE WHEN DATEDIFF(HOUR, EntryDatetime, LastActivity) < 24  THEN N'1) 1日未満'
               WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 7   THEN N'2) 1週間未満'
               WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 30  THEN N'3) 1ヶ月未満'
               WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 180 THEN N'4) 半年未満'
               ELSE                                                       N'5) 半年以上'
          END
 ORDER BY 1;


--------------------------------------------------------------------------------
-- 2) 本題：1日で離脱したチームは、どこまで到達したのか
--    ※ここが今回いちばん知りたいこと
--------------------------------------------------------------------------------
WITH TeamActivity AS (
    SELECT t.TeamID
         , t.EntryDatetime
         , LastActivity =
           (SELECT MAX(v) FROM (VALUES
                (t.UpdateDatetime)
              , ((SELECT MAX(m.UpdateDatetime) FROM dbo.Member    m WHERE m.TeamID = t.TeamID))
              , ((SELECT MAX(g.UpdateDatetime) FROM dbo.Game      g WHERE g.TeamID = t.TeamID))
              , ((SELECT MAX(o.UpdateDatetime) FROM dbo.[Order]   o WHERE o.TeamID = t.TeamID))
              , ((SELECT MAX(s.UpdateDatetime) FROM dbo.GameScene s WHERE s.TeamID = t.TeamID))
           ) AS x(v))
      FROM dbo.Team t
     WHERE t.DeleteFLG = 0
       AND t.SystemDataFLG = 0
)
, TeamProgress AS (
    SELECT a.TeamID
         , MemberCount    = (SELECT COUNT(*) FROM dbo.Member    m WHERE m.TeamID = a.TeamID AND m.DeleteFLG = 0)
         , GameCount      = (SELECT COUNT(*) FROM dbo.Game      g WHERE g.TeamID = a.TeamID AND g.DeleteFLG = 0)
         , OrderCount     = (SELECT COUNT(*) FROM dbo.[Order]   o WHERE o.TeamID = a.TeamID)
         , GameSceneCount = (SELECT COUNT(*) FROM dbo.GameScene s WHERE s.TeamID = a.TeamID)
      FROM TeamActivity a
     WHERE DATEDIFF(HOUR, a.EntryDatetime, a.LastActivity) < 24
)
SELECT 到達段階 =
       CASE WHEN MemberCount    = 0 THEN N'1) チームを作っただけ（選手登録なし）'
            WHEN GameCount      = 0 THEN N'2) 選手は登録したが、試合を作っていない'
            WHEN OrderCount     = 0 THEN N'3) 試合は作ったが、打順を組んでいない'
            WHEN GameSceneCount = 0 THEN N'4) 打順は組んだが、スコアを1球も入れていない'
            WHEN GameCount      = 1 THEN N'5) 1試合は入力したが、2試合目がない'
            ELSE                         N'6) 2試合以上入力した（それでも離脱）'
       END
     , チーム数     = COUNT(*)
     , 平均選手数   = AVG(MemberCount)
     , 平均プレー数 = AVG(GameSceneCount)
  FROM TeamProgress
 GROUP BY CASE WHEN MemberCount    = 0 THEN N'1) チームを作っただけ（選手登録なし）'
               WHEN GameCount      = 0 THEN N'2) 選手は登録したが、試合を作っていない'
               WHEN OrderCount     = 0 THEN N'3) 試合は作ったが、打順を組んでいない'
               WHEN GameSceneCount = 0 THEN N'4) 打順は組んだが、スコアを1球も入れていない'
               WHEN GameCount      = 1 THEN N'5) 1試合は入力したが、2試合目がない'
               ELSE                         N'6) 2試合以上入力した（それでも離脱）'
          END
 ORDER BY 1;


--------------------------------------------------------------------------------
-- 3) 補足：スコア入力方式（プレー毎／結果のみ）と定着の関係
--    「結果だけ入力」を初回の推奨経路にすべきか判断するために見る
--------------------------------------------------------------------------------
WITH TeamActivity AS (
    SELECT t.TeamID
         , t.EntryDatetime
         , LastActivity =
           (SELECT MAX(v) FROM (VALUES
                (t.UpdateDatetime)
              , ((SELECT MAX(m.UpdateDatetime) FROM dbo.Member    m WHERE m.TeamID = t.TeamID))
              , ((SELECT MAX(g.UpdateDatetime) FROM dbo.Game      g WHERE g.TeamID = t.TeamID))
              , ((SELECT MAX(o.UpdateDatetime) FROM dbo.[Order]   o WHERE o.TeamID = t.TeamID))
              , ((SELECT MAX(s.UpdateDatetime) FROM dbo.GameScene s WHERE s.TeamID = t.TeamID))
           ) AS x(v))
      FROM dbo.Team t
     WHERE t.DeleteFLG = 0
       AND t.SystemDataFLG = 0
)
SELECT 入力方式 = g.GameInputTypeClass   -- Enum: GameInputTypeClass を参照
     , 区分 = CASE WHEN DATEDIFF(HOUR, a.EntryDatetime, a.LastActivity) < 24
                   THEN N'1日で離脱' ELSE N'継続' END
     , チーム数 = COUNT(DISTINCT g.TeamID)
     , 試合数   = COUNT(*)
  FROM dbo.Game g
 INNER JOIN TeamActivity a ON g.TeamID = a.TeamID
 WHERE g.DeleteFLG = 0
 GROUP BY g.GameInputTypeClass
        , CASE WHEN DATEDIFF(HOUR, a.EntryDatetime, a.LastActivity) < 24
               THEN N'1日で離脱' ELSE N'継続' END
 ORDER BY 1, 2;


--------------------------------------------------------------------------------
-- 4) 補足：1日で離脱したチームのメールアドレス登録率
--    離脱者向けのフォローメールが技術的に何件に送れるのかを確認する
--------------------------------------------------------------------------------
WITH TeamActivity AS (
    SELECT t.TeamID
         , t.EntryDatetime
         , LastActivity =
           (SELECT MAX(v) FROM (VALUES
                (t.UpdateDatetime)
              , ((SELECT MAX(m.UpdateDatetime) FROM dbo.Member    m WHERE m.TeamID = t.TeamID))
              , ((SELECT MAX(g.UpdateDatetime) FROM dbo.Game      g WHERE g.TeamID = t.TeamID))
              , ((SELECT MAX(o.UpdateDatetime) FROM dbo.[Order]   o WHERE o.TeamID = t.TeamID))
              , ((SELECT MAX(s.UpdateDatetime) FROM dbo.GameScene s WHERE s.TeamID = t.TeamID))
           ) AS x(v))
      FROM dbo.Team t
     WHERE t.DeleteFLG = 0
       AND t.SystemDataFLG = 0
)
SELECT 区分 = CASE WHEN DATEDIFF(HOUR, a.EntryDatetime, a.LastActivity) < 24
                   THEN N'1日で離脱' ELSE N'継続' END
     , ユーザ数     = COUNT(*)
     , メール登録済 = SUM(CASE WHEN NULLIF(LTRIM(RTRIM(u.EmailAddress)), N'') IS NOT NULL THEN 1 ELSE 0 END)
  FROM dbo.UserAccount u
 INNER JOIN TeamActivity a ON u.TeamID = a.TeamID
 WHERE u.DeleteFLG = 0
 GROUP BY CASE WHEN DATEDIFF(HOUR, a.EntryDatetime, a.LastActivity) < 24
               THEN N'1日で離脱' ELSE N'継続' END
 ORDER BY 1;
