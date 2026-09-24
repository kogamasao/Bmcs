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
 *   0) 体験用アカウント・サンプルチームの状態を確認する（参照のみ。結果を見て、必要なら個別に対応する）
 *   1) 変更前の値を退避する
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
-- 0) 体験用アカウント・サンプルチームが書き換えられていないかの確認（参照のみ）
--    修正前は、体験用ユーザで誰でもユーザ情報・チーム情報を変更できたため、デプロイ前に状態を確認する。
--    ・パスワード「1」のハッシュと一致しない → 体験ログインが動かない。パスワードを戻す必要がある
--    ・TeamID が YG でない、DeleteFLG が 1 → 同上
--    ・チーム名・公開設定・チーム紹介が初期値と違う → 不適切な内容に書き換えられていないか確認する
--    ・YGUser 以外に YG に所属しているユーザ → 推測しやすいチームパスワードで参加した可能性がある
--      （今回の修正で新たな参加は禁止したが、既に参加しているユーザは残る）
--------------------------------------------------------------------------------
SELECT UserAccountID
     , UserAccountName
     , TeamID
     , DeleteFLG
     , パスワードが初期値 = CASE WHEN Password = N'ixKJKqaxdfvKD/ZsiWNfhH6xrBlgT7GpHvl8Pa32ciM=' THEN N'○' ELSE N'×（要対応）' END
     , EmailAddress
     , UpdateUserID
     , UpdateDatetime
  FROM dbo.UserAccount
 WHERE UserAccountID = N'YGUser';

SELECT TeamID
     , TeamName
     , TeamAbbreviation
     , PublicFLG
     , DeleteFLG
     , チームパスワードが初期値 = CASE WHEN TeamPassword = N'ixKJKqaxdfvKD/ZsiWNfhH6xrBlgT7GpHvl8Pa32ciM=' THEN N'○' ELSE N'×' END
     , TeamEmailAddress
     , MessageDetail
     , UpdateUserID
     , UpdateDatetime
  FROM dbo.Team
 WHERE TeamID = N'YG';

SELECT UserAccountID, UserAccountName, EntryDatetime, LastLoginDatetime
  FROM dbo.UserAccount
 WHERE TeamID = N'YG'
   AND UserAccountID <> N'YGUser'
 ORDER BY EntryDatetime;

--------------------------------------------------------------------------------
-- 1) 退避
--------------------------------------------------------------------------------

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
