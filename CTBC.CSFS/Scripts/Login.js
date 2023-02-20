//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var systemLogin = $.SystemLogin = {};
    jQuery.extend(systemLogin, {
        documentReady: function () {
            //* init
            $.SystemLogin.initLogin();
        },

        initLogin: function () {
            $("#username").focus();
            $("#btnEditPwd").click(function () { window.open('https://was.ctbcbank.com'); return false; });
            $("#btnLogonDesc").click(function() { alert($("#MSG_LOGON_RESET_PWD").val().replace("\\n", "\n")); return false; });
            $("#btnReset").click(function () { clearErr();});
            $("#f1").submit(function () { return validateForm();});

            //* 清空錯誤訊息
            function clearErr() {
                $("#err-msg").html("");
                $("#username").focus();
            }
            //* 檢查UserID輸入欄位
            function chkUsr() {
                var usertxt = $("#username").val();
                usertxt = $.trim(usertxt);
                var re = new RegExp('^[0-9a-zA-Z]{1,10}$');
                if (!usertxt.match(re)) return false;
                if (usertxt.length > 10 || usertxt.length <= 0) return false;
                else return true;
            }
            //* 檢查Password欄位
            function chkPwd() {
                var pwdtxt = $("#password").val();
                pwdtxt = $.trim(pwdtxt);
                if ((pwdtxt.indexOf('<') > -1) || (pwdtxt.indexOf('>') > -1)) return false;
                if (pwdtxt.length > 16 || pwdtxt.length <= 0) return false;
                else return true;
            }

            //-------------------------------------------------------- Legend 2017/10/25 添加RCAF驗證登陸Start
            function chkUsrRCAF()
            {
                var uRCAFtxt = $.trim($("#usrRCAF").val());
                var pRCAFtxt = $.trim($("#psRCAF").val());
                var bRCAFVal = $.trim($("#brhRCAF").val());
                if (uRCAFtxt.length == 0 && pRCAFtxt.length == 0)
                {
                    if (bRCAFVal.length == 0)
                        return true;
                    else return false; //20130417 horace RCAF帳密沒輸入+RCAF 分行別有輸入=>return false
                }
                else
                {
                    var re = new RegExp('^[0-9a-zA-Z]{1,10}$');
                    if (!uRCAFtxt.match(re)) return false;
                    if (uRCAFtxt.length > 10 || uRCAFtxt.length == 0) return false;
                    else return true;
                }
            }
            function chkPwdRCAF()
            {
                var uRCAFtxt = $.trim($("#usrRCAF").val());
                var pRCAFtxt = $.trim($("#psRCAF").val());
                var bRCAFVal = $.trim($("#brhRCAF").val());
                if (uRCAFtxt.length == 0 && pRCAFtxt.length == 0)
                {
                    if (bRCAFVal.length == 0)
                        return true;
                    else return false; //20130417 horace RCAF帳密沒輸入+RCAF 分行別有輸入=>return false
                }
                else
                {
                    if ((pRCAFtxt.indexOf('<') > -1) || (pRCAFtxt.indexOf('>') > -1)) return false;
                    if (pRCAFtxt.length > 16 || pRCAFtxt.length == 0) return false;
                    else return true;
                }
            }

            function chkbrhRCAF()
            {
                var uRCAFtxt = $.trim($("#usrRCAF").val());
                var pRCAFtxt = $.trim($("#psRCAF").val());
                var bRCAFVal = $.trim($("#brhRCAF").val());
                if (uRCAFtxt.length == 0 && pRCAFtxt.length == 0) return true;
                else
                {
                    if ((uRCAFtxt.length > 0 || pRCAFtxt.length > 0) && bRCAFVal.length > 0)
                    {
                        $("#hid_brhRCAF").val(bRCAFVal)
                        return true;
                    }
                    else return false;
                }
            }
            //-------------------------------------------------------- Legend 2017/10/25 添加RCAF驗證登陸End

            //* 表單提交時驗證表單
            function validateForm() {
                clearErr();
                var msg = "";
                var ck1 = false;
                var ck2 = false;
                var ck3 = false;
                var ck4 = false;
                var ck5 = false;
                if (!chkUsr()) {
                    msg = $("#MSG_LOGONID").val();
                } else ck1 = true;
                if (!chkPwd()) {
                    msg += $("#MSG_LOGONPWD").val();
                } else ck2 = true;
                if (!chkUsrRCAF())
                {
                    msg += '[RCAF帳號]';
                } else ck3 = true;
                if (!chkPwdRCAF())
                {
                    msg += '[RCAF密碼]';
                } else ck4 = true;
                if (!chkbrhRCAF())
                {
                    msg += '[RCAF分行]';
                } else ck5 = true;
                if (ck1 && ck2 && ck3 && ck4 && ck5)
                {
                    $("#err-msg").html($("#MSG_LOGON_ONAUTH").val());
                    $("#username").attr("readOnly", "readonly");
                    $("#password").attr("readOnly", "readonly");
                    $("#btnSubmit").attr("disabled", "disabled");
                    $("#btnReset").attr("disabled", "disabled");
                    
                    //-------------------------------------------------------- Legend 2017/10/25 添加RCAF驗證登陸Start
                    $("#usrRCAF").attr("readOnly", "readonly");
                    $("#psRCAF").attr("readOnly", "readonly");

                    $("#brhRCAF").attr("disabled", "disabled");

                    var usrn = $.trim($("#username").val());
                    var usrp = $.trim($("#password").val());
                    var encodedusrp = encodeURIComponent(usrp);
                    var urcafn = $.trim($("#usrRCAF").val());
                    var urcafp = $.trim($("#psRCAF").val());
                    var encodedurcafp = encodeURIComponent(urcafp);
                    var rcafb = $.trim($("#brhRCAF").val());

                    //alert("usrn=" + usrn + "usrp=" + usrp + "urcafn=" + urcafn + "urcafp=" + urcafp + "rcafb=" + rcafb);
                    var pass = true; //是否繼續登入CSFS系統(進入/Home/Login)
                    if (urcafn != "" && usrp != "" && rcafb != "")
                    {
                        //------------------------------------------------------------------
                        $.ajax({
                            url: getRootURL() + 'Login/ChkRCAF',
                            type: "POST",
                            cache: false,
                            async: false,
                            data: "usrn=" + usrn + "&usrp=" + encodedusrp + "&urcafn=" + urcafn + "&urcafp=" + encodedurcafp + "&rcafb=" + rcafb,
                            datatype: "text",
                            success: function (data)
                            {
                                $("#Span1").hide();
                                if (data == "VALID_LDAP_FAILURE")
                                {
                                    $("#err-msg").text('帳號或密碼錯誤!');
                                    pass = false;
                                } else
                                {
                                    //RCAF認證有錯誤發生時
                                    if (data != "")
                                    {
                                        //請user確認是否繼續登入CSFS系統                        
                                        if (confirm(data))
                                        {
                                            //user繼續登入CSFS系統
                                            pass = true;
                                        } else
                                        {
                                            //user不繼續登入CSFS系統
                                            $("#err-msg").html("");
                                            pass = false;
                                            var loginUrl = getRootURL() + "Home/Login";
                                            window.location = loginUrl;
                                        }
                                    }
                                    else
                                    {
                                        //user繼續登入NUMS系統
                                        pass = true;
                                    }
                                }
                            },
                            error: function (xhr)
                            {
                                displayError(xhr);
                            }
                        });
                        //------------------------------------------------------------------
                    }
                    if (pass) return true; //user繼續登入CSFS系統
                    else
                    {
                        $("#username").removeAttr("readOnly");
                        $("#password").removeAttr("readOnly");
                        $("#usrRCAF").removeAttr("readOnly");
                        $("#psRCAF").removeAttr("readOnly");
                        $("#brhRCAF").removeAttr("disabled");
                        $("#btnSubmit").removeAttr("disabled");
                        $("#btnReset").removeAttr("disabled");
                        return false;
                    }  //user不繼續登入CSFS系統
                    //------------------------------------------------------------------  Legend 2017/10/25 添加RCAF驗證登陸End
                }
                else {
                    $("#err-msg").html(msg + $("#MSG_LOGON_ERR_FORMAT1").val());
                    return false;
                }
            }
        }

    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $.SystemLogin.documentReady();
    });
})(jQuery);

