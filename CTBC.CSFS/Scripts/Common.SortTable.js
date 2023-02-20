//* 翻頁和表格排序
(function () {
    if (typeof (jQuery) === 'undefined') {
        //* need jquery 1.4+
        alert('jQuery Library NotFound.');
        return;
    }
    var $ = jQuery;
    var custPagination = $.custPagination = {};
    jQuery.extend(custPagination, {
        documentready: function () {
            //* init
            //BindPagination();
        },
        //* 綁定翻頁元件
        BindCheckBox: function () {
            if ($("table.sort").length >= 1) {
                $.each($("table.sort"), function() {
                    var tb = $(this);
                    if (tb.find("#CheckAll").length >= 1) {
                        tb.find("#CheckAll").click(function () {
                            var chked = $(this).prop("checked");
                            $(this).parents("table.sort").find("[name=r2]:checkbox").prop("checked", chked);
                        });
                        
                        tb.find("[name=r2]:checkbox").click(function () {
                            var table = $(this).parents("table.sort");
                            var flag = true;
                            table.find("[name=r2]:checkbox").each(function () {
                                if (!this.checked)
                                    flag = false;
                            });
                            table.find("#CheckAll").prop("checked", flag);
                        });
                    }
                });
            }
        },

        //* 綁定排序
        sort: function (parentObj) {
            if (parentObj.length <= 0) {
                return;
            }
            ///<summary>排序就靠我</summary>
            if (parentObj.find('[data-sortcolumn]').length > 0) {
                setDefaultSortColumn();
            }
            //* 設置排序
            function setDefaultSortColumn() {
                ///<summary>設定預設的排序欄位，必須從頁面的 sortColumn 取得</summary>
                if (parentObj.find("#defaultSortColumn").length < 1) {
                    alert("Please set 【#defaultSortColumn】 hidden fields");
                }
                if (parentObj.find("#defaultSort").length < 1) {
                    alert("Please set 【#defaultSort】 hidden fields");
                }

                if (parentObj.find("table.sort").length < 1) {
                    alert("Please set 【.sort】for you want to sort table");
                }
                var defaultSortColumn = parentObj.find("#defaultSortColumn").val();
                var defaultTh = parentObj.find("[data-sortcolumn='" + defaultSortColumn + "']");
                defaultTh.data("sort", parentObj.find("#defaultSort").val());
                //* 設置圖標
                setSortIcon(defaultTh);
                //*
                checkSortColumn();
            }
            function sortEx() {
                //* 實際排序
                var my;
                parentObj.find("table.sort th").each(function () {
                    var domEle = $(this);
                    if (domEle.data('sort') !== undefined) {
                        my = domEle;
                        return;
                    }
                });
                var currentSortDirection = my.data('sort');
                toggleSortDirection(my, currentSortDirection);
                var url;
                if (parentObj.attr("data-target-url") !== "") {
                    url = $.url(parentObj.attr("data-target-url"));
                }
                else {
                    url = $.url();
                }
                //* 排序目標地址
                var currentQueryString = url.attr('query');
                //if (currentQueryString.length > 0) {

                var objPager = parentObj.find(".tm-pagination-right");
                //alert(objPager.html());
                var currentPage = objPager.attr("data-current-page");
                var currentPageSize = objPager.attr("data-records-per-page");
                
                if (!$.isNumeric(currentPage)) {
                    currentPage = 1;
                }
                if (!$.isNumeric(currentPageSize)) {
                    currentPageSize = 10;
                }
                //var r = Math.random();
                var r = new Date();
                var pageQueryString = "&page=" + currentPage
                                    + "&pagesize=" + currentPageSize
                                    + "&strSortExpression=" + my.data("sortcolumn")
                                    + "&strSortDirection=" + my.data('sort')
                                    + "&" + parentObj.find("#querystring").val().replace(/&(sortColumn|desc|page|pagesize)=\w+/gi, '');
                                    + "&randomxx=" + r;
                var qUrl = url.attr('path') + "?" + currentQueryString + pageQueryString;
                // console.log(qUrl);
                //alert(qUrl);
                $.blockUI();
                $.post(qUrl, function (data) {
                    $.unblockUI();
                    parentObj.html(data);
                    parentObj.find("#querystring").val($.custPagination.filterUrl(pageQueryString));
                    //$.custPagination.BindCheckBox();
                    //$.custPagination.sort(parentObj);
                    //$.fn.initModalLink(parentObj.attr("id"));
                    //LMPCommon.bindSepLine();
                });
                //}
            }

            function checkSortColumn() {
                //主要是如果點選的不是目前排序的就要改變 DOM
                var targetDom = parentObj.find("table.sort th[data-sortcolumn]");
                targetDom.click(function (e) {
                    e.preventDefault();
                    var my = $(this);
                    if (my.data('sort') === undefined) {
                        targetDom.removeData('sort').find('#j_sd').remove();
                        my.data("sort", "desc");
                        sortEx();
                    }
                    else {
                        sortEx();
                    }
                });
            }
            //* 設置排序圖標
            function setSortIcon(my) {
                ///<summary>設定排序的圖示</summary>
                if (my.data('sort') === "Asc") {
                    //setSortIconEx(my, '&nbsp;▼', '&nbsp;▲');
                    my.addClass("iconSort-up-sign");
                    my.removeClass("iconSort-down-sign");
                }
                else {
                    //setSortIconEx(my, '&nbsp;▲', '&nbsp;▼');
                    my.removeClass("iconSort-up-sign");
                    my.addClass("iconSort-down-sign");
                }
            }
            //* 實際設置圖標
            function setSortIconEx(my, oldSortDirection, newSortDirection) {
                ///<summary>真。改變DOM的排序圖示</summary>
                my.html($.trim(my.html())).wrapInner("<span />");
                if (my.find("#j_sd").length > 0) {
                    my.html(my.html().replace(oldSortDirection, newSortDirection));
                }
                else {
                    my.append("<span id='j_sd'>" + newSortDirection + "</span>");
                }
            }

            function toggleSortDirection(my, currentSortDirection) {
                ///<summary>切換目前的排序方式</summary>
                if (currentSortDirection === "Asc") {
                    my.data('sort', 'Desc');
                }
                else {
                    my.data('sort', 'Asc');
                }
                setSortIcon(my);
            }
        },

        //* 過濾url中不需要的部分.(公共)
        filterUrl: function (url) {
            ///<summary>處理URL 過多的參數符號問題</summary>
            url = url.replace(/&{2,}/gi, '&');
            if (url.lastIndexOf('&') === (url.length - 1)) {
                url = url.substring(0, url.length - 1);
            }
            return url;
        }
    });
    //===========================================================================================
    jQuery(document).ready(function () {
        //$.custPagination.BindPagination();
    });

})(jQuery);


