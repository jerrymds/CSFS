//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var caseSeizure = $.CaseSeizure = {};
    jQuery.extend(caseSeizure, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "CaseSeizureCreate") {
                $.CaseSeizure.initCaseSeizureCreate();
            }
        },

        //* 進入後初始化查詢畫面
        initCaseSeizureCreate: function () {
            $("#btnSave").click(function () { return checkData(); });       //* 提交表單
            //$.CSFS.bindGovKindAndUnit("ddlGOV_KIND", "txtGovUnit");
            //$("#ddlGovUnit").attr("disabled", "true");                      //* 欄位機關2預設禁用
            $("#ddlGOV_KIND").change(function () { changeGovKind(); });      //* 來文機關
            $("#txtGovNo").blur(function () { checkGovNo(); });             //* 來文編號修改後檢查是否重複
            $(".js-CleanLine").click(function () { return deleteValue(this); });   //* 點選清楚該行義務人
            $("#ddlReceiveKind").change(function () { changeReceiveKind(); });     //* 來文方式
            $("#ddlCaseKind2").change(function () { changeddlCaseKind2(); });     //* 扣押案件細類
            //* 進畫面來先檢查是否結束建檔
            checkEnd();

            //changeGovKind();
            //* 來文機關聯動

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
                if ($("#ddlGOV_KIND").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameGovUnit").val()) + newLine;
                }
                else if ($("#ddlGovUnit").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameGovUnit").val()) + newLine;
                }

                var txtYear = $("#txtGovDate").val().substring(0, 3);
                var year = $("#HidYear").val();
                if ($("#txtGovDate").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameGovDate").val()) + newLine;
                } else if (txtYear > (parseInt(year) + 5) || txtYear < (parseInt(year) - 5)) {//判斷年份在當前年份上下五個差內
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameGovDate").val()) + newLine;
                }
                else if (!checkIsValidDate($("#txtGovDate").val())) {
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

                        if ($("#CaseObligorlistO_" + i + "__ObligorName").val().length <= 0) {
                            msg = msg + $.validator.format($("#PlzInput").val(), $("#NameObligorName").val() + " " + (i + 1)) + newLine;
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
            //* 來文機關
            function changeGovKind() {
                try {
                    var selectedValue = $('#ddlGOV_KIND option:selected').val();
                    if (selectedValue === "") {
                        $("#ddlGovUnit").attr("disabled", "true");
                        $("#ddlGovUnit").empty();
                        $("#ddlGovUnit").append($('<option></option>').val("").text($("#PlzSelect").val()));
                    } else {
                        // 金額未達毋需扣押（不可修改） 電子來文預設450，紙本來文 法院1250 else 450
                        var selectedReceiveKind = $('#ddlReceiveKind option:selected').val();//來文方式
                        if (selectedReceiveKind === "電子公文") {
                            $("#txtNotSeizureAmount").val(450);
                        } else {
                            if (selectedValue === "法院") {
                                $("#txtNotSeizureAmount").val(1250);
                            } else {
                                $("#txtNotSeizureAmount").val(450);
                            }
                        }
                        if ($.trim(selectedValue).length > 0) {
                            $.ajax({
                                type: "POST",
                                async: false,
                                url: $("#GetGovNameUrl").val(),
                                data: { govKind: selectedValue },
                                success: function (data) {
                                    if (data.length > 0) {
                                        $("#ddlGovUnit").removeAttr("disabled");
                                        $("#ddlGovUnit").empty();
                                        $("#ddlGovUnit").append($('<option></option>').val("").text($("#PlzSelect").val()));
                                        $.each(data, function (i, item) {
                                            $("#ddlGovUnit").append($('<option></option>').val(item).text(item));
                                        });
                                    } else {
                                        $("#ddlGovUnit").attr("disabled", "true");
                                        $("#ddlGovUnit").empty();
                                        $("#ddlGovUnit").append($('<option></option>').val("").text($("#PlzSelect").val()));
                                    }
                                }
                            });
                        }
                    }
                }
                catch (e) {
                }
            }
            //*來文方式
            function changeReceiveKind() {
                var selectedValue = $('#ddlReceiveKind option:selected').val();
                var selectedGovKind = $('#ddlGOV_KIND option:selected').val();
                //受文者（不可修改） 電子來文 預設8888，紙本來文 預設與分行別同
                if (selectedValue === "電子公文") {
                    $("#txtReceiver").val("8888");
                    $("#txtNotSeizureAmount").val(450);
                } else {
                    $("#txtReceiver").val($("#txtUnit").val());
                    if (selectedGovKind === "法院") {
                        $("#txtNotSeizureAmount").val(1250);
                    } else {
                        $("#txtNotSeizureAmount").val(450);
                    }
                }
            }
            //*扣押案件細類(只有細類為扣押或扣押並支付時，才會有 [受文者、來函扣押總金額、金額未達毋需扣押] 這三個欄位)
            function changeddlCaseKind2() {
                var selectedValue = $('#ddlCaseKind2 option:selected').val();
                if (selectedValue === "扣押" || selectedValue === "扣押並支付") {
                    $("#SeizureId").removeClass("hidden");
                } else {
                    $("#SeizureId").addClass("hidden");
                }
            }
        }

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.CaseSeizure.documentReady();
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