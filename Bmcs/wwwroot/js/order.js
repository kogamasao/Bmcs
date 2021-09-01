$(function () {

    //初期表示ボタン制御
    IsUserDeleteButton();

    // 行追加
    $("#add-order").on("click", function () {
        //最終行のコピーを取得
        var lastRow = $("#order tbody tr:last-child").clone(true);
        //index取得
        var index = lastRow[0].id.replaceAll('OrderList_', '');
        //リネーム
        var newRow = lastRow[0].outerHTML.replaceAll('OrderList_' + index.toString(), 'OrderList_' + (Number(index) + 1).toString() )
            .replaceAll('OrderList[' + index.toString() + ']', 'OrderList[' + (Number(index) + 1).toString() + ']');
        //最後尾追加
        $('#order tbody').append(newRow);
        //連番振り直し
        ReOrderNumber();

        //削除ボタン使用制御
        IsUserDeleteButton();

        //新規行クリア
        $("#order tbody tr:last-child .js-memberid").first().val(null);
        $("#order tbody tr:last-child .js-positionclass").first().val(null);
    });

    // 行追加
    $("#add-only-defense").on("click", function () {
        //守備のみ最終行を取得
        var onlyDefenseLastRow = $("#only-defense tbody tr:last-child");
        //オーダー最終行のコピーを取得
        var orderLastRow = $("#order tbody tr:last-child").clone(true);

        //守備のみindex取得
        var onlyDefenseIndex = -1;
        if (onlyDefenseLastRow.length > 0) {
            onlyDefenseIndex = onlyDefenseLastRow[0].id.replaceAll('OnlyDefenseList_', '');
        }
        //オーダーindex取得
        var orderIndex = orderLastRow[0].id.replaceAll('OrderList_', '');

        //リネーム
        var newRow = orderLastRow[0].outerHTML.replaceAll('OrderList_' + orderIndex.toString(), 'OnlyDefenseList_' + (Number(onlyDefenseIndex) + 1).toString())
            .replaceAll('OrderList[' + orderIndex.toString() + ']', 'OnlyDefenseList[' + (Number(onlyDefenseIndex) + 1).toString() + ']');

        //最後尾追加
        $('#only-defense tbody').append(newRow);

        //連番振り直し
        ReOnlyDefenseNumber();

        //新規行クリア
        $("#only-defense tbody tr:last-child .js-memberid").first().val(null);
        $("#only-defense tbody tr:last-child .js-positionclass").first().val(null);
        $("#only-defense tbody tr:last-child .js-displaybattingorder").first().text(null);
        $("#only-defense tbody tr:last-child .js-battingorder").first().val(null);
    });

    //連番振り直し
    function ReOrderNumber() {
        //Name、ID振り直し
        $("#order tbody tr").each(function (newIndex) {
            //index取得
            var index = this.id.replaceAll('OrderList_', '');
            var id = $(this).attr('id');
            var name = $(this).attr('name');

            if (id) {
                $(this).attr('id', id.replaceAll('OrderList_' + index.toString(), 'OrderList_' + (newIndex).toString()));
            }

            if (name) {
                $(this).attr('name', name.replaceAll('OrderList[' + index.toString() + ']', 'OrderList[' + (newIndex).toString() + ']'));
            }

            $(this).find('.js-rename').each(function () {
                var childId = $(this).attr('id');
                var childName = $(this).attr('name');

                if (childId) {
                    $(this).attr('id', childId.replaceAll('OrderList_' + index.toString(), 'OrderList_' + (newIndex).toString()));
                }

                if (childName) {
                    $(this).attr('name', childName.replaceAll('OrderList[' + index.toString() + ']', 'OrderList[' + (newIndex).toString() + ']'));
                }
            });
        });

        //打順表示用
        $("#order tbody td.js-displaybattingorder").each(function (i) {
            i = i + 1;
            $(this).text(i);
        });

        //打順内部用
        $("#order tbody td .js-battingorder").each(function (i) {
            i = i + 1;
            $(this).val(i);
        });
    }

    //連番振り直し
    function ReOnlyDefenseNumber() {

        //Name、ID振り直し
        $("#only-defense tbody tr").each(function (newIndex) {
            //index取得
            var index = this.id.replaceAll('OnlyDefenseList_', '');
            var id = $(this).attr('id');
            var name = $(this).attr('name');

            if (id) {
                $(this).attr('id', id.replaceAll('OnlyDefenseList_' + index.toString(), 'OnlyDefenseList_' + (newIndex).toString()));
            }

            if (name) {
                $(this).attr('name', name.replaceAll('OnlyDefenseList[' + index.toString() + ']', 'OnlyDefenseList[' + (newIndex).toString() + ']'));
            }

            $(this).find('.js-rename').each(function () {
                var childId = $(this).attr('id');
                var childName = $(this).attr('name');

                if (childId) {
                    $(this).attr('id', childId.replaceAll('OnlyDefenseList_' + index.toString(), 'OnlyDefenseList_' + (newIndex).toString()));
                }

                if (childName) {
                    $(this).attr('name', childName.replaceAll('OnlyDefenseList[' + index.toString() + ']', 'OnlyDefenseList[' + (newIndex).toString() + ']'));
                }
            });
        });
    }

    //削除ボタン使用制御
    function IsUserDeleteButton() {
        var len = $("#order tbody").children().length;
        if (len == 1) {
            $(".js-order-delete").prop("disabled", true);
        }
        else {
            $(".js-order-delete").prop("disabled", false);
        }
    }

    // オーダー削除
    $("body").on("click", ".js-order-delete", function () {
        //選択行取得
        let row = $(this).closest("tr").remove();
        //行削除
        $(row).remove();
        //連番振り直し
        ReOrderNumber();
        //削除ボタン使用制御
        IsUserDeleteButton();
    });

    // 守備のみ削除
    $("body").on("click", ".js-only-defense-delete", function () {
        //選択行取得
        let row = $(this).closest("tr").remove();
        //行削除
        $(row).remove();
        //連番振り直し
        ReOnlyDefenseNumber();
    });
});