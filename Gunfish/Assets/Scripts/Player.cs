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
    public int gunfishID;

    // Start is called before the first frame update
    void Start()
    {
        playerNumber = 1;
        keyboard = true;
        SetControlNumber(1);
        SetPlayerName("Bob");
        SetPlayerObject();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameState.gameState == GameStateType.Playing && null != gunfish) {
            CaptureGameInput();
        }
    }

    void CaptureGameInput() {
        if (keyboard) {
            if (Mathf.Abs(Input.GetAxis(axisName)) > 0.1f) {
                gunfish.Move((int)Input.GetAxisRaw(axisName));
            }
            if (Input.GetAxis(fireName) > 0.1f) {
                gunfish.Fire();
            } 
        }
    }

    public void SetControlNumber(int controlNumber) {
        this.controlNumber = controlNumber;
        axisName = ((keyboard) ? "" : "joy") + controlNumber + "move";
        fireName = ((keyboard) ? "" : "joy") + controlNumber + "fire";
    }

    public void SetPlayerName(string playerName) {
        this.playerName = playerName;
        nameplate.SetName(playerName);
    }

    public void SetPlayerObject() {
        nameplate.SetOwner(gunfish.gameObject);
    }
}