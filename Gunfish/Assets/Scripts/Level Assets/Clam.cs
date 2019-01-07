using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clam : MonoBehaviour
{
    bool isClammed;
    private Animator anim;

    void Start() {
        anim = GetComponent<Animator>();
    }


    private void OnTriggerEnter2D(Collider2D collider)
    {

        if (collider.CompareTag("Gunfish") && !isClammed)
        {
            GetComponent<FixedJoint2D>().connectedBody = collider.gameObject.GetComponent<Rigidbody2D>();
            anim.SetTrigger("Toggle");
            isClammed = true;
            StartCoroutine(clamRelease());
        }
    }

    private IEnumerator clamRelease()
    {
        yield return new WaitForSeconds(2);
        GetComponent<FixedJoint2D>().connectedBody = null;
        anim.SetTrigger("Toggle");
        StartCoroutine(clamWait());
    }

    private IEnumerator clamWait()
    {
        yield return new WaitForSeconds(2);
        isClammed = false;
    }
}
