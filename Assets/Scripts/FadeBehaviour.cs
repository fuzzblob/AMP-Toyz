/* MIT License
 * 
 * Copyright (c) 2019 Maris Tammik & Nathan Glynn
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THe
 * SOFTWARE.
 */

using SimpleEasing;
using UnityEngine;

/// <summary>
/// Light wrapper over the Easing library for fade functionality
/// </summary>
public struct FadeBehaviour {
    public float Volume;
    public FadeType FadeType;
    
    private float _fromValue;
    private float _toValue;
    private float _duration;
    private float _fadeTime;
    private System.Action onFadeoutComplete;

    public void Reset() {
        Volume = _fromValue = _toValue = 1f;
        _fadeTime = _duration = 0f;
        FadeType = FadeType.NONE;

        onFadeoutComplete = null;
    }

    public bool StartFade(float from, float to, float duration, FadeType type) {
        if (duration == 0f
            || float.IsInfinity(duration)
            || (Volume == to && Volume == from)) {
            Reset();
            // set target value
            Volume = _toValue = _fromValue = to;
            return false;
        }
        Volume = _fromValue = Mathf.Max(0f, Mathf.Min(1f, from));
        _toValue = Mathf.Max(0f, Mathf.Min(1f, to));
        _duration = Mathf.Max(0f, duration);
        FadeType = type;

        _fadeTime = 0f;
        return true;
    }
    public void SetCallback(System.Action action) {
        onFadeoutComplete = action;
    }

    public System.Action UpdateFade(float deltaTime) {
        // advance time
        _fadeTime += deltaTime;
        float progress = Mathf.InverseLerp(0f, _duration, _fadeTime);
        // update volume
        Volume = CalculateVolume(progress);
        // return if target not reached yet
        if (_fadeTime < _duration
            && Mathf.Approximately(Volume, _toValue) == false) {
            // target not reached
            return null;
        }
        // target is reached
        Volume = _toValue;
        FadeType = FadeType.NONE;
        if (onFadeoutComplete == null)
            // no callback
            return null;
        // invoke & return callback
        System.Action returnAction = onFadeoutComplete;
        onFadeoutComplete.Invoke();
        onFadeoutComplete = null;
        return returnAction;
    }
    public float CalculateVolume(float interpolationTime) {
        switch (FadeType) {
            case FadeType.Linear:
                return EasingFunctions.Linear(_fromValue, _toValue, interpolationTime);
            case FadeType.ExpoIn:
                return EasingFunctions.ExponentialIn(_fromValue, _toValue, interpolationTime);
            case FadeType.ExpoOut:
                return EasingFunctions.ExponentialOut(_fromValue, _toValue, interpolationTime);
            case FadeType.QuadraticIn:
                return EasingFunctions.QuadraticIn(_fromValue, _toValue, interpolationTime);
            case FadeType.QuadraticOut:
                return EasingFunctions.QuadraticOut(_fromValue, _toValue, interpolationTime);
            default:
                // If fade type is none or undefined, return fade factor of 1f
                Debug.LogWarning("Hey! Invalid fade type: " + FadeType.ToString());
                return 1f;
        }
    }
}