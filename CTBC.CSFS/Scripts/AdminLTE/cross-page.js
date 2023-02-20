$(function () {
    //cross webpage keep last active accordion url 

    //Menu可展開是利用下列個層的active,下列為開展Action2這個Menu
    //<li class="treeview active">
    //    <a href="#">
    //        <i class="fa fa-angle-double-right"></i>
    //        <span>UI Elements</span>
    //        <i class="fa fa-angle-left pull-right"></i>
    //    </a>
    //    <ul class="treeview-menu">
    //        <li><a href="/Controller/Action1">Action1</a></li>
    //        <li class="active"><a href="/Controller/Action2">Action2</a></li>
    //    </u>
    //</li>

    var url = window.location;
    //alert(url);
    var elementLevel3 = $('ul.sidebar-menu li.treeview ul.treeview-menu li.treeview ul.treeview-menu li a').filter(function () {
        return this.href == url;
    });

    //記憶Menu Level 3上次開啟之URL
    if ($(elementLevel3).attr('href') != null)
    {
        var newParent1 = elementLevel3.parent('li').first().addClass('active');
        //alert('newParent1' + $(newParent1).attr("id"));
        var newParent2 = newParent1.parent('ul').first();
        //alert('newParent2' + $(newParent2).attr("id"));
        var newParent3 = newParent2.parent('li').first().addClass('active');
        //alert('newParent3' + $(newParent3).attr("id"));
        var newParent4 = newParent3.parent('ul').first().parent('li').first().addClass('active');
    } else {
        //記憶Menu Level 2上次開啟之URL
        var elementLevel2 = $('ul.treeview-menu li a').filter(function () {
            return this.href == url;
        });
        //* add by Ge.Song strart
        if (elementLevel2.length > 0) {
            var href = url.href;
            setCookie('AdminLTEoldMenu', href);
        } else {
            if (url.pathname == "/")
                setCookie('AdminLTEoldMenu', "");

            var old = getCookie('AdminLTEoldMenu');
            if (old != null) {
                var href2 = old;
                //alert(href2);
                elementLevel2 = $('ul.treeview-menu li a').filter(function () {
                    return this.href == href2;
                });
            }
        }
        //* add by Ge.Song end


        var newParent5 = elementLevel2.parent('li').first().addClass('active');
        var newParent6 = newParent5.parent('ul').first();
        var newParent7 = newParent6.parent('li').first().addClass('active');
    }


    function setCookie(name, value) {
        var days = 30;
        document.cookie = name + "=" + escape(value) + ";path=/";
    }

    //读取cookies 
    function getCookie(name) {
        var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");

        if (arr = document.cookie.match(reg))
            return unescape(arr[2]);
        else
            return null;
    }

});