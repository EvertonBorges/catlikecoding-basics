using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FrameRateCounter : MonoBehaviour
{
    public enum DisplayMode { FPS, MS }
    
    [SerializeField]
	TextMeshProUGUI display = default;

	[SerializeField]
	DisplayMode displayMode = DisplayMode.FPS;

    [SerializeField, Range(0.1f, 2f)]
	float sampleDuration = 1f;

    private int frames = 0;

	private float duration = 0f;
    private float bestDuration = float.MaxValue;
    private float worstDuration = 0f;

    void Update () 
    {
		float frameDuration = Time.unscaledDeltaTime;
        frames += 1;
		duration += frameDuration;

        if (frameDuration < bestDuration)
			bestDuration = frameDuration;
		if (frameDuration > worstDuration)
			worstDuration = frameDuration;

		if (duration >= sampleDuration) 
        {
			if (displayMode == DisplayMode.FPS)
				display.SetText("FSP\n{0:1}\n{1:1}\n{2:1}", 1f / bestDuration, frames / duration, 1f / worstDuration);
			else
				display.SetText("MS\n{0:1}\n{1:1}\n{2:1}", 1000f * bestDuration, 1000f * duration / frames, 1000f * worstDuration);
			frames = 0;
			duration = 0f;
			bestDuration = float.MaxValue;
			worstDuration = 0f;
		}
	}

}
