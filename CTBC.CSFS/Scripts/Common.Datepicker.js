function numsDatepicker(obj) {
    $(obj).datepicker({
        showOn: "both",
        buttonImage: getRootURL() + "Content/img/calendar.jpg",//$('<i class="fa fa-calendar"></i>'),
        buttonImageOnly: true,
        dateFormat: 'yy/mm/dd',
        buttonText: '日曆',
        inline: true,
        changeMonth: true,
        changeYear: true,
        constrainInput: true,
        beforeShow: function () {
            $(".ui-datepicker").css('font-size', 13);
        }
    });
}