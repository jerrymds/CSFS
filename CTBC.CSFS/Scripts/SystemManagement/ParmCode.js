//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var parmCode = $.ParmCode = {};
    jQuery.extend(parmCode, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "ParmCodeQuery") {
                $.ParmCode.initParmCodeQuery();
            }
            if ($("#NowPage").val() === "ParmCodeCreate") {
                $.ParmCode.initParmCodeCreate();
            }
            if ($("#NowPage").val() === "ParmCodeEdit") {
                $.ParmCode.initParmCodeEdit();
            }
        },

        //* 進入後初始化查詢畫面
        initParmCodeQuery: function () {
            $("#ddlCodeType").click(function () { return ChangeCode(); })
            $("#ddlCode").attr("disabled", "disabled");
            $("#ddlCode").click(function () { $("#hidCodeNo").val($("ddlCode").attr("selected", "selected").val()); })
            $("#btnQuery").click(function () { return $.ParmCode.QueryData(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });
            $("#btnExcel").click(function () { return ExcelClick();})

            // 參數類型下拉列表聯動
            function ChangeCode() {
                var codeType = $("#ddlCodeType").attr("selected", "selected").val();
                $("#hidCodeType").val(codeType);
                if (codeType != "") {
                    $("#ddlCode").removeAttr("disabled", "disabled");
                    $.ajax({
                        type: "post",
                        url: $("#BindCodeUrl").val(),
                        data: { codeType: codeType },
                        dataType: "json",
                        success: function (data) {
                            $("#ddlCode").html("");
                            $("#ddlCode").append("<option value='' selected='selected'>" + $("#SelectOneMsg").val() + "</option>");
                            $.each(data, function (index, item) {
                                $("#ddlCode").append("<option value='" + item.CodeNo + "'>" + item.CodeDesc + "</option>");
                            });
                        }
                    });
                } else {
                    $("#ddlCode").attr("disabled", "disabled");
                }
            }

            function ExcelClick() {
                location.href = $("#ExcelParmCodeUrl").val() + "?CodeType=" + $("#ddlCodeType").val() + "&Code=" + $("#ddlCode").val() + "&Enable=" + $("#Enable").val();
            }

            //* 點選取消
            function btnCancelClick() {
                $("#divResult").html("");
            }
        },

        initParmCodeCreate: function () {
            $("#btnSave").click(function () { return CreateParmCode(); })
            $("#ddlCodeType").change(function () { return TblrIsShow(); })//*點擊參數類別名稱
            $("#txtCodeType").blur(function () { $("#hidCodeType").val($("txtCodeType").val()); })

            //*點擊儲存
            function CreateParmCode() {
                var newLine = "<br/>";
                var msg = "";
                if ($("#ddlCodeType").val().length <= 0) {
                    msg = msg + $("#NameddlCodeTypeCreate").val() + newLine;
                }
                if ($("#txtCodeType").val().length <= 0) {
                    msg = msg + $("#NametxtCodeType").val() + newLine;
                }
                if ($("#CodeTypeDesc").val().length <= 0) {
                    msg = msg + $("#NameCodeTypeDesc").val() + newLine;
                }
                if ($("#CodeNo").val().length <= 0) {
                    msg = msg + $("#NameCodeNo").val() + newLine;
                }
                if ($("#CodeDesc").val().length <= 0) {
                    msg = msg + $("#NameCodeDesc").val() + newLine;
                }
                if ($("#SortOrder").val().length <= 0) {
                    msg = msg + $("#NameSortOrder").val() + newLine;
                }else if (isNaN($("#SortOrder").val()))
                {
                    msg = msg + $("#NameSortOrderNAV").val() + newLine;
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
                        if (data.ReturnCode === "1") {//新增成功
                            jAlertSuccess($("#CreateSuccessMsg").val(), function () { location.href = $("#CancelUrl").val(); });
                        } else if (data.ReturnCode === "2") {//代碼重複
                            jAlertError($("#CountFailMsg").val());
                            $.unblockUI();
                        } else {
                            jAlertError($("#CreateFailMsg").val());
                            $.unblockUI();
                        }

                    }
                });
                return false;
            }

            //點擊參數類別名稱
            function TblrIsShow() {
                var selectVal = $("#ddlCodeType").attr("selected", "selected").val();
                if (selectVal == "") {
                    $("#SortOrder").val("1");
                    $("#hidSortOrder").val("");
                    $("#txtCodeType").val("");
                    $("#CodeTypeDesc").val("");
                }
                else if (selectVal == "other") {
                    $("#SortOrder").val("1");
                    $("#hidSortOrder").val("");
                    $("#txtCodeType").val("");
                    $("#CodeTypeDesc").val("");
                }
                else {
                    $("#txtCodeType").val(selectVal);
                    $("#hidCodeType").val(selectVal);
                    $("#CodeTypeDesc").val($("#ddlCodeType").find("option:selected").text());
                    //獲取順序
                    $.ajax({
                        type: "post",
                        url: $("#GetSortOrderUrl").val(),
                        async: true,
                        data: "codeType=" + selectVal,
                        dataType: "html",
                        success: function (result) {
                            $("#SortOrder").val(result);
                        }
                    });
                }
                return true;
            }
        },

        initParmCodeEdit: function () {
            $("#btnSaveEdit").click(function () { return btnEditClick(); })
            function btnEditClick() {
                var newLine = "<br/>";
                var msg = "";
                if ($("#CodeNo").val().length <= 0) {
                    msg = msg + $("#NameCodeNo").val() + newLine;
                }
                if ($("#CodeDesc").val().length <= 0) {
                    msg = msg + $("#NameCodeDesc").val() + newLine;
                }
                if ($("#SortOrder").val().length <= 0) {
                    msg = msg + $("#NameSortOrder").val() + newLine;
                } else if (isNaN($("#SortOrder").val())) {
                    msg = msg + $("#NameSortOrderNAV").val() + newLine;
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
                            jAlertSuccess(data.ReturnMsg, function () { location.href = $("#CancelUrl").val(); });
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
        $.ParmCode.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}