using UnityEngine;

public class Voice : System.IComparable {
    public bool FinishedPlaying;

    public AudioSource Source;
    public AudioAsset Asset;
    public Vector3 Position;
    public AudioClip Clip;
    public byte Priority;
    public float PlaybackPosition;
    public bool Looping;

    public float OutputVolume;
    public float Volume;
    public float Pitch;
    public FadeBehaviour Fader;

    public AttenuationMode Attenuation;
    public float MinDistance;
    public float MaxDistance;

    public int CompareTo(object obj)
    {
        if(obj is Voice == false)
            return 1;
        Voice other = obj as Voice;
        int p = Priority.CompareTo(other.Priority);
        // if priorities are different, return
        if(p != 0) return p;
        // if priorities are the same use volume comparison
        p = OutputVolume.CompareTo(other.OutputVolume);
        // invert volume comparison favoring high values
        return (p == 1 ? -1 : (p == -1 ? 1 : 0));
    }


}