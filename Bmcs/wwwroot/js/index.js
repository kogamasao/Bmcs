$(function () {

    // 「サンプルチームで体験する」
    // ※以前はログインフォームまでスクロールするだけだったため、
    //   訪問者が画面に表示されたID・パスワードを手で入力する必要があった。
    //   ここで自動入力して送信し、1クリックで体験できるようにする。
    $(".js-sample-login").on("click", function () {

        var $userAccountID = $("#UserAccount_UserAccountID");
        var $password = $("#UserAccount_Password");

        if (!$userAccountID.length || !$password.length) {
            return;
        }

        $userAccountID.val("YGUser");
        $password.val("1");

        // 入力チェックのエラー表示が残っている場合に送信が止まらないようにする
        var form = $userAccountID.closest("form");

        if (form.length) {
            form.trigger("submit");
        }
    });
});
