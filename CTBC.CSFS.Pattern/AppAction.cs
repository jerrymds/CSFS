/// <summary>
/// 程式說明:Action基類
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace CTBC.CSFS.Pattern
{
	//------------------------------------------------
	//      以下為繼承CTBC.FrameWork.Platform元件,請勿更動
	//------------------------------------------------
	public class AppAction : CTBC.FrameWork.Pattern.AppAction
	{
		public AppAction(CTBC.FrameWork.Pattern.AppController controller) : base(controller) { }
	}

	public class AppActionQuery<C, E> : CTBC.FrameWork.Pattern.AppActionQuery<C, E>
		where E : EntityObject
		where C : ObjectContext
	{
		public AppActionQuery(C oc, E eo, CTBC.FrameWork.Pattern.AppController controller) : base(oc, eo, controller) { }
	}

	public class AppActionQueryByPage<C, E> : CTBC.FrameWork.Pattern.AppActionQueryByPage<C, E>
		where E : EntityObject
		where C : ObjectContext
	{
		public AppActionQueryByPage(C oc, E eo, CTBC.FrameWork.Pattern.AppController controller) : base(oc, eo, controller) { }
	}

	public class AppActionExecute<C, E> : CTBC.FrameWork.Pattern.AppActionExecute<C, E>
		where E : EntityObject
		where C : ObjectContext
	{
		public AppActionExecute(C oc, E eo, CTBC.FrameWork.Pattern.AppController controller, string cmd) : base(oc, eo, controller, cmd) { }
	}
	//------------------------------------------------
	//      以上為繼承CTBC.FrameWork.Platform元件,請勿更動
	//------------------------------------------------
}
