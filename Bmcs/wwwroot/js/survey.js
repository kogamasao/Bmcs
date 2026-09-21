// アンケート回答画面
$(function () {

    // 「その他」を選択したときだけ自由入力欄を表示する
    function toggleFreeText(questionIndex) {
        var $freeText = $('#freetext_' + questionIndex);

        if ($freeText.length === 0) {
            return;
        }

        var isChecked = $('.js-survey-choice[data-question-index="' + questionIndex + '"][data-free-text="true"]:checked').length > 0;

        if (isChecked) {
            $freeText.show();
        }
        else {
            $freeText.hide();
            $freeText.find('input').val('');
        }
    }

    // 選択上限のある設問で、残り選択数を表示し、上限に達したら他を選べなくする
    // ※エラーを出す前に気づけるようにするため
    function refreshSelectCount($question) {
        var max = parseInt($question.data('max-select'), 10);

        if (isNaN(max)) {
            return;
        }

        var $choices = $question.find('.js-survey-choice');
        var selected = $choices.filter(':checked').length;

        $choices.not(':checked').prop('disabled', selected >= max);

        var $count = $question.find('.js-survey-count');

        if ($count.length === 0) {
            $count = $('<p class="js-survey-count text-muted small mb-1"></p>');
            $question.find('.form-check').first().before($count);
        }

        if (selected >= max) {
            $count.text(max + 'つ選択しました。変更する場合は、いずれかのチェックを外してください。');
        }
        else {
            $count.text('あと' + (max - selected) + 'つ選べます。');
        }
    }

    $('.js-survey-choice').on('change', function () {
        var $this = $(this);

        toggleFreeText($this.data('question-index'));
        refreshSelectCount($this.closest('.js-survey-question'));
    });

    // 初期表示・入力エラーでの再表示時の状態を反映する
    $('.js-survey-choice[data-free-text="true"]').each(function () {
        toggleFreeText($(this).data('question-index'));
    });

    $('.js-survey-question').each(function () {
        refreshSelectCount($(this));
    });

    // 入力エラーがある場合は、該当箇所までスクロールする
    var $error = $('.text-danger:not(:empty)').filter(function () {
        return $.trim($(this).text()) !== '';
    }).first();

    if ($error.length > 0) {
        $error[0].scrollIntoView({ block: 'center' });
    }
});
