using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using MEC;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI
{
	// MyButton that's meant to work with mouse or touch-based devices.
	[AddComponentMenu("UI/SlowButton", 30)]
	public class SlowButton : Button
	{
		public event Action OnActionNonDelay;
		
		private Animator _animator;

		private static readonly int ShakeTrigger = Animator.StringToHash("Shake");

		protected override void Start()
		{
			base.Start();
			_animator = GetComponent<Animator>();
		}

		protected override void OnDisable()
		{
			Timing.KillCoroutines("buttonShake");
		}

		private void Press()
		{
			if (!IsActive() || !IsInteractable())
				return;

			if (OnActionNonDelay != null)
			{
				OnActionNonDelay();
			}
			
			Timing.RunCoroutine(DelayOnClicked());
		}

		IEnumerator<float> DelayOnClicked()
		{
			if (transition == Transition.Animation)
				yield return Timing.WaitForSeconds(0.15f);
			else
				yield return Timing.WaitForOneFrame;
			
			Timing.KillCoroutines("buttonShake");
			onClick.Invoke();
		}

		// Trigger all registered callbacks.
		public override void OnPointerClick(PointerEventData eventData)
		{
			if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
				return;
			
			Press();
		}

		public void Shake(float delayTime)
		{
			Timing.RunCoroutine(ShakeCoroutine(delayTime), "buttonShake");
		}
		
		IEnumerator<float> ShakeCoroutine(float delayTime)
		{
			while (true)
			{
				yield return Timing.WaitForSeconds(delayTime);
				
				_animator.SetTrigger(ShakeTrigger);
			}
		}
	}
}
