using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bomb : NetworkBehaviour {
    bool triggered;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        
        if (collider.CompareTag("Gunfish") && !triggered)
        {
           triggered = true;
           StartCoroutine( NetworkDestroy() );
        }
    }
    private IEnumerator NetworkDestroy()
    {
        yield return new WaitForSeconds(0.5f);
        NetworkServer.Destroy(gameObject);
    }

}
