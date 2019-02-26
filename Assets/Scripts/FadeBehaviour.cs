using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SimpleEasing;

public enum FadeAction {
    NONE = 0,
    Stop,
}

/// <summary>
/// Light wrapper over the Easing library for fade functionality
/// </summary>
public struct FadeBehaviour
{
    public float FromValue;
    public float ToValue;
    public float Duration;

    public float Volume;
    public float FadeTime;

    public FadeType FadeType;
    
    private UnityEvent onFadeoutComplete;

    public void Reset(){
        Volume = 
        FromValue = 1f;
        ToValue = 1f;
        Duration = 0f;
        FadeType = FadeType.NONE;
        FadeTime = 0f;
        onFadeoutComplete = new UnityEvent();
    }

    public void Fade(float from, float to, float duration, FadeType type = FadeType.Linear, UnityAction action = null) {
        FromValue = from;
        ToValue = to;
        Duration = duration;
        FadeType = type;

        FadeTime = 0f;

        if(action != null)
        {
            onFadeoutComplete.AddListener(action);
        }
    }

    public void UpdateFade(float deltaTime){
        FadeTime += deltaTime;
        float interpolationTime =  Mathf.InverseLerp(0f, Duration, FadeTime);    
        switch(FadeType) {
            case FadeType.Linear:
                Volume = EasingFunctions.Linear(FromValue, ToValue, interpolationTime);
                break;
            case FadeType.ExpoIn:
                Volume = EasingFunctions.ExponentialIn(FromValue, ToValue, interpolationTime);
                break;
            case FadeType.ExpoOut:
                Volume = EasingFunctions.ExponentialOut(FromValue, ToValue, interpolationTime);
                break;
            case FadeType.QuadraticIn:
                Volume = EasingFunctions.QuadraticIn(FromValue, ToValue, interpolationTime);
                break;
            case FadeType.QuadraticOut:
                Volume = EasingFunctions.QuadraticOut(FromValue, ToValue, interpolationTime);
                break;
            default:
                // If fade type is none or undefined, return fade factor of 1f
                Debug.LogWarning("Hey! Invalid fade type: " + FadeType.ToString());
                Volume = 1f;
                break;
        }

        //Debug.Log($"FadeVolume = {Volume}");

        if(Mathf.Approximately(Volume, ToValue)){
            // target is reached
            Volume = ToValue;
            // stop updating this fader
            FadeType = FadeType.NONE;

            onFadeoutComplete.Invoke();
            onFadeoutComplete.RemoveAllListeners();
        }
    }
}
