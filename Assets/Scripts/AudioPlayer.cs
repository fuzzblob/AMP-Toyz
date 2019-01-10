using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance { get; private set; }
    private SourcePool pool;
    private List<AudioSource> activeAudio;

    [Header("Debug")]
    public AudioAsset Sound;

    void Awake(){
        Instance = this;
        // setup runtime data structures
        pool = new SourcePool(0, 16);
        activeAudio = new List<AudioSource>();
    }

    void LateUpdate(){
        // clean up list of playing audio
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
        // set source properties
        s.clip = sound.Clip;
        // apply pitch and volume randomization
        float pitchSemitones = sound.PitchBase + Random.Range(-sound.PitchOffset / 2f, sound.PitchOffset / 2f);
        float volumeDecibels = sound.VolumeBase + Random.Range(-sound.VolumeOffset / 2f, sound.VolumeOffset / 2f);
        s.pitch = AudioUtil.SemitoneToPitchFactor(pitchSemitones);
        s.volume = AudioUtil.DecibelToVolumeFactor(volumeDecibels);
        // playe the AudioSource
        s.Play();
        activeAudio.Add(s);
    }
}
