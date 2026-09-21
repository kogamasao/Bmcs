/*
 * ヘルプ本文の修正（ユーザ作成・チーム作成）
 *
 * 背景：
 *   ユーザ作成画面のチーム選択をプルダウンからテキスト入力に変更したため、
 *   ヘルプの「チームを選択します」という記述が実装と合わなくなった。
 *   あわせてチームIDの説明を補い、誤字（エラーをなります → エラーとなります）を直す。
 *
 * 方針：
 *   本文全体を置き換えず、**該当の文だけを REPLACE で差し替える**。
 *   本番の本文がローカルと一致していなくても、他の記述を壊さないため。
 *   各 UPDATE は CHARINDEX で対象の有無を判定しているので、**再実行しても安全**
 *   （2回目以降は 0 件更新になる）。
 *
 * 実行方法：上から順に実行する。GO は使用していない。
 */

--------------------------------------------------------------------------------
-- 1) 変更前の内容を退避する（戻せるようにしておく）
--------------------------------------------------------------------------------
IF OBJECT_ID('dbo.SystemAdminBackup_20260921') IS NULL
BEGIN
    SELECT SystemAdminClass
         , MessageTitle
         , MessageDetail
         , BackupDatetime = SYSDATETIME()
      INTO dbo.SystemAdminBackup_20260921
      FROM dbo.SystemAdmin
     WHERE SystemAdminClass IN (2, 5);
END;

--------------------------------------------------------------------------------
-- 2) 変更前の確認（置換対象が存在するか）
--------------------------------------------------------------------------------
SELECT 画面 = CASE SystemAdminClass WHEN 2 THEN N'ユーザ作成' WHEN 5 THEN N'チーム作成' END
     , 誤字あり       = CASE WHEN CHARINDEX(N'エラーをなります', MessageDetail) > 0 THEN N'○' ELSE N'-' END
     , チーム選択の記述 = CASE WHEN CHARINDEX(N'チームを選択します', MessageDetail) > 0 THEN N'○' ELSE N'-' END
     , ID説明         = CASE WHEN CHARINDEX(N'任意のIDを入力します。', MessageDetail) > 0 THEN N'○' ELSE N'-' END
  FROM dbo.SystemAdmin
 WHERE SystemAdminClass IN (2, 5)
 ORDER BY SystemAdminClass;

--------------------------------------------------------------------------------
-- 3) ユーザ作成（SystemAdminClass = 2）
--------------------------------------------------------------------------------
-- 3-1) ユーザIDの説明を補い、誤字を直す
UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail
                             , N'任意のIDを入力します。(既に使用されているIDはエラーをなります。)'
                             , N'ログインに使用するIDです。半角英数字がおすすめです。(既に使用されているIDはエラーとなります。)')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass = 2
   AND CHARINDEX(N'任意のIDを入力します。(既に使用されているIDはエラーをなります。)', MessageDetail) > 0;

-- 3-2) チームの選択 → チームIDの入力
UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail
                             , N'別ユーザにて、チームを既に作成している場合はチームを選択します。'
                             , N'別のユーザが既にチームを作成している場合は、「既にあるチームに参加する」を開いてチームIDを入力します。')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass = 2
   AND CHARINDEX(N'別ユーザにて、チームを既に作成している場合はチームを選択します。', MessageDetail) > 0;

-- 3-3) チームIDも代表者から教えてもらう必要があることを明記する
UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail
                             , N'チームを作成したユーザよりパスワードを共有して頂き、チームパスワードを入力してください。'
                             , N'チームIDとチームパスワードは、チームを作成したユーザから教えてもらってください。')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass = 2
   AND CHARINDEX(N'チームを作成したユーザよりパスワードを共有して頂き、チームパスワードを入力してください。', MessageDetail) > 0;

--------------------------------------------------------------------------------
-- 4) チーム作成（SystemAdminClass = 5）
--    チームIDが何に使われるIDなのか、変更できるのかが書かれていなかった
--------------------------------------------------------------------------------
UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail
                             , N'任意のIDを入力します。(既に使用されているIDはエラーをなります。)'
                             , N'チームメイトがこのチームに参加するときに入力するIDです。<br>作成後は変更できません。半角英数字がおすすめです。(既に使用されているIDはエラーとなります。)')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass = 5
   AND CHARINDEX(N'任意のIDを入力します。(既に使用されているIDはエラーをなります。)', MessageDetail) > 0;

--------------------------------------------------------------------------------
-- 5) 変更後の確認（すべて「-」になっていれば置換済み）
--------------------------------------------------------------------------------
SELECT 画面 = CASE SystemAdminClass WHEN 2 THEN N'ユーザ作成' WHEN 5 THEN N'チーム作成' END
     , 誤字あり       = CASE WHEN CHARINDEX(N'エラーをなります', MessageDetail) > 0 THEN N'○' ELSE N'-' END
     , チーム選択の記述 = CASE WHEN CHARINDEX(N'チームを選択します', MessageDetail) > 0 THEN N'○' ELSE N'-' END
     , 旧ID説明       = CASE WHEN CHARINDEX(N'任意のIDを入力します。', MessageDetail) > 0 THEN N'○' ELSE N'-' END
  FROM dbo.SystemAdmin
 WHERE SystemAdminClass IN (2, 5)
 ORDER BY SystemAdminClass;

--------------------------------------------------------------------------------
-- 元に戻す場合
--------------------------------------------------------------------------------
-- UPDATE a
--    SET a.MessageDetail = b.MessageDetail
--   FROM dbo.SystemAdmin a
--  INNER JOIN dbo.SystemAdminBackup_20260921 b ON a.SystemAdminClass = b.SystemAdminClass;

-- 内容を確認して問題なければ、退避テーブルを削除する
-- DROP TABLE dbo.SystemAdminBackup_20260921;
