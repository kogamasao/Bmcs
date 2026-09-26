/*
 * スコア入力のヘルプの「打席後ランナー結果」に、盗塁・盗塁死・WP・PB の説明を加える（issues.md M-20）
 *
 * 背景：打席後ランナー結果の「結果」は失策・補殺しか選べなかった。三振と同時の盗塁・盗塁死（三振ゲッツーなど）、
 *       振り逃げ（WP・PB）などを記録できるよう、盗塁・盗塁死・WP・PB を選べるようにしたため、ヘルプを合わせる。
 * 本文は DbInitializer.cs（新規DBの初期データ）と同じ内容になる。
 * 対象：21 スコア入力（GameScene）
 *
 * 書き換えは2つあり、それぞれ別に判定する（1つの書き換えが当たらなくても、他方に影響しないように）。
 *   A 打席後ランナー結果の説明文（入力するプレーの例を加える）
 *   B 対象選手の説明（プレーごとにどの選手を選ぶか）
 * どちらも書き換え前の文言がある場合だけ書き換えるため、再実行しても安全。GO は使用していない。
 *
 * 手順：
 *   1) 実行前の確認で、「説明文_前」「対象選手_前」が 1 であることを確かめる。
 *      0 の項目がある場合は本文が想定と違う（手で変更された等）ため、その書き換えは行われない。本文を確認して個別に直す。
 *   2) 実行後の確認で、「説明文_後」「対象選手_後」が 1、「説明文_前」「対象選手_前」が 0 になっていることを確かめる。
 */

--------------------------------------------------------------------------------
-- 0) 実行前の確認
--------------------------------------------------------------------------------
SELECT SystemAdminClass
     , 説明文_前 = CASE WHEN MessageDetail LIKE N'%打者結果によって発生したプレー(補殺、失策)、及びランナーの結果を入力します。<br />%' THEN 1 ELSE 0 END
     , 説明文_後 = CASE WHEN MessageDetail LIKE N'%打者結果と同時に発生したプレー(補殺、失策、盗塁、盗塁死、WP、PB)、及びランナーの結果を入力します。<br />%' THEN 1 ELSE 0 END
     , 対象選手_前 = CASE WHEN MessageDetail LIKE N'%失策した野手、補殺した野手を選択します。<br>%' THEN 1 ELSE 0 END
     , 対象選手_後 = CASE WHEN MessageDetail LIKE N'%発生したプレーに応じて対象選手を選択します(失策・補殺は野手、盗塁・盗塁死はランナー、WPは投手、PBは捕手)。<br>%' THEN 1 ELSE 0 END
  FROM dbo.SystemAdmin
 WHERE SystemAdminClass = 21;

--------------------------------------------------------------------------------
-- 1) 変更前の内容を退避する（戻せるようにしておく）
--------------------------------------------------------------------------------
IF OBJECT_ID('dbo.SystemAdminBackup_20260926c') IS NULL
BEGIN
    SELECT SystemAdminClass
         , MessageTitle
         , MessageDetail
         , BackupDatetime = SYSDATETIME()
      INTO dbo.SystemAdminBackup_20260926c
      FROM dbo.SystemAdmin
     WHERE SystemAdminClass = 21;
END;

--------------------------------------------------------------------------------
-- 2) 書き換え
--------------------------------------------------------------------------------
-- A 打席後ランナー結果の説明文
UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail, N'打者結果によって発生したプレー(補殺、失策)、及びランナーの結果を入力します。<br />', N'打者結果と同時に発生したプレー(補殺、失策、盗塁、盗塁死、WP、PB)、及びランナーの結果を入力します。<br />三振と同時の盗塁・盗塁死(三振ゲッツーなど)、振り逃げ(WP・PB)なども本エリアで入力します。ボーク・牽制死は「打席中ランナー結果」で入力します。<br />')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass = 21
   AND MessageDetail LIKE N'%打者結果によって発生したプレー(補殺、失策)、及びランナーの結果を入力します。<br />%';

-- B 対象選手の説明
UPDATE dbo.SystemAdmin
   SET MessageDetail = REPLACE(MessageDetail, N'失策した野手、補殺した野手を選択します。<br>', N'発生したプレーに応じて対象選手を選択します(失策・補殺は野手、盗塁・盗塁死はランナー、WPは投手、PBは捕手)。<br>')
     , UpdateUserID = N'ADMIN'
     , UpdateDatetime = SYSDATETIME()
 WHERE SystemAdminClass = 21
   AND MessageDetail LIKE N'%失策した野手、補殺した野手を選択します。<br>%';

--------------------------------------------------------------------------------
-- 3) 実行後の確認（「説明文_後」「対象選手_後」が 1、「説明文_前」「対象選手_前」が 0 になっていること）
--------------------------------------------------------------------------------
SELECT SystemAdminClass
     , 説明文_前 = CASE WHEN MessageDetail LIKE N'%打者結果によって発生したプレー(補殺、失策)、及びランナーの結果を入力します。<br />%' THEN 1 ELSE 0 END
     , 説明文_後 = CASE WHEN MessageDetail LIKE N'%打者結果と同時に発生したプレー(補殺、失策、盗塁、盗塁死、WP、PB)、及びランナーの結果を入力します。<br />%' THEN 1 ELSE 0 END
     , 対象選手_前 = CASE WHEN MessageDetail LIKE N'%失策した野手、補殺した野手を選択します。<br>%' THEN 1 ELSE 0 END
     , 対象選手_後 = CASE WHEN MessageDetail LIKE N'%発生したプレーに応じて対象選手を選択します(失策・補殺は野手、盗塁・盗塁死はランナー、WPは投手、PBは捕手)。<br>%' THEN 1 ELSE 0 END
  FROM dbo.SystemAdmin
 WHERE SystemAdminClass = 21;
