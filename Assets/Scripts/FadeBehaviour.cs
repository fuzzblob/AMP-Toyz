using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;

/// <summary
/// Light wrapper over the Easing library for fade functionality
/// </summary>
public static class FadeBehaviour
{
    public static float GetFadeFactor(FadeType fadeType, float from, float to, float t) {
        switch(fadeType) {
            case FadeType.Linear:
                return EasingFunctions.Linear(from, to, t);
            case FadeType.ExpoIn:
                return EasingFunctions.ExponentialIn(from, to, t);
            case FadeType.ExpoOut:
                return EasingFunctions.ExponentialOut(from, to, t);
            case FadeType.QuadraticIn:
                return EasingFunctions.QuadraticIn(from, to, t);
            case FadeType.QuadraticOut:
                return EasingFunctions.QuadraticOut(from, to, t);

            //If fade type is none or undefined, return the current value
            case FadeType.NONE:
                return from;
            default:
                return from;
        }
    }
}
