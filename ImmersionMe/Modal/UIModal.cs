using GameClient;
using UnityEngine;

public class UIModal : MonoBehaviour
{
    public enum ModalType
    {
        None,
        BuyPreset,
        NoMoney,
        ResetPresetData
    }

    public bool IsOpened => Tween.IsOpened;
    
    public TweenComponent Tween;

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
    
    public void Open()
    {
        if (Tween.IsOpened)
            return;
            
        Tween.OpenClose();
    }
    
    public void Close()
    {
        if (!Tween.IsOpened)
            return;
        
        Tween.OpenClose();
    }
}