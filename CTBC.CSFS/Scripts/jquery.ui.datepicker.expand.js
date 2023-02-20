(function($) {	
$.extend($.datepicker, {
formatDate: function (format, date, settings) {
var d = date.getDate();
var m = date.getMonth()+1;
var y = date.getFullYear();			
var fm = function(v){			
return (v<10 ? '0' : '')+v;
};
var fy = function (v) {
    return (v < 100 ? v < 10 ? '00' : "0" : '') + v;
};
return fy(y - 1911) + '/' + fm(m) + '/' + fm(d);
},
parseDate: function (format, value, settings) {
var v = new String(value);
var Y,M,D;
if(v.length==7){/*1001215*/
Y = v.substring(0,3)-0+1911;
M = v.substring(3,5)-0-1;
D = v.substring(5,7)-0;
return (new Date(Y,M,D));
}else if(v.length==6){/*981215*/
Y = v.substring(0,2)-0+1911;
M = v.substring(2,4)-0-1;
D = v.substring(4,6)-0;
return (new Date(Y,M,D));
}
return (new Date());
},
formatYear:function(v){
//return '民國'+(v-1911)+'年';
return '民國'+(v-1911)+'年';
}
});	
})(jQuery);
