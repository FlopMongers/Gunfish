using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;




public static class ExtensionMethods {

    public static float GetValueInRange(this Vector2 range, float normalizedValue) {
        return range.x + ((range.y - range.x) * normalizedValue);
    }

    public static float GetNormalizedValueInRange(float value, float minValue, float maxValue) {
        return (value - minValue) / (maxValue - minValue);
    }

    public static void SetGlobalScale(this Transform transform, Vector3 globalScale) {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }

    public static int RandomSign() {
        return Random.Range(0, 2) * 2 - 1;
    }

    public static void AddTorque(this Rigidbody rigidbody, Quaternion targetRotation, float magnitude = 1f) {
        rigidbody.maxAngularVelocity = 1000;

        Quaternion rotation = targetRotation * Quaternion.Inverse(rigidbody.rotation);
        var torque = new Vector3(rotation.x, rotation.y, rotation.z) * rotation.w * Time.fixedDeltaTime * magnitude;
        rigidbody.AddTorque(torque, ForceMode.VelocityChange);
    }

    public static T GetCopyOf<T>(this Component comp, T other) where T : Component {
        System.Type type = comp.GetType();
        if (type != other.GetType())
            return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos) {
            if (pinfo.CanWrite) {
                try {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos) {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddComponent<T>(this UnityEngine.GameObject go, T toAdd) where T : Component {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }

    public static T CheckAddComponent<T>(this UnityEngine.GameObject go) where T : Component {
        if (go.GetComponent<T>() == null)
            go.AddComponent<T>();
        return go.GetComponent<T>();
    }

    public static T FindComponent<T>(this UnityEngine.GameObject g, bool in_parent = true, bool in_children = true, int sibling_depth = 0, bool ignore_self = false) where T : Component {
        if (ignore_self) {
            if (in_children) {
                foreach (Transform child in g.transform) {
                    if (child.GetComponentInChildren<T>())
                        return child.GetComponentInChildren<T>();
                }
            }
            if (in_parent)
                return g.transform.parent.GetComponentInParent<T>();
        }

        if (g.GetComponent<T>() != null) {
            return g.GetComponent<T>();
        }
        if (in_children && g.GetComponentInChildren<T>() != null) {
            return g.GetComponentInChildren<T>();
        }
        if (in_parent)
            if (g.GetComponentInParent<T>() != null)
                return g.GetComponentInParent<T>();

        UnityEngine.GameObject current = g;
        while (sibling_depth > 0) {
            current = current.transform.parent.gameObject;
            if (!current)
                break;
            if (current.GetComponentInChildren<T>() != null) {
                return current.GetComponentInChildren<T>();
            }
            sibling_depth--;
        }

        return g.GetComponent<T>();
    }

    public static T[] FindComponents<T>(this UnityEngine.GameObject g, bool in_parent = true, bool in_children = true, int sibling_depth = 0, bool ignore_self = false) where T : Component {
        HashSet<T> components = new HashSet<T>();
        if (ignore_self) {
            if (in_children) {
                foreach (Transform child in g.transform) {
                    components.AddOrConcat(child.GetComponentsInChildren<T>());

                }
            }
            if (in_parent)
                components.AddOrConcat(g.transform.parent.GetComponentsInParent<T>());
            return components.ToArray();
        }

        if (!in_children && !in_parent)
            return g.GetComponents<T>();
        if (in_children)
            components.AddOrConcat(g.GetComponentsInChildren<T>());
        if (in_parent && g.transform.parent)
            components.AddOrConcat(g.transform.parent.GetComponentsInParent<T>());

        GameObject current = g;
        GameObject last = g;
        while (sibling_depth > 0) {
            current = current.transform.parent.gameObject;
            if (!current)
                break;
            components.AddOrConcat(current.GetComponentsInChildren<T>());
            sibling_depth--;
        }

        return components.ToArray();
    }

    public static float RandomInRange(this Vector2 range) {
        return Random.Range(range.x, range.y);
    }

    public static Vector3 RandomPointInBounds(this Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public static bool AddOrConcat<T>(this HashSet<T> set, IEnumerable<T> enumerable) {
        List<T> list = new List<T>(enumerable);
        if (list.Count == 1)
            return set.Add(list[0]);
        else if (list.Count > 1) {
            enumerable.Concat(enumerable);
            return true;
        }
        return false;
    }

    public static Vector3? GetRandomNavMeshLocation(Vector3 center, float range) {
        Vector3? result = null;
        Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) {
            result = hit.position;
        }
        return result;
    }

    public static Vector3? GetRandomNavMeshLocation(Bounds bounds) {
        Vector3? result = null;
        Vector3 randomPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z));
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) {
            result = hit.position;
        }
        return result;
    }
}


