//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var warningFraud = $.WarningFraud = {};
    jQuery.extend(warningFraud, {
        documentReady: function () {
            //* init
            if ($.CSFS.config.NowPage === "WarningFraud") {
                $.WarningFraud.initWarningFraud();
            }
            if ($.CSFS.config.NowPage === "CreateWarn") {
                $.WarningFraud.initWarningFraudCreate();
            }
            if ($.CSFS.config.NowPage == "EditWarn") {
                $.WarningFraud.initWarningFraudEdit();
            }
        },

        //* 進入後初始化查詢畫面
        initWarningFraud: function () {
            $("#btnQuery").click(function () { return $.WarningFraud.QueryData(); });
            $("#btnCancel").click(function () { return btnCancelClick(); });
            $("#btnExcel").click(function () { return ExcelClick(); });

            function ExcelClick() {
                $("#frmQuery").attr("action", $.CSFS.config.ExcelUrl);
                $("#frmQuery").submit();
            }

            //* 點選取消
            function btnCancelClick() {
                window.location.href = $.CSFS.config.IndexUrl;
            }

        },
        //* 初始化新增頁面
        initWarningFraudCreate: function () {
            $("#btnSave").click(function () {
                $("#frmEdit").submit();
            });
            $("#btnCancel").click(function () {
                parent.$.colorbox.close();
            });
            //註冊表單驗證
            $.WarningFraud.initFormValidate();
            //通報單位下拉選單預設
            $("#Unit").val("客服中心");
        },
        //* 初始化修改頁面
        initWarningFraudEdit: function () {
            $("#btnSave").click(function () {
                $("#frmEdit").submit();
            });
            $("#btnCancel").click(function () {
                parent.$.colorbox.close();
            });

            if ($.CSFS.config.HasAttach > 0) {
                $("#divUpload").hide();
                $("#divAttach").show();
            }
            //註冊表單驗證
            $.WarningFraud.initFormValidate();
            //通報單位下拉選單預設
            $("#Unit").val($("#hidUnit").val());
        },
        //* 初始化表單驗證
        initFormValidate: function () {
            //下拉選單必填檢核
            $.validator.addMethod("selRequired", function (val, element) {
                return $('select[name="' + element.name + '"] option:selected').val() != "";
            });
            //上傳檔案大小檢核
            $.validator.addMethod("chkFileSize", function (val, element) {
                if (element.files.length > 0) {
                    return element.files[0].size < $.CSFS.config.UploadMaxLength;
                }
                return true;
            });

            validRules = {
                "COL_165CASE": { required: true },
                "COL_ACCOUNT2": { required: true },
                "Unit": { selRequired: true },
                "COL_POLICE": { required: true },
                "CaseCreator": { required: true },
                "CreatedDate": { required: true },
                "attachFile": { chkFileSize: true }
            }

            rulesMessage = {
                "COL_165CASE": { required: "165案號不可空白" },
                "COL_ACCOUNT2": { required: "被聯防帳號不可空白" },
                "Unit": { selRequired: "通報單位不可空白" },
                "COL_POLICE": { required: "警局不可空白" },
                "CaseCreator": { required: "通報人員不可空白" },
                "CreatedDate": { required: "鍵檔日期不可空白" },
                "attachFile": { chkFileSize: "上傳檔案大小超過設定值 " + $.CSFS.formatFileSize($.CSFS.config.UploadMaxLength) }
            }

            //指定 url
            var url = $.CSFS.config.CreateWarnUrl;
            if ($.CSFS.config.NowPage == "EditWarn") {
                url = $.CSFS.config.EditWarnUrl;
            }

            function saveHandler() {
                $.blockUI();
                var handler = undefined;
                var formData = new FormData()
                $.each($("#frmEdit")[0], function () {
                    if ($(this).length > 0) {
                        var item = $(this)[0];
                        if (item.tagName == "INPUT") {
                            if (item.type == "text" || item.type == "hidden") {
                                formData.append(item.name, item.value);
                            }
                            else if (item.type == "file") {
                                var files = $(item).get(0).files;
                                if (files.length > 0) {
                                    formData.append(item.name, files[0]);
                                }
                            }
                        }
                        if (item.tagName == "SELECT") {
                            formData.append(item.name, $(item).find("option:selected").text());
                        }
                    }
                });

                $.ajax({
                    url: url,
                    type: "Post",
                    dataType: "json",
                    data: formData,
                    contentType: false,
                    processData: false,
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (result) {
                        if (result.ReturnCode == "1") handler = function () {
                            parent.$.colorbox.close();
                            parent.$.WarningFraud.QueryData();
                        };

                        $.CSFS.resultHandler(result, handler);
                    }
                }).done(function () {
                    $.unblockUI();
                });
            }

            //註冊表單驗證
            $.CSFS.formValid("frmEdit", validRules, rulesMessage, saveHandler);
        },
        //* 查詢資料
        QueryData: function () {
            var msg = "";
            var newLine = "</br>";
            if ($("#CreateDateS").val() == "") {
                msg += "鍵檔日期起不可空白" + newLine;
            }
            if ($("#CreateDateE").val() == "") {
                msg += "鍵檔日期迄不可空白" + newLine;
            }
            if ($.CSFS.diffDays($("#CreateDateE").val(), $("#CreateDateS").val()) > $.CSFS.config.CheckDays) {
                msg += $.CSFS.msgLang.DateRangeMsg + $.CSFS.config.CheckDays + '天' + newLine;
            }

            //* 有必填檢核錯誤
            if (msg.length > 0) {
                jAlertError(msg);
                return false;
            }

            $("#divResult").html("");
            //trimAllInput();

            $("#frmQuery").attr("action", $.CSFS.config.QueryUrl);
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
                    if (data.ReturnCode) {
                        $.CSFS.resultHandler(data);
                    }
                    else {
                        $("#divResult").html(data).show();
                    }
                }
            }).done(function () {
                $.unblockUI();
            });
            return false;
        },
        //* 刪除資料
        DeleteData: function (no) {
            jConfirm($.CSFS.msgLang.ConfirmDelete, "刪除聯防案件", DoDelete);

            function DoDelete(res) {
                if (res == false) return;
                var formData = new FormData();
                formData.append("No", no);
                $.blockUI();
                $.ajax({
                    url: $.CSFS.config.DelUrl,
                    type: "POST",
                    data: formData,
                    dataType: "json",
                    contentType: false,
                    processData: false,
                    error: function (err) {
                        jAlertError($("#LoadErrorMsg").val());
                    },
                    success: function (result) {
                        $.CSFS.resultHandler(result, function () {
                            $.WarningFraud.QueryData();
                        });
                    }
                }).done(function () {
                    $.unblockUI();
                });
            }
        },
        //* 下載
        Download: function (id) {
            var actionUrl = $.CSFS.config.DownloadUrl + "?warningFraudNo=" + id;
            $("#frmQuery").attr("action", actionUrl);
            $("#frmQuery").submit();
        },
        //* 刪除附件
        DelAttach: function (attachmentId) {
            jConfirm($.CSFS.msgLang.ConfirmDelete, "刪除附件", DoDelFile);

            function DoDelFile(res) {
                if (res == false) return;
                var formData = new FormData();
                formData.append("attachmentId", attachmentId);
                $.blockUI();
                $.ajax({
                    url: $.CSFS.config.DelAttchUrl,
                    type: "POST",
                    data: formData,
                    contentType: false,
                    processData: false,
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (result) {
                        $.CSFS.resultHandler(result, function () {
                            parent.$.colorbox.close();
                            parent.$.WarningFraud.QueryData();
                        });
                    }
                }).done(function () {
                    $.unblockUI();
                });
            }
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.WarningFraud.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

