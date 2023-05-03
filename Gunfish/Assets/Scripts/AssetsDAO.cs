using UnityEngine;

public static class AssetsDAO {
    public static T[] LoadScriptableObjects<T>() where T : ScriptableObject {
        return Resources.LoadAll<T>("");
    }
}
