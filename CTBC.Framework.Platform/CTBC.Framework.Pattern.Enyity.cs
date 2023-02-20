using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Xml;
using System.IO;
using CTBC.FrameWork.Platform;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.Entity;

namespace CTBC.FrameWork.Pattern
{
	public class AppEntity<C, E> : EntityObject
		where E : EntityObject
		where C : ObjectContext
	{
		// properties
		public C OC = null;									// EDM object context
		public E EO = null;
		public string OrderBy = "";							// name of the order by column
		public int PageSize = 15;							// number of rows per page
        public ObjectQuery<E> Q;

		// constructor
		public AppEntity(C oc, E eo) {
			this.OC = oc;
			this.EO = eo;
            Q = OC.CreateQuery<E>("[" + typeof(E).Name + "]");
		}

		// Query by Page
		public object QueryByPage(dynamic d, int page = 1)
		{
			//ObjectQuery<E> Q = OC.CreateQuery<E>("[" + typeof(E).Name + "]");
			int Rows = Q.Count();
			int Page = page;
			d.Rows = Rows;
			d.Pages = (Rows / PageSize) + ((Rows % PageSize) == 0 ? 0 : 1);
			if (Page > d.Pages) Page = d.Pages;
			d.Page = Page;
			return Q.OrderBy("it." + this.OrderBy).Page(Page, PageSize);
		}

		// Query
		public List<E> Query()
		{
			ObjectQuery<E> Q = OC.CreateQuery<E>("[" + typeof(E).Name + "]");
			return Q.ToList();
		}

		// Query
		public ObjectQuery<E> Query(string predicate, params ObjectParameter[] parameters)
		{
			ObjectQuery<E> Q = OC.CreateQuery<E>("[" + typeof(E).Name + "]");
			return Q.Where(predicate, parameters);
		}

		// Execute (Insert, Update, Delete)
		public string Execute(E eo, ModelStateDictionary ModelState, string cmd = "?")
		{
			// get entity key
			EntityKey Key = null;
			try { Key = (eo.EntityKey == null ? OC.CreateEntityKey(eo.GetType().Name, eo) : eo.EntityKey); }
			catch { }
			
			if (ModelState.IsValid)
			{
				// update
				if (cmd == "U")
					try {
						OC.AttachTo(Key.EntitySetName, eo);
						OC.ObjectStateManager.ChangeObjectState(eo, EntityState.Modified);
						OC.SaveChanges();
						return "OK";
					} catch (Exception e) { return "ERROR! " + e.Message; }

				// delete
				else if (cmd == "D")
					try	{
						OC.AttachTo(Key.EntitySetName, eo);
						OC.ObjectStateManager.ChangeObjectState(eo, EntityState.Deleted);
						OC.SaveChanges();
						return "OK";
					} catch (Exception e) { return "ERROR! " + e.Message; }

				// insert
				else if (cmd == "C")
					try
					{
						OC.AddObject(Key.EntitySetName, eo);
						OC.SaveChanges();
						return "OK";
					}
					catch (Exception e) { return "ERROR! " + e.Message; }
			}
			return string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
		}

		// Dispose
		public void Dispose()
		{
			OC.Dispose();
		}
	}

}
