$(function () {
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
        $("#order tbody tr:last-child .js-memberid").first().val(null);
        $("#order tbody tr:last-child .js-positionclass").first().val(null);
        //打順振り直し
        ReOrderNumber();
    });

    //打順振り直し
    function ReOrderNumber() {
        //表示用
        $("#order tbody td.js-displaybattingorder").each(function (i) {
            i = i + 1;
            $(this).text(i);
        });

        //内部用
        $("#order tbody td .js-battingorder").each(function (i) {
            i = i + 1;
            $(this).val(i);
        });
    }
});