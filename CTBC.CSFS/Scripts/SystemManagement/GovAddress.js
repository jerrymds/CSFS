//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var govAddress = $.GovAddress = {};
    jQuery.extend(govAddress, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "GovAddressQuery") {
                $.GovAddress.initGovAddressQuery();
            }
            if ($("#NowPage").val() === "GovAddressCreate") {
                $.GovAddress.initGovAddressCreate();
            }
            if ($("#NowPage").val() === "GovAddressEdit") {
                $.GovAddress.initGovAddressEdit();
            }
        },

        //* 進入後初始化查詢畫面
        initGovAddressQuery: function () {
            $("#btnQuery").click(function () { return $.GovAddress.QueryData(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });
            //* 點選取消
            function btnCancelClick() {
                $("#divResult").html("");
                $("#TITLE").val("");
                $("#md_FuncID").val("");
            }
        },

        initGovAddressCreate:function (){
            $("#btnSave").click(function () { return CreateGovAddress(); })
            //*點擊儲存
            function CreateGovAddress() {
                var newLine = "<br/>";
                var msg = "";
                if ($("#ddlkindCreate").val().length <= 0) {
                    msg = msg + $("#NameddlkindCreate").val() + newLine;
                }
                if ($("#txtGovName").val().length <= 0) {
                    msg = msg + $("#NametxtGovName").val() + newLine;
                }
                if ($("#txtGovAddr").val().length <= 0) {
                    msg = msg + $("#NametxtGovAddr").val() + newLine;
                }
                if ($("#ddlIsEnable").val().length <= 0) {
                    msg = msg + $("#NameddlIsEnable").val() + newLine;
                }
                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
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
        },

        initGovAddressEdit:function(){
            $("#btnSaveEdit").click(function () { return btnEditClick();})
            function btnEditClick()
            {
                var newLine = "<br/>";
                var msg = "";
                if ($("#ddlkindEdit").val().length <= 0) {
                    msg = msg + $("#NameddlkindCreate").val() + newLine;
                }
                if ($("#txtGovNameEdit").val().length <= 0) {
                    msg = msg + $("#NametxtGovName").val() + newLine;
                }
                if ($("#txtGovAddrEdit").val().length <= 0) {
                    msg = msg + $("#NametxtGovAddr").val() + newLine;
                }
                if ($("#ddlIsEnableEdit").val().length <= 0) {
                    msg = msg + $("#NameddlIsEnable").val() + newLine;
                }
                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
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
                    $.CSFS.bindFancybox();
                    $("#querystring").val($("#frmQuery").serialize());
                }
            });
            return false;
        },

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.GovAddress.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}