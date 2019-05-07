using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;

[ExecuteInEditMode]
public class AudioPlayer : MonoBehaviour
{
    // culling volume -60dB
    const float cullingVolume = 0.001f;
#if UNITY_EDITOR
    public static bool DebugMode = false;
#else
    public static bool DebugMode = false;
#endif
    public static AudioPlayer Instance { get; private set; }
    private SourcePool pool;
    private List<Voice> activeAudio;
    private AudioListener listener;
    private bool activeAudioAdded;
    private VoiceComparer voiceComp;

    public static AudioPlayer SafeInstance() {
        if (Instance != null)
            return Instance;

        GameObject go = new GameObject("AudioPlayer");
        go.AddComponent<AudioPlayer>();
        return Instance;
    }

    void Awake() {
        if(Instance != null) {
            if (Instance != this && DebugMode)
                Debug.LogError("AudioPlayer already has an instance.");
            this.enabled = false;
            return;
        }

        Instance = this;
        if(Application.isPlaying)
            DontDestroyOnLoad(this);

        // setup runtime data structures
        pool = new SourcePool(AudioSettings.GetConfiguration().numRealVoices);
        activeAudio = new List<Voice>();
        voiceComp = new VoiceComparer();
        listener = (AudioListener)FindObjectOfType(typeof(AudioListener));
    }

    private void OnDestroy() {
        Quit();
    }
    private void OnApplicationQuit() {
        Quit();
    }
    public void Quit(){
        foreach(Voice voice in activeAudio)
            pool.Put(voice.Source);
        activeAudio.Clear();
        pool.Quit();
    }

