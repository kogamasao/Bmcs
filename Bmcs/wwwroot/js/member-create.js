/*
 * メンバー追加（複数人をまとめて登録する）
 * ・行の追加：<template> のひな形を複製し、「__index__」を未使用の番号に置き換える
 *   （送信時は「MemberRowList.Index」で番号を送るため、行を削除して欠番になってもよい）
 * ・行の削除
 * ・登録する人数を「◯人を登録する」としてボタンに表示する
 * ・入力中の Enter で送信しない（途中までの入力が登録されるのを防ぐ）。次の入力欄へ移る
 * ・二重送信の防止
 */
(function () {
    var form = document.querySelector(".js-member-create-form");

    if (!form) {
        return;
    }

    var rows = form.querySelector(".js-member-rows");
    var template = form.querySelector(".js-member-row-template");
    var addButton = form.querySelector(".js-member-row-add");
    var submitButton = form.querySelector(".js-member-submit");
    var submitLabel = form.querySelector(".js-member-submit-label");
    var maxRowCount = Number(rows.dataset.maxRowCount);
    var addRowCount = Number(addButton.dataset.addRowCount);

    //次に使う行の番号（表示中の番号の最大値＋1）
    var nextIndex = 0;

    rows.querySelectorAll("input[name='MemberRowList.Index']").forEach(function (input) {
        nextIndex = Math.max(nextIndex, Number(input.value) + 1);
    });

    function rowList() {
        return rows.querySelectorAll(".js-member-row");
    }

    function addRow() {
        var html = template.innerHTML.replace(/__index__/g, String(nextIndex));
        nextIndex++;
        rows.insertAdjacentHTML("beforeend", html);
    }

    //入力された行（背番号か名前が入っている行）の数
    function inputRowCount() {
        var count = 0;

        rowList().forEach(function (row) {
            var hasValue = Array.prototype.some.call(row.querySelectorAll(".js-member-input"), function (input) {
                return input.value.trim() !== "";
            });

            if (hasValue) {
                count++;
            }
        });

        return count;
    }

    function refresh() {
        var count = inputRowCount();

        submitLabel.textContent = count > 0 ? count + "人を登録する" : "登録する";
        addButton.disabled = rowList().length >= maxRowCount;
    }

    addButton.addEventListener("click", function () {
        var firstNewRow = null;

        for (var i = 0; i < addRowCount && rowList().length < maxRowCount; i++) {
            addRow();

            if (!firstNewRow) {
                firstNewRow = rows.lastElementChild;
            }
        }

        refresh();

        if (firstNewRow) {
            var input = firstNewRow.querySelector(".js-member-input");

            if (input) {
                input.focus();
            }
        }
    });

    rows.addEventListener("click", function (event) {
        var deleteButton = event.target.closest(".js-member-row-delete");

        if (!deleteButton) {
            return;
        }

        deleteButton.closest(".js-member-row").remove();

        //最後の1行を消した場合も、入力できる行を1行残す
        if (rowList().length === 0) {
            addRow();
        }

        refresh();
    });

    rows.addEventListener("input", refresh);

    //Enter で送信せず、次の入力欄（背番号→名前→次の行の背番号）へ移る
    rows.addEventListener("keydown", function (event) {
        //※日本語入力の変換を確定する Enter では移動しない。
        //  Safari（iPhone を含む）は確定後に keydown を出し、isComposing が false になるため、keyCode 229 でも判定する
        if (event.key !== "Enter" || !event.target.classList.contains("js-member-input") || event.isComposing || event.keyCode === 229) {
            return;
        }

        event.preventDefault();

        var inputList = Array.prototype.slice.call(rows.querySelectorAll(".js-member-input"));
        var index = inputList.indexOf(event.target);

        if (index >= 0 && index < inputList.length - 1) {
            inputList[index + 1].focus();
        }
    });

    form.addEventListener("submit", function () {
        //二重送信の防止（同じメンバーが2人ずつ登録されるのを防ぐ）
        submitButton.disabled = true;
    });

    //ブラウザの「戻る」でこの画面に戻ったとき、送信時に無効にしたボタンが押せないままにならないようにする
    window.addEventListener("pageshow", function () {
        submitButton.disabled = false;
    });

    refresh();
})();