//阻止事件執行
function stopDefault(e) {
    if (e && e.preventDefault)
        e.preventDefault();
    else
        window.event.returnValue = false;

    return false;
}

// Grid跳轉到輸入頁碼(button按鈕)
function CkPageNumberDiv(e, divName, SortExpression, SortDirection, extraFunc) {
    ///	<summary>
    ///	Grid跳轉到輸入頁碼
    ///	Ruina 2011/11/25
    ///	</summary>
    ///	<param name="e" type="object">事件對象</param>
    ///	<param name="divName" type="string">分頁所在div名稱</param>

    var keynum

    if (window.event) // IE
    {
        keynum = e.keyCode
    }
    else if (e.which) // Netscape/Firefox/Opera
    {
        keynum = e.which
    }

    if (extraFunc != undefined && extraFunc != "") {
        var flag = eval(extraFunc);
        if (flag == false) {
            stopDefault(e);
            return false;
        }
    }

    // 綁定Grid
    BindGridDiv('#' + divName, SortExpression, SortDirection);
    return false;
}


// 根據輸入頁碼，綁定Grid
function BindGridDiv(divName, SortExpression, SortDirection) {
    var getPath = "";
    //var r = Math.random();
    var r = new Date();
    getPath = $(divName).find("#hidQueryCriteriaPath").val();

    if (getPath.indexOf("?") < 0) {
        getPath = getPath + "?randomxx=" + r + "&pageNum=" + $(divName).find("#txtPageNum").val() + "&strSortExpression=" + SortExpression + "&strSortDirection=" + SortDirection;
    }
    else {
        getPath = getPath + "&randomxx=" + r + "&pageNum=" + $(divName).find("#txtPageNum").val() + "&strSortExpression=" + SortExpression + "&strSortDirection=" + SortDirection;
    }

    $.post(getPath, null, function (data) {
        $(divName).html(data);
    });
}

