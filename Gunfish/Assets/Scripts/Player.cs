using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string playerName;
    public int playerNumber;
    public int controlNumber;
    public bool keyboard;
    public string axisName;
    public string fireName;

    public Gunfish gunfish;
	public Nameplate nameplate;

	public GameObject selectedFishPrefab;
    public GameObject nameplatePrefab;

    public int gunfishID;

	public int score;

    // Start is called before the first frame update
    void Start () {
        playerNumber = 1;
        keyboard = true;
        SetControlNumber(1);
	    SpawnGunfish();

		DontDestroyOnLoad(gameObject);
	}

    // Update is called once per frame
    void Update () {
        if (GameState.gameState == GameStateType.Playing && null != gunfish) {
            CaptureGameInput();
        }
    }

    void CaptureGameInput () {
        if (keyboard) {
            if (Mathf.Abs(Input.GetAxis(axisName)) > 0.1f) {
                gunfish.Move((int)Input.GetAxisRaw(axisName));
            }
            if (Input.GetAxis(fireName) > 0.1f) {
                gunfish.Fire();
            } 
        }
    }

    public void SetControlNumber (int controlNumber) {
        this.controlNumber = controlNumber;
        axisName = ((keyboard) ? "" : "joy") + controlNumber + "move";
        fireName = ((keyboard) ? "" : "joy") + controlNumber + "fire";
    }

    public void SetPlayerName (string playerName) {
        this.playerName = playerName;

		if (null != nameplate) {
			nameplate.SetName(playerName);
		}
    }

    public void AttachNameplateToGunfish () {
		if (nameplate && gunfish) {
			nameplate.SetOwner(gunfish.gameObject);
		}
    }

	public void SetPlayerNumber (int number, bool additive = false) {
		if (false == additive) {
			playerNumber = number;
		} else {
			playerNumber += number;
		}
	}

	public void ChangeGunfish (GameObject newFish) {
		selectedFishPrefab = newFish;
		SpawnGunfish();
	}

	public void RespawnGunfish () {
		SpawnGunfish();
	}

	public void SpawnGunfish () {
		if (null == selectedFishPrefab) {
			Debug.Log("Cannot spawn gunfish because no fish is selected.");
			return;
		}

		if (null != gunfish) {
			DespawnGunfish();
		}

		print("Spawning Gunfish");

		GameObject fish = Instantiate(selectedFishPrefab, GameManager.GetSpawnLocation(), Quaternion.identity) as GameObject;
		gunfish = fish.GetComponent<Gunfish>();
		gunfish.transform.SetParent(transform);

		if (null == nameplate) {
			SpawnNameplate();
		}
	}

	public void DespawnGunfish () {
		if (null == gunfish) return;
		print("Despawning Gunfish");
		Destroy(gunfish.gameObject);
		gunfish = null;

		if (null != nameplate) {
			DespawnNameplate();
		}
	}

	public void SpawnNameplate () {
		if (null == nameplatePrefab) return;

		if (null != nameplate) {
			DespawnNameplate();
		}

		print("Spawning Nameplate");
		GameObject plate = Instantiate(nameplatePrefab, gunfish.transform.position, Quaternion.identity) as GameObject;
		nameplate = plate.GetComponent<Nameplate>();
		nameplate.transform.SetParent(transform);
		AttachNameplateToGunfish();
		nameplate.SetName(playerName);
	}

	public void DespawnNameplate () {
		if (null == nameplate) return;
		print("Despawning Nameplate");
		Destroy(nameplate.gameObject);
		nameplate = null;
	}
}