/*
 * アカウント復旧機能の追加（2026-09-20）
 *
 * 内容
 *   1. ResetToken テーブルの追加（ユーザパスワード／チームパスワードの再設定トークン）
 *   2. ヘルプ文言（SystemAdmin）の更新
 *   3. トップお知らせの更新
 *   4. 書式が不正なメールアドレスのクリア
 *
 * 実行環境：Azure SQL Database（本番）／ローカルSQL Server
 * 実行方法：SSMS、Azure Data Studio、または sqlcmd で実行してください。
 *           ※「GO」はクライアント側のバッチ区切りのため、Azure Portalのクエリエディターでは実行できません。
 *           ※エラーが発生した場合は、以降のバッチを実行しないでください（SSMS/sqlcmdは既定でエラー後も続行します）。
 *             sqlcmd の場合は -b オプション、または先頭に :on error exit を指定すると自動で停止します。
 *             セクション1（テーブル作成）が失敗した状態でセクション2以降を実行すると、
 *             機能が無いのにヘルプ文言だけ更新された状態になります。
 * ※存在チェック・文言チェックを入れているため、再実行しても安全です（冪等）。
 */

SET XACT_ABORT ON;
GO

--------------------------------------------------------------------------------
-- 1. ResetToken テーブル追加
--------------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ResetToken' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[ResetToken]
    (
        [ResetTokenID]    NVARCHAR(100)  NOT NULL,
        [ResetTokenClass] INT            NOT NULL,   -- 1:ユーザパスワード, 2:チームパスワード
        [TargetID]        NVARCHAR(50)   NOT NULL,   -- ユーザID または チームID
        [ExpireDatetime]  DATETIME2(7)   NOT NULL,
        [UsedFLG]         BIT            NOT NULL,
        [EntryUserID]     NVARCHAR(50)   NULL,
        [EntryDatetime]   DATETIME2(7)   NULL,
        [UpdateUserID]    NVARCHAR(50)   NULL,
        [UpdateDatetime]  DATETIME2(7)   NULL,
        [TimeStamp]       ROWVERSION,
        CONSTRAINT [PK_ResetToken] PRIMARY KEY CLUSTERED ([ResetTokenID] ASC)
    );
END
GO

-- インデックス（テーブルのみ存在する環境でも作成されるよう、独立してチェックする）
IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_ResetToken_ResetTokenClass_TargetID'
                 AND object_id = OBJECT_ID('dbo.ResetToken'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ResetToken_ResetTokenClass_TargetID]
        ON [dbo].[ResetToken] ([ResetTokenClass] ASC, [TargetID] ASC);
END
GO

--------------------------------------------------------------------------------
-- 2. ヘルプ文言（SystemAdmin.MessageDetail）の更新
--    ※本番DBは EnsureCreated で作成済みのため、DbInitializer.cs の変更は反映されない。
--      画面のヘルプ文言はこのスクリプトで更新する。
--      SystemAdminClass … 1:トップお知らせ, 2:ユーザ作成, 3:ユーザ情報編集, 5:チーム作成, 6:チーム情報編集
--------------------------------------------------------------------------------
BEGIN TRANSACTION;

-- 2-1. 「パスワードをお忘れの際はお問い合わせください」→ 再設定機能の案内へ（class 2）
UPDATE [dbo].[SystemAdmin]
SET    [MessageDetail]  = REPLACE([MessageDetail],
                                  N'パスワードをお忘れの際はお問い合わせください。<br>',
                                  N'パスワードをお忘れの際は、ログイン画面の「パスワードをお忘れの場合」からお手続きください。ご登録のメールアドレス宛に再設定用URLをお送りします。<br>')
      ,[UpdateUserID]   = N'ADMIN'
      ,[UpdateDatetime] = SYSDATETIME()
WHERE  [SystemAdminClass] = 2
  AND  [MessageDetail] LIKE N'%パスワードをお忘れの際はお問い合わせください。<br>%';

-- 2-2. ユーザのメールアドレス：用途の説明を更新（class 2, 3）
UPDATE [dbo].[SystemAdmin]
SET    [MessageDetail]  = REPLACE([MessageDetail],
                                  N'現在の使用用途はサイト管理者からご連絡させて頂く以外に使用しません。<br>',
                                  N'ユーザIDやパスワードを忘れた際の復旧、およびサイト管理者からのご連絡に使用します。<br>')
      ,[UpdateUserID]   = N'ADMIN'
      ,[UpdateDatetime] = SYSDATETIME()
