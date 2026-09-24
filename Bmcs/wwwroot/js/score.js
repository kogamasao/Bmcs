$(function () {

    //ソート項目セット
    SetSortItem();

    //ソート項目セット
    function SetSortItem() {

        var sortValue = $("#SortItem").val();

        if (sortValue) {
            var sortItem = $("." + sortValue);

            if (sortItem) {
                //並べ替え中の列を強調する
                //※以前は Bootstrap（text-body / font-weight-bold）と customize.css（column-sort-select）の
                //  クラスを付けていたが、Tailwind 版の画面ではどちらも読み込まないため効かなくなっていた。
                //  Tailwind はソース中の完全なクラス名しか生成しないので、クラス名は分割せずに書くこと
                sortItem.addClass("bg-sky-50 font-bold text-slate-900");
            }
        }
    }
});
