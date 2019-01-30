#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class EditorMode
{
    private static GameObject _gameObject;

    [MenuItem("Audio/Run EditoMode", false, 0)]
    public static void Init(){
        // detect if in play mode and return early
        if(Application.isPlaying == true)
            return;
        // detect if editor mode already initialized
        if(_gameObject != null)
            return;
        // receive a callback when the edito state changes
        EditorApplication.playModeStateChanged += OnStateChange;
        // initialize editor mode
        _gameObject = EditorUtility.CreateGameObjectWithHideFlags("Audio_EditorMode", HideFlags.DontSave);
        _gameObject.AddComponent<AudioPlayer>();
        // subscribe to editor updates
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    [MenuItem("Audio/Quit EditoMode", false, 1)]
    public static void Quit(){
        // only quit if initialized
        if(_gameObject == null)
            return;
        // unsubscribe from editor updates
        EditorApplication.update -= Update;
        // shut down audio engine
        _gameObject.GetComponent<AudioPlayer>().Quit();
        _gameObject = null;
    }

    private static void Update(){
        if(EditorApplication.isCompiling) {
            Quit();
        }
    }

    private static void OnStateChange(PlayModeStateChange nextState) {
        Debug.Log(nextState.ToString());
        if(nextState == PlayModeStateChange.EnteredPlayMode
            || nextState == PlayModeStateChange.ExitingEditMode){
            Quit();
        }
    }   
}
#endif