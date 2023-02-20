//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var seizureQuery = $.SeizureQuery = {};
    jQuery.extend(seizureQuery, {
        documentReady: function () {
            if ($("#NowPage").val() === "SeizureQueryQuery") {
                $.SeizureQuery.initSeizureQueryQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initSeizureQueryQuery: function () {
            $.CSFS.bindGovKindAndUnit1("txtGovUnit");
            $("#ddlCaseKind").change(function () { return $.SeizureQuery.changeCaseKind(); });
            $("#frmQuery").submit(function () { return QueryData(); });
            $("#btnQuery").click(function () { return QueryData(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });
            $("#btnExport").click(function () { return $.SeizureQuery.btnReportSelected(); });

            if ($("#isQuery").val() == "1") {
                $.SeizureQuery.changeCaseKind();
                $("#pageNum").val(parseInt($("#CurrentPage").val()));
                $("#ddlCaseKind2").val($("#CaseKind2Query").val());
                QueryData();
            }

            //* 點選取消
            function btnCancelClick() {
                $("#divResult").html("");
            }

            function QueryData(){
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
                            $("#querystring").val($("#frmQuery").serialize());
                            $.unblockUI();
                        }
                    });
                }
                return false;
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
        },
        changeCaseKind: function () {
            var casekind = $("#ddlCaseKind option:selected").val();
            if (casekind.trim().length == 0) {
                $("#ddlCaseKind2").empty();
                $("#ddlCaseKind2").append($("<option></option>").val('').text($("#PleaseSelect").val()));
            }
            else if (casekind == $("#CaseSeizure").val()) {
                $("#ddlCaseKind2").empty();
                $("#ddlCaseKind2").append($("<option></option>").val('').text($("#PleaseSelect").val()));
                $("#ddlCaseKind2").append($("<option></option>").val($("#Seizure").val()).text($("#Seizure").val()));
                $("#ddlCaseKind2").append($("<option></option>").val($("#Pay").val()).text($("#Pay").val()));
                $("#ddlCaseKind2").append($("<option></option>").val($("#Repeal").val()).text($("#Repeal").val()));
                $("#ddlCaseKind2").append($("<option></option>").val($("#SeizureandPay").val()).text($("#SeizureandPay").val()));
            }
            else {
                $("#ddlCaseKind2").empty();
                $("#ddlCaseKind2").append($("<option></option>").val('').text($("#PleaseSelect").val()));
            }
            //var selectedValue = $("#ddlCaseKind option:selected").val();
            //if (selectedValue === "") {
            //    $("#ddlCaseKind2").attr("disabled", "true");
            //    $("#ddlCaseKind2").empty();
            //    $("#ddlCaseKind2").append($("<option></option>").val("").text($("#PleaseSelect").val()));
            //} else {
            //    if ($.trim(selectedValue).length > 0) {
            //        $.ajax({
            //            type: "POST",
            //            async: false,
            //            url: $("#GetCaseKind2Url").val(),
            //            data: { caseKind: selectedValue },
            //            success: function (data) {
            //                if (data.length > 0) {
            //                    $("#ddlCaseKind2").removeAttr("disabled");
            //                    $("#ddlCaseKind2").empty();
            //                    $("#ddlCaseKind2").append($("<option></option>").val('').text($("#PleaseSelect").val()));
            //                    $.each(data, function (i, item) {
            //                        $("#ddlCaseKind2").append($("<option></option>").val(item.Value).text(item.Value));
            //                    });
            //                } else {
            //                    $("#ddlCaseKind2").attr("disabled", "true");
            //                    $("#ddlCaseKind2").empty();
            //                    $("#ddlCaseKind2").append($("<option></option>").val('').text($("#PleaseSelect").val()));
            //                }
            //            }
            //        });
            //    }
            //}
        },
        changeGovKind:function() {
            var selectedValue = $("#ddlGOV_KIND option:selected").val();
            if (selectedValue === "") {
                $("#ddlGovUnit").attr("disabled", "true");
                $("#ddlGovUnit").empty();
                $("#ddlGovUnit").append($("<option></option>").val("").text($("#PleaseSelect").val()));
            } else {
                if ($.trim(selectedValue).length > 0) {
                $.ajax({
                    type: "POST",
                    async: false,
                    url: $("#GetGovUnitUrl").val(),
                    data: { govKind: selectedValue },
                    success: function (data) {
                        if (data.length > 0) {
                            $("#ddlGovUnit").removeAttr("disabled");
                            $("#ddlGovUnit").empty();
                            //$("#ddlGovUnit").append($("<option></option>").val('').text('--請選擇--'));
                            $.each(data, function (i, item) {
                                $("#ddlGovUnit").append($("<option></option>").val(item.Value).text(item.Value));
                            });
                        } else {
                            $("#ddlGovUnit").attr("disabled", "true");
                            $("#ddlGovUnit").empty();
                            $("#ddlGovUnit").append($("<option></option>").val('').text($("#PleaseSelect").val()));
                        }
                    }
                   });
                }
            }
        },
        btnReportSelected: function () {
            var actionUrl = $("#SeizureQueryExport").val();;
            $("#frmForReport").attr("src", actionUrl);
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.SeizureQuery.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}