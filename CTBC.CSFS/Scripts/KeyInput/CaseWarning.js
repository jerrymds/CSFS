//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var caseWarning = $.CaseWarning = {};
    jQuery.extend(caseWarning, {
        documentReady: function () {
            //* init
            $("#ddlKind").change(function () { ddlKindChange(); });
            if ($("#NowPage").val() === "CaseWarningQuery") {//*查詢
                $.CaseWarning.initCaseWarningQuery();
            }
            if ($("#NowPage").val() === "CaseWarningCreate") {//*新增警示通報
                $.CaseWarning.initCaseWarningCreate();
            }
            if ($("#NowPage").val() === "CaseWarningEdit") {//*編輯警示通報
                $.CaseWarning.initCaseWarningEdit();
            }
            if ($("#NowPage").val() === "CreateWarn") {//*新增案發內容
                $.CaseWarning.initCreateWarn();
            }
            if ($("#NowPage").val() === "EditWarn") {//*編輯案發內容
                $.CaseWarning.initEditWarn();
            }
            if ($("#NowPage").val() === "CopyWarn") {//*複製案發內容
                $.CaseWarning.initCopyWarn();
            }
            if ($("#NowPage").val() === "SetStatus") {//*設定警示狀態
                $.CaseWarning.initSetStatus();
            }
            function ddlKindChange() {
                var kind = $("#ddlKind").val();
                if (kind == "正本") {
                    $("#ddlOriginal").val("Y");
                }
            }
        },

        //* 進入後初始化查詢畫面
        initCaseWarningQuery: function () {
            $("#btnQuery").click(function () { return btnQueryClick(); });
            $("#btnChangeID").click(function () { return btnChangeIDClick(); });
            function PrefixInteger(num, length) {
                return ("0000000000000000" + num).substr(-length);
            }
            function ChkYN() {
                return ("0000000000000000" + num).substr(-length);
            }
            // 變更ID
            function btnChangeIDClick() {
                var msg = "";
                var _old = $("#CustIdOld").val();
                if (_old.length < 8 || _old.length > 12)  
                {
                   msg = "輸入ID有誤";
                }
                var _new = $("#CustIdNew").val();
                if (_new.length < 8 || _new.length > 12 ) {
                    msg = "輸入ID有誤";
                }

                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }


                jConfirm("是否確定", $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        $.blockUI();
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            url: $("#ChangeIDUrl").val(),
                            async: false,
                            data: { CustIdOld: _old, CustIdNew: _new },
                            error: function () {
                                jAlertError($("變更異常!").val());
                                $.unblockUI();
                            },
                            success: function (data) {
                                if (data.ReturnCode === "1") {
                                    alert("變更成功!");
                                    $.unblockUI();
                                    //btnQueryclick();
                                    //});
                                } else {
                                    jAlertError("變更失敗!");
                                    $.unblockUI();
                                }
                            }
                        });
                    }
                });

                //$.ajax({
                //    type: "POST",
                //    traditional: true,
                //    url: $("#ChangeIDUrl").val(),
                //    async: false,
                //    data: { CustIdOld: CustIdOld, CustIdNew: CustIdNew },
                //    error: function () {
                //        jAlertError("產生交易明細發生錯誤！");
                //        $.unblockUI();
                //    },
                //    success: function (data) {
                //        if (data.ReturnCode === "1") {
                //            //jAlertSuccess($("#ApproveOkMsg").val(), function () {
                //            $.unblockUI();
                //            btnQueryclick();
                //            //});
                //        } else {
                //            jAlertError(data.ReturnMsg);
                //            $.unblockUI();
                //        }
                //    }
                //});



            }
            //*查詢
            function btnQueryClick() {
                var msg = "";
                var element = $("#CustAccountQuery").val()[0];
                //console.log(element);
                var ddd = $("#CustAccountQuery").val();
                //console.log("ddd",ddd);
                if ((ddd.length < 12) && (element == "0") )
                {
                    $("#CustAccountQuery").val(PrefixInteger(ddd, 12));
                }
                var filter = /^\d{12}$/;
                if (!filter.test($("#CustAccountQuery").val())) {
                    msg = msg + $("#AccountNotNull").val();
                }
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }

                $("#frmQuery").submit();
                return true;
            }
        },

        initCaseWarningCreate: function () {
            $("#btnSave").click(function () { return btnSaveClick(); });
            //Add by zhangwei 20180315 start
            $("#btnGenDetail").click(function () { return btnGenDetail(); });
            $("#btnRequire").click(function () { return btnRequireClick(); });
            $("#ForCDateS").attr('disabled', true);
            $("#ForCDateE").attr('disabled', true);
            $("#btnGenDetail").attr('disabled', true);
            //Add by zhangwei 20180315 end
            $("#txtCustId").focus();

            function btnSaveClick() {
                var newLine = "<br/>";
                var msg = "";
                var re = /^([0-9.]+)$/;
                //if ($("#txtStatus").val().length <= 0) {
                //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameCustId").val()) + newLine;
                //}
                if ($("#txtCustId").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameCustId").val()) + newLine;
                }
                if ($("#txtCustAccount").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameCustAccount").val()) + newLine;
                } else if (!re.test($("#txtCustAccount").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameCustAccount").val()) + newLine;
                }
                if ($("#txtCustName").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameCustName").val()) + newLine;
                }
                if ($("#ddlBankID").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameBankID").val()) + newLine;
                }
                //else if (!re.test($("#txtBankID").val())) {
                //    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameBankID").val()) + newLine;
                //}


                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }
                $("#frmCreate").submit();
                return true;
            }
            //Add by zhangwei 20180315 start
            //產生交易明細
            function btnGenDetailClick()
            {
                var CustId = $("#txtCustId").val();
                var CustAccount = $("#txtCustAccount").val();
                var ForCDateS = $("#ForCDateS").val();
                var ForCDateE = $("#ForCDateE").val();
                $.ajax({
                    type: "POST",
                    traditional: true,
                    url: $("#GenDetailUrl").val(),
                    async: false,
                    data: { CustId: CustId, CustAccount: CustAccount, ForCDateS: ForCDateS, ForCDateE: ForCDateE },
                    error: function () {
                        jAlertError("產生交易明細發生錯誤！");
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            //jAlertSuccess($("#ApproveOkMsg").val(), function () {
                            $.unblockUI();
                            btnQueryclick();
                            //});
                        } else {
                            jAlertError(data.ReturnMsg);
                            $.unblockUI();
                        }
                    }
                });
            }
            //Add by zhangwei 20180315 end
            //Add by zhangwei 20180315 start
            //重新發查33401
            function btnRequireClick()
            {
                var CustAccount = $("#txtCustAccount").val();
                var Currency = $("#txtCurrency").val();
                var DocNo = $("#txtDocNo").val();
                $.ajax({
                    type: "POST",
                    url: $("#RequireUrl").val(),
                    async: false,
                    data: { CustAccount: CustAccount, Currency: Currency, DocNo: DocNo },
                    dataType: "json",
                    error: function () {
                        jAlertError("重新發查33401發生錯誤！");
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            $("#txtNotifyBal").val(data.NotifyBal);
                            $("#txtCustId").val(data.CustId);
                            $("#txtClosedDate").val(data.ClosedDate);
                            $("#txtCustName").val(data.CustName);
                            $("#ddlBankID").val(data.BankID);
                            $("#txtBankName").val(data.BankName);
                            $("#txtCurBal").val(data.CurBal);
                            $("#txtCurrency").val(data.Currency);
                            $("#txtVD").val(data.VD);
                            $("#txtMD").val(data.MD);
                        } else {
                            jAlertError(data.ErrorMsg);
                            $.unblockUI();
                        }
                    }
                });
            }
            //Add by zhangwei 20180315 end
        },

        initCaseWarningEdit: function () {
            $("#btnSaveEdit").click(function () { return btnSaveEditClick(); });
            $("#btnGenDetail").click(function () { return btnGenDetailClick(); });
            $("#btnRequire").click(function () { return btnRequireClick(); });
            $("#txtCustId").focus();
            function btnSaveEditClick() {
                var newLine = "<br/>";
                var msg = "";
                var re = /^([0-9.]+)$/;
                if ($("#txtCustId").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameCustId").val()) + newLine;
                }
                if ($("#txtCustAccount").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameCustAccount").val()) + newLine;
                } else if (!re.test($("#txtCustAccount").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameCustAccount").val()) + newLine;
                }
                if ($("#txtCustName").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameCustName").val()) + newLine;
                }
                if ($("#ddlBankID").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NameBankID").val()) + newLine;
                }
                //else if (!re.test($("#txtBankID").val())) {
                //    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameBankID").val()) + newLine;
                //}


                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }
                $("#frmEdit").submit();
                return true;
            }
            //Add by zhangwei 20180315 start
            //產生交易明細
            function btnGenDetailClick() {
                var DocNo = $("#txtDocNo").val();
                var CustId = $("#txtCustId").val();
                var CustAccount = $("#txtCustAccount").val();
                var ForCDateS = $("#ForCDateS").val();
                var ForCDateE = $("#ForCDateE").val();
                if (ForCDateS == "" || ForCDateE == "")
                {
                    alert("請輸入調閱日期起訖！");
                    return;
                }
                $.ajax({
                    type: "POST",
                    traditional: true,
                    url: $("#GenDetailUrl").val(),
                    async: false,
                    data: { DocNo: DocNo, CustId: CustId, CustAccount: CustAccount, ForCDateS: ForCDateS, ForCDateE: ForCDateE },
                    error: function () {
                        jAlertError("產生交易明細發生錯誤！");
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess("產生交易明細成功！", function () {
                                location.href = location.href
                            });
                            
                        } else {
                            jAlertError(data.ReturnMsg);
                            $.unblockUI();
                        }
                    }
                });
            }
            //Add by zhangwei 20180315 end
            //Add by zhangwei 20180315 start
            //重新發查33401
            function btnRequireClick() {
                var CustAccount = $("#txtCustAccount").val();
                var Currency = $("#txtCurrency").val();
                var DocNo = $("#txtDocNo").val();
                $.ajax({
                    type: "POST",
                    url: $("#RequireUrl").val(),
                    async: false,
                    data: { CustAccount: CustAccount, Currency: Currency, DocNo: DocNo },
                    dataType: "json",
                    error: function () {
                        jAlertError("重新發查33401發生錯誤！");
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            $("#txtDocNoD").val(data.DocNo);
                            $("#txtDocNo").val(data.DocNo);
                            $("#txtNotifyBal").val(data.NotifyBal);
                            $("#txtCustId").val(data.CustId);
                            $("#txtClosedDate").val(data.ClosedDate);
                            $("#txtCustName").val(data.CustName);
                            $("#ddlBankID").val(data.BankID);
                            $("#txtBankName").val(data.BankName);
                            $("#txtCurBal").val(data.CurBal);
                            $("#txtCurrency").val(data.Currency);
                        } else {
                            jAlertError(data.ErrorMsg);
                            $.unblockUI();
                        }
                    }
                });
            }
            //Add by zhangwei 20180315 end
            //* 刪除一筆附件
            $("a[data-deleteLink='true']").click(function () {
                var traget = $(this).attr("data-href");
                jConfirm($("#DeleteConfirm").val(), $("#j_confirm_header").val(), function (bFlag) {
                    if (bFlag === true) {
                        //* click confirm ok
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
                                    jAlertSuccess($("#DeleteSucMsg").val(), function () { location.href = location.href });
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

        initCreateWarn: function () {
            $("#btnSaveWarn").click(function() { $.CaseWarning.BtnSaveWarnClick(); });
            $("#btnCancel").click(function() {
                parent.$.colorbox.close();
            });
            $("#btnSetting").click(function () { return BtnSettingClick(); });
            $("#btnChenHe").click(function () { return BtnChenHeClick(); });
            $("#btnCancelOriginal").click(function () { return BtnCancelOriginalClick(); });
            $("#btnCancelOriginal").attr('disabled', true);//初始化時沖正按鈕不可用
            function BtnSettingClick()
            {
                $.blockUI();
                var DocNo = $("#txtDocNo").val();
                var strKind = $("#ddlKind").val();
                var Currency = $("#txtCurrency").val();
                var NotificationSource = $("#ddlNotificationSource").val();//通報來源
                var codeflag = "0";//0代表新增
                var Setdate = $("#txtSetdate").val();
                var Setdatetime = $("#txtSetdatetime").val();
                var ExtendDate = $("#txtExtendDate").val();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: $("#SettingUrl").val(),
                    data: { DocNo: DocNo, NotificationSource: NotificationSource, codeflag: codeflag, Setdate: Setdate, Setdatetime: Setdatetime, Currency: Currency, Kind: strKind, ExtendDate: ExtendDate},
                    dataType: "json",
                    error: function () {
                        jAlertError("設定發查9091發生錯誤！");
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            $("#txtOriginal").val(data.Original);
                            //$("#txtSetdate").val(data.EtabsDatetime);
                            //$("#txtSetdatetime").val(data.EtabsDatetimeHour);
                            $("#ErrorSpan").html('');
                            $("#btnSetting").attr('disabled', true);//設定按鈕不可用
                            $("#btnCancelOriginal").removeAttr("disabled");//沖正按鈕可用了
                        }
                        else
                        {
                            $("#txtOriginal").val(data.Original);
                            //$("#txtSetdate").val(data.EtabsDatetime);
                            //$("#txtSetdatetime").val(data.EtabsDatetimeHour);
                            $("#ErrorSpan").html(data.ErrorMsg);
                            $("#btnCancelOriginal").attr('disabled', true);//沖正按鈕不可用
                            $("#btnSetting").removeAttr("disabled");//設定按鈕可用了
                        }
                        $.unblockUI();
                    }
                });
            }
            //*呈核
            function BtnChenHeClick() {
                var newLine = "<br/>";
                var msg = "";
                var re = /^([0-9.]+)$/;
                var reDate = /^(\d{3})\/(0\d{1}|1[0-2])\/(0\d{1}|[12]\d{1}|3[01])$/;
                var reTime = /^(0\d{1}|1\d{1}|2[0-3]):([0-5]\d{1})$/;
                //* 案發日期
                if ($("#txtHappenDateTime").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtHappenDateTime").val()) + newLine;
                } else if (!reDate.test($("#txtHappenDateTime").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtHappenDateTime").val()) + newLine;
                }
                if ($("#txtHappenDateTimeHour").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtHappenDateTime").val()) + newLine;
                } else if (!reTime.test($("#txtHappenDateTimeHour").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtHappenDateTime").val()) + newLine;
                }

                if ($("#txtNo_165").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNo_165").val()) + newLine;
                } else if (!re.test($("#txtNo_165").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtNo_165").val()) + newLine;
                }
                //if ($("#txtNo_e").val().length <= 0) {
                //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNo_e").val()) + newLine;
                //}
                //if ($("#txtForCDate").val().length <= 0) {
                //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtForCDate").val()) + newLine;
                //} else if (!reDate.test($("#txtForCDate").val())) {
                //    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtForCDate").val()) + newLine;
                //}

                if ($("#txtSetdate").val().length > 0 && !reDate.test($("#txtSetdate").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameEtabsDatetime").val()) + newLine;
                }
                if ($("#txtSetdatetime").val().length > 0 && !reTime.test($("#txtSetdatetime").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameEtabsDatetime").val()) + newLine;
                }

                if ($("#txtNotificationName").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNotificationName").val()) + newLine;
                }


                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }

                $.blockUI();
                $.ajax({
                    url: $("#ChenHeUrl").val(),
                    type: "Post",
                    cache: false,
                    data: $("#frmCreate").serialize(),
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess($("#CreateSuccessMsg").val(), function () {
                                parent.$.colorbox.close();
                                parent.location.href = parent.location.href;
                            });
                        } else {
                            jAlertError($("#CreateFailMsg").val());
                            $.unblockUI();
                        }
                    }
                });
                return true;
            } 

            function BtnCancelOriginalClick() {
                $.blockUI();
                var DocNo = $("#txtDocNo").val();
                var NotificationSource = $("#ddlNotificationSource").val();//通報來源
                var codeflag = "2";//1代表修改
                var Setdate = $("#txtSetdate").val();
                var Setdatetime = $("#txtSetdatetime").val();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: $("#CancelOriginalUrl").val(),
                    data: { DocNo: DocNo, NotificationSource: NotificationSource, codeflag: codeflag, Setdate: Setdate, Setdatetime: Setdatetime },
                    dataType: "json",
                    error: function () {
                        jAlertError("沖正發查9091發生錯誤！");
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            $("#txtOriginal").val(data.Original);
                            $("#txtSetdate").val(data.EtabsDatetime);
                            $("#txtSetdatetime").val(data.EtabsDatetimeHour);
                            $("#txtEtabsTrnNum").val(data.ErrorMsg);//交互成功的情況下ErrorMsg記錄的是電文唯一主鍵
                            $("#ErrorSpan").html('');
                            $("#btnCancelOriginal").attr('disabled', true);//沖正按鈕不可用
                            $("#btnSetting").removeAttr("disabled");//設定按鈕可用了
                        }
                        else {
                            $("#txtOriginal").val(data.Original);
                            $("#txtSetdate").val(data.EtabsDatetime);
                            $("#txtSetdatetime").val(data.EtabsDatetimeHour);
                            $("#ErrorSpan").html(data.ErrorMsg);
                            $("#btnSetting").attr('disabled', true);//設定按鈕不可用
                            $("#btnCancelOriginal").removeAttr("disabled");//沖正按鈕可用了
                        }
                        $.unblockUI();
                    }
                });
            }
        },

        initCopyWarn: function () {
            $("#btnSaveCopyWarn").click(function() { $.CaseWarning.BtnSaveWarnClick(); });
        },

        initEditWarn: function () {
            //根據正本有無值來判斷設定按鈕和沖正按鈕是否可用
            var strOriginal = $("#ddlOriginal").val();//得到畫面上正本的值
            //if (strOriginal.trim() == "0")
            //{
            //    $("#btnCancelOriginal").attr('disabled', true);//沖正按鈕不可用
            //    $("#btnSetting").removeAttr("disabled");//設定按鈕可用了
            //}
            //else
            //{
            //    $("#btnSetting").attr('disabled', true);//設定按鈕不可用
            //    $("#btnCancelOriginal").removeAttr("disabled");//沖正按鈕可用了
            //}
            $("#ddlKind").change(function () { return changeddlKind(); });
            $("#btnSaveWarnEdit").click(function () { btnSaveWarnEditClick(); });
            $("#btnEditChenHe").click(function () { return BtnEditChenHeClick(); });
            $("#btnSetting").click(function () { return BtnSettingClick(); });
            $("#btnCancelOriginal").click(function () { return BtnCancelOriginalClick(); });
            function changeddlKind() {
                if ($("#ddlKind option:selected").val() === "延長") {
                    $("#btnCancelOriginal").removeAttr("disabled");//沖正按鈕可用了
                    $("#btnSetting").removeAttr("disabled");//設定按鈕可用了
                } 
                return true;
            }
            function btnSaveWarnEditClick() {
                var newLine = "<br/>";
                var msg = "";
                var re = /^([0-9.]+)$/;
                var reDate = /^(\d{3})\/(0\d{1}|1[0-2])\/(0\d{1}|[12]\d{1}|3[01])$/;
                var reTime = /^(0\d{1}|1\d{1}|2[0-3]):([0-5]\d{1})$/;
                //* 案發日期
                if ($("#txtHappenDateTime").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtHappenDateTime").val()) + newLine;
                } else if (!reDate.test($("#txtHappenDateTime").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtHappenDateTime").val()) + newLine;
                }
                if ($("#txtHappenDateTimeHour").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtHappenDateTime").val()) + newLine;
                } else if (!reTime.test($("#txtHappenDateTimeHour").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtHappenDateTime").val()) + newLine;
                }

                if ($("#txtNo_165").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNo_165").val()) + newLine;
                } else if (!re.test($("#txtNo_165").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtNo_165").val()) + newLine;
                }
                //if ($("#txtNo_e").val().length <= 0) {
                //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNo_e").val()) + newLine;
                //}
                //if ($("#txtForCDate").val().length <= 0) {
                //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtForCDate").val()) + newLine;
                //} else if (!reDate.test($("#txtForCDate").val())) {
                //    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtForCDate").val()) + newLine;
                //}

                if ($("#txtSetdate").val().length > 0 && !reDate.test($("#txtSetdate").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameEtabsDatetime").val()) + newLine;
                }
                if ($("#txtSetdatetime").val().length > 0 && !reTime.test($("#txtSetdatetime").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameEtabsDatetime").val()) + newLine;
                }

                if ($("#txtNotificationName").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNotificationName").val()) + newLine;
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
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            if (strOriginal.trim() == "0") {
                                $("#btnCancelOriginal").attr('disabled', true);//沖正按鈕不可用
                                $("#btnSetting").removeAttr("disabled");//設定按鈕可用了
                            }
                            else {
                                $("#btnSetting").attr('disabled', true);//設定按鈕不可用
                                $("#btnCancelOriginal").removeAttr("disabled");//沖正按鈕可用了
                            }
                            jAlertSuccess($("#EditSuccessMsg").val(), function() {
                                parent.$.colorbox.close();
                                parent.location.href = parent.location.href;
                            });
                        } else {
                            jAlertError($("#EditFailMsg").val(), function () {
                                location.href = location.href;
                            });
                            $.unblockUI();
                        }
                    }
                });
                return false;
            }
            //*呈核
            function BtnEditChenHeClick() {
                var newLine = "<br/>";
                var msg = "";
                var re = /^([0-9.]+)$/;
                var reDate = /^(\d{3})\/(0\d{1}|1[0-2])\/(0\d{1}|[12]\d{1}|3[01])$/;
                var reTime = /^(0\d{1}|1\d{1}|2[0-3]):([0-5]\d{1})$/;
                //* 案發日期
                if ($("#txtHappenDateTime").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtHappenDateTime").val()) + newLine;
                } else if (!reDate.test($("#txtHappenDateTime").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtHappenDateTime").val()) + newLine;
                }
                if ($("#txtHappenDateTimeHour").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtHappenDateTime").val()) + newLine;
                } else if (!reTime.test($("#txtHappenDateTimeHour").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtHappenDateTime").val()) + newLine;
                }

                if ($("#txtNo_165").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNo_165").val()) + newLine;
                } else if (!re.test($("#txtNo_165").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtNo_165").val()) + newLine;
                }
                //if ($("#txtNo_e").val().length <= 0) {
                //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNo_e").val()) + newLine;
                //}
                //if ($("#txtForCDate").val().length <= 0) {
                //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtForCDate").val()) + newLine;
                //} else if (!reDate.test($("#txtForCDate").val())) {
                //    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtForCDate").val()) + newLine;
                //}

                if ($("#txtSetdate").val().length > 0 && !reDate.test($("#txtSetdate").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameEtabsDatetime").val()) + newLine;
                }
                if ($("#txtSetdatetime").val().length > 0 && !reTime.test($("#txtSetdatetime").val())) {
                    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameEtabsDatetime").val()) + newLine;
                }

                if ($("#txtNotificationName").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNotificationName").val()) + newLine;
                }


                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }
                $.blockUI();
                $.ajax({
                    url: $("#EditChenHeUrl").val(),
                    type: "Post",
                    cache: false,
                    data: $("#frmEdit").serialize(),
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess($("#EditSuccessMsg").val(), function () {
                                parent.$.colorbox.close();
                                parent.location.href = parent.location.href;
                            });
                        } else {
                            jAlertError($("#EditFailMsg").val(), function () {
                                location.href = location.href;
                            });
                            $.unblockUI();
                        }
                    }
                });
                return false;
            }
            function BtnSettingClick() {
                $.blockUI();
                debugger;
                var DocNo = $("#txtDocNo").val();
                var Currency = $("#txtCurrency").val();
                var strKind = $("#ddlKind").val();
                var NotificationSource = $("#ddlNotificationSource").val();//通報來源
                var codeflag = "1";//1代表修改
                var Setdate = $("#txtSetdate").val();
                var Setdatetime = $("#txtSetdatetime").val();
                var ExtendDate = $("#txtExtendDate").val();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: $("#SettingUrl").val(),
                    data: { DocNo: DocNo, NotificationSource: NotificationSource, codeflag: codeflag, Setdate: Setdate, Setdatetime: Setdatetime, Currency: Currency, Kind: strKind, ExtendDate: ExtendDate },
                    dataType: "json",
                    error: function () {
                        jAlertError("設定發查9091發生錯誤！");
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            $("#ddlOriginal").val(data.Original);
                            //$("#txtSetdate").val(data.EtabsDatetime);
                            //$("#txtSetdatetime").val(data.EtabsDatetimeHour);
                            $("#ErrorSpan").html('');
                            $("#btnSetting").attr('disabled', true);//設定按鈕不可用
                            $("#btnCancelOriginal").removeAttr("disabled");//沖正按鈕可用了
                        }
                        else {
                            $("#ddlOriginal").val(data.Original);
                            //$("#txtSetdate").val(data.EtabsDatetime);
                            //$("#txtSetdatetime").val(data.EtabsDatetimeHour);
                            $("#ErrorSpan").html(data.ErrorMsg);
                            $("#btnCancelOriginal").attr('disabled', true);//沖正按鈕不可用
                            $("#btnSetting").removeAttr("disabled");//設定按鈕可用了
                        }
                        $.unblockUI();
                    }
                });
            }
            function BtnCancelOriginalClick() {
                $.blockUI();
                var DocNo = $("#txtDocNo").val();
                var NotificationSource = $("#ddlNotificationSource").val();//通報來源
                var codeflag = "2";//2代表沖正
                var Setdate = $("#txtSetdate").val();
                var Setdatetime = $("#txtSetdatetime").val();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: $("#CancelOriginalUrl").val(),
                    data: { DocNo: DocNo, NotificationSource: NotificationSource, codeflag: codeflag, Setdate: Setdate, Setdatetime: Setdatetime },
                    dataType: "json",
                    error: function () {
                        jAlertError("沖正發查9091發生錯誤！");
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            $("#ddlOriginal").val(data.Original);
                            $("#txtSetdate").val(data.EtabsDatetime);
                            $("#txtSetdatetime").val(data.EtabsDatetimeHour);
                            $("#txtEtabsTrnNum").val(data.ErrorMsg);//交互成功的情況下ErrorMsg記錄的是電文唯一主鍵
                            $("#ErrorSpan").html('');
                            $("#btnCancelOriginal").attr('disabled', true);//沖正按鈕不可用
                            $("#btnSetting").removeAttr("disabled");//設定按鈕可用了
                        }
                        else {
                            $("#ddlOriginal").val(data.Original);
                            $("#txtSetdate").val(data.EtabsDatetime);
                            $("#txtSetdatetime").val(data.EtabsDatetimeHour);
                            $("#ErrorSpan").html(data.ErrorMsg);
                            $("#btnSetting").attr('disabled', true);//設定按鈕不可用
                            $("#btnCancelOriginal").removeAttr("disabled");//沖正按鈕可用了
                        }
                        $.unblockUI();
                    }
                });
            }
        },

        initSetStatus: function () {
            $("#txtOtherReason").prop("readonly", "readonly");
            $("#ddlRelieveReason").change(function() { return changeRelieveReason(); });
            $("#btnSaveStatus").click(function () { return btnSaveStatusClick(); });
            $("#btnReleaseChenHe").click(function () { return btnReleaseChenHeClick(); });
            //Add by zhangwei 20180315 start
            $("#btnCancelRemove").click(function () { return btnCancelRemoveClick(); });
            $("#btnRemove").click(function () { return btnRemoveClick(); });
            //Add by zhangwei 20180315 end
            function changeRelieveReason() {
                if ($("#ddlRelieveReason option:selected").val() === "L其他") {
                    $("#txtOtherReason").prop("readonly", "");
                } else {
                    $("#txtOtherReason").prop("readonly", "readonly");
                    $("#txtOtherReason").val("");
                }
                return true;
            }

            function btnReleaseChenHeClick() {
                var newLine = "<br/>";
                var msg = "";
                if ($("#txtRelieveDate").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtRelieveDate").val()) + newLine;
                }

                //* 有必填檢核錯誤
                if (msg.length > 0) {
                    jAlertError(msg);
                    return false;
                }


                $.blockUI();
                $.ajax({
                    url: $("#ReleaseChenHeUrl").val(),
                    type: "Post",
                    cache: false,
                    data: $("#frmEdit").serialize(),
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess($("#EditSuccessMsg").val(), function () {
                                parent.$.colorbox.close();
                                parent.location.href = parent.location.href;
                            });
                        } else if (data.ReturnCode === "2") {
                            jAlertSuccess($("#EndDocNo").val(), function () {
                                parent.$.colorbox.close();
                                parent.location.href = parent.location.href;
                            });
                        } else {
                            jAlertError($("#EditFailMsg").val(), function () {
                                location.href = location.href;
                            });
                            $.unblockUI();
                        }
                    }
                });
                return false;
            }

            function btnSaveStatusClick() {
                var newLine = "<br/>";
                var msg = "";
                if ($("#txtRelieveDate").val().length <= 0) {
                    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtRelieveDate").val()) + newLine;
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
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess($("#EditSuccessMsg").val(), function() {
                                parent.$.colorbox.close();
                                parent.location.href = parent.location.href;
                            });
                        } else if (data.ReturnCode === "2") {
                            jAlertSuccess($("#EndDocNo").val(), function() {
                                parent.$.colorbox.close();
                                parent.location.href = parent.location.href;
                            });
                        } else {
                            jAlertError($("#EditFailMsg").val(), function () {
                                location.href = location.href;
                            });
                            $.unblockUI();
                        }
                    }
                });
                return false;
            }
            function btnCancelRemoveClick()
            {
                $.blockUI();
                var DocNo = $("#DocNo").val();
                var No_165 = $("#txtNo_165").val();
                var NotificationSource = $("#NotificationSource").val();//通報來源
                $.ajax({
                    type: "Post",
                    url: $("#CancelRemoveUrl").val(),
                    dataType: "json",
                    data: { DocNo: DocNo, NotificationSource: NotificationSource, No_165: No_165  },
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess("取消解除成功！");
                            $("#txtRelieveDate").val("");//將解除警示日期代入空值
                            $("#txtRelieveDateTimeForHour").val(data.RelieveTime);//將解除警示日期代入空值
                            $("#textRelease").val(" ");
                            $("#txtEtabsTrnNum").val(data.ErrorMsg);//電文編號 
                            $("#ErrorSpan").html('');
                        } else {
                            $("#ErrorSpan").html(data.ErrorMsg);
                            $("#txtEtabsTrnNum").val('');
                        }
                        $.unblockUI();
                    }
                });
            }
            function btnRemoveClick()
            {
                $.blockUI();
                var DocNo = $("#DocNo").val();
                var No_165 = $("#txtNo_165").val();
                var NotificationSource = $("#NotificationSource").val();//通報來源
                var EtabsNo = $("#txtEtabsNo").val();//外來文編號
                $.ajax({
                    type: "Post",
                    url: $("#RemoveUrl").val(),
                    dataType: "json",
                    data: { DocNo: DocNo, NotificationSource: NotificationSource, EtabsNo: EtabsNo, No_165: No_165 },
                    error: function () {
                        jAlertError($("#LoadErrorMsg").val());
                        $.unblockUI();
                    },
                    success: function (data) {
                        if (data.ReturnCode === "1") {
                            jAlertSuccess("解除成功！");
                            $("#txtRelieveDate").val(data.RelieveDate);//回寫解除日期
                            $("#txtRelieveDateTimeForHour").val(data.RelieveTime);//回寫解除日期
                            //$('#txtFlag_Release').click(function () {
                            //    $('#txtFlag_Release').show();
                            //    $("#txtFlag_Release").prop("checked", true);
                            //});
                            //document.getElementById("txtFlag_Release").checked = true;
                            $("#textRelease").val("V");
                            $("#txtEtabsTrnNum").val(data.ErrorMsg);//隱藏控件記錄與主機交互唯一編號
                            $("#ErrorSpan").html('');
                        } else {
                            $("#ErrorSpan").html(data.ErrorMsg);
                            $("#txtEtabsTrnNum").val('');
                        }
                        $.unblockUI();
                    }
                });
            }
        },
 
  
        //*儲存警示明細
        BtnSaveWarnClick: function () {
            var newLine = "<br/>";
            var msg = "";
            var re = /^([0-9.]+)$/;
            var reDate = /^(\d{3})\/(0\d{1}|1[0-2])\/(0\d{1}|[12]\d{1}|3[01])$/;
            var reTime = /^(0\d{1}|1\d{1}|2[0-3]):([0-5]\d{1})$/;
            //* 案發日期
            if ($("#txtHappenDateTime").val().length <= 0) {
                msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtHappenDateTime").val()) + newLine;
            } else if (!reDate.test($("#txtHappenDateTime").val())) {
                msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtHappenDateTime").val()) + newLine;
            }
            if ($("#txtHappenDateTimeHour").val().length <= 0) {
                msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtHappenDateTime").val()) + newLine;
            } else if (!reTime.test($("#txtHappenDateTimeHour").val())) {
                msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtHappenDateTime").val()) + newLine;
            }

            if ($("#txtNo_165").val().length <= 0) {
                msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNo_165").val()) + newLine;
            } else if (!re.test($("#txtNo_165").val())) {
                msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtNo_165").val()) + newLine;
            }
            //if ($("#txtNo_e").val().length <= 0) {
            //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNo_e").val()) + newLine;
            //}
            //if ($("#txtForCDate").val().length <= 0) {
            //    msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtForCDate").val()) + newLine;
            //} else if (!reDate.test($("#txtForCDate").val())) {
            //    msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NametxtForCDate").val()) + newLine;
            //}

            if ($("#txtSetdate").val().length > 0 && !reDate.test($("#txtSetdate").val())) {
                msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameEtabsDatetime").val()) + newLine;
            }
            if ($("#txtSetdatetime").val().length > 0 && !reTime.test($("#txtSetdatetime").val())) {
                msg = msg + $.validator.format($("#PlzCorrectFormat").val(), $("#NameEtabsDatetime").val()) + newLine;
            }

            if ($("#txtNotificationName").val().length <= 0) {
                msg = msg + $.validator.format($("#PlzInput").val(), $("#NametxtNotificationName").val()) + newLine;
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
                error: function () {
                    jAlertError($("#LoadErrorMsg").val());
                    $.unblockUI();
                },
                success: function (data) {
                    if (data.ReturnCode === "1") {
                        jAlertSuccess($("#CreateSuccessMsg").val(), function () {
                            parent.$.colorbox.close();
                            parent.location.href = parent.location.href;
                        });
                    } else {
                        jAlertError($("#CreateFailMsg").val());
                        $.unblockUI();
                    }
                }
            });
            return true;
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.CaseWarning.documentReady();
    });
})(jQuery);
var bSearchFlag = false;

function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}

//* 隱藏iframe回調用
function showMessage(strType, strMsg) {
    if (strType === "1") {
        jAlertSuccess(strMsg, function () { location.href = location.href; });
    }
    if (strType === "0") {
        jAlertError(strMsg);
    }
}


function formatTime(obj) {
    var str = $(obj).val().replace(/:/g, "").replace(/：/g, "");
    if (str.length === 4) {
        $(obj).val(str.substring(0, 2) + ":" + str.substring(2));
    }
}

