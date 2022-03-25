using UnityEngine;
using TMPro;

public class HUDFPS : MonoBehaviour 
{
	// Attach this to a GUIText to make a frames/second indicator.
	//
	// It calculates frames/second over each updateInterval,
	// so the display does not keep changing wildly.
	//
	// It is also fairly accurate at very low FPS counts (<10).
	// We do this not by simply counting frames per interval, but
	// by accumulating FPS for each frame. This way we end up with
	// correct overall FPS even if the interval renders something like
	// 5.5 frames.

	public TextMeshProUGUI TextField;
	public float updateInterval = 0.5f;
	 
	private float _accum; // FPS accumulated over the interval
	private int   _frames; // Frames drawn over the interval
	private float _timeleft; // Left time for current interval
	 
	private void Start()
	{
	    if( !TextField )
	    {
	        Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
	        enabled = false;
	        return;
	    }
	    _timeleft = updateInterval;  
	}
	 
	private void Update()
	{
	    _timeleft -= Time.deltaTime;
	    _accum += Time.timeScale/Time.deltaTime;
	    ++_frames;
	 
	    // Interval ended - update GUI text and start new interval
	    if(_timeleft <= 0.0 )
	    {
	        // display two fractional digits (f2 format)
			var fps = _accum/_frames;
			var format = $"{fps:F2}";
			TextField.text = format;

			if(fps < 10)
				TextField.color = Color.red;
			else if (fps < 30)
				TextField.color = Color.yellow;
			else
				TextField.color = Color.green;

			//	DebugConsole.Log(format,level);
			_timeleft = updateInterval;
			_accum = 0.0F;
			_frames = 0;
	    }
	}
}