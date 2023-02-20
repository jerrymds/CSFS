//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var agentOriginalInfo = $.AgentOriginalInfo = {};
    jQuery.extend(agentOriginalInfo, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "AgentOriginalInfoQuery") { //* 正本調閱
                $.AgentOriginalInfo.initQuery();
            }
            if ($("#NowPage").val() === "AgentOriginalInfoQueryLend") { //*正本歸還
                $.AgentOriginalInfo.initQueryLend();
            }
            if ($("#NowPage").val() === "AgentOriginalInfoCreate") {//* 正本調閱新增
                $.AgentOriginalInfo.initCreate();
            }
            if ($("#NowPage").val() === "AgentOriginalInfoEdit") {//* 正本調閱修改
                $.AgentOriginalInfo.initEdit();
            }
            if ($("#NowPage").val() === "AgentOriginalInfoLendEdit") {//* 正本歸還修改
                $.AgentOriginalInfo.initEditLend();
            }
        },

        //* 正本調閱
        initQuery: function () {
            $("#hidLendCaseId").val($("#hidCaseid").val());      //*查詢CaseId
            $(document).ready(function () { return $.AgentOriginalInfo.GetQueryResult(); });
        },

        //*正本歸還
        initQueryLend: function () {
            $("#hidLendStatus").val($("#hidLendStatus").val())
            $("#hidLendCaseId1").val($("#hidCaseidLend").val())
            $("#hidLendCaseIdLend").val($("#hidCaseidLend").val())
            $(document).ready(function () { return $.AgentOriginalInfo.GetQueryResultLendCase(); })
            $("#btnLendBackQuery").click(function () { return $.AgentOriginalInfo.GetQueryResultLend(); })
        },

        //* 正本調閱新增
        initCreate: function () {
            $("#ddlBranch").change(function () { return ChangBranch(); })
            $("#btnSaveLend").click(function () { return CheckLendData(); })
            $("#btnCancelA").click(function () { return btnCancelAClick(); })
            $("#txtBank").val($("#ddlBranch option:selected").val());

            function ChangBranch() {
                var selectedValue = $("#ddlBranch option:selected").val();
                $("#txtBank").val(selectedValue);
            }

            //* 確定儲存
            function CheckLendData() {
                var newLine = "<br/>";
                var msg = "";
                if ($("#txtClientID").val().length <= 0) {
                    msg = msg + $("#NametxtClientIDLendEdit").val() + newLine;
                }
                if ($("#txtClientName").val().length <= 0) {
                    msg = msg + $("#NametxtClientNameLendEdit").val() + newLine;
                }
                //if ($("#txtBankID").val().length <= 0) {
                //    msg = msg + $("#NametxtBankIDLendEdit").val() + newLine;
                //}
                if ($("#txtPhone").val().length <= 0) {
                    msg = msg + $("#NametxtPhone").val() + newLine;
                }
                //if (isNaN($("#txtPhone").val())) {
                //    msg = msg + $("#NametxtPhone1").val() + newLine;
                //}
                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }
                $("#frmCreate").submit();
                return true;
            }

            //* 點擊取消
            function btnCancelAClick() {
                parent.$.colorbox.close();
            }
        },

        //*正本調閱修改
        initEdit: function () {
            //$("#ddlBranchEdit option:selected").text($("#HidBranchNo").val());
            $("#ddlBranchEdit").change(function () { return ChangBranchEdit(); })
            $("#btnSaveLendEdit").click(function () { return CheckLendEditData(); })
            $("#btnCancel").click(function () { return CancelData(); })

            function ChangBranchEdit() {
                var selectedValue = $("#ddlBranchEdit option:selected").val();
                $.ajax({
                    url: $("#GetBranchNameUrl").val(),
                    type: "Get",
                    cache: false,
                    data: { BranchNo: selectedValue },
                    success: function (data) {
                        $("#txtBankEdit").val(data);
                    },
                    error: function () {
                    }
                });



            }

            //* 點擊修改
            function CheckLendEditData() {
                var newLine = "<br/>";
                var msg = "";
                if ($("#txtClientIDEdit").val().length <= 0) {
                    msg = msg + $("#NametxtClientIDLendEdit").val() + newLine;
                }
                if ($("#txtClientNameEdit").val().length <= 0) {
                    msg = msg + $("#NametxtClientNameLendEdit").val() + newLine;
                }
                //if ($("#txtBankIDEdit").val().length <= 0) {
                //    msg = msg + $("#NametxtBankIDLendEdit").val() + newLine;
                //}
                if ($("#txtPhoneEdit").val().length <= 0) {
                    msg = msg + $("#NametxtPhone").val() + newLine;
                }
                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }

                $("#frmEdit").submit();
                return true;
            }

            //* 點擊取消
            function CancelData() {
                parent.$.colorbox.close();
            }
        },

        //*正本歸還修改
        initEditLend: function () {
            $("#btnSaveBackEdit").click(function () { return CheckLendEditData(); })
            $("#btnCancel").click(function () { return CancelData(); })
            //$("#ddlBranchLendEdit").change(function () { return ChangBranchLendEdit(); })

            //$(document).ready(function () {
            //    if ($("#hidStatusFor").val() == 0) {
            //        $("#txtBankLendEdit").val($("#ddlBranchLendEdit option:selected").val());
            //    }
            //})


            //function ChangBranchLendEdit() {
            //    var selectedValue = $("#ddlBranchLendEdit option:selected").val();
            //    $("#txtBankLendEdit").val(selectedValue);
            //}

            //* 點擊修改
            function CheckLendEditData() {
                var newLine = "<br/>";
                var msg = "";
                if ($("#txtDocNoEdit").val().length <= 0) {
                    msg = msg + $("#NametxtDocNoEdit").val() + newLine;
                }
                if ($("#txtClientIDLendEdit").val().length <= 0) {
                    msg = msg + $("#NametxtClientIDLendEdit").val() + newLine;
                }
                if ($("#txtClientNameLendEdit").val().length <= 0) {
                    msg = msg + $("#NametxtClientNameLendEdit").val() + newLine;
                }
                //if ($("#txtBankIDLendEdit").val().length <= 0) {
                //    msg = msg + $("#NametxtBankIDLendEdit").val() + newLine;
                //}
                if ($("#txtReturnDateEdit").val().length <= 0) {
                    msg = msg + $("#NametxtReturnDateEdit").val() + newLine;
                } else if (!checkIsValidDate($("#txtReturnDateEdit").val())) {
                    msg = msg + $("#NameReturnDate").val() + newLine;
                }
                if ($("#txtReturnBankDate").val().length <= 0) {
                    msg = msg + $("#NametxtReturnBankDate").val() + newLine;
                } else if (!checkIsValidDate($("#txtReturnBankDate").val())) {
                    msg = msg + $("#NameReturnBankDate").val() + newLine;
                }
                if ($("#txtReturnPostNoEdit").val().length <= 0) {
                    msg = msg + $("#NametxtReturnPostNoEdit").val() + newLine;
                }
                if ($("#txtBankReceiverEdit").val().length <= 0) {
                    msg = msg + $("#NametxtBankReceiverEdit").val() + newLine;
                }
                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }

                $("#frmLendEdit").submit();
                return true;
            }

            //* 點擊取消
            function CancelData() {
                parent.$.colorbox.close();
            }
        },

        //* 正本歸還所有案件未歸還查詢
        GetQueryResultLend: function () {
            $("#divResultLend").html("");
            trimAllInput();
            $.blockUI();
            $.ajax({
                url: $("#frmQueryLend").attr("action"),
                type: "Post",
                cache: false,
                data: $("#frmQueryLend").serialize(),
                error: function () {
                    jAlertError($("#LoadErrorMsg").val());
                    $.unblockUI();
                },
                success: function (data) {
                    $("#divResultLend").html(data).show();
                    $.unblockUI();
                    $("#querystring").val($("#frmQuery").serialize());
                    $.CSFS.bindFancybox();
                }
            });
            return false;
        },

        //* 正本歸還本案件查詢
        GetQueryResultLendCase: function () {
            $("#divResultLendCase").html("");
            trimAllInput();
            $.blockUI();
            $.ajax({
                url: $("#frmQueryLendCase").attr("action"),
                type: "Post",
                cache: false,
                data: $("#frmQueryLendCase").serialize(),
                error: function () {
                    jAlertError($("#LoadErrorMsg").val());
                    $.unblockUI();
                },
                success: function (data) {
                    $("#divResultLendCase").html(data).show();
                    $.unblockUI();
                    $.CSFS.bindFancybox();
                }
            });
            return false;
        },

        //* 正本調閱查詢
        GetQueryResult: function () {
            $("#divResult").html("");
            trimAllInput();

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
                    $.CSFS.bindFancybox();
                }
            });
            return false;
        }

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.AgentOriginalInfo.documentReady();
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
    debugger;
    if (strType === "1") {
        jAlertSuccess(strMsg, function () { location.href = location.href; });
    }
    if (strType === "0") {
        jAlertError(strMsg);
    }
}