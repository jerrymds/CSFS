//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var warningToApprove = $.WarningToApprove = {};
    jQuery.extend(warningToApprove, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "WarningToApprove") {
                $.WarningToApprove.initWarningToApprove();
            }
        },

        //* 進入後初始化查詢畫面
        initWarningToApprove: function () {
            $("#btnQuery").click(function () { return $.WarningToApprove.QueryData(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });
            $("#btnApprove").click(function () { return btnApproveSubmit() });
            $("#btnReturn").click(function () { return btnReturnClick() });   //* 退件
            $("#btnCloseSubmit").click(function () { return btnCloseSubmit(); });
            //$("#btnExcel").click(function () { return ExcelClick();})          

            //function ExcelClick() {
            //    //location.href = $("#ExcelUrl").val() + "?CustId=" + $("#txtCustId").val() + "&CustAccount=" + $("#txtCustAccount").val() + "&DocNo=" + $("#txtDocNo").val()
            //    //+ "&VictimName=" + $("#txtVictimName").val() + "&ForCDateS=" + $("#txtForCDateS").val() + "&ForCDateE=" + $("#txtForCDateE").val();
            //    var actionUrl = $("#ExcelUrl").val();
            //    $("#frmForReport").attr("src", actionUrl);
            //}
            function btnCloseSubmit() {
                var aryCaseId = new Array();
                var statusArr = new Array();
                $(".checkfile:checked").each(function () {
                    aryCaseId.push($(this).val());//向数组中添加元素  
                    var cr = $("#CloseReason").val();
                    console.log(cr);
                    //statusArr.push($(".checkfile:checked").attr("data-status"));
                    statusArr.push(cr);
                });
                var strCaseId = aryCaseId.join(',');
                var strStatus = statusArr.join(',');
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return;
                }
                //if (!ajaxValidate()) {
                //    return false;
                //}

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
            //* 查詢
            function btnQueryclick() {
                trimAllInput();
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
                return false;
            }
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
                //jConfirm("是否確定", $("#j_confirm_header").val(), function (bFlag) {
                //    if (bFlag === true) {
                //        $.blockUI();
                //        $.ajax({
                //            type: "POST",
                //            traditional: true,
                //            url: $("#ReturnUrl").val(),
                //            async: false,
                //            data: { CaseIdarr: strCaseId, statusArr: strStatus },
                //            error: function () {
                //                jAlertError($("#LoadErrorMsg").val());
                //                $.unblockUI();
                //            },
                //            success: function (data) {
                //                if (data.ReturnCode === "1") {
                //                    //jAlertSuccess($("#ApproveOkMsg").val(), function () {
                //                    $.unblockUI();
                //                    btnQueryclick();
                //                    //});
                //                } else {
                //                    jAlertError(data.ReturnMsg);
                //                    $.unblockUI();
                //                }
                //            }
                //        });
                //    }
                //});

                $("#modalClose").modal();
            }
            function btnApproveSubmit() {
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
                jConfirm("是否確定", $("#j_confirm_header").val(), function (bFlag) {
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

            //* 點選取消
            function btnCancelClick() {
                $("#divResult").html("");
            }
        },

        QueryData: function () {
            var msg = "";
            var newLine = "<br/>";
            if ($("#txtCustId").val().length <= 0 && $("#txtSet").val().length <= 0 && $("#txtKind").val().length <= 0 && $("#txtCustAccount").val().length <= 0 && $("#txtDocNo").val().length <= 0 && $("#txtForCDateS").val().length <= 0 && $("#txtForCDateE").val().length <= 0 &&  $("#txtRelieveDateS").val().length <= 0 && $("#txtRelieveDateE").val().length <= 0 && $("#txtOriginal").val().length <= 0 ) {
                msg = msg + $("#NameOneQuery").val() + newLine;
            }

            //* 有必填檢核錯誤
            if (msg.length > 0) {
                jAlertError(msg);
                return false;
            }

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
                    //$.CSFS.bindFancybox();
                    $("#querystring").val($("#frmQuery").serialize());
                }
            });
            return false;
        },

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.WarningToApprove.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}