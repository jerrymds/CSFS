//* 登入
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var csfs = $.CSFS = {};
    jQuery.extend(csfs, {
        documentReady: function () {
            //* init
            $.CSFS.bindFancybox();
            $.CSFS.bindDatePicker();
            //moment 設定語系
            moment.locale();
        },
        //* 綁定彈出
        bindFancybox: function () {
            $("[class*='fancy']:not(.fancyimg,.lightboxbinded)").unbind("click").click(function () {
                /// <summary>
                /// fancybox 註冊
                /// </summary>
                var e = $(this);
                e.addClass("lightboxbinded");
                var classValue = e.attr("class").split(' ');
                var fancyClass = '';
                $.each(classValue, function (i) {
                    if (classValue[i].indexOf('fancy') != -1) {
                        fancyClass = classValue[i].replace('fancy', '').split("_");
                    }
                });

                var width = filterInputSize(fancyClass[0]);
                var height = filterInputSize(fancyClass[1]);
                var v = $.colorbox({
                    href: e.attr('href') ? e.attr('href') : e.data('url'),
                    title: e.attr('title'),
                    width: width,
                    height: height,
                    overlayClose: false,
                    scrolling: true,
                    iframe: true,
                    escKey: true,
                    opacity: 0.4,
                    reposition: false,
                    onLoad: function () {
                        //$('#cboxClose').hide();
                        //$(window).load(function () {   });
                    },
                    onComplete: function () {


                    }

                });
                return false;
            });
            //      if ($(".fancyimg").length > 0) {
            //        $(".fancyimg").colorbox().attr('onfocus', 'this.blur()');
            //      };
        },
        //* 綁定日曆
        bindDatePicker: function () {
            if ($("[data-datepicker]").length > 0) {
                var list = $("[data-datepicker]");
                $.each(list, function (i) {
                    numsDatepicker($("#" + list[i].id));
                });
            }
        },
        //* 來文機關聯動
        bindGovKindAndUnit: function (ddlGovKindId, txtGovUnitId) {
            $("#" + ddlGovKindId).change(function () { selectedKindChange(ddlGovKindId, txtGovUnitId); });

            function selectedKindChange(ddlGovKindId, txtGovUnitId) {
                var selectedValue = $("#" + ddlGovKindId + " option:selected").val();
                if ($.trim(selectedValue).length > 0) {
                    $.ajax({
                        type: "POST",
                        async: false,
                        url: $("#GetGovNameUrl").val(),
                        data: { govKind: selectedValue },
                        success: function (data) {
                            $("#" + txtGovUnitId).data('typeahead', '');
                            $("#" + txtGovUnitId).typeahead({ source: data });
                        }
                    });
                }
            }
        },

        bindGovKindAndUnit1: function (txtGovUnitId) {
            selectedKindChange(txtGovUnitId);
            function selectedKindChange(txtGovUnitId) {
                var selectedValue = "";
                $.ajax({
                    type: "POST",
                    async: false,
                    url: $("#GetGovNameUrl").val(),
                    data: { govKind: "" },
                    success: function (data) {
                        $("#" + txtGovUnitId).data('typeahead', '');
                        $("#" + txtGovUnitId).typeahead({ source: data });
                    }
                });
            }
        },
        //用來儲存多語系訊息
        msgLang: {},
        //設定多語系訊息
        setMsg: function (msg) {
            msg = msg || {};
            $.extend(this.msgLang, msg);
        },
        //頁面有用到的參數可設定到這裡
        config: {},
        //設定頁面參數
        setConfig: function (config) {
            config = config || {};
            $.extend(this.config, config);
        },
        //表單驗證
        formValid: function (formId, rules, messages, saveHanlder) {
            var form = $("#" + formId);
            form.validate({
                errorElement: 'span', //default input error message container
                errorClass: 'help-block', // default input error message class
                focusInvalid: false, // do not focus the last invalid input
                ignore: "",
                rules: rules,
                messages: messages,
                invalidHandler: function (event, validator) {
                    //display error alert on form submit              
                },
                highlight: function (element) {
                    // hightlight error inputs
                    $(element).closest('.form-validate').addClass('has-error'); // set error class to the control group
                },
                unhighlight: function (element) {
                    // revert the change done by hightlight
                    $(element).closest('.form-validate').removeClass('has-error'); // set error class to the control group
                },
                success: function (element) {
                    $(element).closest('.form-validate').removeClass('has-error'); // set success class to the control group
                },
                submitHandler: function (form) {
                    if (saveHanlder) {
                        saveHanlder();
                    }
                    else {
                        form.submit();
                    }
                }
            });
        },
        //ajax call result
        resultHandler: function (result, succHandler) {
            //ReturnCode == "1" 成功
            if (result.ReturnCode == "1") {
                if (result.ReturnMsg && result.ReturnMsg != "") {
                    jAlertSuccess(result.ReturnMsg, succHandler);
                }
                else {
                    if (succHandler) succHandler();
                }
            }
            else {
                if (result === "" && succHandler) {
                    succHandler();
                    return;
                }

                var err = "";
                if (result.responseText) {
                    err = result.responseText;
                }
                else if (result.ReturnMsg) {
                    err = result.ReturnMsg;
                }
                else {
                    err = result;
                }

                jAlertError(err, succHandler);
            }
        },
        //民國年轉西元年
        twToDate: function (twd) {
            var d = moment(twd, 'YYYY/MM/DD');
            if (d.isValid() == false) {
                return twd;
            }
            return d.add(1911, 'years').format('YYYY/MM/DD');
        },
        //日期相減返回天數
        diffDays: function (date1, date2) {
            var d1 = moment(date1, 'YYYY/MM/DD');
            var d2 = moment(date2, 'YYYY/MM/DD');
            return d1.diff(d2, 'days');
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        $(".focus:last").focus();
        $.CSFS.documentReady();
    });
})(jQuery);

