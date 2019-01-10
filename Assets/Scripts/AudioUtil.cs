using UnityEngine;
public static class AudioUtil
{
    private static float twelfthRootOfTwo = Mathf.Pow(2f, 1.0f / 12f);

    /// <summary>
    /// Converts a given semitone value to a pitch factor
    /// </summary>
    /// <param name="st">Semitones</param>
    /// <returns></returns>
    public static float SemitoneToPitchFactor(float st)
    {
        return Mathf.Clamp(Mathf.Pow(twelfthRootOfTwo, st), 0f, 4f);
    }

    /// <summary>
    /// Converts a given pitch factor to a semitone value
    /// </summary>
    /// <param name="pitch">pitch factor</param>
    /// <returns></returns>
    public static float PitchFactorToSemitone(float pitch)
    {
        return Mathf.Log(pitch, twelfthRootOfTwo);
    }

    /// <summary>
    /// Converts a given Decibel value to a volume factor
    /// </summary>
    /// <param name="dB">Decibel value</param>
    /// <returns>Linear Volume Factor</returns>
    public static float DecibelToVolumeFactor(float dB)
    {
        if (dB > -80)
            return Mathf.Clamp01(Mathf.Pow(10.0f, dB / 20.0f));
        else
            return 0;
    }

    /// <summary>
    /// Converts a given volume factor to a Decibel value
    /// </summary>
    /// <param name="linear">Linear volume factor</param>
    /// <returns>Decimebl value</returns>
    public static float VolumeFactorToDecibel(float linear)
    {
        if (linear > 0)
            return Mathf.Clamp(20.0f * Mathf.Log10(linear), -80f, 0f);
        else
            return -80.0f;
    }
}
