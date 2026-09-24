$(function () {

    // 作成後の遷移先の案内を、選んだ入力方式に合わせる。
    // ※サーバで描画した時点の値のままだと、「試合結果だけ入力する」を選んでも「打順の設定に進みます」と出ていた
    $("input[name='Game.GameInputTypeClass']").on("change", function () {
        $(".js-next-step").text($(this).data("next-step"));
    });

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