public static class TransformDeepChildExtension {
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName) {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0) {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }

    public static Transform[] FindDeepChildren(this Transform aParent, string aName) {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        List<Transform> children = new List<Transform>();
        while (queue.Count > 0) {
            var c = queue.Dequeue();
            if (c.name == aName)
                children.Add(c);
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return children.ToArray();
    }

    public static Transform FindDeepParent(this Transform aChild, string aName) {
        Transform node = aChild;
        while (node.parent) {
            if (node.parent.name == aName)
                return node.parent;
            node = node.parent;
        }
        return null;
    }
}

public static class Rigidbody2DExtension {
    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius) {
        var dir = (body.transform.position - explosionPosition);
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        body.AddForce(dir.normalized * (wearoff <= 0f ? 0f : explosionForce) * wearoff, ForceMode2D.Impulse);
    }

    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier) {
        var dir = (body.transform.position - explosionPosition);
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        Vector3 baseForce = dir.normalized * (wearoff <= 0f ? 0f : explosionForce) * wearoff;
        body.AddForce(baseForce, ForceMode2D.Impulse);

        float upliftWearoff = 1 - upliftModifier / explosionRadius;
        Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
        body.AddForce(upliftForce, ForceMode2D.Impulse);
    }
}

public static class DictionaryExtension {
    public static V SetDefault<K, V>(this IDictionary<K, V> dict, K key, V @default) {
        V value;
        if (!dict.TryGetValue(key, out value)) {
            dict.Add(key, @default);
            return @default;
        }
        else {
            return dict[key];
        }
    }

    public static TValue GetValueOrDefault<TKey, TValue>
    (this IDictionary<TKey, TValue> dictionary,
    TKey key,
    TValue defaultValue) {
        TValue value;
        return dictionary.TryGetValue(key, out value) ? value : defaultValue;
    }

    public static void Shuffle<T>(this IList<T> list) {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static IEnumerator CoShuffle<T>(this IList<T> list, int blockSize = 100) {
        System.Random rng = new System.Random();
        int n = list.Count;
        int currentBlock = 0;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
            currentBlock++;
            if (currentBlock == blockSize) {
                currentBlock = 0;
                yield return null;
            }
        }
    }

    public static T GetRandom<T>(this IList<T> list) {
        return list[Random.Range(0, list.Count)];
    } 

    public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB) {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
        return list;
    }

    public static T Choose<T>(this Dictionary<T, float> dictProbs, T defaultValue) {
        Dictionary<int, T> indexToKeys = new Dictionary<int, T>();
        List<float> probs = new List<float>();
        int index = 0;
        foreach (T key in dictProbs.Keys) {
            indexToKeys[index] = key;
            probs.Add(dictProbs[key]);
            index++;
        }

        float total = 0;

        foreach (float elem in probs) {
            total += elem;
        }

        float randomPoint = Random.value * total;

        for (int i = 0; i < probs.Count; i++) {
            if (randomPoint < probs[i]) {
                return indexToKeys[i];
            }
            else {
                randomPoint -= probs[i];
            }
        }
        return defaultValue;
    }
}


public static class Helper {

    public static Vector3 ScreenToWorld(RectTransform rectTransform) {
        return Camera.main.ScreenToWorldPoint(rectTransform.transform.position);
    }

    public static Vector2 WorldToScreen(Transform transform) {
        return RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
    }

    public static Vector2 DegreeToVector2(float degree) {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }

    public static Vector2 RadianToVector2(float radian) {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }

    public static T[] FindComponentsInChildrenWithTag<T>(this GameObject parent, string tag, bool forceActive = false) where T : Component {
        if (parent == null) { throw new System.ArgumentNullException(); }
        if (string.IsNullOrEmpty(tag) == true) { throw new System.ArgumentNullException(); }
        List<T> list = new List<T>(parent.GetComponentsInChildren<T>(forceActive));
        if (list.Count == 0) { return null; }

        for (int i = list.Count - 1; i >= 0; i--) {
            if (list[i].CompareTag(tag) == false) {
                list.RemoveAt(i);
            }
        }
        return list.ToArray();
    }

    public static T FindComponentInChildWithTag<T>(this GameObject parent, string tag, bool forceActive = false) where T : Component {
        if (parent == null) { throw new System.ArgumentNullException(); }
        if (string.IsNullOrEmpty(tag) == true) { throw new System.ArgumentNullException(); }

        T[] list = parent.GetComponentsInChildren<T>(forceActive);
        foreach (T t in list) {
            if (t.CompareTag(tag) == true) {
                return t;
            }
        }
        return null;
    }
}