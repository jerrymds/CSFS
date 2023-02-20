//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var WarningUnionQuery = $.WarningUnionQuery = {};
    jQuery.extend(WarningUnionQuery, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "WarningUnionQuery") {
                $.WarningUnionQuery.initWarningUnionQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initWarningUnionQuery: function () {
            $("#btnQuery").click(function () { return $.WarningUnionQuery.QueryData(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });
            $("#btnReturnClose").click(function () { return btnApproveSubmit() });
            //$("#btnExcel").click(function () { return ExcelClick();})          

            //function ExcelClick() {
            //    //location.href = $("#ExcelUrl").val() + "?CustId=" + $("#txtCustId").val() + "&CustAccount=" + $("#txtCustAccount").val() + "&DocNo=" + $("#txtDocNo").val()
            //    //+ "&VictimName=" + $("#txtVictimName").val() + "&ForCDateS=" + $("#txtForCDateS").val() + "&ForCDateE=" + $("#txtForCDateE").val();
            //    var actionUrl = $("#ExcelUrl").val();
            //    $("#frmForReport").attr("src", actionUrl);
            //}
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

            function changeCOL_OTHERBANKIDName(obj) {
                var td = $(obj).parents("tr").children("td").eq(5);
                $.ajax({
                    type: "POST",
                    async: false,
                    url: $("#GetBranchNameUrl").val(),
                    data: { codeNo: $(obj).val() },
                    success: function (data) {
                        td.children("input").val(data);
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
            if ($("#txtCustId").val().length <= 0 && $("#txtKind").val().length <= 0 && $("#txtCustAccount").val().length <= 0 && $("#txtDocNo").val().length <= 0 && $("#txtForCDateS").val().length <= 0 && $("#txtForCDateE").val().length <= 0 && $("#txtRelieveDateS").val().length <= 0 && $("#txtRelieveDateE").val().length <= 0 && $("#txtOriginal").val().length <= 0) {
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
        $.WarningUnionQuery.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}