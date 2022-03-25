using GameClient;
using UnityEngine;

public class UIScreen : MonoBehaviour
{
    public enum ScreenType
    {
        None,
        PresetsScreen,
        PresetEditorScreen,
        PlayerScreen,
        TimerScreen,
        SettingsScreen,
        ShopScreen
    }
    
    public enum StateType
    {
        Showing,
        Showed,
        Hiding,
        Hided
    }
    
    public bool IsOpened => Tween.IsOpened;
    
    public TweenComponent Tween;
    
    public StateType State { get; private set; } = StateType.Showing;
    
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public virtual void SetData(object data)
    {
        
    }
    
    public virtual object GetData()
    {
        return null;
    }
    
    public void OpenClose()
    {
        Tween.OpenClose();
    }
    
    public virtual void Open()
    {
        if (Tween.IsOpened)
            return;
            
        Tween.OpenClose();
    }
    
    public virtual void Close()
    {
        if (!Tween.IsOpened)
            return;
        
        Tween.OpenClose();
    }
}
