using System;
using Kingmaker.Settings;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM._VM.Settings.GammaCorrection
{
	public class GammaCorrectionVM : BaseDisposable, IViewModel
	{
		public float MinValue => GammaCorrection.MinValue;
		public float MaxValue => GammaCorrection.MaxValue;
		
		public UISettingsEntitySliderFloat GammaCorrection => UISettingsRoot.Instance.GammaCorrection;
		private readonly Action m_ActionClose;
		
		public GammaCorrectionVM(Action actionClose)
		{
			m_ActionClose = actionClose;
		}
		
		public void OnLeft(float step)
		{
			GammaCorrection.SetTempValue(Mathf.Clamp(GammaCorrection.GetTempValue() - step, MinValue, MaxValue));
		}

		public void OnRight(float step)
		{
			GammaCorrection.SetTempValue(Mathf.Clamp(GammaCorrection.GetTempValue() + step, MinValue, MaxValue));
		}

		public void OnValueChanged(float value)
		{
			SettingsRoot.Graphics.GammaCorrection.SetTempValue(value);
		}

		public void Reset()
		{
			GammaCorrection.ResetToDefault(false);
		}
		
		public void Close()
		{
			SettingsRoot.Graphics.GammaCorrection.ConfirmTempValue();
			SettingsController.SaveAll();
			
			m_ActionClose?.Invoke();
		}
		
		protected override void DisposeImplementation()
        {
        	
        }
	}
}