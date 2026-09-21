$(function () {

    // 相手チーム名から略名の候補を入れる。
    // ※略名はスコアボード表示に使うが「何を入れればいいか」が分かりにくく、
    //   ここで止まる人がいるため候補を先に入れておく。手で直せる。
    var $opponentName = $("#Game_OpponentTeamName");
    var $abbreviation = $(".js-opponent-abbreviation");

    if (!$opponentName.length || !$abbreviation.length) {
        return;
    }

    //ユーザが自分で入力した後は上書きしない
    //※入力エラーでの再表示では値が復元されているため、その場合も上書きしない
    var isEditedByUser = $abbreviation.val().length > 0;

    $abbreviation.on("input", function () {
        isEditedByUser = true;
    });

    $opponentName.on("input", function () {
        if (isEditedByUser) {
            return;
        }

        $abbreviation.val($(this).val().substring(0, 10));
    });
});
