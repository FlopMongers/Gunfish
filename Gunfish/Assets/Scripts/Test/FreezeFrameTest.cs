using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FreezeFrameTest : MonoBehaviour
{
    float freezeTime;
    bool paused;

    CinemachineImpulseSource impulseSource;

    // Start is called before the first frame update
    void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            impulseSource.GenerateImpulseWithVelocity(Random.insideUnitSphere);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            // freeze frame
            freezeTime = 0.4f;
        }   

        if (freezeTime > 0)
        {
            if (paused == false)
            {
                paused = true;
                PauseManager.instance?.PauseTime(0, 0);
            }
            freezeTime -= Time.unscaledDeltaTime;
        }
        else if (paused == true)
        {
            paused = false;
            PauseManager.instance?.PauseTime(1, 0);
        }
    }
}
