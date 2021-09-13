$(function () {

    //ブラウザバック無効化
    history.pushState(null, null, null);

    //ブラウザバックボタン押下時
    $(window).on("popstate", function (event) {
        history.pushState(null, null, null);
        window.alert('試合中にブラウザバックは使用できません。');
    });

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
