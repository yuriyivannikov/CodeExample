using System.Collections;
using System.Collections.Generic;
using GameClient;
using UnityEngine;

public class UIPopup : MonoBehaviour
{
    public enum PopupType
    {
        None,
        FooterPopup
    }
    
    public enum StateType
    {
        Showing,
        Showed,
        Hiding,
        Hided
    }
    
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
}
