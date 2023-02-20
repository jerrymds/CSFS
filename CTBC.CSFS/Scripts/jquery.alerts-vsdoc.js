// jQuery Alert Dialogs Plugin
//
// Version 1.1.1
//
// Cory S.N. LaViska
// A Beautiful Site (http://abeautifulsite.net/)
// 14 May 2009
//
// Visit http://abeautifulsite.net/notebook/87 for more information
//
// Usage:
//      jAlert( message, [title, callback] )
//      jConfirm( message, [title, callback] )
//      jPrompt( message, [value, title, callback] )
// 
// History:
//
//      1.00 - Released (29 December 2008)
//
//      1.01 - Fixed bug where unbinding would destroy all resize events
//
//      1.1.1 - 簡化呼叫的方式
//      1.1.2 - Ge.Song : 調整適應bootstrap ui 方案 2014/09/19 
// License:
// 
// This plugin is dual-licensed under the GNU General Public License and the MIT License and
// is copyright 2008 A Beautiful Site, LLC. 
//
(function ($) {

    $.alerts = {

        // These properties can be read/written by accessing $.alerts.propertyName from your scripts at any time

        verticalOffset: 0,                // vertical offset of the dialog from center screen, in pixels
        horizontalOffset: 0,                // horizontal offset of the dialog from center screen, in pixels/
        repositionOnResize: false,           // re-centers the dialog on window resize
        draggable: false,                    // make the dialogs draggable (requires UI Draggables plugin)
        okButton: $("#j_confirm_ok").val(),         // text for the OK button
        cancelButton: $("#j_confirm_cancel").val(), // text for the Cancel button
        dialogClass: null,                  // if specified, this class will be applied to all dialogs

        // Public methods

        alert: function (type, message, title, callback, width) {
            if (title == null) {
                if (type.tol == 'success') {
                    title = '成功';
                } else if (type == 'error') {
                    title = '失敗';
                } else {
                    title = '警告';
                }
            }
            $.alerts._show(title, message, null, type, function (result) {
                if (callback) callback(result);
            }, width);
        },

        confirm: function (message, title, callback) {
            if (title == null) title = '確認';
            $.alerts._show(title, message, null, 'confirm', function (result) {
                if (callback) callback(result);
            });
        },

        prompt: function (message, value, title, callback) {
            if (title == null) title = '提示';
            $.alerts._show(title, message, value, 'prompt', function (result) {
                if (callback) callback(result);
            });
        },

        // Private methods

        _show: function (title, msg, value, type, callback, width) {

            $.alerts._hide();
            $.alerts._overlay('show');

            top.$("BODY").append(
                '<div id="popup_container" class="modal fade in" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true" style="display: block;" >' +
                    '<div class="modal-dialog">' +
                        '<div class="modal-content">' +
                            '<div class="modal-header" >' +
                                '<div>' +
                                    '<h4 id="popup_title" style="margin: 0px;"></h6>' +
                                '</div>' +
                            '</div>' +
                            '<div id="popup_content" class="modal-body">' +
                                '<p id="popup_message"></p>' +
                            '</div>' +
                            '<div class="modal-footer" id="modal_footer" style="padding:15px"></div>' +
                      '</div>' +
                      '</div>' +
                '</div>');

            if ($.alerts.dialogClass) top.$("#popup_container").addClass($.alerts.dialogClass);
            //* 20141118 增加图标
            switch (type) {
                case 'info':
                    top.$("#popup_container").addClass("notification notification-info");
                    break;
                case 'warning':
                case 'confirm':
                    top.$("#popup_container").addClass("notification notification-warning");
                    break;
                case 'success':
                    top.$("#popup_container").addClass("notification notification-success");
                    break;
                case 'error':
                    top.$("#popup_container").addClass("notification notification-error");
                    break;
            }

            // IE6 Fix
            //var pos = ($.browser.msie && parseInt($.browser.version) <= 6) ? 'absolute' : 'fixed';
            var pos = 'fixed';

            top.$("#popup_container").css({
                position: pos,
                zIndex: 99999,
                padding: 0,
                margin: 0
            });
            //* 标题
            top.$("#popup_title").text(title);
            //* Body
            if (typeof (msg) != "object") {
                top.$("#popup_message").text(msg);
                top.$("#popup_message").html(top.$("#popup_message").text().replace(/\n/g, '<br />'));
            } else {
                top.$("#popup_message").html(msg.html());
            }
            if (width != null) {
                top.$("#popup_container").css({ width: width });
            }
            top.$("#popup_message").css({
                minWidth: "100%",
                maxWidth: "100%"
            });
            top.$("#popup_container").css({
                minWidth: top.$("#popup_container").outerWidth(),
                maxWidth: top.$("#popup_container").outerWidth()
            });

            $.alerts._reposition();
            $.alerts._maintainPosition(true);

            switch (type) {
                case 'info':
                case 'warning':
                case 'success':
                case 'error':
                    top.$("#popup_content").addClass('');
                    //top.$("#popup_message").after('<div id="popup_panel"><input type="button" value="' + $.alerts.okButton + '" id="popup_ok" /></div>');
                    top.$("#modal_footer").html('<div id="popup_panel"><input type="button" value="' + $.alerts.okButton + '" id="popup_ok" class="btn btn-primary" /></div>');
                    top.$("#popup_ok").click(function () {
                        $.alerts._hide();
                        callback(true);
                    });
                    top.$("#popup_ok").focus().keypress(function (btn) {
                        if (btn.keyCode == 13 || btn.keyCode == 27) top.$("#popup_ok").trigger('click');
                    });
                    break;
                case 'confirm':
                    top.$("#popup_container").addClass(type);
                    //top.$("#popup_message").after('<div id="popup_panel"><input type="button" value="' + $.alerts.okButton + '" id="popup_ok" /> <input type="button" value="' + $.alerts.cancelButton + '" id="popup_cancel" /></div>');
                    top.$("#modal_footer").html('<div id="popup_panel"><input type="button" value="' + $.alerts.okButton + '" id="popup_ok" class="btn btn-primary" /> <input type="button" value="' + $.alerts.cancelButton + '" id="popup_cancel" class="btn btn-default" /></div>');
                    top.$("#popup_ok").click(function () {
                        $.alerts._hide();
                        if (callback) callback(true);
                    });
                    top.$("#popup_cancel").click(function () {
                        $.alerts._hide();
                        if (callback) callback(false);
                    });
                    top.$("#popup_ok").focus();
                    top.$("#popup_ok, #popup_cancel").keypress(function (btn) {
                        if (btn.keyCode == 13) top.$("#popup_ok").trigger('click');
                        if (btn.keyCode == 27) top.$("#popup_cancel").trigger('click');
                    });
                    break;
                case 'prompt':
                    top.$("#popup_container").addClass(type);
                    //top.$("#popup_message").after('<br /><input type="text" size="30" id="popup_prompt" />').after('<div id="popup_panel"><input type="button" value="' + $.alerts.okButton + '" id="popup_ok" /> <input type="button" value="' + $.alerts.cancelButton + '" id="popup_cancel" /></div>');
                    top.$("#popup_message").after('<br /><input type="text" id="popup_prompt" class = "input-xlarge" />');
                    top.$("#modal_footer").html('<div id="popup_panel"><input type="button" value="' + $.alerts.okButton + '" id="popup_ok" class="btn btn-primary" /> <input type="button" value="' + $.alerts.cancelButton + '" id="popup_cancel" class="btn btn-default" /></div>');
                    top.$("#popup_prompt").width(top.$("#popup_message").width());
                    top.$("#popup_ok").click(function () {
                        var val = top.$("#popup_prompt").val();
                        $.alerts._hide();
                        if (callback) callback(val);
                    });
                    top.$("#popup_cancel").click(function () {
                        $.alerts._hide();
                        if (callback) callback(null);
                    });
                    top.$("#popup_prompt, #popup_ok, #popup_cancel").keypress(function (btn) {
                        if (btn.keyCode == 13) top.$("#popup_ok").trigger('click');
                        if (btn.keyCode == 27) top.$("#popup_cancel").trigger('click');
                    });
                    if (value) top.$("#popup_prompt").val(value);
                    top.$("#popup_prompt").focus().select();
                    break;
            }

            // Make draggable
            if ($.alerts.draggable) {
                try {
                    top.$("#popup_container").draggable({ handle: top.$("#popup_title") });
                    top.$("#popup_title").css({ cursor: 'move' });
                } catch (e) { /* requires jQuery UI draggables */ }
            }
        },

        _hide: function () {
            top.$("#popup_container").remove();
            $.alerts._overlay('hide');
            $.alerts._maintainPosition(false);
        },

        _overlay: function (status) {
            switch (status) {
                case 'show':
                    $.alerts._overlay('hide');
                    top.$("BODY").append('<div id="popup_overlay"></div>');
                    top.$("#popup_overlay").css({
                        //position: 'absolute',
                        position: 'fixed',
                        zIndex: 99998,
                        top: '0',
                        left: '0',
                        width: '100%',
                        height: '100%',
                        backgroundColor: 'black',
                        opacity: '0.5'
                        //filter: 'Alpha(Opacity = 0)',
                        //height: $(window.top).height() + 'px'
                    });
                    break;
                case 'hide':
                    top.$("#popup_overlay").remove();
                    break;
            }
        },

        _reposition: function () {
            var itop = (($(window.top).height() / 2) - (top.$("#popup_container").outerHeight() / 2)) + $.alerts.verticalOffset;
            var ileft = (($(window.top).width() / 2) - (top.$("#popup_container").outerWidth() / 2)) + $.alerts.horizontalOffset;

            if (itop < 0) itop = 0;
            if (ileft < 0) ileft = 0;

            // IE6 fix
            //if ($.browser.msie && parseInt($.browser.version) <= 6) itop = itop + top.$(window.top).scrollTop();

            top.$("#popup_container").css({
                top: itop + 'px',
                left: ileft + 'px'
            });
            //top.$("#popup_overlay").height('100%');
        },

        _maintainPosition: function (status) {
            if ($.alerts.repositionOnResize) {
                switch (status) {
                    case true:
                        top.$(window.top).bind('resize', $.alerts._reposition);
                        break;
                    case false:
                        top.$(window.top).unbind('resize', $.alerts._reposition);
                        break;
                }
            }
        }

    }

    // Shortuct functions
    window.jAlert = function (type, message, title, callback, width) {
        $.alerts.alert(type, message, title, callback, width);
    };

    window.jConfirm = function (message, title, callback) {
        $.alerts.confirm(message, title, callback);
    };

    window.jPrompt = function (message, value, title, callback) {
        $.alerts.prompt(message, value, title, callback);
    };

    window.jAlertSuccess = function (message, callback, width) {
        $.alerts.alert('success', message, '成功', callback, width);
    };
    window.jAlertError = function (message, callback, width) {
        $.alerts.alert('error', message, '失敗', callback, width);
    };

})(jQuery);
