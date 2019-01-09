using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnGUI () {
		if (GUILayout.Button("Spawn Player")) {
			GameManager.AddPlayer();
		}

		if (GUILayout.Button("Remove Player")) {
			GameManager.RemovePlayer(0);
		}

		if (GUILayout.Button("Next Level")) {
			GameManager.NextLevel();
		}
	}
}
