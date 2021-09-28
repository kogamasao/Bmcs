$(function () {

    //初期表示ボタン制御
    IsUseDeleteButton();

    //ブラウザバック無効化
    history.pushState(null, null, null);

    //ブラウザバックボタン押下時
    $(window).on("popstate", function (event) {
        history.pushState(null, null, null);
        window.alert('試合中にブラウザバックは使用できません。');
    });

    //表スコア変更
    $("body").on("change", ".js-top-score", function () {
        //スコア計算
        CalcTotalScore();
    });

    //裏スコア変更
    $("body").on("change", ".js-buttom-score", function () {
        //スコア計算
        CalcTotalScore();
    });

    //スコア計算
    function CalcTotalScore() {
        var totalScore = 0;

        //表スコア
        $('.js-top-score').each(function () {
            totalScore = totalScore + Number($(this).val());
        });

        //画面セット
        $('.js-top-sum')[0].innerHTML = totalScore;

        totalScore = 0;

        //裏スコア
        $('.js-buttom-score').each(function () {
            totalScore = totalScore + Number($(this).val());
        });

        //画面セット
        $('.js-buttom-sum')[0].innerHTML = totalScore;
    }

    //削除ボタン使用制御
    function IsUseDeleteButton() {
        var len = $(".js-title-score-th").length;
        if (len == 1) {
            $("#delete-inning").prop("disabled", true);
        }
        else {
            $("#delete-inning").prop("disabled", false);
        }

        len = $("#pitcher-score tbody").children().length;
        if (len == 1) {
            $(".js-pitcher-delete").prop("disabled", true);
        }
        else {
            $(".js-pitcher-delete").prop("disabled", false);
        }

        len = $("#fielder-score tbody").children().length;
        if (len == 1) {
            $(".js-fielder-delete").prop("disabled", true);
        }
        else {
            $(".js-fielder-delete").prop("disabled", false);
        }
    }

    // イニング追加
    $("#add-inning").on("click", function () {
        //最終イニングタイトルのコピーを取得
        var lastScore = $(".js-title-score-th:last").clone(true);
        //index取得
        var index = lastScore[0].innerHTML;
        //リネーム
        lastScore[0].innerHTML = lastScore[0].innerHTML.replaceAll(index.toString(), (Number(index) + 1).toString());
        var newInning = lastScore[0];
        //追加
        $('.js-title-score-th:last').after(newInning);

        //最終イニング表のコピーを取得
        lastScore = $(".js-top-score-td:last").clone(true);
        //index取得
        index = lastScore[0].id.replaceAll('InningScoreTopList_', '');
        //リネーム
        newInning = lastScore[0].outerHTML.replaceAll('InningScoreTopList_' + index.toString(), 'InningScoreTopList_' + (Number(index) + 1).toString())
            .replaceAll('InningScoreTopList[' + index.toString() + ']', 'InningScoreTopList[' + (Number(index) + 1).toString() + ']');
        //追加
        $('.js-top-score-td:last').after(newInning);

        //最終イニング裏のコピーを取得
        lastScore = $(".js-buttom-score-td:last").clone(true);
        //index取得
        index = lastScore[0].id.replaceAll('InningScoreButtomList_', '');
        //リネーム
        newInning = lastScore[0].outerHTML.replaceAll('InningScoreButtomList_' + index.toString(), 'InningScoreButtomList_' + (Number(index) + 1).toString())
            .replaceAll('InningScoreButtomList[' + index.toString() + ']', 'InningScoreButtomList[' + (Number(index) + 1).toString() + ']');
        //追加
        $('.js-buttom-score-td:last').after(newInning);

        //連番振り直し
        ReInningNumber();

        //削除ボタン使用制御
        IsUseDeleteButton();

        //新規列クリア
        $(".js-top-score:last").first().val(null);
        $(".js-buttom-score:last").first().val(null);
    });

    // 投手スコア行追加
    $("#add-pitcher-score").on("click", function () {
        //最終行のコピーを取得
        var lastRow = $("#pitcher-score tbody tr:last-child").clone(true);
        //index取得
        var index = lastRow[0].id.replaceAll('GameScorePitcherList_', '');
        //リネーム
        var newRow = lastRow[0].outerHTML.replaceAll('GameScorePitcherList_' + index.toString(), 'GameScorePitcherList_' + (Number(index) + 1).toString())
            .replaceAll('GameScorePitcherList[' + index.toString() + ']', 'GameScorePitcherList[' + (Number(index) + 1).toString() + ']');
        //最後尾追加
        $('#pitcher-score tbody').append(newRow);

        //連番振り直し
        RePitcherNumber();

        //削除ボタン使用制御
        IsUseDeleteButton();

        //新規行クリア
        $("#pitcher-score tbody tr:last-child .js-rename").val(null);
    });

    // 野手スコア行追加
    $("#add-fielder-score").on("click", function () {
        //最終行のコピーを取得
        var lastRow = $("#fielder-score tbody tr:last-child").clone(true);
        //index取得
        var index = lastRow[0].id.replaceAll('GameScoreFielderList_', '');
        //リネーム
        var newRow = lastRow[0].outerHTML.replaceAll('GameScoreFielderList_' + index.toString(), 'GameScoreFielderList_' + (Number(index) + 1).toString())
            .replaceAll('GameScoreFielderList[' + index.toString() + ']', 'GameScoreFielderList[' + (Number(index) + 1).toString() + ']');
        //最後尾追加
        $('#fielder-score tbody').append(newRow);

        //連番振り直し
        ReFielderNumber();

        //削除ボタン使用制御
        IsUseDeleteButton();

        //新規行クリア
        $("#fielder-score tbody tr:last-child .js-rename").val(null);
    });

    //イニング連番振り直し
    function ReInningNumber() {
        //イニングタイトル振り直し
        $(".js-title-score-th").each(function (newIndex) {
            var inningTitle = $(this);

            //イニング
            inningTitle.innerHTML = newIndex + 1;
        });

        //表イニングName、ID振り直し
        $(".js-top-score-td").each(function (newIndex) {
            //index取得
            var index = this.id.replaceAll('InningScoreTopList_', '');
            var id = $(this).attr('id');
            var name = $(this).attr('name');

            if (id) {
                $(this).attr('id', id.replaceAll('InningScoreTopList_' + index.toString(), 'InningScoreTopList_' + (newIndex).toString()));
            }

            if (name) {
                $(this).attr('name', name.replaceAll('InningScoreTopList[' + index.toString() + ']', 'InningScoreTopList[' + (newIndex).toString() + ']'));
            }

            $(this).find('.js-rename').each(function () {
                var childId = $(this).attr('id');
                var childName = $(this).attr('name');

                if (childId) {
                    $(this).attr('id', childId.replaceAll('InningScoreTopList_' + index.toString(), 'InningScoreTopList_' + (newIndex).toString()));
                }

                if (childName) {
                    $(this).attr('name', childName.replaceAll('InningScoreTopList[' + index.toString() + ']', 'InningScoreTopList[' + (newIndex).toString() + ']'));
                }
            });
        });

        //裏イニングName、ID振り直し
        $(".js-buttom-score-td").each(function (newIndex) {
            //index取得
            var index = this.id.replaceAll('InningScoreButtomList_', '');
            var id = $(this).attr('id');
            var name = $(this).attr('name');

            if (id) {
                $(this).attr('id', id.replaceAll('InningScoreButtomList_' + index.toString(), 'InningScoreButtomList_' + (newIndex).toString()));
            }

            if (name) {
                $(this).attr('name', name.replaceAll('InningScoreButtomList[' + index.toString() + ']', 'InningScoreButtomList[' + (newIndex).toString() + ']'));
            }

            $(this).find('.js-rename').each(function () {
                var childId = $(this).attr('id');
                var childName = $(this).attr('name');

                if (childId) {
                    $(this).attr('id', childId.replaceAll('InningScoreButtomList_' + index.toString(), 'InningScoreButtomList_' + (newIndex).toString()));
                }

                if (childName) {
                    $(this).attr('name', childName.replaceAll('InningScoreButtomList[' + index.toString() + ']', 'InningScoreButtomList[' + (newIndex).toString() + ']'));
                }
            });
        });
    }

    //投手スコア連番振り直し
    function RePitcherNumber() {
        //Name、ID振り直し
        $("#pitcher-score tbody tr").each(function (newIndex) {
            //index取得
            var index = this.id.replaceAll('GameScorePitcherList_', '');
            var id = $(this).attr('id');
            var name = $(this).attr('name');

            if (id) {
                $(this).attr('id', id.replaceAll('GameScorePitcherList_' + index.toString(), 'GameScorePitcherList_' + (newIndex).toString()));
            }

            if (name) {
                $(this).attr('name', name.replaceAll('GameScorePitcherList[' + index.toString() + ']', 'GameScorePitcherList[' + (newIndex).toString() + ']'));
            }

            $(this).find('.js-rename').each(function () {
                var childId = $(this).attr('id');
                var childName = $(this).attr('name');

                if (childId) {
                    $(this).attr('id', childId.replaceAll('GameScorePitcherList_' + index.toString(), 'GameScorePitcherList_' + (newIndex).toString()));
                }

                if (childName) {
                    $(this).attr('name', childName.replaceAll('GameScorePitcherList[' + index.toString() + ']', 'GameScorePitcherList[' + (newIndex).toString() + ']'));
                }
            });
        });
    }

    //野手スコア連番振り直し
    function ReFielderNumber() {
        //Name、ID振り直し
        $("#fielder-score tbody tr").each(function (newIndex) {
            //index取得
            var index = this.id.replaceAll('GameScoreFielderList_', '');
            var id = $(this).attr('id');
            var name = $(this).attr('name');

            if (id) {
                $(this).attr('id', id.replaceAll('GameScoreFielderList_' + index.toString(), 'GameScoreFielderList_' + (newIndex).toString()));
            }

            if (name) {
                $(this).attr('name', name.replaceAll('GameScoreFielderList[' + index.toString() + ']', 'GameScoreFielderList[' + (newIndex).toString() + ']'));
            }

            $(this).find('.js-rename').each(function () {
                var childId = $(this).attr('id');
                var childName = $(this).attr('name');

                if (childId) {
                    $(this).attr('id', childId.replaceAll('GameScoreFielderList_' + index.toString(), 'GameScoreFielderList_' + (newIndex).toString()));
                }

                if (childName) {
                    $(this).attr('name', childName.replaceAll('GameScoreFielderList[' + index.toString() + ']', 'GameScoreFielderList[' + (newIndex).toString() + ']'));
                }
            });
        });
    }

    //イニング削除
    $("#delete-inning").on("click", function () {
        //イニングタイトル
        $(".js-title-score-th:last").remove();
        //表イニング
        $(".js-top-score-td:last").remove();
        //裏イニング
        $(".js-buttom-score-td:last").remove();

        //連番振り直し
        ReInningNumber();

        //スコア計算
        CalcTotalScore();

        //削除ボタン使用制御
        IsUseDeleteButton();
    });

    // 投手スコア行削除
    $("body").on("click", ".js-pitcher-delete", function () {
        //選択行取得
        let row = $(this).closest("tr").remove();
        //行削除
        $(row).remove();
        //連番振り直し
        RePitcherNumber();
        //削除ボタン使用制御
        IsUseDeleteButton();
    });

    // 野手スコア行削除
    $("body").on("click", ".js-fielder-delete", function () {
        //選択行取得
        let row = $(this).closest("tr").remove();
        //行削除
        $(row).remove();
        //連番振り直し
        ReFielderNumber();
        //削除ボタン使用制御
        IsUseDeleteButton();
    });
});
