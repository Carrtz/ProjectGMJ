using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject PointA;
    [SerializeField] private GameObject PointB;
    [SerializeField] private playerDeath playerDeathScript; // Refer�ncia ao script playerDeath
    private Rigidbody2D rb;
    private Transform CurrentPoint;
    public float speed;
    private Vector3 initialPosition;

  
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        CurrentPoint = PointB.transform;
        initialPosition = transform.position;
    }

  
    void Update()
    {
        if (playerDeathScript != null && playerDeathScript.Death)
        {
            
            transform.position = initialPosition;
            rb.velocity = Vector2.zero; 
            CurrentPoint = PointB.transform; 
        }
        else
        {
           
            Vector2 direction = (CurrentPoint.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

            if (Vector2.Distance(transform.position, CurrentPoint.position) < 0.5f)
            {
                CurrentPoint = (CurrentPoint == PointB.transform) ? PointA.transform : PointB.transform;
            }
        }
    }
}
