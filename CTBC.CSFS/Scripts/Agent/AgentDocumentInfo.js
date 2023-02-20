//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var agentDocumentInfo = $.AgentDocumentInfo = {};
    jQuery.extend(agentDocumentInfo, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "agentDocumentInfoEdit") {
                $.AgentDocumentInfo.initAgentDocumentInfoEdit();
            }
        },

        //* 進入後初始化查詢畫面
        initAgentDocumentInfoEdit: function () {
            $(function () {
                if ($("#ddlReceiveKind").val() == "電子公文")
                {
                    if ($("#IsGovNameExist").val() == 0)
                    {
                        $("#lblValiGovName").show();
                    }
                    disableData();
                }
                ChangeText();
            });
            $("#btnSave").click(function () { return checkData(); });       //* 提交表單
            //$.CSFS.bindGovKindAndUnit("ddlGOV_KIND1", "txtGovUnit1");
            $.CSFS.bindGovKindAndUnit1("txtGovUnit1");
            $("#ddlCaseKind").change(function () { changeCaseKind(); });
            $("#ddlCaseKind2").change(function () { changeCaseKind2(); });

            //if ($("#IsreadOnlyMsg").val() == "isnotreadonly") {
            //    $("#ddlCaseKind").removeAttr("disabled");
            //} else {
            //    $("#ddlCaseKind").prop("disabled", "disabled");
            //}

            function ChangeText() {
                var casekind = $("#ddlCaseKind option:selected").val();
                if (casekind == "外來文案件") {
                    $("#txtPropertyDeclaration").show();
                }
                else {
                    $("#txtPropertyDeclaration").hide();
                }
            }
            //* 刪除一筆附件
            $("a[data-deleteLink='true']").click(function () {
                var traget = $(this).attr("data-href");
                jConfirm($("#DeleteConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag == true) {
                        //* click confirm ok
                        $.blockUI();
                        $.ajax({
                            type: "Post",
                            url: traget,
                            dataType: "json",
                            error: function () {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    jAlertSuccess($("#DeleteSucMsg").val(), function () { location.href = location.href });
                                } else {
                                    jAlertError($("#DeleteFailMsg").val());
                                    $.blockUI();
                                }
                            }
                        });
                    }
                });
                return false;
            });

            function enableData()
            {
                $("#ddlReceiveKind").attr("disabled", false);
                $("#txtGovDate").attr("disabled", false);
                $("#ddlSpeed").attr("disabled", false);
                $("#txtGovNo").attr("disabled", false);
                $("#ddlCaseKind2").attr("disabled", false);
            }

            function disableData() {
                $("#ddlReceiveKind").attr("disabled", true);
                $("#txtGovDate").attr("disabled", true);
                $("#ddlSpeed").attr("disabled", true);
                $("#txtGovNo").attr("disabled", true);
                $("#ddlCaseKind2").attr("disabled", true);
            }

            //* 檢核畫面資料.如果無問題則提交
            function checkData() {
                var newLine = "<br/>";
                var msg = "";
                if ($("#txtGovDate").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameGovDate").val()) + newLine;
                } else if (!checkIsValidDate($("#txtGovDate").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameGovDate").val()) + newLine;
                }

                if ($("#txtGovUnit1").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameGovUnit").val()) + newLine;
                }
                if ($("#ddlSpeed").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameSpeed").val()) + newLine;
                }
                if ($("#ddlReceiveKind").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameReceiveKind").val()) + newLine;
                }
                if ($("#txtGovNo").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameGovNo").val()) + newLine;
                }
                if ($("#ddlCaseKind2").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameCaseKind").val()) + newLine;
                }
                //if ($("#ddlCaseKind2").val() == "財產申報") {
                //    if ($("#txtPropertyDeclaration").val().length <= 0) {
                //        msg = msg + $.validator.format($("#PlzInput").val(), $("#NamePropertyDeclaration").val()) + newLine;
                //    }
                //}
                var errorId = false;
                var oNum = 0;
                for (var i = 0; i < 10; i++) {
                    if ($("#CaseObligorlistO_" + i + "__ObligorName").val().length > 0
                        || $("#CaseObligorlistO_" + i + "__ObligorNo").val().length > 0
                        || $("#CaseObligorlistO_" + i + "__ObligorAccount").val().length > 0) {

                        if ($("#ddlCaseKind").val() == "扣押案件") {
                            if ($("#CaseObligorlistO_" + i + "__ObligorName").val().length <= 0) {
                                msg = msg + $.validator.format($("#PlzInput").val(), $("#NameObligorName").val() + " " + (i + 1)) + newLine;
                            }
                        }

                        if ($("#CaseObligorlistO_" + i + "__ObligorNo").val().length <= 0) {
                            msg = msg + $.validator.format($("#PlzInput").val(), $("#NameObligorNo").val() + " " + (i + 1)) + newLine;
                        }
                        //* 都有輸入,檢核ID格式
                        if ($("#CaseObligorlistO_" + i + "__ObligorNo").val().length > 0) {
                            if (!checkId($("#CaseObligorlistO_" + i + "__ObligorNo").val())) {
                                errorId = true;
                            }
                        }
                        oNum = oNum + 1;
                    }
                }
                if (oNum === 0) {
                    msg = msg + $("#AtLeastOne").val() + newLine;
                }

                var confirmMsg = "";
                if ($("#iLogo").hasClass("fa-exclamation-circle")) {
                    confirmMsg = confirmMsg + $("#GovNoExistConfirmMsg").val() + newLine;
                }
                if (errorId === true) {
                    confirmMsg = confirmMsg + $("#ObligorNoErrorConfirmMsg").val() + newLine;
                }

                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }
                //* 有confirm
                if (confirmMsg.length > 0) {
                    jConfirm(confirmMsg, $("#j_confirm_header").val(), function (bFlag) {
                        if (bFlag === true) {
                            enableData();
                            $("#frmCreate").submit();
                        }
                    });
                    return false;
                }
                enableData();
                $("#frmCreate").submit();
                return true;
            }

            //* 檢查義務人統編格式
            function checkId(str) {
                //* 判斷第一碼為英文時，採用身份證字號檢核邏輯，檢核邏輯：身分證檢核邏輯
                if (checkTwID(str))
                    return true;
                //* 判斷前兩碼為英文時，採用統一證號檢核邏輯，檢核邏輯：兩碼英文+六碼數字
                var pattern2 = /^[a-zA-Z]{2}[0-9]{6}$/g;
                if (pattern2.test(str))
                    return true;
                //* 判斷第一碼為數字時，採用統編檢核邏輯，檢核邏輯：8碼數字
                var pattern3 = /^[0-9]{8}$/g;
                if (pattern3.test(str))
                    return true;
                //* 都不對就驗證失敗
                return false;
            }

            //* 檢查來文編號是否重複
            function checkGovNo() {
                var txtGovNo = $.trim($("#txtGovNo").val());
                if (txtGovNo != null && txtGovNo !== "") {
                    $.ajax({
                        type: "Post",
                        url: $("#CheckGovNoExistUrl").val(),
                        dataType: "json",
                        data: { txtGovNo: txtGovNo },
                        success: function (data) {
                            if (data.ReturnCode === "1") {//數據重複
                                $("#iLogo").removeClass();
                                $("#iLogo").addClass("fa fa-exclamation-circle");
                            } else {//數據未重複
                                $("#iLogo").removeClass();
                                $("#iLogo").addClass("fa fa-check-circle");
                            }
                        }
                    });
                } else {
                    $("#iLogo").removeClass();
                }
            }

            //* 點選義務人後面的X清空該行
            function deleteValue(obj) {
                var tds = $(obj).parent().parent().children("td");
                tds.eq(0).children().val("");
                tds.eq(1).children().val("");
                tds.eq(2).children().val("");
                return false;
            }

            //* 隱藏iframe回調用
            function showMessage(strType, strMsg) {
                if (strType === "1") {
                    jAlertSuccess(strMsg, function () { location.href = location.href; });
                }
                if (strType === "0") {
                    jAlertError(strMsg);
                }
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

            //*扣押案件細類(只有細類為扣押或扣押並支付時，[受文者、來函扣押總金額、金額未達毋需扣押] 這三個欄位才可修改，否則disable只顯示)
            function changeCaseKind2() {
                var selectedValue = $('#ddlCaseKind2 option:selected').val();
                if (selectedValue === "扣押" || selectedValue === "扣押並支付") {
                    $("#txtReceiver").removeAttr("disabled");
                    $("#txtReceiveAmount").removeAttr("disabled");
                    $("#txtNotSeizureAmount").removeAttr("disabled");
                } else {
                    $("#txtReceiver").attr("disabled", "true");
                    $("#txtReceiveAmount").attr("disabled", "true");
                    $("#txtNotSeizureAmount").attr("disabled", "true");
                }
            }
        }

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.AgentDocumentInfo.documentReady();
    });
})(jQuery);

//* 隱藏iframe回調用
function showMessage(strType, strMsg) {
    if (strType === "1") {
        jAlertSuccess(strMsg, function () { location.href = location.href; });
    }
    if (strType === "0") {
        jAlertError(strMsg);
    }
}
function deleteValue(obj) {
    var tds = $(obj).parent().parent().children("td");
    tds.eq(0).children().val("");
    tds.eq(1).children().val("");
    tds.eq(2).children().val("");
    tds.eq(3).children().val("");
    return false;
}
