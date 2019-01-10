using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance { get; private set; }
    private SourcePool pool;
    private List<AudioSource> activeAudio;

    [Header("Debug")]
    public AudioAsset sound;

    void Awake(){
        Instance = this;

        pool = new SourcePool(0, 16);
        activeAudio = new List<AudioSource>();
    }

    void LateUpdate(){
        for(int i = activeAudio.Count - 1; i >= 0; i--){
            if(activeAudio[i].isPlaying)
                continue;
            pool.Put(activeAudio[i]);
            activeAudio.RemoveAt(i);
        }
    }

    public void Play(AudioAsset sound){
        var s = pool.Get();
        if(s == null)
            return;
        Debug.Log("got AudioSource");
        s.clip = sound.clip;
        s.Play();
        activeAudio.Add(s);
    }
}
