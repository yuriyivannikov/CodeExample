using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Settings;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM._VM.Settings.KeyBindSetupDialog;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM._ConsoleView.Settings.KeyBindSetupDialog
{
	public class KeyBindingSetupDialogConsoleView : ViewBase<KeyBindingSetupDialogVM>
    {
        [SerializeField]
        private TextMeshProUGUI m_PressedKeysText;

        [SerializeField]
        private TextMeshProUGUI m_BindingIsOccupied;
        
        [SerializeField]
        private Color m_NormalColor = Color.white;
        [SerializeField]
        private Color m_TempColor = new Color(.8f, .8f, .8f);
        [SerializeField]
        private Color m_OccupiedColor = Color.red;

        [SerializeField]
        private OwlcatButton m_CloseButton;

        [SerializeField]
        private OwlcatButton m_UnbindButton;
        
        [Header("Animator")]
        [SerializeField]
        private FadeAnimator m_Animator;
        
        private UIKeyboardTexts m_KeyboardTexts;
        
        private bool m_IsShowed;
        
        public void Initialize()
        {
            gameObject.SetActive(false);
            m_Animator.Initialize();

            m_BindingIsOccupied.text = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI
                .HotkeyInUseErrorMessage;
        }
        
        protected override void BindViewImplementation()
        {
            Show();// need show before start coroutine
            
            m_KeyboardTexts = UIStrings.Instance.KeyboardTexts;

            Coroutine bindingRoutine = StartCoroutine(BindingRoutine());
            AddDisposable(Disposable.Create(() => StopCoroutine(bindingRoutine)));
            
            Game.Instance.UISettingsManager.IsNewKeyBindingSelectionHappening = true;
            
            AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(ViewModel.Close));
            AddDisposable(m_UnbindButton.OnLeftClickAsObservable().Subscribe(ViewModel.Unbind));

            m_BindingIsOccupied.gameObject.SetActive(false);
        }
        
        private void Show()
        {
            if (m_IsShowed)
                return;
			
            m_IsShowed = true;
			
            m_Animator.AppearAnimation();
            UISoundController.Instance.Play(UISoundType.SettingsKeyBindingOpen);
        }

        public void Hide()
        {
            if (!m_IsShowed)
                return;

            m_Animator.DisappearAnimation(() =>
            {
                gameObject.SetActive(false);
                m_IsShowed = false;
            });
			
            UISoundController.Instance.Play(UISoundType.SettingsKeyBindingClose);
        }
		
		private IEnumerator BindingRoutine()
        {
            while (true)
            {
                yield return null;
                if (!Game.Instance.UISettingsManager.IsNewKeyBindingSelectionHappening) { yield break; }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ViewModel.Close();
                    yield break;
                }
                
                bool hasBinding = GetValidBinding(out KeyBindingData keyBindingData);
                DisplayPressedKeys();

                if (!hasBinding)
                {
                    continue;
                }

                ViewModel.OnBindingChosen(keyBindingData);
            }
        }

        private void DisplayPressedKeys()
        {
            KeyBindingData keyBindingData = GetTempBinding();
            m_BindingIsOccupied.gameObject.SetActive(false);

            bool tempBinding = true;
            // if we don't press anything right now, chose last pressed binding
            if (keyBindingData.Key == KeyCode.None && 
                !keyBindingData.IsCtrlDown && !keyBindingData.IsAltDown && !keyBindingData.IsShiftDown)
            {
                tempBinding = false;
                keyBindingData = ViewModel.CurrentKeyBinding;
                // if last pressed binding is none, set empty text
                if (keyBindingData.Key == KeyCode.None)
                {
                    m_PressedKeysText.text = string.Empty;
                    return;
                }
            }
            
            m_PressedKeysText.color = tempBinding
                ? m_TempColor
                : ViewModel.CurrentBindingIsOccupied ? m_OccupiedColor : m_NormalColor;
            m_PressedKeysText.text = keyBindingData.GetPrettyString();

            if (ViewModel.CurrentBindingIsOccupied)
            {
                m_BindingIsOccupied.gameObject.SetActive(true);
            }
        }

        private KeyBindingData GetTempBinding()
        {
            KeyBindingData keyBindingData = new KeyBindingData();
            if (Input.anyKey && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                keyBindingData.IsCtrlDown = KeyboardAccess.IsCtrlHold();
                keyBindingData.IsAltDown = KeyboardAccess.IsAltHold();
                keyBindingData.IsShiftDown = KeyboardAccess.IsShiftHold();
                
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (!Input.GetKey(key))
                    {
                        continue;
                    }

                    if (key == KeyCode.LeftShift || key == KeyCode.RightShift)
                    {
                        continue;
                    }
                    
                    if (key == KeyCode.LeftAlt || key == KeyCode.RightAlt)
                    {
                        continue;
                    }
                    
                    if (key == KeyCode.LeftControl || key == KeyCode.RightControl)
                    {
                        continue;
                    }

                    keyBindingData.Key = key;
                    break;
                }
            }

            return keyBindingData;
        }
        
        private bool GetValidBinding(out KeyBindingData keyBindingData)
        {
            keyBindingData = new KeyBindingData() {Key = KeyCode.None};

            if (CommandKeyDown())
            {
                return false;
            }
            else if (Input.anyKeyDown && !CommandKeyDown() && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            {
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (!Input.GetKeyDown(key))
                    {
                        continue;
                    }

                    keyBindingData.Key = key;
                    keyBindingData.IsCtrlDown = KeyboardAccess.IsCtrlHold();
                    keyBindingData.IsAltDown = KeyboardAccess.IsAltHold();
                    keyBindingData.IsShiftDown = KeyboardAccess.IsShiftHold();
                    return true;
                }
            }
            else if (CommandKeyUp() && !CommandKeyHold())
            {
                KeyCode key;
                if (KeyboardAccess.IsAltUp()) { key = KeyCode.LeftAlt; }
                else if (KeyboardAccess.IsCtrlUp()) { key = KeyCode.LeftControl; }
                else if (KeyboardAccess.IsShiftUp()) { key = KeyCode.LeftShift; }
                else return false;

                keyBindingData.Key = key;
                return true;
            }
            
            return false;
        }

        private bool CommandKeyDown()
        {
            return KeyboardAccess.IsAltDown() || KeyboardAccess.IsCtrlDown() || KeyboardAccess.IsShiftDown();
        }

        private bool CommandKeyUp()
        {
            return KeyboardAccess.IsAltUp() || KeyboardAccess.IsCtrlUp() || KeyboardAccess.IsShiftUp();
        }

        private bool CommandKeyHold()
        {
            return KeyboardAccess.IsAltHold() || KeyboardAccess.IsCtrlHold() || KeyboardAccess.IsShiftHold();
        }
        
		protected override void DestroyViewImplementation()
		{
            Game.Instance.UISettingsManager.IsNewKeyBindingSelectionHappening = false;
            Hide();
		}
	}
}