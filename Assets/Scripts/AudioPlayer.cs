using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;

[ExecuteInEditMode]
public class AudioPlayer : MonoBehaviour
{
    // culling volume -60dB
    const float cullingVolume = 0.001f;

    public static AudioPlayer Instance { get; private set; }
    private SourcePool pool;
    private List<Voice> activeAudio;
    private AudioListener listener;

    void Awake(){
        Instance = this;
        if(Application.isPlaying)
            DontDestroyOnLoad(this);
        // setup runtime data structures
        pool = new SourcePool(0, 16);
        activeAudio = new List<Voice>();
        listener = (AudioListener)FindObjectOfType(typeof(AudioListener));
    }

    private void OnApplicationQuit() {}

    public void Quit(){
        foreach(Voice voice in activeAudio)
            pool.Put(voice.Source);
        activeAudio.Clear();
        pool.Quit();
    }

    void LateUpdate(){
        float deltaTime = Time.deltaTime; // TODO: maybe use Time.unscaledDeltaTime?
        // update state of all Voices
        for(int i = activeAudio.Count - 1; i >= 0; i--){
            Voice voice = activeAudio[i];
            if(voice == null){
                activeAudio.RemoveAt(i);
                continue;
            }
            // clean up list of playing audio
            if(voice.Source != null)
            {
                if(voice.Source.isPlaying == false) {
                    pool.Put(voice.Source);
                    activeAudio.RemoveAt(i);
                    continue;
                }
                voice.PlaybackPosition = voice.Source.time;
            }
            else {
                voice.PlaybackPosition += (Time.deltaTime * voice.Pitch);
            }
            // TODO: stop virtual voice if PlaybackPosition == Clip.length
            // TODO: only if looping
            voice.PlaybackPosition = voice.PlaybackPosition % voice.Clip.length;
            // TODO: fade out at end of file?
            /* if(voice.Source.loop == false
                && voice.Fader.FadeType == FadeType.NONE
                && voice.Asset.FadeTypeOut != FadeType.NONE
                && voice.Source.time + voice.Asset.FadeTimeOut >= voice.Source.clip.length)
            {
                voice.Fader.Fade(
                    voice.Fader.FadeVolume, 0f,
                    voice.Asset.FadeTimeOut, voice.Asset.FadeTypeOut);
            }*/
            // claculate fade volume factor
            if(voice.Fader.FadeType != FadeType.NONE) {
                voice.Fader.UpdateFade(deltaTime);
            }
            float distanceVolume = 1f;
            if(voice.Attenuation != AttenuationMode.None){
                distanceVolume = CalculateDistanceVolume(voice);
            }
            float volume = voice.Volume * voice.Fader.Volume * distanceVolume;
            //value with the fadeFactor here
            if(volume <= cullingVolume){
                if(voice.Source != null) {
                    Debug.Log("virtualizing");
                    voice.Source.Stop();
                    pool.Put(voice.Source);
                    voice.Source = null;
                }
            }
            else if(EnsureSource(voice)){
                if(voice.Source.isPlaying == false)
                {
                    AssignSourceProperties(voice, distanceVolume);
                    voice.Source.Play();
                }
                else
                    voice.Source.volume = volume;
            }
        }
    }
    private float CalculateDistanceVolume(Voice voice){
        float distanceVolume = 1f;
        Vector3 headPos = listener.transform.position;
        float distance = Vector3.Distance(headPos, voice.Position);
        float time = Mathf.InverseLerp(voice.MinDistance, voice.MaxDistance, distance);       
        switch (voice.Attenuation){
            case AttenuationMode.Linear:
                distanceVolume = 1f - time;
                break;
            case AttenuationMode.Logarithmic:
                distanceVolume = EasingFunctions.ExponentialOut(1f, 0f, time);
                break;
            default: 
                break;

        }
        return distanceVolume;
            
    }

