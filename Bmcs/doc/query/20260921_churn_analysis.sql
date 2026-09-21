/*
 * 離脱チームの「到達段階」を集計する（参照のみ。更新はしない）
 *
 * 目的：登録直後に離脱したチームが、どの画面で止まったのかを確定させる。
 *       この結果によって、次に直すべき画面が変わる。
 *
 * 実行方法：Azureポータルのクエリエディター、または SSMS で上から順に実行する。
 *          SELECT と一時テーブルのみのため、本番データへの影響はない。
 *
 * ■性能について
 *   チームごとに関連テーブルを毎回検索すると、チーム数×テーブル数のスキャンが
 *   発生して Azure SQL の下位プランでは返ってこない。
 *   そのため、**先に1回のGROUP BYで集計してから結合する**。
 *   一時テーブルは接続を切ると消えるため、**このファイル全体を一度に実行する**こと。
 *   （Azureポータルのクエリエディターは実行ごとに接続が変わる場合があるため、
 *     部分実行を繰り返すと「#TeamStat が無い」というエラーになる）
 *   GO は使用していないので、ポータルのクエリエディターでもそのまま実行できる。
 *
 * 判定の考え方：
 *   チームの「最終活動日時」を、そのチームに紐づくデータの更新日時の最大値とする。
 *   最終活動日時 - チーム登録日時 が24時間未満のチームを「1日で離脱」とみなす。
 */

--------------------------------------------------------------------------------
-- 1) 集計用の一時テーブルを作る（ここだけ時間がかかる）
--------------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#TeamStat') IS NOT NULL DROP TABLE #TeamStat;

--テーブルごとに1回だけ集計する
SELECT TeamID
     , LastUpdate = MAX(UpdateDatetime)
     , Cnt        = COUNT(*)
  INTO #MemberStat
  FROM dbo.Member
 WHERE DeleteFLG = 0
 GROUP BY TeamID;

SELECT TeamID
     , LastUpdate = MAX(UpdateDatetime)
     , Cnt        = COUNT(*)
  INTO #GameStat
  FROM dbo.Game
 WHERE DeleteFLG = 0
 GROUP BY TeamID;

SELECT TeamID
     , LastUpdate = MAX(UpdateDatetime)
     , Cnt        = COUNT(*)
  INTO #OrderStat
  FROM dbo.[Order]
 GROUP BY TeamID;

SELECT TeamID
     , LastUpdate = MAX(UpdateDatetime)
     , Cnt        = COUNT(*)
  INTO #SceneStat
  FROM dbo.GameScene
 GROUP BY TeamID;

--チーム単位に1行へまとめる
SELECT t.TeamID
     , t.TeamName
     , t.TeamCategoryClass
     , t.EntryDatetime
     , MemberCount = ISNULL(m.Cnt, 0)
     , GameCount   = ISNULL(g.Cnt, 0)
     , OrderCount  = ISNULL(o.Cnt, 0)
     , SceneCount  = ISNULL(s.Cnt, 0)
     , la.LastActivity
       --1日で離脱したかどうかを、ここで確定させておく
     , IsOneDayChurn = CASE WHEN DATEDIFF(HOUR, t.EntryDatetime, la.LastActivity) < 24 THEN 1 ELSE 0 END
  INTO #TeamStat
  FROM dbo.Team t
  LEFT JOIN #MemberStat m ON t.TeamID = m.TeamID
  LEFT JOIN #GameStat   g ON t.TeamID = g.TeamID
  LEFT JOIN #OrderStat  o ON t.TeamID = o.TeamID
  LEFT JOIN #SceneStat  s ON t.TeamID = s.TeamID
 CROSS APPLY (SELECT LastActivity = MAX(v)
                FROM (VALUES (t.UpdateDatetime)
                           , (m.LastUpdate)
                           , (g.LastUpdate)
                           , (o.LastUpdate)
                           , (s.LastUpdate)) AS x(v)) la
 WHERE t.DeleteFLG = 0
   AND t.SystemDataFLG = 0;   -- サンプルチームを除外する

DROP TABLE #MemberStat;
DROP TABLE #GameStat;
DROP TABLE #OrderStat;
DROP TABLE #SceneStat;

