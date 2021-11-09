$(function () {

    $(".js-sample-login").on("click", function () {
        let speed = 500;
        $("html, body").animate({ scrollTop: 0 }, speed, "swing");
    });
});
