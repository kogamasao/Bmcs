/*
 * 1) セッションをSQL Serverに保持するためのテーブル
 * 2) 最終ログイン日時（UserAccount・Team）
 *
 * ■実行タイミング
 *   **アプリのデプロイ「前」に実行する。**
 *   SessionCache テーブルが無い状態でデプロイすると、セッションの読み書きで
 *   例外となりログインできない。
 *
 * ■1) セッションテーブルについて
 *   従来はメモリ保持（AddDistributedMemoryCache）だったため、アプリの再起動
 *   （デプロイ・スケール・プラットフォーム保守）で全員がログアウトしていた。
 *   スコア入力中のユーザが弾き出されるため、SQL Server 保持に変更する。
 *   列の定義は Microsoft.Extensions.Caching.SqlServer が要求する形式で固定。
 *
 * ■2) 最終ログイン日時について
 *   長期間使用されていないデータを判別するために保持する。
 *   Team 側にも持たせ、所属ユーザを1人ずつ調べずに未使用チームを抽出できるようにする。
 *   既存データは値が無いため、**関連データの更新日時から推定した値**を設定する
 *   （推定値であることは設計書にも記載する）。
 *
 * ■実行方法
 *   **SSMS または sqlcmd で実行する。**（既存の移行SQLと同様に GO を使用している）
 *   列を追加した直後に参照するため、バッチを区切る必要がある。
 *   Azureポータルのクエリエディターは GO を解釈しないため使用しない。
 */

--------------------------------------------------------------------------------
-- 1) セッション保存用テーブル
--------------------------------------------------------------------------------
IF OBJECT_ID('dbo.SessionCache') IS NULL
BEGIN
    CREATE TABLE dbo.SessionCache
    (
        Id                         nvarchar(449)  NOT NULL
      , Value                      varbinary(MAX) NOT NULL
      , ExpiresAtTime              datetimeoffset NOT NULL
      , SlidingExpirationInSeconds bigint         NULL
      , AbsoluteExpiration         datetimeoffset NULL
      , CONSTRAINT PK_SessionCache PRIMARY KEY (Id)
    );

    --期限切れレコードの削除で使用する
    CREATE NONCLUSTERED INDEX Index_ExpiresAtTime ON dbo.SessionCache (ExpiresAtTime);
END;

--------------------------------------------------------------------------------
-- 2) 最終ログイン日時の列を追加
--------------------------------------------------------------------------------
IF COL_LENGTH('dbo.UserAccount', 'LastLoginDatetime') IS NULL
BEGIN
    ALTER TABLE dbo.UserAccount ADD LastLoginDatetime datetime2(7) NULL;
END;

IF COL_LENGTH('dbo.Team', 'LastLoginDatetime') IS NULL
BEGIN
    ALTER TABLE dbo.Team ADD LastLoginDatetime datetime2(7) NULL;
END;
GO
--※ここでバッチを区切らないと、以降の文で追加した列を参照できない

--------------------------------------------------------------------------------
-- 3) 既存データへ推定値を設定する
--    ※実際のログイン日時は記録していないため、関連データの更新日時の最大値で代用する。
--      性能のため、先にテーブルごとに1回だけ集計してから結合する。
--------------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#TeamLastActivity') IS NOT NULL DROP TABLE #TeamLastActivity;

SELECT TeamID, LastUpdate = MAX(UpdateDatetime) INTO #MemberAct FROM dbo.Member    GROUP BY TeamID;
SELECT TeamID, LastUpdate = MAX(UpdateDatetime) INTO #GameAct   FROM dbo.Game      GROUP BY TeamID;
SELECT TeamID, LastUpdate = MAX(UpdateDatetime) INTO #OrderAct  FROM dbo.[Order]   GROUP BY TeamID;
SELECT TeamID, LastUpdate = MAX(UpdateDatetime) INTO #SceneAct  FROM dbo.GameScene GROUP BY TeamID;