WHERE  [SystemAdminClass] IN (2, 3)
  AND  [MessageDetail] LIKE N'%現在の使用用途はサイト管理者からご連絡させて頂く以外に使用しません。<br>%';

-- 2-3. ユーザのメールアドレス：任意 → 必須（class 2, 3）
UPDATE [dbo].[SystemAdmin]
SET    [MessageDetail]  = REPLACE([MessageDetail],
                                  N'必須ではありませんので、空欄でも構いません。<br>',
                                  N'登録がない場合、チームIDとチームパスワードによる復旧のみとなるため、必ずご登録ください。<br>')
      ,[UpdateUserID]   = N'ADMIN'
      ,[UpdateDatetime] = SYSDATETIME()
WHERE  [SystemAdminClass] IN (2, 3)
  AND  [MessageDetail] LIKE N'%必須ではありませんので、空欄でも構いません。<br>%';

-- 2-4. ユーザのヘルプ見出しに (必須) を付与（他項目と表記を揃える。class 2, 3）
UPDATE [dbo].[SystemAdmin]
SET    [MessageDetail]  = REPLACE([MessageDetail],
                                  N'メールアドレス<br>',
                                  N'メールアドレス(必須)<br>')
      ,[UpdateUserID]   = N'ADMIN'
      ,[UpdateDatetime] = SYSDATETIME()
WHERE  [SystemAdminClass] IN (2, 3)
  AND  [MessageDetail] LIKE N'%メールアドレス<br>%';

-- 2-5. チームのメールアドレス：チームパスワード再設定に使用することを明記（class 5, 6）
UPDATE [dbo].[SystemAdmin]
SET    [MessageDetail]  = REPLACE([MessageDetail],
                                  N'現在の使用用途はサイト管理者からご連絡させて頂いたり、メッセージ機能以外で他チームから連絡を取る際に使用します。<br>',
                                  N'サイト管理者からのご連絡、他チームからの連絡のほか、チームパスワードを忘れた際の再設定に使用します。<br>')
      ,[UpdateUserID]   = N'ADMIN'
      ,[UpdateDatetime] = SYSDATETIME()
WHERE  [SystemAdminClass] IN (5, 6)
  AND  [MessageDetail] LIKE N'%現在の使用用途はサイト管理者からご連絡させて頂いたり、メッセージ機能以外で他チームから連絡を取る際に使用します。<br>%';

UPDATE [dbo].[SystemAdmin]
SET    [MessageDetail]  = REPLACE([MessageDetail],
                                  N'必須ではありませんので、空欄でも構いません。<br>',
                                  N'登録がない場合、チームパスワードを忘れた際はお問い合わせによる対応となりますので、ご登録をおすすめします。<br>')
      ,[UpdateUserID]   = N'ADMIN'
      ,[UpdateDatetime] = SYSDATETIME()
WHERE  [SystemAdminClass] IN (5, 6)
  AND  [MessageDetail] LIKE N'%必須ではありませんので、空欄でも構いません。<br>%';

-- 2-6. チームヘルプの前置き（非公開なら入力不要）が、チームパスワード再設定の仕様と矛盾するため修正（class 5, 6）
UPDATE [dbo].[SystemAdmin]
SET    [MessageDetail]  = REPLACE([MessageDetail],
                                  N'非公開チームの場合は、使用用途がありませんので入力不要です。',
                                  N'非公開チームの場合、他チームへは公開されませんが、メールアドレスはチームパスワードの再設定に使用します。')
      ,[UpdateUserID]   = N'ADMIN'
      ,[UpdateDatetime] = SYSDATETIME()
WHERE  [SystemAdminClass] IN (5, 6)
  AND  [MessageDetail] LIKE N'%非公開チームの場合は、使用用途がありませんので入力不要です。%';

COMMIT TRANSACTION;
GO

--------------------------------------------------------------------------------
-- 3. トップお知らせ（新機能の告知）
--    ※リリースノートとして手動運用している項目のため、内容は適宜調整してください。
--------------------------------------------------------------------------------
UPDATE [dbo].[SystemAdmin]
SET    [MessageDetail]  = N'ver 1.1.2 ユーザID・パスワードの再設定機能を追加しました。ログイン画面の「パスワードをお忘れの場合」「ユーザIDをお忘れの場合」からお手続きいただけます。'
      ,[UpdateUserID]   = N'ADMIN'
      ,[UpdateDatetime] = SYSDATETIME()
WHERE  [SystemAdminClass] = 1;
GO

