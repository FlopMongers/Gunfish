using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerDriverTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.InitializeGame();
    }
}
