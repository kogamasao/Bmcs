/*
 * 数字以外が入っている背番号の確認（参照のみ）
 *
 * 背景：
 *   背番号（Member.UniformNumber）は並べ替えで数値として扱うが、2026-09-26 まで入力チェックが無く、
 *   全角数字・前後の空白・数字以外の文字でも登録できた。2026-09-26 に入力チェック（半角数字3桁以内。全角は半角に変換）を追加した。
 *   本SQLは、それ以前に登録されたデータの件数と内容を確認する。
 *
 * 区分：
 *   A 全角数字・前後の空白 … 20260926_uniform_number_halfwidth.sql で半角に直せる（並び順も正しくなる）
 *   B 数字以外の文字を含む … 自動では直せない（例「10a」「新人」）。アプリでは一覧の末尾に並べ、
 *                             編集画面では背番号を変えなければ他の項目は保存できる。件数を見て扱いを決める
 *
 * ※照合順序は全角半角・大文字小文字を区別しないため、比較はバイナリ照合（Latin1_General_BIN2）で行う。
 */

-- 1) 区分ごとの件数（削除済みを含む）
SELECT 区分
     , 件数 = COUNT(*)
     , うち削除済み = SUM(CASE WHEN DeleteFLG = 1 THEN 1 ELSE 0 END)
  FROM (
        SELECT DeleteFLG
             , 区分 = CASE
                        WHEN LTRIM(RTRIM(TRANSLATE(UniformNumber, N'０１２３４５６７８９', N'0123456789'))) COLLATE Latin1_General_BIN2 NOT LIKE '%[^0-9]%'
                         AND LEN(LTRIM(RTRIM(UniformNumber))) BETWEEN 1 AND 3
                        THEN N'A 全角数字・前後の空白'
                        ELSE N'B 数字以外の文字を含む'
                      END
          FROM dbo.Member
         WHERE UniformNumber IS NOT NULL
           AND UniformNumber <> ''
           AND UniformNumber COLLATE Latin1_General_BIN2 LIKE '%[^0-9]%'
       ) x
 GROUP BY 区分
 ORDER BY 区分;

-- 2) 内容（B 数字以外の文字を含む もの）
SELECT m.MemberID
     , m.TeamID
     , t.TeamName
     , m.MemberName
     , m.UniformNumber
     , m.DeleteFLG
  FROM dbo.Member m
  LEFT JOIN dbo.Team t ON t.TeamID = m.TeamID
 WHERE m.UniformNumber IS NOT NULL
   AND m.UniformNumber <> ''
   AND m.UniformNumber COLLATE Latin1_General_BIN2 LIKE '%[^0-9]%'
   AND NOT (LTRIM(RTRIM(TRANSLATE(m.UniformNumber, N'０１２３４５６７８９', N'0123456789'))) COLLATE Latin1_General_BIN2 NOT LIKE '%[^0-9]%'
            AND LEN(LTRIM(RTRIM(m.UniformNumber))) BETWEEN 1 AND 3)
 ORDER BY m.TeamID, m.MemberID;
