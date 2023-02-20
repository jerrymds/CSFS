//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var directorApproved = $.DirectorApproved = {};
    jQuery.extend(directorApproved, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "DirectorApprovedQuery") {
                $.DirectorApproved.initQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initQuery: function () {
            $.CSFS.bindGovKindAndUnit1("txtGovUnit");
            $("#ddlCaseKind2").attr("disabled", "true");
            $("#btnQuery").click(function () { return btnQueryclick(); });
            $("#ddlCaseKind").change(function () { changeCaseKind(); });
            $("#btnReturn").click(function () { return btnReturnClick() });   //* 退件
            //* 退件提交
            $("#btnCloseSubmit").click(function () { return btnCloseSubmit(); });

            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add start
            $("#ddlAgentDepartment2").prop("disabled", "disabled");
            $("#ddlAgentDepartmentUser").prop("disabled", "disabled");
            //* 經辦人員
            $("#ddlAgentDepartment").change(function () { changeAgentDepartment(); });
            $("#ddlAgentDepartment2").change(function () { changeAgentDepartment2(); });
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add end

            if ($("#isQuery").val() == "1") {
                changeCaseKind();
                $("#pageNum").val(parseInt($("#CurrentPage").val()));
                $("#ddlCaseKind2").val($("#CaseKind2Query").val());
                btnQueryclick();
            }

            //* 查詢
            function btnQueryclick() {
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
                if (!checkIsValidDate($("#SendDateS").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#SendDateText").val()) + newLine;
                }
                if (!checkIsValidDate($("#SendDateE").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#SendDateText").val()) + newLine;
                }
                if (!checkIsValidDate($("#ApproveDateS").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#ApproveDateText").val()) + newLine;
                }
                if (!checkIsValidDate($("#ApproveDateE").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#ApproveDateText").val()) + newLine;
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
            
            
            //* 點選退件
            function btnReturnClick() {
                var caseIdArr = new Array();
                var i = 0;
                $(".checkfile:checked").each(function () {
                    caseIdArr.push($(this).val());//向数组中添加元素
                    if ($(this).parent().parent().attr("sendupdate") || $(this).parent().parent().attr("mailno")) {
                        i++;
                    }
                });
                var strCaseId = caseIdArr.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                if (i > 0)
                {
                    jAlertError($("#ReturnMsg").val());
                    return;
                }
                $("#modalClose").modal();
            }

            //20170714 固定 RQ-2015-019666-019 派件至跨單位(廠商bug一併修正) 宏祥 update start
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
                            jAlertError($("#ReturnFaileMsg").val());
                            $.unblockUI();
                        }
                    }
                });
            }
            //20170714 固定 RQ-2015-019666-019 派件至跨單位(廠商bug一併修正) 宏祥 update end

            function ajaxValidate() {
                if ($("#CloseReason").val() <= 0) {
                    jAlertError($("#TextNotNull").val());
                    return false;
                }
                return true;
            }

            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add start
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
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add end
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.DirectorApproved.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}