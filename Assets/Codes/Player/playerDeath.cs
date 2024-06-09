using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerDeath : MonoBehaviour
{
    // Start is called before the first frame update
    private bool Death = false;
    [SerializeField] private GameObject inicialPos;
    [SerializeField] private GameObject PlayerPos;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Death)
        {
        PlayerPos.transform.position = inicialPos.transform.position;
        Death = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.CompareTag("Death"))
        {
            Death = true;
        }
    }
}
