/*
 * ヘルプの文言を画面の用語に合わせる
 *
 * 背景：
 *   画面の用語を次のとおり見直したため、ヘルプ（SystemAdmin）の記述を合わせる。
 *
 *   (1) 試合の状態名：EndGame / EndGameLock の表示名を「試合終了」→「確定済み」
 *       （「試合終了」ボタンを押した直後の状態は「確定前」で、「確定する」を押すと「試合終了」になるという、
 *         ボタン名と状態名の交差を解消するため）
 *       試合一覧・イニング詳細のヘルプにある「試合結果(試合終了後に表示)」の「試合結果」ボタンは、
 *       実際には確定済みの試合にしか表示されないため「確定後に表示」に改める。
 *       ※「試合結果編集(試合終了後に表示)」とスコア入力の「試合結果へ(試合終了後のみ表示)」は、
 *         「試合終了」ボタンを押した後に表示されるもので事実と合っているため変更しない。
 *
 *   (2) 打者結果の選択肢：「ﾁｪﾝｼﾞ」→「打席なし(チェンジ)」
 *       （「チェンジ」ボタンと同じ言葉で意味が違い、打席が成立しないことが分からなかったため）
 *
 *   (3) 項目名：「試合入力タイプ」→「入力方式」（試合作成画面・試合一覧の列名と統一）
 *
 *   (4) ヘルプの見出し（画面名）：
 *       24 試合結果詳細 → 試合結果、25 試合結果編集 → 試合結果の入力・確定
 *       （一覧の「試合結果」を押すと「試合結果詳細」、「試合結果編集」を押すと「試合結果」が開くという
 *         名前と行き先の交差を解消するため、画面タイトルを変更した）
 *
 * 方針：
 *   本文全体を置き換えず、該当の語句だけを REPLACE で差し替える。
 *   各 UPDATE は対象の有無を判定しているので、再実行しても安全。
 *
 * 実行方法：上から順に実行する。GO は使用していない。
 */

--------------------------------------------------------------------------------
-- 1) 変更前の内容を退避する（戻せるようにしておく）
--    ※(3) は全画面が対象になり得るため、全行を退避する
--------------------------------------------------------------------------------
IF OBJECT_ID('dbo.SystemAdminBackup_20260924') IS NULL
BEGIN
    SELECT SystemAdminClass
         , MessageTitle
         , MessageDetail
         , BackupDatetime = SYSDATETIME()
      INTO dbo.SystemAdminBackup_20260924
      FROM dbo.SystemAdmin;
END;

--------------------------------------------------------------------------------
-- 2) 変更前の確認（○ が置換対象）
--------------------------------------------------------------------------------
SELECT SystemAdminClass
     , MessageTitle
     , 試合結果_試合終了後 = CASE WHEN CHARINDEX(N'試合結果(試合終了後に表示)', MessageDetail) > 0
                                OR CHARINDEX(N'試合結果(試合終了後のみ表示)', MessageDetail) > 0 THEN N'○' ELSE N'-' END
     , 旧選択肢名         = CASE WHEN CHARINDEX(N'チェンジになった場合は「チェンジ」を選択します。', MessageDetail) > 0 THEN N'○' ELSE N'-' END
     , 試合入力タイプ     = CASE WHEN CHARINDEX(N'試合入力タイプ', MessageDetail) > 0 THEN N'○' ELSE N'-' END
  FROM dbo.SystemAdmin
 WHERE CHARINDEX(N'試合結果(試合終了後', MessageDetail) > 0
    OR CHARINDEX(N'チェンジになった場合は「チェンジ」を選択します。', MessageDetail) > 0
    OR CHARINDEX(N'試合入力タイプ', MessageDetail) > 0
    OR SystemAdminClass IN (24, 25)
 ORDER BY SystemAdminClass;

--------------------------------------------------------------------------------
-- 3) (1) 試合一覧（14, 15）・イニング詳細（22, 23）の「試合結果」ボタンの表示条件
--------------------------------------------------------------------------------
UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail
                             , N'試合結果(試合終了後に表示)'
                             , N'試合結果(確定後に表示)')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass IN (14, 15)
   AND CHARINDEX(N'試合結果(試合終了後に表示)', MessageDetail) > 0;

UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail
                             , N'試合結果(試合終了後のみ表示)'
                             , N'試合結果(確定後のみ表示)')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass IN (22, 23)
   AND CHARINDEX(N'試合結果(試合終了後のみ表示)', MessageDetail) > 0;

--------------------------------------------------------------------------------
-- 4) (2) スコア入力（21）の打者結果の選択肢名
--------------------------------------------------------------------------------
UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail
                             , N'チェンジになった場合は「チェンジ」を選択します。'
                             , N'チェンジになった場合は「打席なし(チェンジ)」を選択します。')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass = 21
   AND CHARINDEX(N'チェンジになった場合は「チェンジ」を選択します。', MessageDetail) > 0;

--------------------------------------------------------------------------------
-- 5) (3) 「試合入力タイプ」→「入力方式」（全画面）
--------------------------------------------------------------------------------
UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail, N'試合入力タイプ', N'入力方式')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE CHARINDEX(N'試合入力タイプ', MessageDetail) > 0;

--------------------------------------------------------------------------------
-- 6) (4) ヘルプの見出し
--------------------------------------------------------------------------------
UPDATE dbo.SystemAdmin
   SET MessageTitle = N'試合結果'
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass = 24
   AND MessageTitle = N'試合結果詳細';

UPDATE dbo.SystemAdmin
   SET MessageTitle = N'試合結果の入力・確定'
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass = 25
   AND MessageTitle = N'試合結果編集';

--------------------------------------------------------------------------------
-- 7) 変更後の確認（1行も返らず、24・25 の見出しが新しい名前になっていれば完了）
--------------------------------------------------------------------------------
SELECT SystemAdminClass
     , MessageTitle
  FROM dbo.SystemAdmin
 WHERE CHARINDEX(N'試合結果(試合終了後', MessageDetail) > 0
    OR CHARINDEX(N'チェンジになった場合は「チェンジ」を選択します。', MessageDetail) > 0
    OR CHARINDEX(N'試合入力タイプ', MessageDetail) > 0;

SELECT SystemAdminClass, MessageTitle
  FROM dbo.SystemAdmin
 WHERE SystemAdminClass IN (24, 25);

--------------------------------------------------------------------------------
-- 元に戻す場合
--------------------------------------------------------------------------------
-- UPDATE a
--    SET a.MessageTitle  = b.MessageTitle
--      , a.MessageDetail = b.MessageDetail
--   FROM dbo.SystemAdmin a
--  INNER JOIN dbo.SystemAdminBackup_20260924 b ON a.SystemAdminClass = b.SystemAdminClass;

-- 内容を確認して問題なければ、退避テーブルを削除する
-- DROP TABLE dbo.SystemAdminBackup_20260924;
