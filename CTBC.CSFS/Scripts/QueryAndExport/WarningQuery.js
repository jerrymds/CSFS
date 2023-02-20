//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var warningQuery = $.WarningQuery = {};
    jQuery.extend(warningQuery, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "WarningQuery") {
                $.WarningQuery.initWarningQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initWarningQuery: function () {
            $("#btnQuery").click(function () { return $.WarningQuery.QueryData(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });
            $("#btnExcel").click(function () { return ExcelClick();})          

            function ExcelClick() {
                //location.href = $("#ExcelUrl").val() + "?CustId=" + $("#txtCustId").val() + "&CustAccount=" + $("#txtCustAccount").val() + "&DocNo=" + $("#txtDocNo").val()
                //+ "&VictimName=" + $("#txtVictimName").val() + "&ForCDateS=" + $("#txtForCDateS").val() + "&ForCDateE=" + $("#txtForCDateE").val();
                var actionUrl = $("#ExcelUrl").val();
                $("#frmForReport").attr("src", actionUrl);
            }

            //* 點選取消
            function btnCancelClick() {
                $("#divResult").html("");
            }
        },

        QueryData: function () {
            var msg = "";
            var newLine = "<br/>";
            if ($("#txtCustId").val().length <= 0 && $("#txtCustAccount").val().length <= 0 && $("#txtNo_165").val().length <= 0  && $("#txtKind").val().length <= 0 && $("#txtDocNo").val().length <= 0  && $("#txtVictimName").val().length <= 0 && $("#txtForCDateS").val().length <= 0 && $("#txtForCDateE").val().length <= 0 && $("#txtType").val().length <= 0 && $("#txtRelieveDateS").val().length <= 0 && $("#txtRelieveDateE").val().length <= 0 && $("#txtOriginal").val().length <= 0 && $("#txtModifyDateS").val().length <= 0 && $("#txtModifyDateE").val().length <= 0) {
                msg = msg + $("#NameOneQuery").val() + newLine;
            }

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
                    //$.CSFS.bindFancybox();
                    $("#querystring").val($("#frmQuery").serialize());
                }
            });
            return false;
        },

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.WarningQuery.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}