    public Voice Play(AudioAsset sound) {
        Voice voice = new Voice() {
            Asset = sound
        };
        voice.Fader.Reset();
        // get AudioClip
        voice.Clip = GetNextClip(sound);
        voice.PlaybackPosition = 0f;
        // fading in
        if(sound.FadeTypeIn != FadeType.NONE){
            voice.Fader.Fade(0f, 1f, sound.FadeTimeIn, sound.FadeTypeIn);
        }
        // apply pitch and volume randomization
        float pitchSemitones = sound.PitchBase + Random.Range(-sound.PitchOffset / 2f, sound.PitchOffset / 2f);
        float volumeDecibels = sound.VolumeBase + Random.Range(-sound.VolumeOffset / 2f, sound.VolumeOffset / 2f);
        voice.Pitch = AudioUtil.SemitoneToPitchFactor(pitchSemitones);
        voice.Volume = AudioUtil.DecibelToVolumeFactor(volumeDecibels);
        // set distance attentuation properties
        voice.Attenuation = sound.Attenuation;
        voice.MinDistance = sound.MinimumDistance;
        voice.MaxDistance = sound.MaximumDistance;
        activeAudio.Add(voice);
        float distanceVolume = CalculateDistanceVolume(voice);
        // assign AudioSource
        if((voice.Volume * voice.Fader.Volume * distanceVolume) > cullingVolume
            && EnsureSource(voice))
        {
            AssignSourceProperties(voice, distanceVolume);
            voice.Source.Play();
        }
        else
            Debug.LogError("can't assign AudioSource for sound asset " + sound);
        return voice;
    }
    private bool EnsureSource(Voice voice){
        if(voice.Source != null)
            return true;
        Debug.Log("getting source");
        AudioSource s = pool.Get();
        if(s == null){
            return false;
        }
        voice.Source = s;
        return true;
    }
    private void AssignSourceProperties(Voice voice, float distanceVolume = 1f){
        voice.Source.clip = voice.Clip;
        voice.Source.time = voice.PlaybackPosition;
        voice.Source.pitch = voice.Pitch;
        voice.Source.spatialBlend = voice.Attenuation == AttenuationMode.None ? 0f : 1f;
        voice.Source.volume = voice.Volume * voice.Fader.Volume * distanceVolume;
    }

    public void Stop(Voice voice, float fadeOutTime = -1f){
        if(voice.Asset.FadeTypeOut == FadeType.NONE){
            StopVoice(voice);
            return;
        }
        // calculate fade time
        if(fadeOutTime >= 0f)
            fadeOutTime = Mathf.Min(fadeOutTime, voice.Asset.FadeTimeOut);
        else
            fadeOutTime = voice.Asset.FadeTimeOut;
        // start fading
        voice.Fader.Fade(
            voice.Fader.Volume, 0f,
            fadeOutTime, voice.Asset.FadeTypeIn,
            // make the fader stop the voice when done
            delegate { StopVoice(voice); });
    }

    private void StopVoice(Voice voice)
    {
        voice.Source.Stop();
        pool.Put(voice.Source);
        voice.Source = null;
        activeAudio.Remove(voice);
    }

    private AudioClip GetNextClip(AudioAsset sound){
        int index = sound.LastIndex;
        int length = sound.Clips.Length;

        switch(sound.PlayMode){
        case PlaylistMode.Sequence:
            if(++index >= length)
                index = 0;
            break;
        case PlaylistMode.Random:
            do{
                index = Random.Range(0, length);
            }
            // prevent repeating index unless only two options
            while(index == sound.LastIndex && length > 2);
            break;
        case PlaylistMode.Shuffle:
            index = GetNextShuffled(sound, index, length);
            break;
        case PlaylistMode.WeightedRandom:
            index = GetNextWeightedRandom(sound);
            break;
        default:
            Debug.LogError(sound.PlayMode + " is not implemented yet!");
            return null;
        }
        sound.LastIndex = index;
        return sound.Clips[index];
    }

    private int GetNextShuffled(AudioAsset sound, int index, int length){
        if(sound.ShuffleQueue == null)
            sound.ShuffleQueue = new Queue<int>(length);
        if(sound.ShuffleQueue.Count ==0){
            // TODO: infinyte loop if playlist empty?
            while(index == sound.LastIndex)
                index = Random.Range(0, length);
            sound.ShuffleQueue.Enqueue(index);
            // reshuffle
            while(sound.ShuffleQueue.Count < length){
                int num = Random.Range(0, length);
                if(sound.ShuffleQueue.Contains(num) == false)
                    sound.ShuffleQueue.Enqueue(num);
            }
        }
        
        return index;
    }

    private int GetNextWeightedRandom(AudioAsset sound){
        //TODO: UX issue here that is maybe unintuitive: total values don't always add to 100%,
        //they can be more or less!
        //We account for that here by getting a random number using the sum of all weights,
        //then using each weight value as a threshold for "landing on" the associated clip

        float sum = 0f;
        for (int i = 0; i < sound.Weights.Count; i++){
            sum += sound.Weights[i];
        }

        float random = Random.Range(0f, sum);
        float nextThreshold = 0f;

        int index = 0;
        for (int i = 0; i < sound.Weights.Count; i++) {
            nextThreshold += sound.Weights[i];
            if (random < nextThreshold) {
                index = i;
                break;
            }
        }
        
        return index;
    }
}
