$(function () {

    //選択メッセージセット
    SetSelectMessage();

    //選択メッセージセット
    function SetSelectMessage() {

        var messagePageClass = $("#MessagePageClass").val();

        if (messagePageClass) {
            var selectItem = $("." + messagePageClass);

            if (selectItem) {
                selectItem.addClass("select-message");
            }
        }
    }
});
