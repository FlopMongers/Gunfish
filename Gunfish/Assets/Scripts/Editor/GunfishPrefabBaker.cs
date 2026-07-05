using UnityEditor;
using UnityEngine;

public static class GunfishPrefabBaker {
    const string OutputFolder = "Assets/Resources/Prefabs/Player/Fish";

    [MenuItem("Tools/Gunfish/Bake All Roster Fish Prefabs")]
    public static void BakeAllRosterFish() {
        var list = AssetDatabase.LoadAssetAtPath<GunfishDataList>(
            "Assets/Resources/ScriptableObjects/GunfishList.asset");
        if (list == null) {
            Debug.LogError("GunfishPrefabBaker: could not load GunfishList.asset");
            return;
        }

        int success = 0;
        foreach (var data in list.gunfishes) {
            if (BakeFish(data)) success++;
        }
        Debug.Log($"GunfishPrefabBaker: baked {success}/{list.gunfishes.Count} fish.");
    }

    public static bool BakeFish(GunfishData data) {
        if (data == null) {
            Debug.LogError("GunfishPrefabBaker: null GunfishData passed to BakeFish.");
            return false;
        }
        if (data.gun == null) {
            Debug.LogError($"GunfishPrefabBaker: {data.name} has no gun data assigned; skipping.");
            return false;
        }
        if (data.segmentCount < 3) {
            Debug.LogError($"GunfishPrefabBaker: {data.name} has segmentCount < 3; skipping.");
            return false;
        }

        GameObject scratchDriverObject = null;
        GameObject rootSegment = null;
        try {
            // scratch driver, needed only because GunfishGenerator's ctor takes a Gunfish
            // reference to stamp onto segments (GunfishSegment.gunfish / .index)
            scratchDriverObject = new GameObject("ScratchGunfishDriver");
            var scratchGunfish = scratchDriverObject.AddComponent<Gunfish>();
            scratchGunfish.data = data;

            var generator = new GunfishGenerator(scratchGunfish);
            var segments = generator.Generate(0, Vector3.zero); // layer 0 while baking

            rootSegment = segments[0];
            rootSegment.name = $"{data.name}Body";

            // gun: parented under RootSegment (not a sibling, per single-root requirement)
            var gunInstance = (GameObject)PrefabUtility.InstantiatePrefab(data.gun.gunPrefab, rootSegment.transform);
            var gun = gunInstance.GetComponent<Gun>();

            // gunSprite
            var gunSpriteInstance = (GameObject)PrefabUtility.InstantiatePrefab(data.gun.gunSpritePrefab, rootSegment.transform);
            gunSpriteInstance.transform.localPosition = new Vector3(data.gunOffset.position.x, data.gunOffset.position.y);
            float gunLength = gunSpriteInstance.GetComponentInChildren<SpriteRenderer>().sprite.texture.width;
            float desiredWorldLength = gunLength * (data.length / data.spriteMat.mainTexture.width);
            float currentWorldLength = gunSpriteInstance.GetComponentInChildren<SpriteRenderer>().sprite.bounds.size.x;
            gunSpriteInstance.transform.localScale *= desiredWorldLength / currentWorldLength;

            // barrels
            foreach (TransformTuple tuple in data.gun.gunBarrels) {
                var barrelInstance = (GameObject)PrefabUtility.InstantiatePrefab(data.gun.gunBarrelPrefab, rootSegment.transform);
                barrelInstance.transform.localPosition = tuple.position;
                barrelInstance.transform.localEulerAngles = Vector3.forward * tuple.rotation;
                gun.barrels.Add(barrelInstance.GetComponent<GunBarrel>());
            }

            // 4 root-only components. CompositeCollisionDetector is added but NEVER Init()'d here —
            // its own Start() would double-init on prefab load; Gunfish.Spawn's explicit
            // .Init(true, true, true) call remains the sole real initializer at runtime.
            rootSegment.CheckAddComponent<Destroyer>();
            rootSegment.CheckAddComponent<CollisionDamageReceiver>();
            rootSegment.CheckAddComponent<CompositeCollisionDetector>();
            rootSegment.CheckAddComponent<GroundDetector>();

            if (!AssetDatabase.IsValidFolder(OutputFolder)) {
                // ensure nested folder chain exists: Assets/Resources/Prefabs/Player/Fish
                EnsureFolder("Assets", "Resources");
                EnsureFolder("Assets/Resources", "Prefabs");
                EnsureFolder("Assets/Resources/Prefabs", "Player");
                EnsureFolder("Assets/Resources/Prefabs/Player", "Fish");
            }

            string path = $"{OutputFolder}/{data.name}.prefab";
            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootSegment, path, out bool prefabSuccess);
            if (!prefabSuccess) {
                Debug.LogError($"GunfishPrefabBaker: failed to save prefab for {data.name} at {path}");
                return false;
            }

            data.fishPrefab = savedPrefab;
            data.bakedSnapshotJson = ComputeBakeSnapshot(data);
            EditorUtility.SetDirty(data);

            return true;
        }
        finally {
            if (rootSegment != null) Object.DestroyImmediate(rootSegment);
            if (scratchDriverObject != null) Object.DestroyImmediate(scratchDriverObject);
        }
    }

    static void EnsureFolder(string parent, string newFolderName) {
        if (!AssetDatabase.IsValidFolder($"{parent}/{newFolderName}")) {
            AssetDatabase.CreateFolder(parent, newFolderName);
        }
    }

    public static string ComputeBakeSnapshot(GunfishData data) {
        var curveKeys = string.Join(",", System.Array.ConvertAll(data.width.keys, k => $"{k.time}:{k.value}"));
        return string.Join("|", new[] {
            data.segmentCount.ToString(),
            data.mass.ToString(),
            data.fixedJointDamping.ToString(),
            data.fixedJointFrequency.ToString(),
            data.length.ToString(),
            curveKeys,
            data.gunOffset.position.x.ToString(),
            data.gunOffset.position.y.ToString(),
            data.gunOffset.rotation.ToString(),
            data.gunSegmentIndex.ToString(),
            GetPersistentId(data.gun),
            GetPersistentId(data.spriteMat)
        });
    }

    static string GetPersistentId(Object obj) {
        if (obj == null) return "null";
        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
    }
}
