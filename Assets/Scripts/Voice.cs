using UnityEngine;

public class Voice {
    public AudioSource Source;
    public AudioAsset Asset;
    public AudioClip Clip;

    public float Volume;
    public float Pitch;
    public FadeBehaviour Fader;

    public bool Spatialized;
    public float MinDistance;
    public float MaxDistance;
}