using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class PowerUps : MonoBehaviour
{
    private float originalMaxSpeed;
    private GameObject SpeedPU;
    [SerializeField] private ScriptableStats SO;
    // Start is called before the first frame update
    void Start()
    {
        originalMaxSpeed = SO.MaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private IEnumerator TemporarilyIncreaseSpeed(float newSpeed, float duration)
    {
        // Altere o valor de MaxSpeed para o novo valor
        SO.MaxSpeed = newSpeed;
        Debug.Log("MaxSpeed temporariamente alterado para: " + newSpeed);


        yield return new WaitForSeconds(duration);

        // Restaure o valor original de MaxSpeed
        SO.MaxSpeed = originalMaxSpeed;
        Debug.Log("MaxSpeed restaurado para: " + originalMaxSpeed);
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            StartCoroutine(TemporarilyIncreaseSpeed(15f, 3f));
        }
    }

}
