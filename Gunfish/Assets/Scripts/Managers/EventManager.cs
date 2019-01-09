using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void JoinGame ();
public delegate void LeaveGame ();

public class EventManager : MonoBehaviour {
	public static EventManager instance;

	public static event JoinGame OnJoinGame;
	public static event LeaveGame OnLeaveGame;

	private void Start () {
		if (instance) {
			Destroy(gameObject);
		} else {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
}
