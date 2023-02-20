//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var collectionToAssign = $.CollectionToAssign = {};
    jQuery.extend(collectionToAssign, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "CollectionToAssignQuery") {
                $.CollectionToAssign.initQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initQuery: function () {

            $.CSFS.bindGovKindAndUnit1("txtGovUnit");
            //* 查詢
            $("#frmQuery").submit(function () { return btnQueryClick(); });
            $("#btnQuery").click(function () { return btnQueryClick(); });
            //* 取消
            $("#btnCancel").click(function () { return btnCancelClick(); });
            //* 點選分派,顯示分派選擇畫面
            $("#btnAssignShow").click(function () { return btnAssignShowClick(); });
            //* 分派提交
            $("#btnAssignSubmit").click(function () { return btnAssignSubmit(); });
            //* 退件
            $("#btnReturn").click(function () { return btnReturnClick(); });
            //* 退件提交
            $("#btnReturnSubmit").click(function () { return btnReturnSubmitClick(); });
            //二級連動
            $('#ddlGovUnit').attr("disabled", "true");

            $("#ddlCaseKind").change(function () { changeCaseKind(); });

            $("#btnSelectGovUnit").click(function () { return btnSelectGovUnitClick(); });            
            
            $("#btnDetails").click(function () { return btnDetailsClick(); });
            
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
            //* 部门下拉框改變時.改變列表中員工姓名
            //$("#ddlAssignDept").change(function () { changeAssignBu(); });
            $("#Department").change(function () { changeAssignBu(); });

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

            //* 點選查詢
            function btnQueryClick() {
                $("#divResult").html("");
                trimAllInput();
                if (!ajaxValidate()) {
                    return false;
                }
                //派件按鈕不Disable
                //var selectedValue = $("#ddlCaseKind option:selected").val();
                ////如果選擇扣押案件,且參數設定(AutoDispatch)1.True: 反灰，不可使用 2.False: 藍色，可以派件
                //if (selectedValue == "扣押案件") {
                //    if ($("#isAutoDispatch").val() === "true") {
                //        $("#btnAssignShow").attr("disabled", "true");
                //    } else {
                //        $("#btnAssignShow").removeAttr("disabled");
                //    }
                //}
                ////如果選擇外來文案件,參數設定(AutoDispatchFS)1.True: 反灰，不可使用 2.False: 藍色，可以派件
                //if (selectedValue == "外來文案件") {
                //    if ($("#isAutoDispatchFS").val() === "true") {
                //        $("#btnAssignShow").attr("disabled", "true");
                //    } else {
                //        $("#btnAssignShow").removeAttr("disabled");
                //    }
                //}
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
            //* 點選退回
            function btnReturnClick() {
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
                    jAlertError($("#SelectOneMsg1").val());
                    return;
                }
                $("#txtReturnReason").val(strReturnReason);
                $("#modalReturn").modal();
            }

            function btnReturnSubmitClick() {
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

                jConfirm($("#ReturnConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        //* click confirm ok
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#ReturnUrl").val(),
                            async: false,
                            data: { strIds: strCaseId, returnReason: $("#txtReturnReason").val() },
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    jAlertSuccess($("#ReturnSuccessMsg").val(), function () {
                                        $("#modalReturn").modal("hide");
                                        $("#txtReturnReason").val("");
                                        btnQueryClick();
                                    });
                                } else {
                                    jAlertError($("#ReturnFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
            }

            //* 分派畫面部門選擇改變.
            //function changeAssignBu() {
            //    $("#tbobyAgentList").html("");
            //    if (agentList.length > 0) {
            //        $.each(agentList, function (i, item) {
            //            if (item.DeptId === $("#ddlAssignDept").val()) {
            //                $("#tbobyAgentList").append('<tr><td class="text-center"><input type="checkbox" value="' + item.EmpId + '" /></td><td>' + item.EmpId + ' - ' + item.EmpName + '</td></tr>');
            //            }
            //        });
            //    }
            //}
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
            //* 點選簽收
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
                changeAssignBu();
                $("#modalAgent").modal();
            }

            //* 提交分派
            function btnAssignSubmit() {
                var aryCaseId = new Array();
                $(".checkfile:checked").each(function () {
                    aryCaseId.push($(this).val());//向数组中添加元素  
                });
                var strCaseId = aryCaseId.join(',');
                var aryAgent = new Array();
                $("#tbobyAgentList input:checked").each(function () {
                    aryAgent.push($(this).val());//向数组中添加元素  
                });
                var strAgent = aryAgent.join(',');
                if (strAgent.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
               
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
                });
            }

            //
            function btnDetailsClick() {
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
                    jAlertError($("#SelectOneMsg1").val());
                    return;
                }
                strKey = strKey.trim().substr(0, strKey.length - 1);
                location.href = $("#DetailsUrl").val() + "?CaseId=" + strKey;
            }

            function ajaxValidateOver() {
                if ($.trim($("#txtReturnReason").val()) === "") {
                    jAlertError($("#TextNotNull").val());
                    return false;
                }
                return true;
            }

            function changeCaseKind() {
                try {
                    var selectedValue = $("#ddlCaseKind option:selected").val();
                    if (selectedValue === "") {
                        $("#ddlGovUnit").attr("disabled", "true");
                        $("#ddlCaseKind2").empty();
                        $("#ddlCaseKind2").append($("<option></option>").val("").text($("#PlzSelect").val()));
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
                                        $("#ddlCaseKind2").append($("<option></option>").val('').text($("#PlzSelect").val()));
                                        $.each(data, function (i, item) {
                                            $("#ddlCaseKind2").append($("<option></option>").val(item.Value).text(item.Value));
                                        });
                                    } else {
                                        $("#ddlCaseKind2").attr("disabled", "true");
                                        $("#ddlCaseKind2").empty();
                                        $("#ddlCaseKind2").append($("<option></option>").val('').text($("#PlzSelect").val()));
                                    }
                                }
                            });
                        }
                    }
                }
                catch (e) {
                }
            }

            function btnSelectGovUnitClick() {
                var selectedValue = $("#ddlGovUnit option:selected").val();
                if (selectedValue === "") {
                    $("#modalUnits").modal("hide");
                } else {
                    $("#GovUnit").val(selectedValue);
                    $("#modalUnits").modal("hide");
                }
            }

            function ajaxValidate() {
                var newLine = "<br/>";
                var msg = "";
                //派件按鈕不Disable
                ////選擇案件類別(用來判斷查詢后派件按鈕是否反灰)
                //var selectedValue = $("#ddlCaseKind option:selected").val();
                //if (selectedValue === "") {
                //    msg = msg + $("#PlzCaseKindMsg").val() + newLine;
                //}
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
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.CollectionToAssign.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}