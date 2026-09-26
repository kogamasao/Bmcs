/*
 * 全角数字・前後の空白が入っている背番号を、半角数字に直す
 *
 * 背景：
 *   2026-09-26 に背番号の入力チェック（半角数字3桁以内。全角は半角に変換して保存）を追加した。
 *   それ以前に全角数字（例「１２」）や前後に空白のある背番号（例「 12」）で登録されたメンバーがいると、
 *   保存し直すまで並び順がずれたり（アプリ側でも全角は数値として並べるよう対応済み）、表示が不揃いになる。
 *   先に doc/query/20260926_uniform_number_check.sql で件数を確認すること。
 *
 * 対象：全角数字・前後の空白を直すと「半角数字1〜3桁」になる背番号のみ。
 *       数字以外の文字を含む背番号（例「10a」）は変更しない（件数を見て扱いを決める）。
 * ※更新日時（UpdateDatetime）は変更しない（利用者による変更ではないため）。
 * 再実行しても安全（2回目は対象が0件）。GO は使用していない。デプロイの前後どちらに実行してもよい。
 */

--------------------------------------------------------------------------------
-- 1) 変更前の内容を退避する（戻せるようにしておく）
--------------------------------------------------------------------------------
IF OBJECT_ID('dbo.MemberUniformNumberBackup_20260926') IS NULL
BEGIN
    SELECT MemberID
         , UniformNumber
         , BackupDatetime = SYSDATETIME()
      INTO dbo.MemberUniformNumberBackup_20260926
      FROM dbo.Member
     WHERE UniformNumber IS NOT NULL
       AND UniformNumber <> ''
       AND UniformNumber COLLATE Latin1_General_BIN2 LIKE '%[^0-9]%';
END;

--------------------------------------------------------------------------------
-- 2) 半角に直す
--------------------------------------------------------------------------------
UPDATE dbo.Member
   SET UniformNumber = LTRIM(RTRIM(TRANSLATE(UniformNumber, N'０１２３４５６７８９', N'0123456789')))
 WHERE UniformNumber IS NOT NULL
   AND UniformNumber <> ''
   AND UniformNumber COLLATE Latin1_General_BIN2 LIKE '%[^0-9]%'
   AND LTRIM(RTRIM(TRANSLATE(UniformNumber, N'０１２３４５６７８９', N'0123456789'))) COLLATE Latin1_General_BIN2 NOT LIKE '%[^0-9]%'
   AND LEN(LTRIM(RTRIM(UniformNumber))) BETWEEN 1 AND 3;

--------------------------------------------------------------------------------
-- 3) 確認（残っているのは「数字以外の文字を含む」背番号のみのはず）
--------------------------------------------------------------------------------
SELECT MemberID, TeamID, MemberName, UniformNumber
  FROM dbo.Member
 WHERE UniformNumber IS NOT NULL
   AND UniformNumber <> ''
   AND UniformNumber COLLATE Latin1_General_BIN2 LIKE '%[^0-9]%';
