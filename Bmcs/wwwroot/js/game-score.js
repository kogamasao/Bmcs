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
        ReNumber();

        //削除ボタン使用制御
        IsUseDeleteButton();

        //新規列クリア
        $(".js-top-score:last").first().val(null);
        $(".js-buttom-score:last").first().val(null);
    });

    //連番振り直し
    function ReNumber() {
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

    //イニング削除
    //$("body").on("click", "#delete-inning", function () {
    $("#delete-inning").on("click", function () {
        //イニングタイトル
        $(".js-title-score-th:last").remove();
        //表イニング
        $(".js-top-score-td:last").remove();
        //裏イニング
        $(".js-buttom-score-td:last").remove();

        //連番振り直し
        ReNumber();

        //スコア計算
        CalcTotalScore();

        //削除ボタン使用制御
        IsUseDeleteButton();
    });


});
