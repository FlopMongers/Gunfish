using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Teleporter : NetworkBehaviour
{
    public NetworkStartPosition[] startPostitions;
    public int pos = 0;
    public void Start()
    {
       startPostitions =  FindObjectsOfType<NetworkStartPosition>();
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {

        if (collider.CompareTag("Gunfish"))
        {
            collider.GetComponentInParent<Gunfish>().transform.position = startPostitions[pos].transform.position;
            pos++;
            if( pos >= startPostitions.Length )
            {
                pos = 0;
            }
        }
    }

}
