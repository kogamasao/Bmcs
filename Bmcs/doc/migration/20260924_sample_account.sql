/*
 * 体験用アカウント（YGUser）のメールアドレスを空にする
 *
 * 背景：
 *   体験用ユーザは、トップの「サンプルチームで体験する」で誰でもログインできる共有アカウント。
 *   2026-09-24 の修正より前は、このアカウントでユーザ情報を変更できたため、
 *   第三者がメールアドレスを自分のものに書き換えている可能性がある。
 *   書き換えた人は「パスワードをお忘れの場合」から再設定メールを受け取り、パスワードを変えられる
 *   （変えられると、体験ログインが全員に対して動かなくなる）。
 *
 *   コード側でも、体験用ユーザ・サンプルチームはパスワード再設定の対象外にした（ForgotPassword・ResetPassword・
 *   ForgotTeamPassword・ResetTeamPassword・RecoverByTeam）。本SQLは、DBに残っている値の後始末。
 *
 * 内容：
 *   1) 変更前の値を確認・退避する
 *   2) YGUser のメールアドレスを空（NULL）にする
 *   3) YGUser・サンプルチーム（YG）宛ての、未使用の再設定トークンを失効させる
 *
 * ※サンプルチーム（YG）のチームのメールアドレスは変更しない（ユーザ承認の範囲外）。
 *   チームパスワードの再設定はコード側でサンプルチームを対象外にしたため、残っていても再設定には使われない。
 *   必要であれば、末尾のコメントアウトしたSQLで空にする。
 *
 * 再実行しても安全（2回目以降は 0 件更新になる）。GO は使用していない。
 */

--------------------------------------------------------------------------------
-- 1) 変更前の確認と退避
--------------------------------------------------------------------------------
SELECT UserAccountID, EmailAddress, UpdateUserID, UpdateDatetime
  FROM dbo.UserAccount
 WHERE UserAccountID = N'YGUser';

SELECT TeamID, TeamEmailAddress, UpdateUserID, UpdateDatetime
  FROM dbo.Team
 WHERE TeamID = N'YG';

IF OBJECT_ID('dbo.SampleAccountBackup_20260924') IS NULL
BEGIN
    SELECT UserAccountID
         , EmailAddress
         , BackupDatetime = SYSDATETIME()
      INTO dbo.SampleAccountBackup_20260924
      FROM dbo.UserAccount
     WHERE UserAccountID = N'YGUser';
END;

--------------------------------------------------------------------------------
-- 2) 体験用ユーザのメールアドレスを空にする
--------------------------------------------------------------------------------
UPDATE dbo.UserAccount
   SET EmailAddress = NULL
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE UserAccountID = N'YGUser'
   AND EmailAddress IS NOT NULL;

--------------------------------------------------------------------------------
-- 3) 未使用の再設定トークンを失効させる
--    ResetTokenClass：1 = ユーザパスワード、2 = チームパスワード
--------------------------------------------------------------------------------
UPDATE dbo.ResetToken
   SET UsedFLG = 1
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE UsedFLG = 0
   AND ((ResetTokenClass = 1 AND TargetID = N'YGUser')
     OR (ResetTokenClass = 2 AND TargetID = N'YG'));

--------------------------------------------------------------------------------
-- 4) 変更後の確認（EmailAddress が NULL、未使用トークンが 0 件なら完了）
--------------------------------------------------------------------------------
SELECT UserAccountID, EmailAddress
  FROM dbo.UserAccount
 WHERE UserAccountID = N'YGUser';

SELECT 未使用トークン = COUNT(*)
  FROM dbo.ResetToken
 WHERE UsedFLG = 0
   AND ((ResetTokenClass = 1 AND TargetID = N'YGUser')
     OR (ResetTokenClass = 2 AND TargetID = N'YG'));

--------------------------------------------------------------------------------
-- （任意）サンプルチームのメールアドレスも空にする場合
--------------------------------------------------------------------------------
-- UPDATE dbo.Team
--    SET TeamEmailAddress = NULL
--      , UpdateUserID = N'ADMIN'
--      , UpdateDatetime = SYSDATETIME()
--  WHERE TeamID = N'YG';

--------------------------------------------------------------------------------
-- 元に戻す場合
--------------------------------------------------------------------------------
-- UPDATE a
--    SET a.EmailAddress = b.EmailAddress
--   FROM dbo.UserAccount a
--  INNER JOIN dbo.SampleAccountBackup_20260924 b ON a.UserAccountID = b.UserAccountID;

-- 内容を確認して問題なければ、退避テーブルを削除する
-- DROP TABLE dbo.SampleAccountBackup_20260924;
