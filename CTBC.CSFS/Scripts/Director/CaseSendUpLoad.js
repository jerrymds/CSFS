//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var caseSendUpLoad = $.CaseSendUpLoad = {};
    jQuery.extend(caseSendUpLoad, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "CaseSendUpLoadQuery") {
                $.CaseSendUpLoad.initQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initQuery: function () {
            $.CSFS.bindGovKindAndUnit1("txtGovUnit");
            $("#ddlCaseKind2").attr("disabled", "true");
            $("#btnQuery").click(function () { return btnQueryclick(); });
            $("#ddlCaseKind").change(function () { changeCaseKind(); });
            $("#btnUpLoad").click(function () { return btnUpLoadclick(); });
            $("#btnBatch").click(function () { return btnBatchclick(); });
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
           
            function ajaxValidate() {
                if ($("#CloseReason").val() <= 0) {
                    jAlertError($("#TextNotNull").val());
                    return false;
                }
                return true;
            }

            //上傳
            function btnUpLoadclick()
            {
                var aryCaseId = new Array();
                $(".checkfile:checked").each(function () {
                    aryCaseId.push($(this).val());//向数组中添加元素  
                });
                var strCaseId = aryCaseId.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return false;
                }

                var count = parseInt($("#UploadCount").val());
                if (aryCaseId.length > count) {
                    jAlertError($("#CheckCount").val());
                    return false;
                }
                jConfirm($.validator.format($("#UploadfirmMsg").val(), aryCaseId.length.toString()), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#UploadUrl").val(),
                            async: false,
                            data: { CaseIdarr: strCaseId },
                            error: function () {
                                jAlertError($("#UploadFaileMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "2") {
                                    jAlertSuccess("上傳成功", function () {
                                    $.unblockUI();
                                    btnQueryclick();
                                    });
                                } else if (data.ReturnCode === "0") {
                                    jAlertError("上傳失敗");
                                    $.unblockUI();
                                    btnQueryclick();
                                } else if (data.ReturnCode === "1")
                                {
                                    jAlertError("寄送Email時發生錯誤");
                                    $.unblockUI();
                                    btnQueryclick();
                                } else if (data.ReturnCode === "3")
                                {
                                    jAlertError("沒有可上傳的檔案");
                                    $.unblockUI();
                                    btnQueryclick();
                                }
                                
                            }
                        });
                    }
                });
                                   
            }

            //上傳
            function btnBatchclick() {
                var aryCaseIdall = new Array();
                $(".checkfile").each(function () {
                    aryCaseIdall.push($(this).val());//向数组中添加元素  
                });
                var strCaseIdall = aryCaseIdall.join(',');
                if (strCaseIdall.length <= 0) {
                    jAlertError($("#ResultZero").val());
                    return false;
                }

                var count = parseInt($("#txtCountNum").val());
                if (count == 0)
                {
                    jAlertError($("#BatchZero").val());
                    return false;
                }
                if (aryCaseIdall.length < count) {
                    jAlertError($("#MatchError").val());
                    return false;
                }
                var aryCaseId = new Array();
                var i;
                for (i = 0; i < count ; ++i) {
                    aryCaseId.push(aryCaseIdall[i]);
                }
                var strCaseId = aryCaseId.join(',');;
                jConfirm($.validator.format($("#UploadfirmMsg").val(), aryCaseId.length.toString()), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#BatchUrl").val(),
                            async: false,
                            data: { CaseIdarr: strCaseId },
                            error: function () {
                                jAlertError($("#UploadFaileMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "2") {
                                    jAlertSuccess("上傳成功", function () {
                                        $.unblockUI();
                                        btnQueryclick();
                                    });
                                } else if (data.ReturnCode === "0") {
                                    jAlertError("上傳失敗");
                                    $.unblockUI();
                                    btnQueryclick();
                                } else if (data.ReturnCode === "1") {
                                    jAlertError("寄送Email時發生錯誤");
                                    $.unblockUI();
                                    btnQueryclick();
                                } else if (data.ReturnCode === "3") {
                                    jAlertError("沒有可上傳的檔案");
                                    $.unblockUI();
                                    btnQueryclick();
                                }

                            }
                        });
                    }
                });

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
        $.CaseSendUpLoad.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}