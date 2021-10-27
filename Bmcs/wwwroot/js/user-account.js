$(function () {

    //登録ボタン制御
    IsEnableSubmit();

    //登録ボタン制御
    function IsEnableSubmit() {

        var term = $("#term");
        var privacy = $("#privacy");
        var submit = $("#submit");

        if (term.prop('checked') && privacy.prop('checked')) {
            submit.prop("disabled", false);
        }
        else {
            submit.prop("disabled", true);
        }
    }

    //利用規約
    $("#term").change(function () {
        //登録ボタン制御
        IsEnableSubmit();
    });

    //プライバシーポリシー
    $("#privacy").change(function () {
        //登録ボタン制御
        IsEnableSubmit();
    });
});
