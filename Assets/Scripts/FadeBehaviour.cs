using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;

/// <summary
/// Light wrapper over the Easing library for fade functionality
/// </summary>
public static class FadeBehaviour
{
    public static float GetFadeFactor(FadeType fadeType, float t) {
        switch(fadeType) {
            case FadeType.LinearIn:
                return EasingFunctions.Linear(0f, 1f, t);
            case FadeType.LinearOut:
                return EasingFunctions.Linear(1f, 0f, t);
            case FadeType.ExpoIn:
                return EasingFunctions.ExponentialIn(0f, 1f, t);
            case FadeType.ExpoOut:
                return EasingFunctions.ExponentialOut(1f, 0f, t);
            case FadeType.QuadraticIn:
                return EasingFunctions.QuadraticIn(0f, 1f, t);
            case FadeType.QuadraticOut:
                return EasingFunctions.QuadraticOut(1f, 0f, t);

            //If fade type is none or undefined, return fade factor of 1f
            case FadeType.NONE:
                return 1f;
            default:
                return 1f;
        }
    }
}
