using UnityEngine;

[CreateAssetMenu(fileName = "AudioAsset", menuName = "Audio/Create Asset")]
public class AudioAsset : ScriptableObject
{
    public AudioClip clip;

    public void Play(){
        var go = new GameObject("Source");
        var s = go.AddComponent<AudioSource>();
        s.clip = clip;
        s.spatialBlend = 0f;
        
        s.Play();
    }
}
