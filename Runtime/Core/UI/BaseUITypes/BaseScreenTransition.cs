using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public class BaseScreenTransition : BaseView 
	{	
		[SerializeField]
		private BaseViewMotion _motion = null;
		
		protected virtual void OnTransitionScreenShowed(params object[] args) { }

		protected virtual void OnTransitionScreenHided(params object[] args) { }
		
		/// <summary>
		/// DO NOT override this function
		/// </summary>
		/// <param name="args"></param>
		protected override void OnHide(params object[] args)
		{
			_motion.HideMotion(this, () =>
			{
				OnTransitionScreenHided(args);

				OnHideCompleted?.Invoke(this);
			});
		}

		/// <summary>
		/// DO NOT override this function
		/// </summary>
		/// <param name="args"></param>
		protected override void OnShow(params object[] args)
		{
			OnTransitionScreenShowed(args);
        
			_motion.ShowMotion(this, () =>
			{
				OnShowCompleted?.Invoke(this);
			});
		}
	}
}

