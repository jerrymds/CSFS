//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var menuMaintenance = $.MenuMaintenance = {};
    jQuery.extend(menuMaintenance, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "MenuQuery") {
                $.MenuMaintenance.initMenuQuery();
            }
            if ($("#NowPage").val() === "MenuCreate") {
                $.MenuMaintenance.initMenuCreate();
            }
            if ($("#NowPage").val() === "MenuEdit") {
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
                $("#TITLE").val("");
                $("#md_FuncID").val("");
            }
        },
        QueryData:function() {
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
                    //$.custPagination.BindCheckBox();
                    //$.custPagination.sort($("#divResult"));
                }
            });
            return false;
        },
        bindGrid: function () {
            $("a[data-deleteLink='true']").click(function() {
                var traget = $(this).attr("data-href");
                jConfirm($("#DeleteConfirmMsg").val(), $("#j_confirm_header").val(), function(bFlag) {
                    if (bFlag == true) {
                        //* click confirm ok
                        $.blockUI();
                        $.ajax({
                            type: "Post",
                            url: traget,
                            dataType: "json",
                            error: function() {
                                jAlertError($("#LoadErrorMsg").val());
                                $.unblockUI();
                            },
                            success: function(data) {
                                if (data.ReturnCode === "1") {
                                    jAlertSuccess($("#DeleteSuccessMsg").val(), function () { $.MenuMaintenance.QueryData()(); });
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
        initMenuCreate: function() {
            $("#frmCreate").submit(function () { return btnSaveClick(); });
            $("#btnSave").click(function () { return btnSaveClick(); });
            $("#MenuType").change(function() { upperCaseMenuType() });
            
            function btnSaveClick() {
                upperCaseMenuType();
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
                if ($.trim($("#MenuType").val()) === "") {
                    jAlertError($("#MenuTypeRequire").val());
                    return false;
                }
                var menuType = $.trim($("#MenuType").val().toUpperCase());
                if (menuType !== "M" && menuType !== "P" && menuType !== "C" && menuType !== "A") {
                    jAlertError($("#MenuTypeNotCorrected").val());
                    return false;
                }
                if ($.trim($("#TITLE").val()) === "") {
                    jAlertError($("#TittleRequire").val());
                    return false;
                }
                if ($.trim($("#MenuType").val()) !== "P") {
                    if ($.trim($("#MenuLevel").val()) === "") {
                        jAlertError($("#MenuLevelRequire").val());
                        return false;
                    }
                    if ($.trim($("#Parent").val()) === "") {
                        jAlertError($("#ParentRequire").val());
                        return false;
                    }
                    if ($.trim($("#md_FuncID").val()) === "") {
                        jAlertError($("#FuncIdRequire").val());
                        return false;
                    }
                    if ($.trim($("#MenuType").val()) === "M") {
                        if ($.trim($("#MenuSort").val()) === "") {
                            jAlertError($("#MenuSortReuqire").val());
                            return false;
                        }
                    }
                    if (!/^[0-9]{0,3}$/i.test($.trim($("#MenuLevel").val()))) {
                        jAlertError($("#MenuLevelMax3Int").val());
                        return false;
                    }
                    if (!/^[0-9]{0,9}$/i.test($.trim($("#Parent").val()))) {
                        jAlertError($("#ParentMustInt").val());
                        return false;
                    }
                    if (!/^[0-9]{0,3}$/i.test($.trim($("#MenuSort").val()))) {
                        jAlertError($("#MenuSortMax3Int").val());
                        return false;
                    }
                }
                return true;
            }

            function upperCaseMenuType() {
                $("#MenuType").val($.trim($("#MenuType").val().toUpperCase()));
            }
        },
        //*
        initMenuEdit: function () {
            $("#frmEdit").submit(function () { return btnSaveClick(); });
            $("#btnSave").click(function () { return btnSaveClick(); });
            $("#MenuType").change(function () { upperCaseMenuType() });

            function btnSaveClick() {
                upperCaseMenuType();
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
                if ($.trim($("#MenuType").val()) === "") {
                    jAlertError($("#MenuTypeRequire").val());
                    return false;
                }
                var menuType = $.trim($("#MenuType").val().toUpperCase());
                if (menuType !== "M" && menuType !== "P" && menuType !== "C" && menuType !== "A") {
                    jAlertError($("#MenuTypeNotCorrected").val());
                    return false;
                }
                if ($.trim($("#TITLE").val()) === "") {
                    jAlertError($("#TittleRequire").val());
                    return false;
                }
                if ($.trim($("#MenuType").val()) !== "P") {
                    if ($.trim($("#MenuLevel").val()) === "") {
                        jAlertError($("#MenuLevelRequire").val());
                        return false;
                    }
                    if ($.trim($("#Parent").val()) === "") {
                        jAlertError($("#ParentRequire").val());
                        return false;
                    }
                    if ($.trim($("#md_FuncID").val()) === "") {
                        jAlertError($("#FuncIdRequire").val());
                        return false;
                    }
                    if ($.trim($("#MenuType").val()) === "M") {
                        if ($.trim($("#MenuSort").val()) === "") {
                            jAlertError($("#MenuSortReuqire").val());
                            return false;
                        }
                    }
                    if (!/^[0-9]{0,3}$/i.test($.trim($("#MenuLevel").val()))) {
                        jAlertError($("#MenuLevelMax3Int").val());
                        return false;
                    }
                    if (!/^[0-9]{0,9}$/i.test($.trim($("#Parent").val()))) {
                        jAlertError($("#ParentMustInt").val());
                        return false;
                    }
                    if (!/^[0-9]{0,3}$/i.test($.trim($("#MenuSort").val()))) {
                        jAlertError($("#MenuSortMax3Int").val());
                        return false;
                    }
                }
                return true;
            }

            function upperCaseMenuType() {
                $("#MenuType").val($.trim($("#MenuType").val().toUpperCase()));
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