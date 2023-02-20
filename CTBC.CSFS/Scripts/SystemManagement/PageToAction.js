//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var pageToAction = $.PageToAction = {};
    jQuery.extend(pageToAction, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "PageToActionQuery") {
                $.PageToAction.initPageToActionQuery();
            }
            if ($("#NowPage").val() === "PageToActionEdit") {
                $.PageToAction.initPageToActionEdit();
            }
        },

        //* 進入後初始化查詢畫面
        initPageToActionQuery : function () {
            
        },
        initPageToActionEdit: function () {
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
                            jAlertError($("#EditFailMsg").val());
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
        $.PageToAction.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}