$(function () {

    // チーム名からチーム略名の候補を入れる。
    // ※略名は必須だが「何を入れればいいか」が分かりにくく、
    //   ここで止まる人がいるため候補を先に入れておく。手で直せる。
    var $teamName = $("#Team_TeamName");
    var $abbreviation = $(".js-team-abbreviation");

    if (!$teamName.length || !$abbreviation.length) {
        return;
    }

    //ユーザが自分で入力した後は上書きしない
    var isEditedByUser = false;

    $abbreviation.on("input", function () {
        isEditedByUser = true;
    });

    $teamName.on("input", function () {
        if (isEditedByUser) {
            return;
        }

        //10文字までしか登録できないため切り詰める
        $abbreviation.val($(this).val().substring(0, 10));
    });
});
