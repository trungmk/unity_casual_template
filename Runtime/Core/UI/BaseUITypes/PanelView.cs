using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public class PanelView : BaseView 
	{
		protected virtual void OnPanelShowed(params object[] args) { }

		protected virtual void OnPanelHided(params object[] args) { }
	
		/// <summary>
		/// DO NOT override this function
		/// </summary>
		/// <param name="args"></param>
		protected override void OnHide(params object[] args)
		{
			OnPanelHided(args);

			OnHideCompleted?.Invoke(this);
		}

		/// <summary>
		/// DO NOT override this function
		/// </summary>
		/// <param name="args"></param>
		protected override void OnShow(params object[] args)
		{
			OnPanelShowed(args);

			OnShowCompleted?.Invoke(this);
		}
	}
}