--------------------------------------------------------------------------------
-- 2) 全体像：チームを「継続期間」で分類する
--------------------------------------------------------------------------------
SELECT 継続期間 =
       CASE WHEN DATEDIFF(HOUR, EntryDatetime, LastActivity) < 24  THEN N'1) 1日未満'
            WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 7   THEN N'2) 1週間未満'
            WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 30  THEN N'3) 1ヶ月未満'
            WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 180 THEN N'4) 半年未満'
            ELSE                                                        N'5) 半年以上'
       END
     , チーム数 = COUNT(*)
  FROM #TeamStat
 GROUP BY CASE WHEN DATEDIFF(HOUR, EntryDatetime, LastActivity) < 24  THEN N'1) 1日未満'
               WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 7   THEN N'2) 1週間未満'
               WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 30  THEN N'3) 1ヶ月未満'
               WHEN DATEDIFF(DAY,  EntryDatetime, LastActivity) < 180 THEN N'4) 半年未満'
               ELSE                                                       N'5) 半年以上'
          END
 ORDER BY 1;

--------------------------------------------------------------------------------
-- 3) 本題：1日で離脱したチームは、どこまで到達したのか
--    ※ここが今回いちばん知りたいこと
--------------------------------------------------------------------------------
SELECT 到達段階 =
       CASE WHEN MemberCount = 0 THEN N'1) チームを作っただけ（選手登録なし）'
            WHEN GameCount   = 0 THEN N'2) 選手は登録したが、試合を作っていない'
            WHEN OrderCount  = 0 THEN N'3) 試合は作ったが、打順を組んでいない'
            WHEN SceneCount  = 0 THEN N'4) 打順は組んだが、スコアを1球も入れていない'
            WHEN GameCount   = 1 THEN N'5) 1試合は入力したが、2試合目がない'
            ELSE                      N'6) 2試合以上入力した（それでも離脱）'
       END
     , チーム数     = COUNT(*)
     , 平均選手数   = AVG(MemberCount)
     , 平均プレー数 = AVG(SceneCount)
  FROM #TeamStat
 WHERE IsOneDayChurn = 1
 GROUP BY CASE WHEN MemberCount = 0 THEN N'1) チームを作っただけ（選手登録なし）'
               WHEN GameCount   = 0 THEN N'2) 選手は登録したが、試合を作っていない'
               WHEN OrderCount  = 0 THEN N'3) 試合は作ったが、打順を組んでいない'
               WHEN SceneCount  = 0 THEN N'4) 打順は組んだが、スコアを1球も入れていない'
               WHEN GameCount   = 1 THEN N'5) 1試合は入力したが、2試合目がない'
               ELSE                      N'6) 2試合以上入力した（それでも離脱）'
          END
 ORDER BY 1;

--------------------------------------------------------------------------------
-- 4) 補足：スコア入力方式（プレー毎／結果のみ）と定着の関係
--    「結果だけ入力」を初回の推奨経路にすべきか判断するために見る
--------------------------------------------------------------------------------
SELECT 入力方式 = g.GameInputTypeClass   -- Enum: GameInputTypeClass を参照
     , 区分     = CASE WHEN t.IsOneDayChurn = 1 THEN N'1日で離脱' ELSE N'継続' END
     , チーム数 = COUNT(DISTINCT g.TeamID)
     , 試合数   = COUNT(*)
  FROM dbo.Game g
 INNER JOIN #TeamStat t ON g.TeamID = t.TeamID
 WHERE g.DeleteFLG = 0
 GROUP BY g.GameInputTypeClass
        , CASE WHEN t.IsOneDayChurn = 1 THEN N'1日で離脱' ELSE N'継続' END
 ORDER BY 1, 2;

--------------------------------------------------------------------------------
-- 5) 補足：1日で離脱したチームのメールアドレス登録率
--    離脱者向けのフォローメールが技術的に何件に送れるのかを確認する
--------------------------------------------------------------------------------
SELECT 区分         = CASE WHEN t.IsOneDayChurn = 1 THEN N'1日で離脱' ELSE N'継続' END
     , ユーザ数     = COUNT(*)
     , メール登録済 = SUM(CASE WHEN NULLIF(LTRIM(RTRIM(u.EmailAddress)), N'') IS NOT NULL THEN 1 ELSE 0 END)
  FROM dbo.UserAccount u
 INNER JOIN #TeamStat t ON u.TeamID = t.TeamID
 WHERE u.DeleteFLG = 0
 GROUP BY CASE WHEN t.IsOneDayChurn = 1 THEN N'1日で離脱' ELSE N'継続' END
 ORDER BY 1;

--------------------------------------------------------------------------------
-- 後片付け（同じ接続で続けて実行する場合は最後に）
--------------------------------------------------------------------------------
-- DROP TABLE #TeamStat;
