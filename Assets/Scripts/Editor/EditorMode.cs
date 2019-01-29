#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class EditorMode
{
    public static bool ShutDown = false;
    private static GameObject _gameObject;

    [MenuItem("Audio/Run EditoMode", false, 0)]
    public static void Init(){
        if(_gameObject != null)
            return;
        // initialize editor mode
        _gameObject = EditorUtility.CreateGameObjectWithHideFlags("Audio_EditorMode", HideFlags.DontSave);
        _gameObject.AddComponent<AudioPlayer>();
        // subscribe to editor updates
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    [MenuItem("Audio/Quit EditoMode", false, 0)]
    public static void Quit(){
        if(_gameObject == null)
            return;
        EditorApplication.update -= Update;
        GameObject.DestroyImmediate(_gameObject);
        _gameObject = null;
    }

    private static void Update(){
        if(ShutDown) {
            ShutDown = false;
            Quit();
        }

        Debug.Log("blah");
    }
}
#endif