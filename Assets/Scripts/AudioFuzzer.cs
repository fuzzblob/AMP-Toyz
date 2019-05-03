using System.Collections.Generic;
using UnityEngine;

public class AudioFuzzer : MonoBehaviour {
    public int Amount = 10;
    public List<AudioAsset> Sounds;
    private List<Voice> _voices;

    void Start() {
        _voices = new List<Voice>(Amount);
        for(int i = 0; i < Amount; i++){
            Play();
        }
    }

    public void Update() {
        for(int i = _voices.Count - 1; i >= 0; i--) {
            if(_voices[i].FinishedPlaying){
                _voices.RemoveAt(i);
                Play();
            }
        }
    }
    private void Play() {
        int index = Random.Range(0, Sounds.Count);
        _voices.Add(Sounds[index].Play());
    }
}