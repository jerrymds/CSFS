//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var directorToApprove = $.DirectorToApprove = {};
    jQuery.extend(directorToApprove, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "DirectorToApproveQuery") {
                $.DirectorToApprove.initQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initQuery: function () {
            $.CSFS.bindGovKindAndUnit1("txtGovUnit");
            $("#ddlCaseKind2").attr("disabled", "true");
            $("#btnQuery").click(function () { return btnQueryclick(); });
            $("#ddlCaseKind").change(function () { changeCaseKind(); });
            $("#btnChengHe").click(function () { return btnChengHeClick() });
            $("#btnApprove").click(function () { return btnApproveSubmit() });
            //Add by zhangwei 20180315 start
            $("#btnBatchApprove").click(function () { return btnBatchApprove() });
            $("#btnRemit").click(function () { return btnRemit() });
            //$("#ddlCaseKind2").change(function () { changeCaseKind2(); });
            $("#btnBatchApprove").attr("disabled", "true");//整批放行按鈕不可用
            $("#btnRemit").attr("disabled", "true");//匯出按鈕不可用
            //Add by zhangwei 20180315 end
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
            //* 收發待辦
            $("#AssignAgents").click(function () { return JZChangeVlue(); });
            function JZChangeVlue() {
                //$("#AssAgents").val("1");
                btnAssignShowClick();
            }
            //* 儲存逾期原因
            $("#btnOverSubmit").click(function () { return btnOverSubmit(); });
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
            $("#btnReturn").click(function () { return btnReturnClick() });   //* 退件

            //$("#btnReturnClose").click(function () { return btnReturnCloseClick(); });
            //* 退件提交
            $("#btnCloseSubmit").click(function () { return btnCloseSubmit(); });

            //* 點選列印
            $("#btnReportSelected").click(function () { return btnReportSelected(); });

            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
            if ($("#IsBranchDirector").val() == "0") {
                $("#ddlAgentDepartment2").prop("disabled", "disabled");
                $("#ddlAgentDepartmentUser").prop("disabled", "disabled");
            }
            else {
                $("#ddlAgentDepartmentUser").prop("disabled", "disabled");
            }
            //* 經辦人員
            $("#ddlAgentDepartment").change(function () { changeAgentDepartment(); });
            $("#ddlAgentDepartment2").change(function () { changeAgentDepartment2(); });
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end

            if ($("#isQuery").val() == "1") {
                changeCaseKind();
                $("#pageNum").val(parseInt($("#CurrentPage").val()));
                $("#ddlCaseKind2").val($("#CaseKind2Query").val());
                //Add by zhangwei 20180315 start
                if ($("#CaseKind2Query").val().trim() == "扣押" || $("#CaseKind2Query").val().trim() == "撤銷" || $("#CaseKind2Query").val().trim() == "支付")
                {
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
                    if (selectedValue != "扣押" && selectedValue != "撤銷"  && selectedValue != "支付" && selectedValue !="扣押並支付") {
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
            //Add by zhangwei 20180315 start
            //細項改變事件
            function changeCaseKind2()
            {
                var selectedValue = $("#ddlCaseKind2 option:selected").val();
                if (selectedValue != "扣押" && selectedValue != "撤銷" && selectedValue != "支付") {
                    $("#btnRemit").attr("disabled", "true");//匯出按鈕不可用
                    $("#btnBatchApprove").attr("disabled", "true");//整批放行按鈕不可用
                }
                else {
                    $("#btnRemit").removeAttr("disabled");//匯出按鈕可用
                    $("#btnBatchApprove").removeAttr("disabled");//整批放行按鈕可用
                }
            }
            //Add by zhangwei 20180315 end
            //* 點選呈核
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
            //* 點選放行
            // 2022-07-27 增加 data-bind="@item.AgentUser" data-content="@item.CaseKind" data-container="@item.CaseNo"
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
                    strAgentUser = $(".checkfile:checked").attr("data-bind").substr(0,4);
                    if (strCaseKind == "外來文案件" && strAgentUser =="CRPA") {
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
                    jAlertError(errormsg+ " CRPA 請改派!!");
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
                                    jAlertError(data.ReturnMsg);
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }
            //Add by zhangwei 20180315 start
            //整批放行
            function btnBatchApprove()
            {
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
            function btnRemit()
            {
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
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
            //* 點選收發待辦
            function btnAssignShowClick() {
                var iLen = 0;
                var strKey = "";
                $(".checkfile").each(function () {
                    if ($(this).prop("checked") === true) {
                        iLen = iLen + 1;
                        strKey = strKey + $(this).val() + ",";
                    }
                });
                if (iLen <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                var arylimitDate = new Array();
                var arylimit = new Array();
                var strOverDueMemo = new Array();
                $(".checkfile:checked").each(function () {
                    arylimitDate.push($(this).attr("data-limitDate").replace("/", "").replace("/", ""));//向数组中添加元素
                    strOverDueMemo.push($(this).attr("data-overduememo"));
                });
                var strlimitDate = arylimitDate.sort().join(',');
                var strNowDate = GetDateStr(parseInt($("#AddDay").val()));

                if (strlimitDate < strNowDate) {
                    if (iLen <= 0) {
                        jAlertError($("#SelectOneMsg").val());
                        return;
                    }

                    if (iLen >= 2) {
                        jAlertError($("#NuclearOneMsg").val());
                        return;
                    }
                    $("#titleForAgent").text($("#NuclearTitle1").val());
                    $("#OverDueMemo").val(strOverDueMemo);
                    $("#modalOver").modal();
                }
                else {
                    var CaseIDArr = new Array();
                    $(".checkfile:checked").each(function () {
                        CaseIDArr.push($(this).val());//向数组中添加元素
                    });
                    var strCaseId = CaseIDArr.join(',');

                    if (strCaseId.length <= 0) {
                        jAlertError($("#SelectOneMsg").val());
                        return;
                    }
                    jConfirm($("#AssAgentsConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                        if (bFlag === true) {
                            $.blockUI();
                            //* click confirm ok
                            $.ajax({
                                type: "POST",
                                traditional: true,
                                url: $("#AgentsUrl").val(),
                                async: false,
                                data: { caseIdList: strCaseId },
                                error: function () {
                                    jAlertError($("#LoadErrorMsg").val());
                                    $.unblockUI();
                                },
                                success: function (data) {
                                    if (data.ReturnCode === "1") {
                                        jAlertSuccess($("#AssAgentsSuccessMsg").val(), function () {
                                            btnQueryclick();
                                        });
                                    } else {
                                        jAlertError($("#AssAgentsFailMsg").val());
                                        $.unblockUI();
                                    }
                                }
                            });
                        }
                    });
                }
            }

            function GetDateStr(AddDayCount) {
                var dd = new Date();
                dd.setDate(dd.getDate() + AddDayCount);//获取AddDayCount天后的日期
                var y = dd.getFullYear();
                var m = dd.getMonth() + 1;//获取当前月份的日期
                var d = dd.getDate();
                return y + "" + (m < 10 ? "0" + m : m) + "" + (d < 10 ? "0" + d : d);
            }

            //*儲存逾期原因
            function btnOverSubmit() {
                var aryCaseId = new Array();
                $(".checkfile:checked").each(function () {
                    aryCaseId.push($(this).val());//向数组中添加元素  
                });
                var strCaseId = aryCaseId.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                if (!ajaxValidateOver()) {
                    return false;
                }
                jConfirm($("#AssAgentsConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        //* click confirm ok
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#AgentsUrl").val(),
                            async: false,
                            data: { caseIdList: strCaseId, OverDueMemo: $("#OverDueMemo").val() },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    jAlertSuccess($("#AssAgentsSuccessMsg").val(), function () {
                                        $("#modalOver").modal("hide");
                                        $("#OverDueMemo").val("");
                                        btnQueryclick();
                                    });
                                } else {
                                    jAlertError($("#AssignFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }

            function ajaxValidateOver() {
                if ($.trim($("#OverDueMemo").val()) === "") {
                    jAlertError($("#TextNotNull").val());
                    return false;
                }
                return true;
            }
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
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

                $("#modalClose").modal();
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
                    data: { strIds: strCaseId, statusArr: strStatus, CloseReason: $("#CloseReason").val() },
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

            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
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
                    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update start
                    //if ($("#IsBranchDirector").val() == "0") {
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
                    //}
                    //else {
                    //    var selectedValue = $("#ddlAgentDepartment2 option:selected").val();
                    //    if (selectedValue === "") {
                    //        $("#ddlAgentDepartmentUser").attr("disabled", "true");
                    //        $("#ddlAgentDepartmentUser").val("");
                    //    } else {
                    //        if ($.trim(selectedValue).length > 0) {
                    //            $("#ddlAgentDepartmentUser").removeAttr("disabled");
                    //        }
                    //    }
                    //}
                    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end
                }
                catch (e) {
                }
            }
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.DirectorToApprove.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}