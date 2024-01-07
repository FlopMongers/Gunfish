using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RockSpawner : MonoBehaviour
{
    [SerializeField] private GameObject rock;
    [SerializeField] private bool avalancheIsActive;
    
    [Range(5f, 30f)]
    [SerializeField] private float averageAvalancheDuration = 10f;
    [Range(0.1f, 10f)]
    [SerializeField] private float averageSecondsBetweenRocks = 1f;
    [Range(10f, 60f)]
    [SerializeField] private float averageSecondsBetweenAvalanches = 10f;
    

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (avalancheIsActive) {
            UpdateAvalanche();
        } else {
            UpdateNormal();
        }
    }

    void UpdateNormal() {
        if (Random.value < Time.deltaTime / averageSecondsBetweenAvalanches) {
            avalancheIsActive = true;
        }
    }

    void UpdateAvalanche() {
        if (Random.value < Time.deltaTime / averageSecondsBetweenRocks) {
            SpawnRock();
        }

        if (Random.value < Time.deltaTime / averageAvalancheDuration) {
            avalancheIsActive = false;
        }
    }

    void SpawnRock() {
        var position = new Vector3(Random.Range(-12f, 24f), 15f, 0f);
        var rockInstance = Instantiate(rock, position, Quaternion.identity);
        rockInstance.transform.SetParent(transform);
        var flip = Random.value < 0.5f;
        var scale = new Vector3(flip ? -1f : 1f, 1f, 1f) * Random.Range(0.2f, 0.4f);
        rockInstance.transform.SetGlobalScale(scale);
        rockInstance.transform.eulerAngles = Vector3.forward * Random.Range(0f, 360f);
        Destroy(rockInstance, 10f);
    }
}
