using System.Collections;
using UnityEngine;
using TarodevController;

public class PowerUps : MonoBehaviour
{
    [SerializeField] private GameObject SpeedPU;
    [SerializeField] private ScriptableStats playerStats; // Referência ao ScriptableStats
    private float originalMaxSpeed;

    private void Start()
    {
        originalMaxSpeed = playerStats.MaxSpeed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            StartCoroutine(TemporarilyIncreaseSpeed(15f, 5f));
        }
    }

    private IEnumerator TemporarilyIncreaseSpeed(float newSpeed, float duration)
    {
        // Altere o valor de MaxSpeed para o novo valor
        playerStats.MaxSpeed = newSpeed;
        Debug.Log("MaxSpeed temporariamente alterado para: " + newSpeed);

     
        yield return new WaitForSeconds(duration);

        // Restaure o valor original de MaxSpeed
        playerStats.MaxSpeed = originalMaxSpeed;
        Debug.Log("MaxSpeed restaurado para: " + originalMaxSpeed);
    }
}
