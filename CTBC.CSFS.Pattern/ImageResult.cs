/// <summary>
/// 程式說明:圖片Result
/// </summary>

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web.Mvc;

namespace CTCB.NUMS.Pattern
{
	public class ImageResult : ActionResult
	{
		public ImageResult() { }
		public Image Image { get; set; }
		public ImageFormat ImageFormat { get; set; }
		public int Index { get; set; }
		public FrameDimension FrameDim { get; set; }

		public override void ExecuteResult(ControllerContext context)
		{
			// verify properties 
			if (Image == null)
			{
				throw new ArgumentNullException("Image");
			}
			if (ImageFormat == null)
			{
				throw new ArgumentNullException("ImageFormat");
			}
			// output 
			context.HttpContext.Response.Clear();
			if (ImageFormat.Equals(ImageFormat.Bmp)) context.HttpContext.Response.ContentType = "image/bmp";
			if (ImageFormat.Equals(ImageFormat.Gif)) context.HttpContext.Response.ContentType = "image/gif";
			if (ImageFormat.Equals(ImageFormat.Icon)) context.HttpContext.Response.ContentType = "image/vnd.microsoft.icon";
			if (ImageFormat.Equals(ImageFormat.Jpeg)) context.HttpContext.Response.ContentType = "image/jpeg";
			if (ImageFormat.Equals(ImageFormat.Png)) context.HttpContext.Response.ContentType = "image/png";
			if (ImageFormat.Equals(ImageFormat.Tiff)) context.HttpContext.Response.ContentType = "image/tiff";
			if (ImageFormat.Equals(ImageFormat.Wmf)) context.HttpContext.Response.ContentType = "image/wmf";

			//if (ImageFormat.Equals(ImageFormat.Tiff))
			//{
				Image.SelectActiveFrame(FrameDim, Index);
			//}

			Image.Save(context.HttpContext.Response.OutputStream, ImageFormat);
		}
	}

}
