using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class PowerUps : MonoBehaviour
{
    private float sumSpeed;
    private GameObject SpeedPU;


    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
        col.gameObject.GetComponent<PlayerController>().PowerUpsSpeed(sumSpeed);
        Destroy(gameObject);
        }
        
    }

}
