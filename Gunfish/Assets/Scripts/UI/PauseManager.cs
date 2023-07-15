using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseManager : Singleton<PauseManager>
{
    public AudioMixer audioMixer;
    bool paused = false;
    [HideInInspector]
    public bool megaPaused;
    Animator anim;

    int pausePriority;

    public void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Update()
    {
        if (megaPaused)
            return;
        if (Input.GetButtonDown("Pause"))
            Pause();
    }

    public void MainMenu()
    {
        PauseTime(1, 1);
        //FaderCanvas.instance.GoAway("MainMenu");
    }


    public void Pause()
    {
        StopAllCoroutines();
        StartCoroutine(CoPause());
    }

    IEnumerator CoPause()
    {
        PauseTime(1, 1);
        paused = !paused;
        //FX_Spawner.instance?.SpawnFX((paused) ? FXType.PauseIn : FXType.PauseOut, Vector3.zero, Quaternion.identity);
        anim.SetBool("Pause", paused);
        audioMixer.SetFloat("MasterLowpass", (paused) ? 500f : 22000f);
        if (paused)
        {
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Pause_Pause") || (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 && !anim.IsInTransition(0)))
            {
                yield return null;
            }
            PauseTime(0, 1);
        }
    }

    public void PauseTime(int pause, int priority=0)
    {
        if (priority < pausePriority)
            return;
        pausePriority = (pause == 0) ? 0 : priority;
        Time.timeScale = (pause == 0) ? 0f : 1f;
    }
}
