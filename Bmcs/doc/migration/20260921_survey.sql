/*
 * アンケート機能の追加（2026-09-21）
 *
 * 内容
 *   1. アンケート用テーブル5つの追加
 *   2. 第1回アンケート（機能要望）の設問データ投入
 *
 * 実行環境：Azure SQL Database（本番）／ローカルSQL Server
 * 実行方法：SSMS、Azure Data Studio、または sqlcmd で実行してください。
 *           ※「GO」はクライアント側のバッチ区切りのため、Azure Portalのクエリエディターでは実行できません。
 *           ※エラーが発生した場合は、以降のバッチを実行しないでください（sqlcmd は -b オプションで自動停止）。
 * ※存在チェックを入れているため、再実行しても安全です（冪等）。
 * ※【重要】このSQLは、アプリのデプロイ「前」に実行してください。
 *   テーブルが無い状態で新しいアプリを動かすと、ログイン時にアンケートの判定でエラーとなります
 *   （アプリ側で例外を捕捉してログインは通るようにしていますが、毎回エラーログが出ます）。
 */

SET XACT_ABORT ON;
GO

--------------------------------------------------------------------------------
-- 1. テーブル追加
--------------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Survey' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Survey]
    (
        [SurveyID]        INT            IDENTITY(1,1) NOT NULL,
        [SurveyTitle]     NVARCHAR(100)  NOT NULL,
        [SurveyDetail]    NVARCHAR(MAX)  NULL,
        [StartDatetime]   DATETIME2(7)   NULL,
        [EndDatetime]     DATETIME2(7)   NULL,
        [StatusClass]     INT            NOT NULL,   -- 1:下書き 2:公開中 3:終了
        [DeleteFLG]       BIT            NOT NULL,
        [EntryUserID]     NVARCHAR(50)   NULL,
        [EntryDatetime]   DATETIME2(7)   NULL,
        [UpdateUserID]    NVARCHAR(50)   NULL,
        [UpdateDatetime]  DATETIME2(7)   NULL,
        [TimeStamp]       ROWVERSION,
        CONSTRAINT [PK_Survey] PRIMARY KEY CLUSTERED ([SurveyID] ASC)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SurveyQuestion' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[SurveyQuestion]
    (
        [SurveyQuestionID] INT            IDENTITY(1,1) NOT NULL,
        [SurveyID]         INT            NOT NULL,
        [DisplayOrder]     INT            NOT NULL,
        [QuestionText]     NVARCHAR(200)  NOT NULL,
        [QuestionNote]     NVARCHAR(200)  NULL,
        [AnswerTypeClass]  INT            NOT NULL,   -- 1:単一選択 2:複数選択 3:自由記述
        [RequiredFLG]      BIT            NOT NULL,
        [MaxSelectCount]   INT            NULL,
        [EntryUserID]      NVARCHAR(50)   NULL,
        [EntryDatetime]    DATETIME2(7)   NULL,
        [UpdateUserID]     NVARCHAR(50)   NULL,
        [UpdateDatetime]   DATETIME2(7)   NULL,
        [TimeStamp]        ROWVERSION,
        CONSTRAINT [PK_SurveyQuestion] PRIMARY KEY CLUSTERED ([SurveyQuestionID] ASC),
        CONSTRAINT [FK_SurveyQuestion_Survey] FOREIGN KEY ([SurveyID]) REFERENCES [dbo].[Survey]([SurveyID])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SurveyChoice' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[SurveyChoice]
    (
        [SurveyChoiceID]   INT            IDENTITY(1,1) NOT NULL,
        [SurveyQuestionID] INT            NOT NULL,
        [DisplayOrder]     INT            NOT NULL,
        [ChoiceText]       NVARCHAR(200)  NOT NULL,
        [FreeTextFLG]      BIT            NOT NULL,   -- 「その他」選択時に自由入力欄を出すか
        [EntryUserID]      NVARCHAR(50)   NULL,
        [EntryDatetime]    DATETIME2(7)   NULL,
        [UpdateUserID]     NVARCHAR(50)   NULL,
        [UpdateDatetime]   DATETIME2(7)   NULL,
        [TimeStamp]        ROWVERSION,
        CONSTRAINT [PK_SurveyChoice] PRIMARY KEY CLUSTERED ([SurveyChoiceID] ASC),
        CONSTRAINT [FK_SurveyChoice_SurveyQuestion] FOREIGN KEY ([SurveyQuestionID]) REFERENCES [dbo].[SurveyQuestion]([SurveyQuestionID])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SurveyAnswer' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[SurveyAnswer]
    (
        [SurveyAnswerID]  INT            IDENTITY(1,1) NOT NULL,
        [SurveyID]        INT            NOT NULL,
        [UserAccountID]   NVARCHAR(50)   NOT NULL,
        [TeamID]          NVARCHAR(50)   NULL,       -- 回答時点のチーム（集計の絞り込みに使用）
        [AnswerDatetime]  DATETIME2(7)   NOT NULL,
        [EntryUserID]     NVARCHAR(50)   NULL,
        [EntryDatetime]   DATETIME2(7)   NULL,
        [UpdateUserID]    NVARCHAR(50)   NULL,
        [UpdateDatetime]  DATETIME2(7)   NULL,
        [TimeStamp]       ROWVERSION,
        CONSTRAINT [PK_SurveyAnswer] PRIMARY KEY CLUSTERED ([SurveyAnswerID] ASC),
        CONSTRAINT [FK_SurveyAnswer_Survey] FOREIGN KEY ([SurveyID]) REFERENCES [dbo].[Survey]([SurveyID])
    );
END
GO

-- 同一ユーザの二重回答を防ぐ
IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_SurveyAnswer_SurveyID_UserAccountID'
                 AND object_id = OBJECT_ID('dbo.SurveyAnswer'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_SurveyAnswer_SurveyID_UserAccountID]
        ON [dbo].[SurveyAnswer] ([SurveyID] ASC, [UserAccountID] ASC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SurveyAnswerDetail' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[SurveyAnswerDetail]
    (
        [SurveyAnswerDetailID] INT           IDENTITY(1,1) NOT NULL,
        [SurveyAnswerID]       INT           NOT NULL,
        [SurveyQuestionID]     INT           NOT NULL,
        [SurveyChoiceID]       INT           NULL,       -- 自由記述の場合はNULL
        [AnswerText]           NVARCHAR(MAX) NULL,   -- ※FreeTextはSQL Serverの予約語のため使用しない
        [EntryUserID]          NVARCHAR(50)  NULL,
        [EntryDatetime]        DATETIME2(7)  NULL,
        [UpdateUserID]         NVARCHAR(50)  NULL,
        [UpdateDatetime]       DATETIME2(7)  NULL,
        [TimeStamp]            ROWVERSION,
        CONSTRAINT [PK_SurveyAnswerDetail] PRIMARY KEY CLUSTERED ([SurveyAnswerDetailID] ASC),
        CONSTRAINT [FK_SurveyAnswerDetail_SurveyAnswer] FOREIGN KEY ([SurveyAnswerID]) REFERENCES [dbo].[SurveyAnswer]([SurveyAnswerID])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_SurveyAnswerDetail_SurveyAnswerID'
                 AND object_id = OBJECT_ID('dbo.SurveyAnswerDetail'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SurveyAnswerDetail_SurveyAnswerID]
        ON [dbo].[SurveyAnswerDetail] ([SurveyAnswerID] ASC);
END
GO

--------------------------------------------------------------------------------
-- 2. 第1回アンケート（機能要望）の設問データ
--    ※StatusClass = 1（下書き）で作成する。
--      内容を確認のうえ、公開するときに 2（公開中）へ更新すること。
--      UPDATE dbo.Survey SET StatusClass = 2 WHERE SurveyID = <ID>;
--------------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM dbo.Survey WHERE SurveyTitle = N'Bmcsへのご要望をお聞かせください')
BEGIN
    DECLARE @SurveyID INT;
    DECLARE @QuestionID INT;

    INSERT INTO dbo.Survey (SurveyTitle, SurveyDetail, StatusClass, DeleteFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (N'Bmcsへのご要望をお聞かせください',
            N'いつもBmcsをご利用いただきありがとうございます。' + CHAR(10) +
            N'今後どの機能から作るかを決めるため、アンケートにご協力をお願いいたします。' + CHAR(10) +
            N'全6問・ほとんどが選択式で、2分ほどで終わります。' + CHAR(10) +
            N'いただいたご意見は集計してお知らせし、開発の優先順位に反映します。',
            1, 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());

    SET @SurveyID = SCOPE_IDENTITY();

    ----------------------------------------------------------------------------
    -- Q1 立場（単一選択・必須）
    ----------------------------------------------------------------------------
    INSERT INTO dbo.SurveyQuestion (SurveyID, DisplayOrder, QuestionText, AnswerTypeClass, RequiredFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@SurveyID, 1, N'Bmcsを主にどのお立場で使っていますか？（いちばん近いものを1つ）', 1, 1, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());
    SET @QuestionID = SCOPE_IDENTITY();

    INSERT INTO dbo.SurveyChoice (SurveyQuestionID, DisplayOrder, ChoiceText, FreeTextFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@QuestionID, 1, N'チームの代表・運営担当', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 2, N'監督・コーチ・顧問', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 3, N'スコアラー・記録係・マネージャー', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 4, N'選手（運営も担当している場合を含む）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 5, N'保護者', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 6, N'その他', 1, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());

    ----------------------------------------------------------------------------
    -- Q2 利用目的（複数選択・必須）
    ----------------------------------------------------------------------------
    INSERT INTO dbo.SurveyQuestion (SurveyID, DisplayOrder, QuestionText, AnswerTypeClass, RequiredFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@SurveyID, 2, N'主にどんな目的で使っていますか？（いくつでも）', 2, 1, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());
    SET @QuestionID = SCOPE_IDENTITY();

    INSERT INTO dbo.SurveyChoice (SurveyQuestionID, DisplayOrder, ChoiceText, FreeTextFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@QuestionID, 1, N'試合結果の記録', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 2, N'チーム全体の成績を管理する', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 3, N'自分自身の成績を記録・確認する', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 4, N'チーム内での共有', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 5, N'大会・リーグの記録', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 6, N'記録として残す・思い出として見返す', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 7, N'他チームの成績を見る', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 8, N'その他', 1, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());

    ----------------------------------------------------------------------------
    -- Q3 追加してほしい機能（複数選択・最大3・必須）
    ----------------------------------------------------------------------------
    INSERT INTO dbo.SurveyQuestion (SurveyID, DisplayOrder, QuestionText, QuestionNote, AnswerTypeClass, RequiredFLG, MaxSelectCount, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@SurveyID, 3, N'追加してほしい機能を3つまで選んでください', N'優先して開発する機能を決める参考にします。無ければ「特にない」を選んでください。', 2, 1, 3, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());
    SET @QuestionID = SCOPE_IDENTITY();

    INSERT INTO dbo.SurveyChoice (SurveyQuestionID, DisplayOrder, ChoiceText, FreeTextFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@QuestionID,  1, N'使い方のガイド・説明の充実', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID,  2, N'試合中の入力をもっと簡単に（手順を減らす・押しやすくする）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID,  3, N'画面デザインの刷新（見やすく、今風のデザインに）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID,  4, N'打順表・スタメン表の作成', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID,  5, N'成績のグラフ表示・推移の可視化', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID,  6, N'より詳しい成績指標の追加（IsoP・BABIP・FIP など）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID,  7, N'守備成績の充実（守備率・刺殺など）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID,  8, N'球数・投球数の管理', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID,  9, N'対戦相手チームの成績記録', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 10, N'成績表示のカスタマイズ（不要な項目を隠す）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 11, N'試合結果の共有（画像やリンクでLINE・SNSに送る）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 12, N'試合動画（YouTube）の登録・表示', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 13, N'閲覧専用のログイン（編集できないメンバー用アカウント）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 14, N'スコアブックの印刷・PDF出力', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 15, N'出欠管理（試合・練習）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 16, N'特にない（いまのままで十分）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 17, N'その他', 1, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());

    ----------------------------------------------------------------------------
    -- Q4 不便に感じること（複数選択・任意）
    ----------------------------------------------------------------------------
    INSERT INTO dbo.SurveyQuestion (SurveyID, DisplayOrder, QuestionText, AnswerTypeClass, RequiredFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@SurveyID, 4, N'いま不便に感じることはありますか？（いくつでも）', 2, 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());
    SET @QuestionID = SCOPE_IDENTITY();

    INSERT INTO dbo.SurveyChoice (SurveyQuestionID, DisplayOrder, ChoiceText, FreeTextFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@QuestionID, 1, N'入力に時間がかかる', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 2, N'画面のデザインが古い・使いにくい', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 3, N'最初に何をすればいいか分かりにくい', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 4, N'スマートフォンで見づらい', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 5, N'項目が多すぎて分かりにくい', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 6, N'過去のデータを探しにくい', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 7, N'チーム内で共有しにくい', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 8, N'使い方が分からない機能がある', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 9, N'エラーが出る・うまく動かないことがある', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 10, N'チームの他のメンバーに使ってもらえない', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 11, N'特にない', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 12, N'その他', 1, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());

    ----------------------------------------------------------------------------
    -- Q5 価値の強さ（単一選択・任意）
    ----------------------------------------------------------------------------
    INSERT INTO dbo.SurveyQuestion (SurveyID, DisplayOrder, QuestionText, QuestionNote, AnswerTypeClass, RequiredFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@SurveyID, 5, N'もしBmcsが使えなくなったら、どうしますか？', N'他のサービスと比べたときの位置づけを知るための質問です。', 1, 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());
    SET @QuestionID = SCOPE_IDENTITY();

    INSERT INTO dbo.SurveyChoice (SurveyQuestionID, DisplayOrder, ChoiceText, FreeTextFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@QuestionID, 1, N'とても困る（代わりになるものが無い）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 2, N'やや困る（他を探すか、紙やExcelに戻す）', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME()),
           (@QuestionID, 3, N'あまり困らない', 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());

    ----------------------------------------------------------------------------
    -- Q6 自由記述（任意）
    ----------------------------------------------------------------------------
    INSERT INTO dbo.SurveyQuestion (SurveyID, DisplayOrder, QuestionText, QuestionNote, AnswerTypeClass, RequiredFLG, EntryUserID, EntryDatetime, UpdateUserID, UpdateDatetime)
    VALUES (@SurveyID, 6, N'その他、ご要望やお気づきの点があればお聞かせください', N'任意です。ひとことでも構いません。', 3, 0, N'ADMIN', SYSDATETIME(), N'ADMIN', SYSDATETIME());

    PRINT N'アンケートを作成しました。SurveyID = ' + CAST(@SurveyID AS NVARCHAR(10));
END
GO

--------------------------------------------------------------------------------
-- 確認用
--------------------------------------------------------------------------------
-- SELECT * FROM dbo.Survey;
-- SELECT q.DisplayOrder, q.QuestionText, c.DisplayOrder, c.ChoiceText
--   FROM dbo.SurveyQuestion q LEFT JOIN dbo.SurveyChoice c ON q.SurveyQuestionID = c.SurveyQuestionID
--  ORDER BY q.DisplayOrder, c.DisplayOrder;

--------------------------------------------------------------------------------
-- 公開する場合
--   UPDATE dbo.Survey SET StatusClass = 2, UpdateUserID = N'ADMIN', UpdateDatetime = SYSDATETIME() WHERE SurveyID = <ID>;
-- 終了する場合
--   UPDATE dbo.Survey SET StatusClass = 3, UpdateUserID = N'ADMIN', UpdateDatetime = SYSDATETIME() WHERE SurveyID = <ID>;
--------------------------------------------------------------------------------
