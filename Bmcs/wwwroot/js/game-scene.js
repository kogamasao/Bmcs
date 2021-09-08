$(function () {

    //submitボタン
    $("body").on("click", ".js-submit", function () {
        //区分取得
        let submitclass = $(this).data("submitclass");

        //区分セット
        $('#game-scene-submit-class').val(submitclass);

        //submit
        $('form').submit();
    });
});