using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.Areas.Director.Controllers
{
    public class DirectorToSignController : Controller
    {
        [HttpGet]
        // GET: Director/DirectorToSign
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(PARMMenuVO model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            string str = "0";
            #region  保存附件
            try
            {
                //* 取得model裏其他值
                string title = model.TITLE;


                foreach (var fileAttName in fileAttNames)
                {

                    if (fileAttName != null && fileAttName.ContentLength > 0)
                    {
                        
                    }
                }
            }
            catch
            {
                return Content("0");
            }
            #endregion
            return Content(str);
        }
    }
}