(function ($, undefined) {

    var tag2Attr = {
        a: 'href',
        img: 'src',
        form: 'action',
        base: 'href',
        script: 'src',
        iframe: 'src',
        link: 'href'
    },

          key = ["source", "protocol", "authority", "userInfo", "user", "password", "host", "port", "relative", "path", "directory", "file", "query", "fragment"],

          aliases = {
              "anchor": "fragment"
          },

          parser = {
              strict: /^(?:([^:\/?#]+):)?(?:\/\/((?:(([^:@]*):?([^:@]*))?@)?([^:\/?#]*)(?::(\d*))?))?((((?:[^?#\/]*\/)*)([^?#]*))(?:\?([^#]*))?(?:#(.*))?)/,
              loose: /^(?:(?![^:@]+:[^:@\/]*@)([^:\/?#.]+):)?(?:\/\/)?((?:(([^:@]*):?([^:@]*))?@)?([^:\/?#]*)(?::(\d*))?)(((\/(?:[^?#](?![^?#\/]*\.[^?#\/.]+(?:[?#]|$)))*\/?)?([^?#\/]*))(?:\?([^#]*))?(?:#(.*))?)/
          },

          querystringParser = /(?:^|&|;)([^&=;]*)=?([^&;]*)/g,

          fragmentParser = /(?:^|&|;)([^&=;]*)=?([^&;]*)/g;

    function parseUri(url, strictMode) {
        var str = decodeURI(url),
                res = parser[strictMode || false ? 'strict' : "loose"].exec(str),
                uri = {
                    attr: {},
                    param: {},
                    seg: {}
                },
                i = 14;
        while (i--) {
            uri.attr[key[i]] = res[i] || "";
        }
        uri.param['query'] = {};
        uri.param['fragment'] = {};
        uri.attr['query'].replace(querystringParser, function ($0, $1, $2) {
            if ($1) {
                uri.param['query'][$1] = $2;
            }
        });
        uri.attr['fragment'].replace(fragmentParser, function ($0, $1, $2) {
            if ($1) {
                uri.param['fragment'][$1] = $2;
            }
        });
        uri.seg['path'] = uri.attr.path.replace(/^\/+|\/+$/g, '').split('/');
        uri.seg['fragment'] = uri.attr.fragment.replace(/^\/+|\/+$/g, '').split('/');
        uri.attr['base'] = uri.attr.host ? uri.attr.protocol + "://" + uri.attr.host + (uri.attr.port ? ":" + uri.attr.port : '') : '';
        return uri;
    };

    function getAttrName(elm) {
        var tn = elm.tagName;
        if (tn !== undefined) return tag2Attr[tn.toLowerCase()];
        return tn;
    }
    $.fn.url = function (strictMode) {
        var url = '';
        if (this.length) {
            url = $(this).attr(getAttrName(this[0])) || '';
        }
        return $.url(url, strictMode);
    };
    $.url = function (url, strictMode) {
        if (arguments.length === 1 && url === true) {
            strictMode = true;
            url = undefined;
        }
        strictMode = strictMode || false;
        url = url || window.location.toString();
        return {
            data: parseUri(url, strictMode),
            attr: function (attr) {
                attr = aliases[attr] || attr;
                return attr !== undefined ? this.data.attr[attr] : this.data.attr;
            },
            param: function (param) {
                return param !== undefined ? this.data.param.query[param] : this.data.param.query;
            },
            fparam: function (param) {
                return param !== undefined ? this.data.param.fragment[param] : this.data.param.fragment;
            },
            segment: function (seg) {
                if (seg === undefined) {
                    return this.data.seg.path;
                } else {
                    seg = seg < 0 ? this.data.seg.path.length + seg : seg - 1;
                    return this.data.seg.path[seg];
                }
            },
            fsegment: function (seg) {
                if (seg === undefined) {
                    return this.data.seg.fragment;
                } else {
                    seg = seg < 0 ? this.data.seg.fragment.length + seg : seg - 1;
                    return this.data.seg.fragment[seg];
                }
            }
        };
    };

    //$.blockUI = function () {
    //    $('#loadingModel').modal();
    //};
    //$.unblockUI = function () {
    //    $('#loadingModel').modal('hide');
    //};
})(jQuery);