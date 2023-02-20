//* 登入
(function () {
	if (typeof (jQuery) === 'undefined') {
		//* need jquery 1.4+
		alert('jQuery Library NotFound.');
		return;
	}
	var $ = jQuery;
	var csfsUpload = $.CsfsUpload = {};
	jQuery.extend(csfsUpload, {
		
		//* init
		documentReady: function () {
			$("#fileAtt1").bind("change", selectLastFile);
			$(".ctdelete").bind("click", btnDeleteClick);

			//* 選擇了最後一個上傳元件,再新增一個
			function selectLastFile() {
				//* file的name
				$(this).attr("name", "fileAttNames");
				//* 改寫前面textbox值
				if ($("#fileAtt" + fid).val() != "") {
					fid++;
					$("#divUploadList").append('<tr><td><input type="file" name="" id="fileAtt' + fid + '" class="type-file-file col-sm-12 no-padding" /></td></tr>');
					$(this).parent().parent().append('<td><a class="ctdelete">移除</a></td>');
					$(".type-file-file").bind("change", selectLastFile);
					$(".ctdelete").bind("click", btnDeleteClick); 
				}
			};
			function btnDeleteClick(e) {
				//e.preventDefault();
			    var $s = $(this).parent().parent();
				$s.remove();
			};
		}
		
	});
	//===========================================================================================
	jQuery(document).ready(function () {
		$.CsfsUpload.documentReady();
	});
})(jQuery);

var fid = 1;