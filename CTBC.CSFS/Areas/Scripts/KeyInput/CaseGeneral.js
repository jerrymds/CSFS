//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var caseGeneral = $.CaseGeneral = {};
    jQuery.extend(caseGeneral, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "CaseGeneralCreate") {
                $.CaseGeneral.initCaseGeneralQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initCaseGeneralQuery: function () {
            $("#btnSave").click(function () { return checkData(); });       //* 提交表單
            $.CSFS.bindGovKindAndUnit1("txtGovUnit");
            $("#txtGovNo").blur(function () { checkGovNo(); });             //* 來文編號修改後檢查是否重複
            $(".js-CleanLine").click(function () { return deleteValue(this); });   //* 點選清楚該行義務人
            //$("#ddlCaseKind2").change(function () { ChangeText(); });

            //function ChangeText() {
            //    var casekind2 = $("#ddlCaseKind2 option:selected").val();
            //    if (casekind2 == "財產申報") {
            //        $("#txtPropertyDeclaration").show();
            //    }
            //    else {
            //        $("#txtPropertyDeclaration").hide();
            //    }
            //}
            //* 進畫面來先檢查是否結束建檔
            checkEnd();
            //* 檢查目前是否已到停止建檔時間
            function checkEnd(nowarning) {
                var rtn = false;
                $.ajax({
                    url: $("#CheckEndTimeUrl").val(),
                    type: "Post",
                    cache: false,
                    dataType: "json",
                    async: !nowarning,
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        return false;
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            rtn = true;
                        } else if (data.ReturnCode === "2") {
                            //* 即將過期
                            if (nowarning !== true) { jAlertError(data.ReturnMsg); }
                            rtn = true;
                        } else {
                            //* 過期
                            jAlertError(data.ReturnMsg);
                            $("#btnSave").prop("disabled", "disabled");
                            rtn = false;
                        }
                    }
                });
                return rtn;
            }
           
            //* 檢核畫面資料.如果無問題則提交
            function checkData() {
                if (!checkEnd(true)) {
                    //* 超出期限不准提交
                    return false;
                }
                var newLine = "<br/>";
                var msg = "";
                //if ($("#ddlGOV_KIND").val().length <= 0) {
                //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameGovUnit").val()) + newLine;
                //}else 
                if ($("#txtGovUnit").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameGovUnit").val()) + newLine;
                }
                if ($("#txtLimitDate").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameLimitDate").val()) + newLine;
                }                
                if ($("#txtGovDate").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameGovDate").val()) + newLine;
                } else if (!checkIsValidDate($("#txtGovDate").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameGovDate").val()) + newLine;
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

                var errorId = false;
                var oNum = 0;
                for (var i = 0; i < 10; i++) {
                    if ($("#CaseObligorlistO_" + i + "__ObligorName").val().length > 0
                        || $("#CaseObligorlistO_" + i + "__ObligorNo").val().length > 0
                        || $("#CaseObligorlistO_" + i + "__ObligorAccount").val().length > 0) {

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
                            $("#frmCreate").submit();
                        }
                    });
                    return false;
                }

                $("#frmCreate").submit();
                return true;
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
            //* 點選義務人後面的X清空該行
            function deleteValue(obj) {
                var tds = $(obj).parent().parent().children("td");
                tds.eq(0).children().val("");
                tds.eq(1).children().val("");
                tds.eq(2).children().val("");
                return false;
            }
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.CaseGeneral.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
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

