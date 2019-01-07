using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AnchorObstacle : NetworkBehaviour {

    private Vector3 originalPos;

    public float maxDropDistance; //Max distance the anchor falls
    public float raiseSpeed; 
    public float waitTime;
    private float waitTill;

    private short stage; //0 = Falling, 1 = Waiting, 2 = Rising, 3 = Waiting to Fall
    
	void Start () {
        originalPos = transform.position;
        stage = 3;
        waitTill = Time.time + waitTime;
    }
	
	// Update is called once per frame
    [ServerCallback]
	void Update ()
    {
        //When falling...
		if(stage == 0)
        {
            GetComponent<Rigidbody2D>().gravityScale = 1;
            if (Vector3.Distance(transform.position, originalPos) > maxDropDistance)
            {
                CycleStage();
            }
        }

        if(stage == 1)
        {
            GetComponent<Rigidbody2D>().gravityScale = 0;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            if(Time.time > waitTill)
            {
                CycleStage();
            }
        }

        if(stage == 2)
        {
            GetComponent<Rigidbody2D>().velocity = transform.up * raiseSpeed;
            if (Vector3.Distance(transform.position, originalPos) < 1f)
            {
                CycleStage();
                waitTill = Time.time + waitTime;
            }
        }

        if(stage == 3)
        {
            GetComponent<Rigidbody2D>().gravityScale = 0;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            if (Time.time > waitTill)
            {
                CycleStage();
            }
        }
        //LineRenderer(originalPos, transform.position)
	}

    //Switches to the next stage (if falling, go to waiting, etc)
    private void CycleStage()
    {
        stage++;
        if(stage > 3) stage = 0;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //DONK
        if(stage == 0 && other.gameObject.CompareTag("Ground"))
        {
            waitTill = Time.time + waitTime;
            CycleStage();
        }
    }
}
