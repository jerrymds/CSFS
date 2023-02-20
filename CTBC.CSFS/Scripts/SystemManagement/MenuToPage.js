//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var menuToPage = $.MenuToPage = {};
    jQuery.extend(menuToPage, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "MenuToPageQuery") {
                $.MenuToPage.initMenuToPageQuery();
            }
            if ($("#NowPage").val() === "MenuToPageEdit") {
                $.MenuToPage.initMenuToPageEdit();
            }
        },

        //* 進入後初始化查詢畫面
        initMenuToPageQuery: function () {
            $("#btnSyncAuthZ").click(function () { return syncAuthZ(); });

            //* 同步
            function syncAuthZ() {
                $.blockUI();
                $.ajax({
                    url: $("#SyncAuthUrl").val(),
                    type: "Post",
                    cache: false,
                    dataType: "json",
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess($("#SyncOkMsg").val());
                            $.unblockUI();
                        } else {
                            jAlertError($("#SyncFailMsg").val());
                            $.unblockUI();
                        }
                    }
                });
            }

        },
        initMenuToPageEdit: function () {
            $("#btnSelectAll").click(function () { return checkAllRole(); });
            $("#btnSelectNone").click(function () { return unCheckAllRole(); });
            $("#btnSave").click(function () { return btnSaveClick(); });



            function checkAllRole() {
                //$(":checkbox").prop("checked", true);
                //* icheck寫法
                $(":checkbox").iCheck("check");
            }
            function unCheckAllRole() {
                //$(":checkbox").removeProp('checked');
                //* icheck寫法
                $(":checkbox").iCheck("uncheck");
            }

            function btnSaveClick() {
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
                            jAlertError(data.ReturnMsg);
                            $.unblockUI();
                        }
                    }
                });
                return false;
            }
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.MenuToPage.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}