function filterInputSize(val) {
    /// <summary>
    /// 專門為了 輸入方塊 高寬設計的函式
    /// </summary>
    try {
        if (isNaN(val)) {
            var temp = val.toLowerCase();
            if (temp.indexOf('p') != -1) return temp.replace('p', '%');
        } else {
            return val;
        }
    } catch (e) {
        alert('Fail! \nUsing Default Size 50%');
        return '50%';
    }
}


function checkIsValidDate(str) {
    //如果为空，则通过校验
    if (str == "")
        return true;
    var pattern = /^[0-9]{3,4}\/[0-1]?[0-9]{1}\/[0-3]?[0-9]{1}$/g;
    if (!pattern.test(str))
        return false;
    var arrDate = str.split("\/");

    if (parseInt(arrDate[0], 10) < 100)
        arrDate[0] = 1900 + parseInt(arrDate[0], 10) + "";
    //var date = new Date(arrDate[0], (parseInt(arrDate[1], 10) - 1) + "", arrDate[2]);
    var year = 1911 + parseInt(arrDate[0], 10) + "";
    var date = new Date(str);

    var leap = false;
    if (arrDate[2] == "29" && parseInt(arrDate[1], 10) == "02") {
        if (parseInt(year, 10) % 4 == 0 && (parseInt(year, 10) % 100 != 0 || parseInt(year, 10) % 400 == 0)) {
            leap = true;
        }
    }

    if (leap == false) {
        if (arrDate[2] == "29" && parseInt(arrDate[1], 10) == "02") {
            return false;
        }
    }

    if (date.getFullYear() == arrDate[0]
       && (leap == true ? date.getMonth() -1 : date.getMonth()) == (parseInt(arrDate[1], 10) - 1) + ""
       && ((leap == true && parseInt(arrDate[1], 10) == "02") ? "29" : date.getDate() == arrDate[2]))
        return true;
    else
        return false;
}


function trimAllInput() {
    $(":input[type='text']").each(function () {
        $(this).val($.trim($(this).val()));
    });
}

//**************************************
// 台灣身份證檢查簡短版 for Javascript
//**************************************
function checkTwID(id) {
    //建立字母分數陣列(A~Z)
    var city = new Array(
        1, 10, 19, 28, 37, 46, 55, 64, 39, 73, 82, 2, 11,
        20, 48, 29, 38, 47, 56, 65, 74, 83, 21, 3, 12, 30
    );
    id = id.toUpperCase();
    // 使用「正規表達式」檢驗格式
    if (id.search(/^[A-Z](1|2)\d{8}$/i) == -1) {
        //alert('基本格式錯誤');
        return false;
    } else {
        //將字串分割為陣列(IE必需這麼做才不會出錯)
        id = id.split('');
        //計算總分
        var total = city[id[0].charCodeAt(0) - 65];
        for (var i = 1; i <= 8; i++) {
            total += eval(id[i]) * (9 - i);
        }
        //補上檢查碼(最後一碼)
        total += eval(id[9]);
        //檢查比對碼(餘數應為0);
        return ((total % 10 == 0));
    }
}


//* 20151127 Ge.Song 記錄AJAX錯誤到Console
function logAjaxError(XMLHttpRequest, textStatus, errorThrown) {
    if (!window.console) {
        window.console = { log: function () { return; } };
    }
    console.log(XMLHttpRequest.textStatus);
    console.log("狀態碼:", XMLHttpRequest.readyState);
    console.log("請求狀態:", textStatus);
    console.log("錯誤代號:", XMLHttpRequest.status);
    console.log("錯誤訊息:", errorThrown);
}