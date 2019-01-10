using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public static GameManager instance;

	[Header("Prefabs")]
	public GameObject playerPrefab;

	[Header("Static Info")]

	[Header("Dynamic Info")]
    public List<GameObject> players;
	public int playerCount;

    public string lobbyScene;

	public int levelIndex;
    public List<string> levelList;

	public Vector3[] spawnLocations;

    // Start is called before the first frame update
    void Start () {
		if (instance) {
			Destroy(gameObject);
		} else {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		levelIndex = 0;
		players = new List<GameObject>();
		playerCount = 0;

		UpdateSpawnLocations();
    }

	public static void NextLevel () {
		if (instance.levelIndex == 0) {
			SceneManager.LoadScene(instance.lobbyScene);
		} else if (instance.levelIndex == instance.levelList.Count + 1) {
			instance.levelIndex = 0;
			SceneManager.LoadScene(instance.lobbyScene);
		} else {
			SceneManager.LoadScene(instance.levelList[instance.levelIndex - 1]);
		}
		instance.levelIndex++;
		instance.UpdateSpawnLocations();

		foreach (GameObject player in instance.players) {
			player.GetComponent<Player>().SpawnGunfish();
		}
	}

	public void UpdateSpawnLocations () {
		GameObject[] spawns = GameObject.FindGameObjectsWithTag("Respawn");
		spawnLocations = new Vector3[spawns.Length];

		for (int i = 0; i < spawnLocations.Length; i++) {
			spawnLocations[i] = spawns[i].transform.position;
		}
	}

	public static Vector3 GetSpawnLocation () {
		return GetSpawnLocation(instance.playerCount - 1);
	}

	public static Vector3 GetSpawnLocation (int index) {
		if (instance.spawnLocations.Length == 0) return Vector3.zero;
		return instance.spawnLocations[index % instance.spawnLocations.Length];
	}

	public static void AddPlayer () {
		GameObject newPlayer = Instantiate(instance.playerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		instance.players.Add(newPlayer);
		newPlayer.GetComponent<Player>().playerNumber = instance.playerCount;
		newPlayer.name = newPlayer.name.Replace("(Clone)", instance.playerCount.ToString());
		instance.playerCount++;
	}

	public static void RemovePlayer (int index) {
		if (index >= 0 && index < instance.playerCount) {
			GameObject oldPlayer = instance.players[index];

			for (int i = index + 1; i < instance.playerCount; i++) {
				instance.players[i].GetComponent<Player>().SetPlayerNumber(-1, true);
			}

			instance.players.RemoveAt(index);

			oldPlayer.GetComponent<Player>().DespawnGunfish();
			oldPlayer.GetComponent<Player>().DespawnNameplate();
			instance.playerCount--;
			Destroy(oldPlayer);
		}
	}
}
