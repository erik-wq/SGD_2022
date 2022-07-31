using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAgro : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("trigger");
        if (collision.gameObject.tag == "Zombie")
        {
            print("chase");
            WanderingAI ai = collision.gameObject.GetComponent<WanderingAI>();
            ai.ChasePlayer(transform.parent.transform);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Zombie")
        {
            WanderingAI ai = collision.gameObject.GetComponent<WanderingAI>();
            ai.ReturnToSpawn();
        }
    }
}
