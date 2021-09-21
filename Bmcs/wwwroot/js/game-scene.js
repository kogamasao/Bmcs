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

    //数値⇒クラス値に変換
    function ConvertValueToRunnerClass(value) {

        if (value == 2) {
            return 'OnFirstBase'
        }
        else if (value == 3) {
            return 'OnSecondBase'
        }
        else if (value == 4) {
            return 'OnThirdBase'
        }
        else {
            return ''
        }
    }

    //クラス値⇒数値に変換
    function ConvertRunnerClassToValue(runnerClass) {
        if (runnerClass == 'OnFirstBase') {
            return 2
        }
        else if (runnerClass == 'OnSecondBase') {
            return 3
        }
        else if (runnerClass == 'OnThirdBase') {
            return 4
        }
        else {
            return 1
        }
    }

    //submitボタン
    $("body").on("click", ".js-submit", function () {
        //区分取得
        let submitclass = $(this).data("submitclass");

        //区分セット
        $('#game-scene-submit-class').val(submitclass);

        //submit
        $('form').submit();
    });

    //打席中ランナーメンバー
    $(".js-before-runner-member").change(function () {
        var memberID = $(this).val();
        var runnerclass = $(this).data("runnerclass");

        var afterRunnerMember = $(".js-after-runner-member[data-runnerclass='" + runnerclass + "']")

        if (afterRunnerMember) {
            afterRunnerMember.val(memberID);
        }
    });

    //打席中ランナー結果
    $(".js-before-runner-result").change(function () {
        var select = $(this);
        var result = select.val();
        var resultName = select.find('option:selected');
        
        var runnerclass = $(this).data("runnerclass");

        var afterRunnerResult = $(".js-after-runner-result[data-runnerclass='" + runnerclass + "']")
        var afterRunnerClass = $(".js-after-runner-class[data-runnerclass='" + runnerclass + "']")
        var afterRunnerClassName = $(".js-after-runner-class-name[data-runnerclass='" + runnerclass + "']")

        if (afterRunnerResult) {
            afterRunnerResult.val(result);

            //アウト、得点
            if (result == 1
                || (result >= 5
                    && result <= 8)) {
                afterRunnerResult.parent().parent().addClass('d-none');
            }
            else {
                afterRunnerResult.parent().parent().removeClass('d-none');
            }
        }

        if (afterRunnerClass) {
            afterRunnerClass.val(ConvertValueToRunnerClass(result));
        }

        if (afterRunnerClassName) {
            afterRunnerClassName[0].innerHTML = resultName[0].innerHTML;
        }
    });

    //打者メンバー
    $(".js-batter-member").change(function () {
        var memberID = $(this).val();

        var batterRunnerMember = $(".js-after-runner-member[data-runnerclass='Batter']")

        if (batterRunnerMember) {
            batterRunnerMember.val(memberID);
        }
    });

    //打者結果
    $(".js-batter-result").change(function () {
        var result = $(this).val();

        var batterRunnerResult = $(".js-after-runner-result[data-runnerclass='Batter']")
        //var beforeBatterRunnerResult = $("#before-batter-runner-result")

        //if (!beforeBatterRunnerResult.val()) {
        //    beforeBatterRunnerResult.val(1);
        //}

        if (batterRunnerResult) {

            var nextBatter = $(".js-submit[data-submitclass='NextBatter']");
            nextBatter.prop("disabled", false);
            $("#after-detail-runner").show();

            if ((result >= 1 && result <= 10)
                || (result == 23 || result == 24)) {
                batterRunnerResult.val(1);
            }
            else if ((result >= 11 && result <= 31)
                && (result != 23 && result != 24)) {
                batterRunnerResult.val(2);
            }
            else if (result == 32) {
                batterRunnerResult.val(3);
            }
            else if (result == 33) {
                batterRunnerResult.val(4);
            }
            else if (result == 34) {
                batterRunnerResult.val(5);
            }
            else if (result == 91) {
                batterRunnerResult.val(1);
                nextBatter.prop("disabled", true);
                $("#after-detail-runner").hide();
            }

            var backFirstRunnerFLG = false;
            var backSecondRunnerFLG = false;

            //他ランナー
            $('.js-after-runner').each(function () {
                var afterRunnerClass = $(this).find('.js-after-runner-class');
                var afterRunnerResult = $(this).find('.js-after-runner-result');

                if (afterRunnerClass.val() == ''
                    || afterRunnerClass.val() == 'Batter') {
                    return true;
                }

                var afterRunnerClassValue = ConvertRunnerClassToValue(afterRunnerClass.val());

                var runnerResult = Number(batterRunnerResult.val()) + Number(afterRunnerClassValue) - 1;

                //得点は打点、自責あり
                if (runnerResult > 5) {
                    runnerResult = 5;
                }

                if (afterRunnerResult) {
                    //エラー、野選、犠打、犠牲、安打系
                    if ((result == 12 || result == 13)
                        || (result >= 31 && result <= 34)) {
                        afterRunnerResult.val(runnerResult);
                    }
                    //犠打、犠牲
                    else if (result == 23 || result == 24) {
                        afterRunnerResult.val(runnerResult + 1);
                    }
                    //振逃、四球、死球、打撃妨害
                    else if ((result == 11)
                        || (result == 21 || result == 22 || result == 25)) {

                        //一塁ランナーあり
                        if (afterRunnerClass.val() == 'OnFirstBase') {
                            afterRunnerResult.val(runnerResult);
                            backFirstRunnerFLG = true;
                        }
                        //二塁ランナーあり
                        else if (afterRunnerClass.val() == 'OnSecondBase' && backFirstRunnerFLG) {
                            afterRunnerResult.val(runnerResult);
                            backSecondRunnerFLG = true;
                        }
                        //三塁ランナーあり
                        else if (afterRunnerClass.val() == 'OnThirdBase' && backSecondRunnerFLG) {
                            afterRunnerResult.val(runnerResult);
                        }
                        else {
                            afterRunnerResult.val(afterRunnerClassValue);
                        }
                    }
                    //併殺
                    else if (result == 4) {
                        //一塁ランナーあり
                        if (afterRunnerClass.val() == 'OnFirstBase') {
                            afterRunnerResult.val(1);
                        }
                        else {
                            afterRunnerResult.val(afterRunnerClassValue);
                        }
                    }
                    //その他
                    else {
                        afterRunnerResult.val(runnerResult);
                    }
                }

            });
        }
    });


    //削除ボタン使用制御
    function IsUseDeleteButton() {
        var len = $("#before-detail tbody").children().length;
        if (len == 1) {
            $(".js-before-delete").prop("disabled", true);
        }
        else {
            $(".js-before-delete").prop("disabled", false);
        }

        len = $("#after-detail tbody").children().length;
        if (len == 1) {
            $(".js-after-delete").prop("disabled", true);
        }
        else {
            $(".js-after-delete").prop("disabled", false);
        }
    }

    // 打席中詳細行追加
    $("#add-before-detail").on("click", function () {
        //最終行のコピーを取得
        var lastRow = $("#before-detail tbody tr:last-child").clone(true);
        //index取得
        var index = lastRow[0].id.replaceAll('BeforeGameSceneDetailList_', '');
        //リネーム
        var newRow = lastRow[0].outerHTML.replaceAll('BeforeGameSceneDetailList_' + index.toString(), 'BeforeGameSceneDetailList_' + (Number(index) + 1).toString())
            .replaceAll('BeforeGameSceneDetailList[' + index.toString() + ']', 'BeforeGameSceneDetailList[' + (Number(index) + 1).toString() + ']');
        //最後尾追加
        $('#before-detail tbody').append(newRow);

        //連番振り直し
        ReBeforeNumber();

        //削除ボタン使用制御
        IsUseDeleteButton();

        //新規行クリア
        $("#before-detail tbody tr:last-child .js-memberid").first().val(null);
        $("#before-detail tbody tr:last-child .js-result").first().val(null);
    });

    //打席中詳細連番振り直し
    function ReBeforeNumber() {
        //Name、ID振り直し
        $("#before-detail tbody tr").each(function (newIndex) {
            //index取得
            var index = this.id.replaceAll('BeforeGameSceneDetailList_', '');
            var id = $(this).attr('id');
            var name = $(this).attr('name');

            if (id) {
                $(this).attr('id', id.replaceAll('BeforeGameSceneDetailList_' + index.toString(), 'BeforeGameSceneDetailList_' + (newIndex).toString()));
            }

            if (name) {
                $(this).attr('name', name.replaceAll('BeforeGameSceneDetailList[' + index.toString() + ']', 'BeforeGameSceneDetailList[' + (newIndex).toString() + ']'));
            }

            $(this).find('.js-rename').each(function () {
                var childId = $(this).attr('id');
                var childName = $(this).attr('name');

                if (childId) {
                    $(this).attr('id', childId.replaceAll('BeforeGameSceneDetailList_' + index.toString(), 'BeforeGameSceneDetailList_' + (newIndex).toString()));
                }

                if (childName) {
                    $(this).attr('name', childName.replaceAll('BeforeGameSceneDetailList[' + index.toString() + ']', 'BeforeGameSceneDetailList[' + (newIndex).toString() + ']'));
                }
            });
        });
    }

    // 打席中詳細行削除
    $("body").on("click", ".js-before-delete", function () {
        //選択行取得
        let row = $(this).closest("tr").remove();
        //行削除
        $(row).remove();
        //連番振り直し
        ReBeforeNumber();
        //削除ボタン使用制御
        IsUseDeleteButton();
    });

    // 打席後詳細行追加
    $("#add-after-detail").on("click", function () {
        //最終行のコピーを取得
        var lastRow = $("#after-detail tbody tr:last-child").clone(true);
        //index取得
        var index = lastRow[0].id.replaceAll('AfterGameSceneDetailList_', '');
        //リネーム
        var newRow = lastRow[0].outerHTML.replaceAll('AfterGameSceneDetailList_' + index.toString(), 'AfterGameSceneDetailList_' + (Number(index) + 1).toString())
            .replaceAll('AfterGameSceneDetailList[' + index.toString() + ']', 'AfterGameSceneDetailList[' + (Number(index) + 1).toString() + ']');
        //最後尾追加
        $('#after-detail tbody').append(newRow);

        //連番振り直し
        ReAfterNumber();

        //削除ボタン使用制御
        IsUseDeleteButton();

        //新規行クリア
        $("#after-detail tbody tr:last-child .js-memberid").first().val(null);
        $("#after-detail tbody tr:last-child .js-result").first().val(null);
    });

    //打席後詳細連番振り直し
    function ReAfterNumber() {
        //Name、ID振り直し
        $("#after-detail tbody tr").each(function (newIndex) {
            //index取得
            var index = this.id.replaceAll('AfterGameSceneDetailList_', '');
            var id = $(this).attr('id');
            var name = $(this).attr('name');

            if (id) {
                $(this).attr('id', id.replaceAll('AfterGameSceneDetailList_' + index.toString(), 'AfterGameSceneDetailList_' + (newIndex).toString()));
            }

            if (name) {
                $(this).attr('name', name.replaceAll('AfterGameSceneDetailList[' + index.toString() + ']', 'AfterGameSceneDetailList[' + (newIndex).toString() + ']'));
            }

            $(this).find('.js-rename').each(function () {
                var childId = $(this).attr('id');
                var childName = $(this).attr('name');

                if (childId) {
                    $(this).attr('id', childId.replaceAll('AfterGameSceneDetailList_' + index.toString(), 'AfterGameSceneDetailList_' + (newIndex).toString()));
                }

                if (childName) {
                    $(this).attr('name', childName.replaceAll('AfterGameSceneDetailList[' + index.toString() + ']', 'AfterGameSceneDetailList[' + (newIndex).toString() + ']'));
                }
            });
        });
    }

    // 打席後詳細行削除
    $("body").on("click", ".js-after-delete", function () {
        //選択行取得
        let row = $(this).closest("tr").remove();
        //行削除
        $(row).remove();
        //連番振り直し
        ReAfterNumber();
        //削除ボタン使用制御
        IsUseDeleteButton();
    });
});
