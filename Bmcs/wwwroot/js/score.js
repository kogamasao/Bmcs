$(function () {

    //ソート項目セット
    SetSortItem();

    //ソート項目セット
    function SetSortItem() {

        var sortValue = $("#SortItem").val();

        if (sortValue) {
            var sortItem = $("." + sortValue);

            if (sortItem) {
                sortItem.children().addClass("text-body");
                sortItem.addClass("font-weight-bold");
                sortItem.addClass("bg-light");
            }
        }
    }
});
