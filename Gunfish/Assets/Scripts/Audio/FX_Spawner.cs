using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum FXType
{
    Default,
    Bang,
    Fish_Death,
    Fish_Hit,
    Flop,
    Ground_Hit,
    Sand_Flop,
    Sand_Shoot,
    Beachball_Shoot,
    Beachball_Pop,
    Spawn,
}

public class FX_Spawner : MonoBehaviour
{
    [System.Serializable]
    public class FX_Tuple
    {
        public FXType key;
        public FXType trackKey;
        public UnityEngine.GameObject fx; 
    }

    [System.Serializable]
    public class FX_Track_Tuple
    {
        public FXType key;
        public int limit = -1;
        public bool ignore;
        public bool cycle;
        [HideInInspector]
        public List<GameObject> fx_objects = new List<GameObject>();

        public void HandleFXDestroy(FX_Object fx_obj)
        {
            fx_obj.Destroy_Event -= HandleFXDestroy;
            fx_objects.Remove(fx_obj.gameObject);
        }
    }

    public AudioMixerGroup mixer;
    private UnityEngine.GameObject holder;

    public List<FX_Tuple> Serialized_FX_Dict = new List<FX_Tuple>();
    public List<FX_Track_Tuple> Seriaized_FX_Track_Dict = new List<FX_Track_Tuple>();
    public Dictionary<FXType, FX_Tuple> FX_Dict = new Dictionary<FXType, FX_Tuple>();
    public Dictionary<FXType, FX_Track_Tuple> FX_Tracker = new Dictionary<FXType, FX_Track_Tuple>();

    public FX_Tuple fx_default;

    public GameObject healthUIPrefab, fishHealthUIPrefab;
    [HideInInspector]
    public float freezeTime;
    bool paused;

    CinemachineImpulseSource impulseSource;

    // Singleton code
    public static FX_Spawner instance;
    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (var entry in Serialized_FX_Dict)
        {
            FX_Dict[entry.key] = entry;
        }
        if (FX_Dict.ContainsKey(FXType.Default))
        {
            FX_Dict[FXType.Default] = null;
        }
        holder = new UnityEngine.GameObject("FX Objects");
        holder.transform.parent = transform;

        impulseSource = GetComponent<CinemachineImpulseSource>();
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
    }

    public void BAM(float time=0.4f) {
        impulseSource.GenerateImpulseWithVelocity(Random.insideUnitSphere);
        freezeTime = time;
    }

    public void Update() {
        if (freezeTime > 0) {
            if (paused == false) {
                paused = true;
                PauseManager.instance?.PauseTime(0, 0);
            }
            freezeTime -= Time.unscaledDeltaTime;
        }
        else if (paused == true) {
            paused = false;
            PauseManager.instance?.PauseTime(1, 0);
        }
    }

    public UnityEngine.GameObject SpawnFX(GameObject fx, Vector3 position, Vector3 rotation, float vol = -1, Transform parent = null, FXType effectName = FXType.Default)
    {
        if (fx == null) return null;

        UnityEngine.GameObject spawned_fx = Instantiate(fx, position, Quaternion.identity);


        if (spawned_fx == null) return null;

        spawned_fx.transform.parent = (parent != null ? parent : holder.transform);

        if (rotation != Vector3.zero)
            spawned_fx.transform.eulerAngles = rotation;
        FX_Object fx_obj = spawned_fx.GetComponent<FX_Object>();
        // get tracker and hook up event
        if (FX_Tracker.ContainsKey(fx_obj.track_fx_type))
        {
            fx_obj.Destroy_Event += FX_Tracker[fx_obj.track_fx_type].HandleFXDestroy;
        }
        fx_obj.vol = vol;
        fx_obj.mixerGroup = mixer;

        return spawned_fx;
    }

    public UnityEngine.GameObject SpawnFX(FXType effectName, Vector3 position, Vector3 rotation, float vol = -1, Transform parent = null)
    {
        if (!FX_Dict.ContainsKey(effectName))
            return SpawnFX(fx_default.fx, position, rotation, vol, parent, FXType.Default);

        if (!FX_Tracker.ContainsKey(FX_Dict[effectName].trackKey))
            return SpawnFX(FX_Dict[effectName].fx, position, rotation, vol, parent, effectName);


        var track_tuple = FX_Tracker[FX_Dict[effectName].trackKey];
        GameObject fx_obj = null;
        if (track_tuple.limit > -1 && track_tuple.fx_objects.Count > track_tuple.limit && !track_tuple.ignore)
        {
            if (track_tuple.cycle)
            {
                fx_obj = track_tuple.fx_objects[0];
                fx_obj.GetComponent<FX_Object>().Replay();
                track_tuple.fx_objects.Remove(fx_obj);
            }
            else
            {
                track_tuple.fx_objects[0].GetComponent<FX_Object>().Kill();
                fx_obj = SpawnFX(FX_Dict[effectName].fx, position, rotation, vol, parent, effectName);
            }
            track_tuple.fx_objects.Add(fx_obj);
        }
        return fx_obj;
    }

    public UnityEngine.GameObject SpawnFX(FXType effectName, Vector3 position, Quaternion rotation, float vol = -1, Transform parent = null)
    {
        if (!FX_Dict.ContainsKey(effectName))
            return SpawnFX(fx_default.fx, position, rotation.eulerAngles, vol, parent, FXType.Default);

        return SpawnFX(effectName, position, rotation.eulerAngles, vol: vol, parent: parent);
    }
}