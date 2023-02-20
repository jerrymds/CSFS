//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var caseReturn = $.CaseReturn = {};
    jQuery.extend(caseReturn, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "CaseReturnQuery") {
                $.CaseReturn.initCaseReturnQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initCaseReturnQuery: function () {
            $.CSFS.bindGovKindAndUnit("ddlGOV_KIND", "txtGovUnit");
            $("#ddlCaseKind").change(function () { changeCaseKind(); });
            $("#btnClosed").click(function () { return btnClosedClick(); });
            $("#btnQuery").click(function () { return btnQueryClick(); });
            $("#btnReturnSubmit").click(function () { return btnReturnSubmitClick(); });

            //*點選查詢
            function btnQueryClick() {
                var txtStartDate = $("#txtGovDate").val(); //*起始日
                var txtEndDate = $("#txtLimitDate").val(); //*截止日

                var newLine = "<br/>";
                var msg = "";
                if (!checkIsValidDate($("#txtGovDate").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#txtGovDate").val()) + newLine;
                }

                if (!checkIsValidDate($("#txtLimitDate").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#txtLimitDate").val()) + newLine;
                }

                if (dateCompare(txtStartDate, txtEndDate) === 1) {
                    msg = msg + $("#NameDate").val() + newLine;
                }
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                } else {
                    $.blockUI();
                    $("#divResult").html("");
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
                        }
                    });
                    return true;
                }
            }
            //*點選結案
            function btnClosedClick() {
                var a = 0;
                var strReturnReason = new Array();
                var strReturnAnswer = new Array();
                $("input[name='r2']").each(function () {
                    if ($(this).prop("checked")) {
                        a++;
                        strReturnReason.push($(this).attr("data-returnreason"));
                        strReturnAnswer.push($(this).attr("data-returnanswer"));
                    }
                });
                if (a === 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                if (a > 1) {
                    jAlertError($("#SelectOnlyOneMsg").val());
                    return;
                }
                $("#txtReturnReason").val(strReturnReason);
                $("#txtReturnAnswer").val(strReturnAnswer);
                $("#modalClose").modal();
              
            }

            function btnReturnSubmitClick() {
                var a = 0;
                $("input[name='r2']").each(function () {
                    if ($(this).prop("checked")) {
                        a++;
                    }
                });
                if (a === 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                if (a > 1) {
                    jAlertError($("#SelectOnlyOneMsg").val());
                    return;
                }

                if (!ajaxValidateOver()) {
                    return false;
                }

                var caseId = "";
                $("input[name='r2']").each(function () {
                    if ($(this).prop("checked")) {
                        caseId = $(this).val();
                    }
                });
                jConfirm($("#ReturnCloseConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        $.ajax({
                            url: $("#CloseCaseUrl").val(),
                            type: "Post",
                            cache: false,
                            data: { CaseId: caseId, ClosedReson: $("#txtReturnReason").val(), ReturnAnswer: $("#txtReturnAnswer").val() },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data === "1") {
                                    jAlertSuccess($("#ReturnCloseSuccessMsg").val(), function () {
                                        $("#modalClose").modal("hide");
                                        btnQueryClick();
                                        $("input[name='r2']").each(function () {
                                            $(this).prop("checked", false);
                                        });
                                        $.unblockUI();
                                    });
                                } else {
                                    jAlertError($("#ReturnCloseFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }

            function ajaxValidateOver() {
                if ($.trim($("#txtReturnReason").val()) === "") {
                    jAlertError($("#TextNotNull").val());
                    return false;
                }
                return true;
            }

            //* 類別聯動
            function changeCaseKind() {
                try {
                    var selectedValue = $("#ddlCaseKind option:selected").val();
                    if (selectedValue === "") {
                        $("#ddlGovUnit").attr("disabled", "true");
                        $("#ddlCaseKind2").empty();
                        $("#ddlCaseKind2").append($("<option></option>").val("").text($("#PlaseSelect").val()));
                    } else {
                        if ($.trim(selectedValue).length > 0) {
                            $.ajax({
                                type: "POST",
                                async: false,
                                url: $("#GetCaseKind2Url").val(),
                                data: { caseKind: selectedValue },
                                success: function (data) {
                                    if (data.length > 0) {
                                        $("#ddlCaseKind2").removeAttr("disabled");
                                        $("#ddlCaseKind2").empty();
                                        $("#ddlCaseKind2").append($("<option></option>").val("").text($("#PlaseSelect").val()));
                                        $.each(data, function (i, item) {
                                            $("#ddlCaseKind2").append($("<option></option>").val(item.Value).text(item.Value));
                                        });
                                    } else {
                                        $("#ddlCaseKind2").attr("disabled", "true");
                                        $("#ddlCaseKind2").empty();
                                        $("#ddlCaseKind2").append($("<option></option>").val("").text($("#PlaseSelect").val()));
                                    }
                                }
                            });
                        }
                    }
                } catch (e) {
                }
            }

            function dateCompare(date1, date2) {
                date1 = date1.replace(/\-/gi, "/");
                date2 = date2.replace(/\-/gi, "/");
                var time1 = new Date(date1).getTime();
                var time2 = new Date(date2).getTime();
                if (time1 > time2) {
                    return 1;
                } else {
                    return 0;
                }
            }

        }

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.CaseReturn.documentReady();
    });
})(jQuery);
