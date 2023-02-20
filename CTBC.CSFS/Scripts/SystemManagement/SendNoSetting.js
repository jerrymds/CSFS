//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var menuMaintenance = $.MenuMaintenance = {};
    jQuery.extend(menuMaintenance, {
        documentReady: function () {
            if ($("#NowPage").val() === "NowQuery") {
                $.MenuMaintenance.initMenuQuery();
            }
            if ($("#NowPage").val() === "NowCreate") {
                $.MenuMaintenance.initMenuCreate();
            }
            if ($("#NowPage").val() === "NowEdit") {
                $.MenuMaintenance.initMenuEdit();
            }
        },

        //* 進入後初始化查詢畫面
        initMenuQuery: function () {
            $("#frmQuery").submit(function () { return $.MenuMaintenance.QueryData(); });
            $("#btnQuery").click(function () { return $.MenuMaintenance.QueryData(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });
            //* 點選取消
            function btnCancelClick() {
                $("#divResult").html("");
            }
        },
        QueryData: function () {
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
                }
            });
            return false;
        },
        bindGrid: function () {
            $("a[data-deleteLink='true']").click(function () {
                var traget = $(this).attr("data-href");
                jConfirm($("#DeleteConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag == true) {
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
                                    jAlertSuccess($("#DeleteSuccessMsg").val(), function () { $.MenuMaintenance.QueryData(); });
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
        },
        //* 初始化新增畫面
        initMenuCreate: function () {
            $("#frmCreate").submit(function () { return btnSaveClick(); });
            $("#btnSave").click(function () { return btnSaveClick(); });

            function btnSaveClick() {
                trimAllInput();
                if (!ajaxValidate()) {
                    return false;
                }

                $.blockUI();
                $.ajax({
                    url: $("#frmCreate").attr("action"),
                    type: "Post",
                    cache: false,
                    data: $("#frmCreate").serialize(),
                    dataType: "json",
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess($("#CreateSuccessMsg").val(), function () { location.href = $("#CancelUrl").val(); });
                        } else {
                            jAlertError($("#CreateFailMsg").val());
                            $.unblockUI();
                        }

                    }
                });
                return false;
            }

            function ajaxValidate() {
                var NewLine = "<br/>";
                var Msg = "";
                //*20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update start
                //var Filter = /^\d{14}$/;
                var Filter = /^\d{15}$/;
                //*20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update end
                var Filter2 = /^([0-9]+)$/;
                if (!Filter2.test($("#txtSendNoYear").val())) {
                    Msg = Msg + $.validator.format($("#YearNotNull").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if (!Filter.test($("#txtSendNoStart").val())) {
                    Msg = Msg + $.validator.format($("#StartNotNull").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if (!Filter.test($("#txtSendNoEnd").val())) {
                    Msg = Msg + $.validator.format($("#EndNotNull").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if (!Filter.test($("#txtSendNoNow").val())) {
                    Msg = Msg + $.validator.format($("#NowNotNull").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if ($("#txtSendNoEnd").val() < $("#txtSendNoStart").val()) {
                    Msg = Msg + $.validator.format($("#EndThanStart").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if ($("#txtSendNoNow").val() < $("#txtSendNoStart").val()) {
                    Msg = Msg + $.validator.format($("#NowThanStart").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if (Msg.length > 0) {
                    jAlertError(Msg);
                    return false;
                }
                return true;
            }
        },
        //*
        initMenuEdit: function () {
            $("#frmEdit").submit(function () { return btnSaveClick(); });
            $("#btnSave").click(function () { return btnSaveClick(); });

            function btnSaveClick() {
                trimAllInput();
                if (!ajaxValidate()) {
                    return false;
                }
                $.blockUI();
                $.ajax({
                    url: $("#frmEdit").attr("action"),
                    type: "Post",
                    cache: false,
                    data: $("#frmEdit").serialize(),
                    dataType: "json",
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess($("#EditSuccessMsg").val(), function () { location.href = $("#CancelUrl").val(); });
                        } else {
                            jAlertError($("#EditFailMsg").val());
                            $.unblockUI();
                        }

                    }
                });
                return false;
            }

            function ajaxValidate() {
                var NewLine = "<br/>";
                var Msg = "";
                //*20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update start
                //var Filter = /^\d{14}$/;
                var Filter = /^\d{15}$/;
                //*20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update end
                var Filter2 = /^([0-9]+)$/;
                if (!Filter2.test($("#txtSendNoYear").val())) {
                    Msg = Msg + $.validator.format($("#YearNotNull").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if (!Filter.test($("#txtSendNoStart").val())) {
                    Msg = Msg + $.validator.format($("#StartNotNull").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if (!Filter.test($("#txtSendNoEnd").val())) {
                    Msg = Msg + $.validator.format($("#EndNotNull").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if (!Filter.test($("#txtSendNoNow").val())) {
                    Msg = Msg + $.validator.format($("#NowNotNull").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if ($("#txtSendNoEnd").val() < $("#txtSendNoStart").val()) {
                    Msg = Msg + $.validator.format($("#EndThanStart").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if ($("#txtSendNoNow").val() < $("#txtSendNoStart").val()) {
                    Msg = Msg + $.validator.format($("#NowThanStart").val(), $("#TextIsNumber").val()) + NewLine;
                }
                if (Msg.length > 0) {
                    jAlertError(Msg);
                    return false;
                }
                return true;
            }
        }

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.MenuMaintenance.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}