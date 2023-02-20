//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var collectionToAssign = $.AgentToHandle = {};
    jQuery.extend(collectionToAssign, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "AgentToHandleQuery") {
                $.AgentToHandle.initQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initQuery: function () {
            //* 查詢
            $("#ddlCaseKind2").prop("disabled", "disabled"),
            $.CSFS.bindGovKindAndUnit1("txtGovUnit");
            $("#frmQuery").submit(function () { return btnQueryClick(); });
            $("#btnQuery").click(function () { return btnQueryClick(); });

            //*返回按鈕
            if ($("#isQuery").val() == "1") {
                $("#ddlCaseKind").val($("#CaseKindQuery").val());
                changeCaseKind();
                if ($("#CaseKindQuery").val() != "") {
                    $("#ddlCaseKind2").prop("disabled", "");
                }
                $("#ddlCaseKind2").val($("#CaseKind2Query").val());
                $("#pageNum").val(parseInt($("#CurrentPage").val()));
                btnQueryClick();
            }

            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
            //*判斷是否為分行經辦角色，隱藏收發代辦及取消再次發文功能
            if ($("#IsBranchAgent").val() == "1") {
                $("#AssignAgents").hide();
                $("#btnCancelSend").hide();
            }
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end

            //* 取消
            $("#btnCancel").click(function () { return btnCancelClick(); });
            //* 點選改派,顯示改派選擇畫面
            $("#btnAssignShow").click(function () { return GPChangeVlue(); });
            //* 改派提交
            $("#btnAssignSubmit").click(function () { return btnAssignSubmit(); });
            //*集作代辦提交
            //$("#btnAgentsSubmit").click(function () { return btnAssignSubmit(); });
            //* 退件
            $("#btnReturnClose").click(function () { return btnReturnCloseClick(); });
            //* 退件提交
            $("#btnCloseSubmit").click(function () { return btnCloseSubmit(); });

            //* 點擊呈核
            //$("#btnNuclear").click(function () { return btnNuclearSubmit() });
            $("#btnNuclear").click(function () { return btnOverClick(); });
            $("#btnOverSubmit").click(function () { return btnOverSubmit(); });
            //* 點選列印
            $("#btnReportSelected").click(function () { return btnReportSelected(); });

            $("#btnCancelSend").click(function () { return btnCancelSendClick(); })

            //二級連動
            $('#ddlGovUnit').attr("disabled", "true");
            //* 案件類型
            $("#ddlCaseKind").change(function () { changeCaseKind(); });
            //* 收發待辦
            $("#AssignAgents").click(function () { return JZChangeVlue(); });
            //* 收發待辦
            function JZChangeVlue() {
                $("#AssAgents").val("1");
                btnAssignShowClick();
            }
            //* 改派ChangeVlue
            function GPChangeVlue() {
                $("#AssAgents").val("0");
                btnAssignShowClick();
            }

            function btnCancelSendClick() {
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

                if (iLen > 1) {
                    jAlertError($("#SelectOneMsgOnly").val());
                    return;
                }

                jConfirm($("#SendAgainConfirmCancelMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();

                        $.ajax({
                            type: "POST",
                            url: $("#CancelSendAgainUrl").val(),
                            async: false,
                            data: { strCaseNos: $(".checkfile:checked").attr("data-caseno") },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").attr());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    jAlertSuccess($("#SendAgainOKMsg").val(), function () { btnQueryClick(); });
                                } else if (data.ReturnCode === "2") {
                                    jAlertError($("#SendAgainMsg").val());
                                    $.unblockUI();
                                } else if (data.ReturnCode === "3") {
                                    jAlertError($("#SendAgainMaxMsg").val());
                                    $.unblockUI();
                                }
                                else {
                                    jAlertError($("#SendAgainFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }

            function changeCaseKind() {
                try {
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
            //ddlAgentsDept
            //* 如果有分派員工列表.填充部門下拉框
            //if (agentList != null && agentList.length > 0) {
            //    $.each(agentList, function (i, item) {
            //        if ($("#ddlAssignDept option[value='" + item.DeptId + "']").length <= 0) {
            //            //* 不存在
            //            $("#ddlAssignDept").append($('<option></option>').val(item.DeptId).text(item.DepName));
            //        }
            //    });
            //    changeAssignBu();
            //}

            //if (AgentsList != null && AgentsList.length > 0) {
            //    $.each(AgentsList, function (i, item) {
            //        if ($("#ddlAgentsDept option[value='" + item.DeptId + "']").length <= 0) {
            //            //* 不存在
            //            $("#ddlAgentsDept").append($('<option></option>').val(item.DeptId).text(item.DepName));
            //        }
            //    });
            //    changeAgentsFn();
            //}


            //* 部门下拉框改變時.改變列表中員工姓名
            //$("#ddlAssignDept").change(function () { changeAssignBu(); });
            $("#Department").change(function () { changeAssignBu(); });
            $("#ddlAgentsDept").change(function () { changeAgentsFn(); });

            //* 點選查詢
            function btnQueryClick() {
                $("#divResult").html("");
                trimAllInput();
                if (!ajaxValidateQuery()) {
                    return false;
                }

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
            }
            //* 點選取消
            function btnCancelClick() {
                $("#divResult").html("");
                $("#GovUnit").val("");
                $("#GovNo").val("");
                $("#Person").val("");
            }
            //* 點選退回
            function btnReturnClick() {
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

            function btnNuclearSubmit() {
                var CaseIDArr = new Array();
                $(".checkfile:checked").each(function () {
                    CaseIDArr.push($(this).val());//向数组中添加元素
                });
                var strCaseId = CaseIDArr.join(',');

                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                jConfirm($("#NuclearConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#AssignChenHe").val(),
                            async: false,
                            data: { CaseIdarr: strCaseId },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    jAlertSuccess($("#NuclearOkMsg").val(), function () {
                                        btnQueryClick();
                                    });
                                } else {
                                    jAlertError($("#NuclearFaileMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }

            //* 分派畫面部門選擇改變
            function changeAssignBu() {
                $("#tbobyAgentList").html("");
                if (agentList.length > 0) {
                    $.each(agentList, function (i, item) {
                        if (item.DeptId === $("#Department").val()) {
                            $("#tbobyAgentList").append('<tr><td class="text-center"><input type="checkbox" value="' + item.EmpId + '" /></td><td>' + item.EmpId + ' - ' + item.EmpName + '</td></tr>');
                        }
                    });
                }
            }

            function changeAgentsFn() {
                $("#tbobyAgentsList").html("");
                if (AgentsList.length > 0) {
                    $.each(AgentsList, function (i, item) {
                        if (item.DeptId === $("#ddlAgentsDept").val()) {
                            $("#tbobyAgentsList").append('<tr><td class="text-center"><input type="checkbox" value="' + item.EmpId + '" /></td><td>' + item.EmpId + ' - ' + item.EmpName + '</td></tr>');
                        }
                    });
                }
            }
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
                if ($("#AssAgents").val() == "0") {//*改派
                    changeAssignBu();
                    $("#modalAgent").modal();//* 選擇指派經辦
                } else {//*收發待辦
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
                        $("#hidTypeForOver").val("1");//*收發待辦
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
                                                btnQueryClick();
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
            }

            //* 提交改派||集作代辦
            function btnAssignSubmit() {
                var aryCaseId = new Array();
                $(".checkfile:checked").each(function () {
                    aryCaseId.push($(this).val());//向数组中添加元素  
                });
                var strCaseId = aryCaseId.join(',');

                var aryAgent = new Array();//改派
                $("#tbobyAgentList input:checked").each(function () {
                    aryAgent.push($(this).val());//改派向数组中添加元素  
                });
                var strAgent = aryAgent.join(',');

                var aryAgents = new Array();//集作
                $("#tbobyAgentsList input:checked").each(function () {
                    aryAgents.push($(this).val());//集作向数组中添加元素  
                });
                var aryAgents = aryAgents.join(',');
                if (strAgent.length <= 0 && aryAgents.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                } else {
                    if ($("#AssAgents").val() == "0") {//* 改派
                        jConfirm($("#AssignConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                            if (bFlag === true) {
                                $.blockUI();
                                //* click confirm ok
                                $.ajax({
                                    type: "POST",
                                    traditional: true,
                                    url: $("#AssignUrl").val(),
                                    async: false,
                                    data: { caseIdList: strCaseId, agentIdList: strAgent },
                                    error: function () {
                                        jAlertError($("#LoadErrorMsg").val());
                                        $.unblockUI();
                                    },
                                    success: function (data) {
                                        if (data.ReturnCode === "1") {
                                            jAlertSuccess($("#AssignSuccessMsg").val(), function () {
                                                $("#modalAgent").modal("hide");
                                                btnQueryClick();
                                            });
                                        } else {
                                            jAlertError($("#AssignFailMsg").val());
                                            $.unblockUI();
                                        }
                                    }
                                });
                            }
                        });//改派
                    } else {//*收發待辦
                        //jConfirm($("#AssAgentsConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                        //    if (bFlag === true) {
                        //        $.blockUI();
                        //        //* click confirm ok
                        //        $.ajax({
                        //            type: "POST",
                        //            traditional: true,
                        //            url: $("#AgentsUrl").val(),
                        //            async: false,
                        //            data: { caseIdList: strCaseId, agentIdList: aryAgents },
                        //            error: function () {
                        //                jAlertError($("#LoadErrorMsg").val());
                        //                $.unblockUI();
                        //            },
                        //            success: function (data) {
                        //                if (data.ReturnCode === "1") {
                        //                    jAlertSuccess($("#AssAgentsSuccessMsg").val(), function () {
                        //                        $("#modalAgents").modal("hide");
                        //                        btnQueryClick();
                        //                    });
                        //                } else {
                        //                    jAlertError($("#AssignFailMsg").val());
                        //                    $.unblockUI();
                        //                }
                        //            }
                        //        });
                        //    }
                        //});//集作代辦
                    }
                }

            }
            //退件結案
            function btnReturnCloseClick() {
                var iLen = 0;
                var strKey = "";
                var strReturnReason = new Array();
                $(".checkfile").each(function () {
                    if ($(this).prop("checked") === true) {
                        iLen = iLen + 1;
                        strKey = strKey + $(this).val() + ",";
                        strReturnReason.push($(this).attr("data-returnreason"));
                    }
                });
                if (iLen <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                if (iLen > 1) {
                    jAlertError($("#SelectOnlyOneMsg").val());
                    return;
                }
                $("#ReturnReason").val(strReturnReason);
                $("#modalClose").modal();
            }

            function btnCloseSubmit() {
                var aryCaseId = new Array();
                $(".checkfile:checked").each(function () {
                    aryCaseId.push($(this).val());//向数组中添加元素  
                });
                var strCaseId = aryCaseId.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                if (!ajaxValidate()) {
                    return false;
                }
                jConfirm($("#ReturnCloseConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {

                        $.blockUI();
                        //* click confirm ok
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#ReturnCloseUrl").val(),
                            async: false,
                            data: { strIds: strCaseId, ReturnReason: $("#ReturnReason").val() },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    jAlertSuccess($("#ReturnCloseSuccessMsg").val(), function () {
                                        $("#modalClose").modal("hide");
                                        $("#ReturnReason").val("");
                                        btnQueryClick();
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
            //*點擊呈核
            function GetDateStr(AddDayCount) {
                var dd = new Date();
                dd.setDate(dd.getDate() + AddDayCount);//获取AddDayCount天后的日期
                var y = dd.getFullYear();
                var m = dd.getMonth() + 1;//获取当前月份的日期
                var d = dd.getDate();
                return y + "" + (m < 10 ? "0" + m : m) + "" + (d < 10 ? "0" + d : d);
            }

            function btnOverClick() {
                var arylimitDate = new Array();
                var arylimit = new Array();
                $(".checkfile:checked").each(function () {
                    arylimitDate.push($(this).attr("data-limitDate").replace("/", "").replace("/", ""));//向数组中添加元素
                });
                var strlimitDate = arylimitDate.sort().join(',');
                var strNowDate = GetDateStr(parseInt($("#AddDay").val()));

                if (strlimitDate < strNowDate) {
                    var iLen = 0;
                    var strKey = "";
                    var strOverDueMemo = new Array();
                    $(".checkfile").each(function () {
                        if ($(this).prop("checked") === true) {
                            iLen = iLen + 1;
                            strKey = strKey + $(this).val() + ",";
                            strOverDueMemo.push($(this).attr("data-overduememo"));
                        }
                    });
                    if (iLen <= 0) {
                        jAlertError($("#SelectOneMsg").val());
                        return;
                    }

                    if (iLen >= 2) {
                        jAlertError($("#NuclearOneMsg").val());
                        return;
                    }
                    $("#hidTypeForOver").val("0");//*呈核
                    $("#OverDueMemo").val(strOverDueMemo);
                    $("#titleForAgent").text($("#NuclearTitle").val());
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
                    jConfirm($("#NuclearConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                        if (bFlag === true) {
                            $.blockUI();
                            $.ajax({
                                type: "POST",
                                traditional: true,
                                url: $("#AssignChenHe").val(),
                                async: false,
                                data: { CaseIdarr: strCaseId },
                                error: function () {
                                    jAlertError($("#LoadErrorMsg").val());
                                    $.unblockUI();
                                },
                                success: function (data) {
                                    if (data.ReturnCode === "1") {
                                        jAlertSuccess($("#NuclearOkMsg").val(), function () {
                                            btnQueryClick();
                                        });
                                    } else {
                                        jAlertError($("#NuclearFaileMsg").val());
                                        $.unblockUI();
                                    }
                                }
                            });
                        }
                    });
                }
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
                if ($("#hidTypeForOver").val() == "0") {//*呈核
                    jConfirm($("#NuclearConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                        if (bFlag === true) {
                            $.blockUI();
                            //* click confirm ok
                            $.ajax({
                                type: "POST",
                                traditional: true,
                                url: $("#OverDueUrl").val(),
                                async: false,
                                data: { strIds: strCaseId, OverDueMemo: $("#OverDueMemo").val() },
                                error: function () {
                                    jAlertError($("#LoadErrorMsg").val());
                                    $.unblockUI();
                                },
                                success: function (data) {
                                    if (data.ReturnCode === "1") {
                                        jAlertSuccess($("#NuclearOkMsg").val(), function () {
                                            $("#modalOver").modal("hide");
                                            $("#OverDueMemo").val("");
                                            btnQueryClick();
                                        });
                                    } else {
                                        jAlertError($("#NuclearFaileMsg").val());
                                        $.unblockUI();
                                    }
                                }
                            });
                        }
                    });
                }
                if ($("#hidTypeForOver").val() == "1") {//*收發待辦
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
                                            btnQueryClick();
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
            }

            function ajaxValidateOver() {
                if ($.trim($("#OverDueMemo").val()) === "") {
                    jAlertError($("#TextNotNull").val());
                    return false;
                }
                return true;
            }

            function ajaxValidate() {
                if ($.trim($("#ReturnReason").val()) === "") {
                    jAlertError($("#TextNotNull").val());
                    return false;
                }
                return true;
            }

            function ajaxValidateQuery() {
                var NewLine = "<br/>";
                var Msg = "";
                if (!checkIsValidDate($("#GovDateS").val())) {
                    Msg = Msg + $.validator.format($("#PlzCorrectFormat").val(), $("#GovDateText").val()) + NewLine;
                }
                if (!checkIsValidDate($("#GovDateE").val())) {
                    Msg = Msg + $.validator.format($("#PlzCorrectFormat").val(), $("#GovDateText").val()) + NewLine;
                }
                if (!checkIsValidDate($("#CreatedDateS").val())) {
                    Msg = Msg + $.validator.format($("#PlzCorrectFormat").val(), $("#CreatedDateText").val()) + NewLine;
                }
                if (!checkIsValidDate($("#CreatedDateE").val())) {
                    Msg = Msg + $.validator.format($("#PlzCorrectFormat").val(), $("#CreatedDateText").val()) + NewLine;
                }
                if (Msg.length > 0) {
                    jAlertError(Msg);
                    return false;
                }
                return true;
            }



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

                if (aryCaseId.length > 100) {
                    jAlertError($("#CheckCount").val());
                }

                var actionUrl = $("#ReportUrl").val() + "?caseIdList=" + strCaseId;
                $("#frmForReport").attr("src", actionUrl);
                return false;
            }
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.AgentToHandle.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}