SELECT t.TeamID
     , LastActivity = (SELECT MAX(v) FROM (VALUES (t.UpdateDatetime)
                                                , (m.LastUpdate)
                                                , (g.LastUpdate)
                                                , (o.LastUpdate)
                                                , (s.LastUpdate)) AS x(v))
  INTO #TeamLastActivity
  FROM dbo.Team t
  LEFT JOIN #MemberAct m ON t.TeamID = m.TeamID
  LEFT JOIN #GameAct   g ON t.TeamID = g.TeamID
  LEFT JOIN #OrderAct  o ON t.TeamID = o.TeamID
  LEFT JOIN #SceneAct  s ON t.TeamID = s.TeamID;

DROP TABLE #MemberAct;
DROP TABLE #GameAct;
DROP TABLE #OrderAct;
DROP TABLE #SceneAct;

--チーム：関連データの最終更新日時
UPDATE t
   SET t.LastLoginDatetime = a.LastActivity
  FROM dbo.Team t
 INNER JOIN #TeamLastActivity a ON t.TeamID = a.TeamID
 WHERE t.LastLoginDatetime IS NULL
   AND a.LastActivity IS NOT NULL;

--ユーザ：自身の更新日時と、所属チームの最終活動日時の遅い方
UPDATE u
   SET u.LastLoginDatetime =
       CASE WHEN a.LastActivity IS NULL OR u.UpdateDatetime > a.LastActivity
            THEN u.UpdateDatetime ELSE a.LastActivity END
  FROM dbo.UserAccount u
  LEFT JOIN #TeamLastActivity a ON u.TeamID = a.TeamID
 WHERE u.LastLoginDatetime IS NULL;

DROP TABLE #TeamLastActivity;

--------------------------------------------------------------------------------
-- 4) 確認
--------------------------------------------------------------------------------
SELECT 対象 = N'ユーザ'
     , 件数 = COUNT(*)
     , 設定済 = SUM(CASE WHEN LastLoginDatetime IS NOT NULL THEN 1 ELSE 0 END)
  FROM dbo.UserAccount
 WHERE DeleteFLG = 0
UNION ALL
SELECT 対象 = N'チーム'
     , 件数 = COUNT(*)
     , 設定済 = SUM(CASE WHEN LastLoginDatetime IS NOT NULL THEN 1 ELSE 0 END)
  FROM dbo.Team
 WHERE DeleteFLG = 0;

--未使用チームの分布（削除方針の検討に使用する）
SELECT 最終利用 =
       CASE WHEN LastLoginDatetime IS NULL                       THEN N'9) 不明'
            WHEN LastLoginDatetime >= DATEADD(DAY,   -30, SYSDATETIME()) THEN N'1) 1ヶ月以内'
            WHEN LastLoginDatetime >= DATEADD(DAY,   -90, SYSDATETIME()) THEN N'2) 3ヶ月以内'
            WHEN LastLoginDatetime >= DATEADD(DAY,  -365, SYSDATETIME()) THEN N'3) 1年以内'
            WHEN LastLoginDatetime >= DATEADD(DAY,  -730, SYSDATETIME()) THEN N'4) 2年以内'
            ELSE                                                              N'5) 2年以上前'
       END
     , チーム数 = COUNT(*)
     , うち公開 = SUM(CASE WHEN PublicFLG = 1 THEN 1 ELSE 0 END)
  FROM dbo.Team
 WHERE DeleteFLG = 0
   AND SystemDataFLG = 0
 GROUP BY CASE WHEN LastLoginDatetime IS NULL                       THEN N'9) 不明'
               WHEN LastLoginDatetime >= DATEADD(DAY,   -30, SYSDATETIME()) THEN N'1) 1ヶ月以内'
               WHEN LastLoginDatetime >= DATEADD(DAY,   -90, SYSDATETIME()) THEN N'2) 3ヶ月以内'
               WHEN LastLoginDatetime >= DATEADD(DAY,  -365, SYSDATETIME()) THEN N'3) 1年以内'
               WHEN LastLoginDatetime >= DATEADD(DAY,  -730, SYSDATETIME()) THEN N'4) 2年以内'
               ELSE                                                              N'5) 2年以上前'
          END
 ORDER BY 1;