    void LateUpdate(){
        UpdateVoices();
        AssignSources();  
    }
    private void UpdateVoices() {
        float deltaTime = Time.deltaTime; // TODO: maybe use Time.unscaledDeltaTime?
        
        // update state of all Voices
        for(int i = activeAudio.Count - 1; i >= 0; i--){
            Voice voice = activeAudio[i];
            // TODO: remove when priority system is all checked in
            // update playback position
            if(voice.Source != null) {
                if(voice.Source.isPlaying == false
#if UNITY_EDITOR
                    && UnityEditor.EditorApplication.isPaused == false
#endif
                    )
                {
                    // source stopped playing
                    Stop(voice);
                    continue;
                }
                // keep current time
                voice.PlaybackPosition = voice.Source.time;
            }
            else {
                // advance time
                voice.PlaybackPosition += (deltaTime * voice.Pitch);
                if(voice.Looping) {
                    // ensure safe range
                    voice.PlaybackPosition %= voice.Clip.length;
                }
                else if(voice.PlaybackPosition >= voice.Clip.length){
                    // virtual voice finished playback
                    Stop(voice);
                    continue;
                }
            }
            
            // claculate fade volume factor
            if(voice.Fader.FadeType != FadeType.NONE) {
                voice.Fader.UpdateFade(deltaTime);
                // TODO: fade out at end of file?
                /* if(voice.Source.loop == false
                    && voice.Fader.FadeType == FadeType.NONE
                    && voice.Asset.FadeTypeOut != FadeType.NONE
                    && voice.Source.time + voice.Asset.FadeTimeOut >= voice.Source.clip.length)
                */
                // TODO: break out, the voice has stopped playing
                // Alternatively, simply calculate volume and let the loop end on it's own.
                //var action = voice.Fader.UpdateFade(deltaTime);
                //if (action == delegate { Stop(voice); })
            }
            // distance volume
            float distanceVolume = 1f;
            if(voice.Attenuation != AttenuationMode.None) {
                distanceVolume = CalculateDistanceVolume(voice);
            }
            voice.OutputVolume = voice.Volume * voice.Fader.Volume * distanceVolume;
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
    
    private void AssignSources() {
        // TODO: profile for Garbace GC
        //activeAudio.Sort(voiceComp);
        InsertSort(activeAudio);

        int count = activeAudio.Count;
        int lastSourceSteal = activeAudio.Count - 1;
        for(int i = 0; i < count; i++) {
            Voice voice = activeAudio[i];
             // value with the fadeFactor here
            if(voice.OutputVolume <= cullingVolume) {
                // below culling volume
                if(voice.Source != null) {
                    // virtualize voice
                    voice.Source.Stop();
                    pool.Put(voice.Source);
                    voice.Source = null;
                }
                continue;
            }
            else if(voice.Source != null){
                continue;
            }
            
            bool assignedSource = false;
            if(pool.GetAvaialbleCount() > 0) {
                voice.Source = pool.Get();
                assignedSource = true;
            }
            else {
                // attempt stealing source 
                for(; lastSourceSteal >= 0 && lastSourceSteal > i; lastSourceSteal--){
                    if(activeAudio[lastSourceSteal].Source == null)
                        continue;
                    // steal source
                    voice.Source = activeAudio[lastSourceSteal].Source;
                    activeAudio[lastSourceSteal].Source = null;
                    assignedSource = true;
                    break;
                }
            }
            
            // setup source properties
            if(assignedSource == false)
                continue;
            
            if(DebugMode)
                Debug.LogWarning("de virtualizing");
            // didn't have a source but claimed one
            AssignSourceProperties(voice);
            voice.Source.Play();
        }
    }
    private void InsertSort<T>(List<T> list) where T : System.IComparable {
        // insert sort
        int n = list.Count, i, ii;
        T val;
        for (i = 1; i < n; i++) {
            val = list[i];
            int flag = 0;
            for (ii = i - 1; ii >= 0 && flag != 1;) {
                if (val.CompareTo(list[ii]) < 0) {
                    list[ii + 1] = list[ii];
                    ii--;
                    list[ii + 1] = val;
                }
                else flag = 1;
            }
        }
    }

    public void Stop(Voice voice) {
        if(voice.Source != null) {
            voice.Source.Stop();
            pool.Put(voice.Source);
        }
        voice.Source = null;
        activeAudio.Remove(voice);
        voice.FinishedPlaying = true;
        // TODO: pool voice to avoid GC? optimization?            
    }
    public Voice Play(AudioAsset sound) {
        Voice voice = new Voice() {
            Asset = sound
        };
        voice.Fader.Reset();
        voice.Priority = sound.Priority;
        // get AudioClip
        voice.Clip = GetNextClip(sound);
        voice.PlaybackPosition = 0f;
        // fading in
        if(sound.FadeTypeIn != FadeType.NONE){
            voice.Fader.StartFade(0f, 1f, sound.FadeTimeIn, sound.FadeTypeIn);
        }
        // apply pitch and volume randomization
        float pitchSemitones = sound.PitchBase + Random.Range(-sound.PitchOffset / 2f, sound.PitchOffset / 2f);
        float volumeDecibels = sound.VolumeBase + Random.Range(-sound.VolumeOffset / 2f, sound.VolumeOffset / 2f);
        voice.Pitch = AudioUtil.SemitoneToPitchFactor(pitchSemitones);
        voice.Volume = AudioUtil.DecibelToVolumeFactor(volumeDecibels);
        voice.Looping = sound.Looping;
        // set distance attentuation properties
        voice.Attenuation = sound.Attenuation;
        voice.MinDistance = sound.MinimumDistance;
        voice.MaxDistance = sound.MaximumDistance;
        
        float distanceVolume = CalculateDistanceVolume(voice);
        voice.OutputVolume = (voice.Volume * voice.Fader.Volume * distanceVolume);
        // assign AudioSource
        if(voice.OutputVolume > cullingVolume
            && pool.GetAvaialbleCount() > 0)
        {
            voice.Source = pool.Get();
            AssignSourceProperties(voice);
            voice.Source.Play();
        }
        else if(DebugMode)
            Debug.LogWarning("can't assign AudioSource for sound asset " + sound + " (" + voice.Priority + ")");
        
        activeAudio.Add(voice);
        activeAudioAdded = true;
        return voice;
    }

    public class VoiceComparer : IComparer<Voice>
    {
        // Compares by Height, Length, and Width.
        public int Compare(Voice x, Voice y)
        {
            int p = x.Priority.CompareTo(y.Priority);
            // if priorities are different, return
            if(p != 0) return p;
            // if priorities are the same use volume comparison
            p = x.OutputVolume.CompareTo(y.OutputVolume);
            // invert volume comparison favoring high values
            return (p == 1 ? -1 : (p == -1 ? 1 : 0));
        }
    }

    private void AssignSourceProperties(Voice voice){
        voice.Source.priority = voice.Priority;
        voice.Source.clip = voice.Clip;
        voice.Source.time = voice.PlaybackPosition;
        voice.Source.loop = voice.Looping;
        voice.Source.pitch = voice.Pitch;
        voice.Source.spatialBlend = voice.Attenuation == AttenuationMode.None ? 0f : 1f;
        voice.Source.volume = voice.OutputVolume;
    }

    public void Stop(Voice voice, float fadeOutTime = -1f){
        if(voice.Asset.FadeTypeOut == FadeType.NONE){
            Stop(voice);
            return;
        }
        // calculate fade time
        if(fadeOutTime >= 0f)
            fadeOutTime = Mathf.Min(fadeOutTime, voice.Asset.FadeTimeOut);
        else
            fadeOutTime = voice.Asset.FadeTimeOut;
        // start fading
        voice.Fader.StartFade(
            voice.Fader.Volume, 0f,
            fadeOutTime, voice.Asset.FadeTypeIn);
        // make the fader stop the voice when done
        voice.Fader.SetCallback(delegate { Stop(voice); });
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
