using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;


namespace CTBC.FrameWork.Platform
{
	public class AppException
	{
		// enumerations
		public enum Policy { Shielding, Wrapping, Replacing, Logging };

		// properties
		public string FunctionId = "";
		public string LocationId = "";
		public Policy PolicyId = Policy.Logging;

		private ExceptionManager E = null;

		//---------------------------------------
		//			Public Method
		//---------------------------------------
		
		// constructor
		public AppException()
		{
			// get default exception manager
			try	{
				E = EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>();
			}
			catch { }
		}

        // exception handler
        public void Handle(Action action)
        {
            E.Process(action, PolicyName(this.PolicyId));
        }

		// exception handler
		public void Handle<TResult>(Func<TResult> Action)
		{
            E.Process(Action, PolicyName(this.PolicyId));
		}

        // exception handler
        public void Handle<TResult>(Func<TResult> Action, string locationId)
        {
            this.LocationId = locationId;
            E.Process(Action, PolicyName(this.PolicyId));
        }

        // exception handler
        public void Handle(Action action, string locationId)
        {
            this.LocationId = locationId;
            E.Process(action, PolicyName(this.PolicyId));
        }

        // exception handler
        public void Handle(Action action, string locationId, Policy policyId)
        {
            this.LocationId = locationId;
            this.PolicyId = policyId;
            E.Process(action, PolicyName(this.PolicyId));
        }

		// exception handler
		public void Handle<TResult>(Func<TResult> Action, string locationId, Policy policyId)
		{
            this.LocationId = locationId;
            this.PolicyId = policyId;
            E.Process(Action, PolicyName(this.PolicyId));
		}

        private string PolicyName(Policy policyId)
        {
            string ls_PolicyName = "";
            if (policyId == Policy.Logging)
                ls_PolicyName = "Logging";
            else if (policyId == Policy.Replacing)
                ls_PolicyName = "Replacing";
            else if (policyId == Policy.Wrapping)
                ls_PolicyName = "Wrapping";
            else if (policyId == Policy.Shielding)
                ls_PolicyName = "Shielding";
            return ls_PolicyName;
        }
		
	}
}
