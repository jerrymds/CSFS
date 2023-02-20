//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var caseSendUpLoad = $.CaseSendUpLoad = {};
    jQuery.extend(caseSendUpLoad, {
        documentReady: function () {
            //* init
            if ($("#NowPage").val() === "CaseSendUpLoadQuery") {
                $.CaseSendUpLoad.initQuery();
            }
        },

        //* 進入後初始化查詢畫面
        initQuery: function () {
            $("#btnUpLoad").click(function () { return btnUpLoadclick(); });
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add start
 
            //* 驗證
   
            //上傳
            function btnUpLoadclick()
            {                
                /*
                $(".checkfile:checked").each(function () {
                    aryCaseId.push($(this).val());//向数组中添加元素  
                });
                var strCaseId = aryCaseId.join(',');
                */
                var strCaseId = document.getElementById("CaseNo").value;
                if (strCaseId.length <= 0) {
                    jAlertError($("#SelectOneMsg").val());
                    return false;
                }

                jConfirm($.validator.format("是否確定重傳批號:" + strCaseId, strCaseId.toString()), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#UploadUrl").val(),
                            async: false,
                            data: { CaseIdarr: strCaseId },
                            error: function () {
                                jAlertError("重傳失敗!!");
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "2") {
                                    jAlertSuccess("重傳成功", function () {
                                    $.unblockUI();
                                    btnQueryclick();
                                    });
                                } else if (data.ReturnCode === "0") {
                                    jAlertError("系統異常未執行重傳");
                                    $.unblockUI();
                                    btnQueryclick();
                                } else if (data.ReturnCode === "1")
                                {
                                    jAlertError("重傳時系統發生錯誤");
                                    $.unblockUI();
                                    btnQueryclick();
                                } else if (data.ReturnCode === "3")
                                {
                                    jAlertError("無此批號");
                                    $.unblockUI();
                                    btnQueryclick();
                                }
                                
                            }
                        });
                    }
                });
                                   
            }

        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.CaseSendUpLoad.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}