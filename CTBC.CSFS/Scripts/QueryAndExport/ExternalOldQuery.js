//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var externalOldQuery = $.ExternalOldQuery = {};
    jQuery.extend(externalOldQuery, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "ExternalOldQuery") {
                $.ExternalOldQuery.initExternalOldQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initExternalOldQuery: function () {
            $("#frmQuery").submit(function () { return QueryData(); });
            $("#btnQuery").click(function () { return QueryData(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });

            if ($("#isQuery").val() == "1") {
                $("#pageNum").val(parseInt($("#CurrentPage").val()));
                QueryData();
            }

            //* 點選取消
            function btnCancelClick() {
                $("#divResult").html("");
            }

            function QueryData() {
                trimAllInput();
                if (!ajaxValidateQuery()) {
                    return false;
                } else {
                    $.blockUI();
                    $.ajax({
                        url: $("#frmQuery").attr("action"),
                        type: "Post",
                        cache: false,
                        data: $("#frmQuery").serialize(),
                        error: function () {
                            jAlertError($("#LoadErrorMsg").val());
                            $.unblockUI();
                        },
                        success: function (data) {
                            $("#divResult").html(data).show();
                            $("#querystring").val($("#frmQuery").serialize());
                            $.unblockUI();
                        }
                    });
                }
                return false;
            }

            function ajaxValidateQuery() {
                var NewLine = "<br/>";
                var Msg = "";
                var Filter = /^(0|[1-9]\d*)$/;
                if ($("#BranchCodeS").val() != "" && Filter.test($("#BranchCodeS").val()) == false) {
                    //Msg = Msg + $.validator.format($("#PositiveintFormat").val(), $("#BranchText").val()) + NewLine;;
                }
                if ($("#BranchCodeE").val() != "" && Filter.test($("#BranchCodeE").val()) == false) {
                    //Msg = Msg + $.validator.format($("#PositiveintFormat").val(), $("#BranchText").val()) + NewLine;;
                }
                if (!checkIsValidDate($("#ReceiveDateS").val())) {
                    Msg = Msg + $.validator.format($("#PlzCorrectFormat").val(), $("#ReceivedDateText").val()) + NewLine;
                }
                if (!checkIsValidDate($("#ReceiveDateE").val())) {
                    Msg = Msg + $.validator.format($("#PlzCorrectFormat").val(), $("#ReceivedDateText").val()) + NewLine;
                }
                if (!checkIsValidDate($("#CloseDateS").val())) {
                    Msg = Msg + $.validator.format($("#PlzCorrectFormat").val(), $("#CreatedDateText").val()) + NewLine;
                }
                if (!checkIsValidDate($("#CloseDateE").val())) {
                    Msg = Msg + $.validator.format($("#PlzCorrectFormat").val(), $("#CreatedDateText").val()) + NewLine;
                }
                if (Msg.length > 0) {
                    jAlertError(Msg);
                    return false;
                }
                return true;
            }
        },

        QueryData: function () {
            var msg = "";
            var newLine = "<br/>";

            //* 有必填檢核錯誤
            if (msg.length > 0) {
                jAlertError(msg);
                return false;
            }

            $("#divResult").html("");
            trimAllInput();

            $.blockUI();
            $.ajax({
                url: $("#frmQuery").attr("action"),
                type: "Post",
                cache: false,
                data: $("#frmQuery").serialize(),
                error: function () {
                    jAlertError($("#LoadErrorMsg").val());
                    $.unblockUI();
                },
                success: function (data) {
                    $("#divResult").html(data).show();
                    $.unblockUI();
                    $("#querystring").val($("#frmQuery").serialize());
                }
            });
            return false;
        },

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.ExternalOldQuery.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}