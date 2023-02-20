//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var CheckNoSetting = $.CheckNoSetting = {};
    jQuery.extend(CheckNoSetting, {
        documentReady: function () {
            if ($("#NowPage").val() === "CheckNoSettingQuery") {
                $.CheckNoSetting.initCheckNoSettingQuery();
            }
            if ($("#NowPage").val() === "CheckNoSettingCreate") {
                $.CheckNoSetting.initCheckNoSettingCreate();
            }
            if ($("#NowPage").val() === "CheckNoSettingEdit") {
                $.CheckNoSetting.initCheckNoSettingEdit();
            }
            if ($("#NowPage").val() === "CheckNoSettingDetail") {
                $.CheckNoSetting.initCheckNoSettingDetail();
            }
        },

        //* 進入後初始化查詢畫面
        initCheckNoSettingQuery: function () {
            $("#frmQuery").submit(function () { return $.CheckNoSetting.QueryData(); });
            $("#btnQuery").click(function () { return $.CheckNoSetting.QueryData(); });
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
                                    jAlertSuccess($("#DeleteSuccessMsg").val(), function () { $.CheckNoSetting.QueryData(); });
                                } else {
                                    jAlertError($("#DeleteFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
                return false;
            });
            $("a[data-activeLink='true']").click(function () {
                var traget = $(this).attr("data-href");
                jConfirm($("#ActiveConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
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
                                    jAlertSuccess($("#ActiveSuccessMsg").val(), function () { $.CheckNoSetting.QueryData(); });
                                } else {
                                    jAlertError($("#ActiveFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
                return false;
            });
        },
        //* 初始化新增畫面
        initCheckNoSettingCreate: function () {
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
                var Filter = /^\d+$/;
                var Filter3 = /^([0-9]+)$/;
                var Filter2 = /^([A-Z]+)$/;
                if (!Filter.test($("#txtCheckNoStart").val())) {
                    Msg = Msg + $("#CheckNoStartName").val() + $("#CheckLength").val() + NewLine;
                }
                if (!Filter.test($("#txtCheckNoEnd").val())) {
                    Msg = Msg + $("#CheckNoEndName").val() + $("#CheckLength").val() + NewLine;
                }
                if (!Filter3.test($("#txtWeekTempAmount").val())) {
                    Msg = Msg + $("#WeekTempAmount").val() + $("#CheckLength").val() + NewLine;
                }
                if (parseInt($("#txtCheckNoEnd").val()) < parseInt($("#txtCheckNoStart").val())) {
                    Msg = Msg + $("#EndThanStart").val() + NewLine;
                }
                if (parseInt($("#txtWeekTempAmount").val()) > (parseInt($("#txtCheckNoEnd").val()) - parseInt($("#txtCheckNoStart").val()))) {
                    Msg = Msg + $("#WeekTempAmount").val() + $("#ErrMsg").val() + NewLine;
                }
                if (parseInt($("#txtCheckNoEnd").val()) - parseInt($("#txtCheckNoStart").val()) > 5000) {
                    Msg = Msg + $("#CheckNoTooMany").val() + NewLine;
                }
                if (Msg.length > 0) {
                    jAlertError(Msg);
                    return false;
                }
                return true;
            }
        },
        //*
        initCheckNoSettingEdit: function () {
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
                var Filter = /^\d+$/;
                var Filter3 = /^([0-9]+)$/;
                var Filter2 = /^([A-Z]+)$/;
                if (!Filter.test($("#txtCheckNoStart").val())) {
                    Msg = Msg +  $("#CheckNoStartName").val() + $("#CheckLength").val() + NewLine;
                }
                if (!Filter.test($("#txtCheckNoEnd").val())) {
                    Msg = Msg + $("#CheckNoEndName").val() + $("#CheckLength").val() + NewLine;
                }
                if (!Filter3.test($("#txtWeekTempAmount").val())) {
                    Msg = Msg + $("#WeekTempAmount").val() + $("#CheckLength").val() + NewLine;
                }
                if (parseInt($("#txtCheckNoEnd").val()) < parseInt($("#txtCheckNoStart").val())) {
                    Msg = Msg + $("#EndThanStart").val() + NewLine;
                }
                if (parseInt($("#txtWeekTempAmount").val()) > (parseInt($("#txtCheckNoEnd").val()) - parseInt($("#txtCheckNoStart").val()))) {
                    Msg = Msg + $("#WeekTempAmount").val() + $("#ErrMsg").val() + NewLine;
                }
                if (parseInt($("#txtCheckNoEnd").val()) - parseInt($("#txtCheckNoStart").val()) > 5000) {
                    Msg = Msg + $("#CheckNoTooMany").val() + NewLine;
                }
                if (Msg.length > 0) {
                    jAlertError(Msg);
                    return false;
                }
                return true;
            }
        },
        initCheckNoSettingDetail: function () {
            $.CheckNoSetting.QueryDetail();
            $("#btnQueryDetail").click(function () { return $.CheckNoSetting.QueryDetail(); });
            $("#btnCancelDetail").click(function () { return btnCancelDetailClick(); });
            //* 點選取消
            function btnCancelDetailClick() {
                $("#divResult").html("");
            }

        },
        QueryDetail: function () {
            $("#divResult").html("");
            $.blockUI();
            $.ajax({
                url: $("#divResult").attr("data-target-url"),
                type: "Post",
                cache: false,
                data: $("#frmDetail").serialize(),
                error: function () {
                    jAlertError($("#LoadErrorMsg").val());
                    $.unblockUI();
                },
                success: function (data) {
                    $("#divResult").html(data).show();
                    $.unblockUI();
                    $("#querystring").val($("#frmDetail").serialize());
                }
            });
            return false;
        },
        bindDetail: function () {
            $("a[data-payLink='true']").click(function () {
                var traget = $(this).attr("data-href");
                jConfirm($("#SetConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
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
                                    jAlertSuccess($("#SetSuccessMsg").val(), function () { $.CheckNoSetting.QueryDetail(); });
                                } else {
                                    jAlertError($("#SetFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
                return false;
            });
            $("a[data-invalidLink='true']").click(function () {
                var traget = $(this).attr("data-href");
                jConfirm($("#SetConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
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
                                    jAlertSuccess($("#SetSuccessMsg").val(), function () { $.CheckNoSetting.QueryDetail(); });
                                } else {
                                    jAlertError($("#SetFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
                return false;
            });
            $("a[data-othersLink='true']").click(function () {
                var traget = $(this).attr("data-href");
                jConfirm($("#SetConfirmMsg").val(), $("#j_confirm_header").val(), function (bFlag) {
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
                                    jAlertSuccess($("#SetSuccessMsg").val(), function () { $.CheckNoSetting.QueryDetail(); });
                                } else {
                                    jAlertError($("#SetFailMsg").val());
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });
                return false;
            });
        }

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.CheckNoSetting.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}