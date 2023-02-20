//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var directorCooperative = $.DirectorCooperative = {};
    jQuery.extend(directorCooperative, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "DirectorCooperativeQuery") {
                $.DirectorCooperative.initQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initQuery: function () {
            $("#ddlCaseKind2").attr("disabled", "true");
            $.CSFS.bindGovKindAndUnit1("txtGovUnit");
            $("#btnQuery").click(function () { return btnQueryclick(); });
            $("#ddlCaseKind").change(function () { changeCaseKind(); });
            $("#btnChengHe").click(function () { return btnChengHeClick() });
            $("#btnApprove").click(function () { return btnApproveSubmit() });
            $("#btnReturn").click(function () { return btnReturnClick() });
            $("#btnCloseSubmit").click(function () { return btnCloseSubmit(); });
            //* 點選列印
            $("#btnReportSelected").click(function () { return btnReportSelected(); });
            //Add by zhangwei 20180315 start
            $("#btnBatchApprove").click(function () { return btnBatchApprove() });
            $("#btnRemit").click(function () { return btnRemit() });
            //$("#ddlCaseKind2").change(function () { changeCaseKind2(); });
            $("#btnBatchApprove").attr("disabled", "true");//整批放行按鈕不可用
            $("#btnRemit").attr("disabled", "true");//匯出按鈕不可用
            //Add by zhangwei 20180315 end
            //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add start
            $("#ddlAgentDepartment2").prop("disabled", "disabled");
            $("#ddlAgentDepartmentUser").prop("disabled", "disabled");
            //* 經辦人員
            $("#ddlAgentDepartment").change(function () { changeAgentDepartment(); });
            $("#ddlAgentDepartment2").change(function () { changeAgentDepartment2(); });
            //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add end

            if ($("#isQuery").val() == "1") {
                changeCaseKind();
                $("#pageNum").val(parseInt($("#CurrentPage").val()));
                $("#ddlCaseKind2").val($("#CaseKind2Query").val());
                //Add by zhangwei 20180315 start
                if ($("#CaseKind2Query").val().trim() == "扣押" || $("#CaseKind2Query").val().trim() == "撤銷") {
                    $("#btnRemit").removeAttr("disabled");//匯出按鈕可用
                    $("#btnBatchApprove").removeAttr("disabled");//整批放行按鈕可用
                }
                //Add by zhangwei 20180315 end
                //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update start
                changeAgentDepartment();
                $("#ddlAgentDepartment2").removeAttr("disabled");
                $("#ddlAgentDepartment2").val($("#AgentDepartment2Query").val());
                changeAgentDepartment2();
                $("#ddlAgentDepartmentUser").removeAttr("disabled");
                $("#ddlAgentDepartmentUser").val($("#AgentDepartmentUserQuery").val());
                //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end
                btnQueryclick();
            }
            //* 查詢
            function btnQueryclick() {
                trimAllInput();
                if (!ajaxValidateQuery()) {
                    return false;
                } else {
                    var selectedValue = $("#ddlCaseKind2 option:selected").val();
                    if (selectedValue != "扣押" && selectedValue != "撤銷" && selectedValue != "扣押並支付") {
                        $("#btnRemit").attr("disabled", "true");//匯出按鈕不可用
                        $("#btnBatchApprove").attr("disabled", "true");//整批放行按鈕不可用
                    }
                    else {
                        $("#btnRemit").removeAttr("disabled");//匯出按鈕可用
                        $("#btnBatchApprove").removeAttr("disabled");//整批放行按鈕可用
                    }
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
                }
                return false;
            }
            //* 驗證
            function ajaxValidateQuery() {
                var newLine = "<br/>";
                var msg = "";
                if (!checkIsValidDate($("#GovDateS").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#GovDateText").val()) + newLine;
                }
                if (!checkIsValidDate($("#GovDateE").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#GovDateText").val()) + newLine;
                }
                if (!checkIsValidDate($("#CreatedDateS").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#CreatedDateText").val()) + newLine;
                }
                if (!checkIsValidDate($("#CreatedDateE").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#CreatedDateText").val()) + newLine;
                }
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }
                return true;
            }
            //* 案件類型
            function changeCaseKind() {
                try {
                    $("#btnRemit").attr("disabled", "true");//匯出按鈕不可用
                    $("#btnBatchApprove").attr("disabled", "true");//整批放行按鈕不可用
                    var selectedValue = $("#ddlCaseKind option:selected").val();
                    if (selectedValue === "") {
                        $("#ddlCaseKind2").attr("disabled", "true");
                        $("#ddlCaseKind2").empty();
                        $("#ddlCaseKind2").append($("<option></option>").val("").text($("#PleaseSelect").val()));
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
                                        $("#ddlCaseKind2").append($("<option></option>").val('').text($("#PleaseSelect").val()));
                                        $.each(data, function (i, item) {
                                            $("#ddlCaseKind2").append($("<option></option>").val(item.Value).text(item.Value));
                                        });
                                    } else {
                                        $("#ddlCaseKind2").attr("disabled", "true");
                                        $("#ddlCaseKind2").empty();
                                        $("#ddlCaseKind2").append($("<option></option>").val('').text($("#PleaseSelect").val()));
                                    }
                                }
                            });
                        }
                    }
                }
                catch (e) {
                }
            }
            //* 點擊呈核
            function btnChengHeClick() {
                var caseIdArr = new Array();
                var statusArr = new Array();
                $(".checkfile:checked").each(function () {
                    caseIdArr.push($(this).val());//向数组中添加元素
                    statusArr.push($(".checkfile:checked").attr("data-status"));
                });
                var strCaseId = caseIdArr.join(',');
                var strStatus = statusArr.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                jConfirm($("#ChengHeConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#ChengHeUrl").val(),
                            async: false,
                            data: { CaseIdarr: strCaseId, statusArr: strStatus },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    //jAlertSuccess($("#ChengHeOkMsg").val(), function () {
                                    $.unblockUI();
                                    btnQueryclick();
                                    //});
                                } else if (data.ReturnCode === "2") {
                                    jAlertError($("#ChengHeConfirmMsgMax").val());
                                    $.unblockUI();
                                } else {
                                    //jAlertError($("#ChengHeFaileMsg").val());
                                    jAlertError(data.ReturnMsg);
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }
            //* 放行
            function btnApproveSubmit() {
                var caseIdArr = new Array();
                var statusArr = new Array();
                var agentArr = new Array();
                var casekindArr = new Array();
                var casenoArr = new Array();
                var strCaseKind = "";
                var strAgentUser = "";
                var errormsg = "";
                $(".checkfile:checked").each(function () {
                    caseIdArr.push($(this).val());//向数组中添加元素
                    statusArr.push($(".checkfile:checked").attr("data-status"));
                    strCaseKind = $(".checkfile:checked").attr("data-content");
                    strAgentUser = $(".checkfile:checked").attr("data-bind").substr(0, 4);
                    if (strCaseKind == "外來文案件" && strAgentUser == "CRPA") {
                        casenoArr.push($(".checkfile:checked").attr("data-container"));
                        errormsg = casenoArr.join(',');
                    }
                });
                var strCaseId = caseIdArr.join(',');
                var strStatus = statusArr.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                if (errormsg.length > 0) {
                    jAlertError(errormsg + " CRPA 請改派!!");
                    return;
                }
                jConfirm($("#ApprovefirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#ApproveUrl").val(),
                            async: false,
                            data: { CaseIdarr: strCaseId, statusArr: strStatus },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    //jAlertSuccess($("#ApproveOkMsg").val(), function () {
                                    $.unblockUI();
                                    btnQueryclick();
                                    //});
                                } else {
                                    //jAlertError($("#ApproveFaileMsg").val());
                                    jAlertError(data.ReturnMsg);
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }
            //Add by zhangwei 20180315 start
            //細項改變事件
            function changeCaseKind2() {
                var selectedValue = $("#ddlCaseKind2 option:selected").val();
                if (selectedValue != "扣押" && selectedValue != "撤銷") {
                    $("#btnRemit").attr("disabled", "true");//匯出按鈕不可用
                    $("#btnBatchApprove").attr("disabled", "true");//整批放行按鈕不可用
                }
                else {
                    $("#btnRemit").removeAttr("disabled");//匯出按鈕可用
                    $("#btnBatchApprove").removeAttr("disabled");//整批放行按鈕可用
                }
            }
            //整批放行
            function btnBatchApprove() {
                var iLen = 0;
                $(".checkfile:checked").each(function () {
                    iLen = iLen + 1;
                });
                if (iLen > 0) {
                    jAlertError("整批放行不允許勾選案件!");//IR-0089
                    return;
                }
                var caseIdArr = new Array();
                var statusArr = new Array();
                $(".checkfile").each(function () {
                    caseIdArr.push($(this).val());//向数组中添加元素
                    statusArr.push($(".checkfile").attr("data-status"));
                });

                var strCaseId = caseIdArr.join(',');
                var strStatus = statusArr.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError("無數據！");
                    return;
                }
                jConfirm("是否整批放行", $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#BatchApproveUrl").val(),
                            async: false,
                            data: { CaseIdarr: strCaseId, statusArr: strStatus },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    //jAlertSuccess($("#ApproveOkMsg").val(), function () {
                                    $.unblockUI();
                                    btnQueryclick();
                                    //});
                                } else {
                                    jAlertError(data.ReturnMsg);
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }
            function btnRemit() {
                var caseIdArr = new Array();
                $(".checkfile").each(function () {
                    caseIdArr.push($(this).val());//向数组中添加元素
                });

                var strCaseId = caseIdArr.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError("無數據！");
                    return;
                }
                var selectedValue = $("#ddlCaseKind2 option:selected").val();
                var actionUrl = $("#RemitUrl").val();
                $("#frmForReport").attr("src", actionUrl);
            }
            //Add by zhangwei 20180315 end
            //* 點選退件
            function btnReturnClick() {
                var caseIdArr = new Array();
                $(".checkfile:checked").each(function () {
                    caseIdArr.push($(this).val());//向数组中添加元素
                });
                var strCaseId = caseIdArr.join(',');

                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                $("#CloseReason").val("");
                $("#modalClose").modal();
                //jConfirm($("#ReturnfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                //    if (bFlag === true) {
                //        $.blockUI();
                //        $.ajax({
                //            type: "POST",
                //            traditional: true,
                //            url: $("#ReturnUrl").val(),
                //            async: false,
                //            data: { caseIdList: strCaseId },
                //            error: function () {
                //                jAlertError($("#LoadErrorMsg").val());
                //                $.unblockUI();
                //            },
                //            success: function (data) {
                //                if (data.ReturnCode === "1") {
                //                    jAlertSuccess($("#ReturnOkMsg").val(), function () {
                //                        $.unblockUI();
                //                        btnQueryclick();
                //                    });
                //                } else {
                //                    jAlertError($("#ReturnFaileMsg").val());
                //                    $.unblockUI();
                //                }
                //            }
                //        });
                //    }
                //});
            }

            function btnCloseSubmit() {
                var aryCaseId = new Array();
                var statusArr = new Array();
                $(".checkfile:checked").each(function () {
                    aryCaseId.push($(this).val());//向数组中添加元素  
                    statusArr.push($(".checkfile:checked").attr("data-status"));
                });
                var strCaseId = aryCaseId.join(',');
                var strStatus = statusArr.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                if (!ajaxValidate()) {
                    return false;
                }

                $.blockUI();
                $.ajax({
                    type: "POST",
                    traditional: true,
                    url: $("#ReturnUrl").val(),
                    async: false,
                    data: { caseIdList: strCaseId, statusArr: strStatus, CloseReason: $("#CloseReason").val() },
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            $("#modalClose").modal("hide");
                            btnQueryclick();
                        } else {
                            //jAlertError($("#ReturnFaileMsg").val());
                            jAlertError(data.ReturnMsg);
                            $.unblockUI();
                        }
                    }
                });
            }

            function ajaxValidate() {
                if ($("#CloseReason").val() <= 0) {
                    jAlertError($("#TextNotNull").val());
                    return false;
                }
                return true;
            }

            //* 列印
            function btnReportSelected() {
                var aryCaseId = new Array();
                $(".checkfile:checked").each(function () {
                    aryCaseId.push($(this).val());//向数组中添加元素  
                });
                var strCaseId = aryCaseId.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return false;
                }

                if (aryCaseId.length > 30) {
                    jAlertError($("#CheckCount").val());
                    return false;
                }

                var actionUrl = $("#ReportUrl").val() + "?caseIdList=" + strCaseId + "&Con=Director";
                $("#frmForReport").attr("src", actionUrl);
                return true;
            }

            //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add start
            //* 經辦人員下拉(處)    
            function changeAgentDepartment() {
                try {
                    $("#ddlAgentDepartmentUser").attr("disabled", "true");
                    $("#ddlAgentDepartmentUser").empty();
                    $("#ddlAgentDepartmentUser").append($("<option></option>").val('').text($("#PleaseSelect").val()));
                    var selectedValue = $("#ddlAgentDepartment option:selected").val();
                    if (selectedValue === "") {
                        $("#ddlAgentDepartment2").attr("disabled", "true");
                        $("#ddlAgentDepartment2").empty();
                        $("#ddlAgentDepartment2").append($("<option></option>").val("").text($("#PleaseSelect").val()));
                    } else {
                        if ($.trim(selectedValue).length > 0) {
                            $.ajax({
                                type: "POST",
                                async: false,
                                url: $("#GetAgentDepartment2Url").val(),
                                data: { AgentDepartment: selectedValue },
                                success: function (data) {
                                    if (data.length > 0) {
                                        $("#ddlAgentDepartment2").removeAttr("disabled");
                                        $("#ddlAgentDepartment2").empty();
                                        $("#ddlAgentDepartment2").append($("<option></option>").val('').text($("#PleaseSelect").val()));
                                        $.each(data, function (i, item) {
                                            $("#ddlAgentDepartment2").append($("<option></option>").val(item.Value).text(item.Value));
                                        });
                                    } else {
                                        $("#ddlAgentDepartment2").attr("disabled", "true");
                                        $("#ddlAgentDepartment2").empty();
                                        $("#ddlAgentDepartment2").append($("<option></option>").val('').text($("#PleaseSelect").val()));
                                    }
                                }
                            });
                        }
                    }
                }
                catch (e) {
                }
            }
            //* 經辦人員下拉(科組)
            function changeAgentDepartment2() {
                try {
                    var selectedValue = $("#ddlAgentDepartment2 option:selected").val();
                    if (selectedValue === "") {
                        $("#ddlAgentDepartmentUser").attr("disabled", "true");
                        $("#ddlAgentDepartmentUser").empty();
                        $("#ddlAgentDepartmentUser").append($("<option></option>").val("").text($("#PleaseSelect").val()));
                    } else {
                        if ($.trim(selectedValue).length > 0) {
                            $.ajax({
                                type: "POST",
                                async: false,
                                url: $("#GetAgentDepartmentUserUrl").val(),
                                data: { AgentDepartment: selectedValue },
                                success: function (data) {
                                    if (data.length > 0) {
                                        $("#ddlAgentDepartmentUser").removeAttr("disabled");
                                        $("#ddlAgentDepartmentUser").empty();
                                        $("#ddlAgentDepartmentUser").append($("<option></option>").val('').text($("#PleaseSelect").val()));
                                        $.each(data, function (i, item) {
                                            $("#ddlAgentDepartmentUser").append($("<option></option>").val(item.Value).text(item.Value));
                                        });
                                    } else {
                                        $("#ddlAgentDepartmentUser").attr("disabled", "true");
                                        $("#ddlAgentDepartmentUser").empty();
                                        $("#ddlAgentDepartmentUser").append($("<option></option>").val('').text($("#PleaseSelect").val()));
                                    }
                                }
                            });
                        }
                    }
                }
                catch (e) {
                }
            }
            //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add end
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.DirectorCooperative.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}