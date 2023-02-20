//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var menuMaintenance = $.MenuMaintenance = {};
    jQuery.extend(menuMaintenance, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "MenuQuery") {
                $.MenuMaintenance.initMenuQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initMenuQuery: function () {
            $("#frmQuery").submit(function () { return btnQueryClick(); });
            $("#btnQuery").click(function () { return btnQueryClick(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });
            $("#btnSign").click(function () { return btnSignClick(); });
            $("#btnReturn").click(function () { return btnReturnClick(); });
            
            //* 沒有新增.因為新增是一個超鏈接直接跳過去

            //* 點選查詢
            function btnQueryClick() {
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
                        //$.custPagination.BindCheckBox();
                        //$.custPagination.sort($("#divResult"));
                    }
                });
                return false;
            }
            //* 點選取消
            function btnCancelClick() {
                $("#divResult").html("");
                $("#GovUnit").val("");
                $("#GovNo").val("");
                $("#Person").val("");
            }
            //* 點選簽收
            function btnSignClick() {
                var iLen = 0;
                var strKey = "";
                $(".checkfile").each(function () {
                    if ($(this).prop("checked") == true) {
                        iLen = iLen + 1;
                        strKey = strKey + $(this).val() + ",";
                    }
                });
                if (iLen <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                jConfirm($("#SignConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        //* click confirm ok
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#SignUrl").val(),
                            async: false,
                            data: { strIds: strKey },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    jAlertSuccess($("#SignSuccessMsg").val(), function () { btnQueryClick(); });
                                } else {
                                    jAlertError($("#SignFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }
            //* 點選退回
            function btnReturnClick() {
                var iLen = 0;
                var strKey = "";
                $(".checkfile").each(function () {
                    if ($(this).prop("checked") == true) {
                        iLen = iLen + 1;
                        strKey = strKey + $(this).val() + ",";
                    }
                });
                if (iLen <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }

                jConfirm($("#ReturnConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        //* click confirm ok
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#ReturnUrl").val(),
                            async: false,
                            data: { strIds: strKey },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    jAlertSuccess($("#ReturnSuccessMsg").val(), function () { btnQueryClick(); });
                                } else {
                                    jAlertError($("#ReturnFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }
        },
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.MenuMaintenance.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}