--------------------------------------------------------------------------------
-- 4. 書式が不正なメールアドレスのクリア
--    アプリ側にメールアドレスの書式検証（[EmailAddress]）を追加したため、
--    書式が不正な既存データが残っていると、そのユーザ・チームは編集画面で保存できなくなる。
--    また、到達しないアドレスが登録されているとアカウント復旧もできないため、NULLに戻す。
--    ※NULLにすることで、ユーザは「チーム情報による復旧」の対象に含まれるようになる。
--------------------------------------------------------------------------------
-- 事前確認と退避（NULL化は取り消せないため、必ず実行してください）
-- SELECT UserAccountID, EmailAddress INTO dbo.UserAccount_EmailBackup_20260920 FROM dbo.UserAccount
--  WHERE EmailAddress IS NOT NULL
--    AND (CHARINDEX('@',EmailAddress) <= 1 OR CHARINDEX('@',EmailAddress) = LEN(EmailAddress)
--         OR CHARINDEX('@',EmailAddress,CHARINDEX('@',EmailAddress)+1) > 0);
-- 事前確認（件数と内容を確認してから実行してください）
-- SELECT UserAccountID, EmailAddress FROM dbo.UserAccount
--  WHERE EmailAddress IS NOT NULL AND LTRIM(RTRIM(EmailAddress)) <> ''
--    AND EmailAddress NOT LIKE '%_@_%.__%';
-- SELECT TeamID, TeamEmailAddress FROM dbo.Team
--  WHERE TeamEmailAddress IS NOT NULL AND LTRIM(RTRIM(TeamEmailAddress)) <> ''
--    AND TeamEmailAddress NOT LIKE '%_@_%.__%';

UPDATE [dbo].[UserAccount]
SET    [EmailAddress]   = NULL
      ,[UpdateUserID]   = N'ADMIN'
      ,[UpdateDatetime] = SYSDATETIME()
--アプリ側の検証（[EmailAddress]属性）と同じ規則：@がちょうど1つ、かつ先頭・末尾でないこと
WHERE  [EmailAddress] IS NOT NULL
  AND  (LTRIM(RTRIM([EmailAddress])) = N''
        OR CHARINDEX(N'@', [EmailAddress]) <= 1
        OR CHARINDEX(N'@', [EmailAddress]) = LEN([EmailAddress])
        OR CHARINDEX(N'@', [EmailAddress], CHARINDEX(N'@', [EmailAddress]) + 1) > 0);
GO

UPDATE [dbo].[Team]
SET    [TeamEmailAddress] = NULL
      ,[UpdateUserID]     = N'ADMIN'
      ,[UpdateDatetime]   = SYSDATETIME()
WHERE  [TeamEmailAddress] IS NOT NULL
  AND  (LTRIM(RTRIM([TeamEmailAddress])) = N''
        OR CHARINDEX(N'@', [TeamEmailAddress]) <= 1
        OR CHARINDEX(N'@', [TeamEmailAddress]) = LEN([TeamEmailAddress])
        OR CHARINDEX(N'@', [TeamEmailAddress], CHARINDEX(N'@', [TeamEmailAddress]) + 1) > 0);
GO

--------------------------------------------------------------------------------
-- 確認用
--------------------------------------------------------------------------------
-- SELECT * FROM sys.tables WHERE name = 'ResetToken';
-- SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.ResetToken');
-- SELECT SystemAdminClass, MessageTitle FROM dbo.SystemAdmin ORDER BY SystemAdminClass;
-- SELECT * FROM dbo.ResetToken ORDER BY EntryDatetime DESC;

--------------------------------------------------------------------------------
-- 運用メモ
--   ・期限切れ・使用済トークンが残っても動作に影響はありませんが、定期的に削除して構いません。
--     DELETE FROM dbo.ResetToken WHERE UsedFLG = 1 OR ExpireDatetime < DATEADD(day, -7, SYSDATETIME());
--   ・試行回数制限はアプリのメモリ上で管理しているため、再起動でリセットされます。
--     複数インスタンスへスケールアウトする場合は、インスタンス毎の制限になる点に注意してください。
--   ・アプリ側の日時は DateTime.Now（サーバーのローカル時刻）を使用しています。
--     SYSDATETIME() はSQL Serverホストのローカル時刻（Azure SQL Databaseでは実質UTC）を返すため、
--     App Service で WEBSITE_TIME_ZONE を設定している場合は基準が異なります。
--------------------------------------------------------------------------------
