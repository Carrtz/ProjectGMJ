using System.Collections;
using UnityEngine;
using TarodevController;

public class PowerUps : MonoBehaviour
{
    private float sumSpeed;
    private Vector3 originalPosition;
    private Renderer objectRenderer;

    private void Start()
    {
        // Salva a posição original e obtém o renderer
        originalPosition = transform.position;
        objectRenderer = GetComponent<Renderer>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            col.gameObject.GetComponent<PlayerController>().PowerUpsSpeed(sumSpeed);
            StartCoroutine(DeactivateAndReactivate());
        }
    }

    private IEnumerator DeactivateAndReactivate()
    {
        // Desativa visualmente o objeto e o move para fora da tela
        objectRenderer.enabled = false;
        transform.position = new Vector3(1000, 1000, 1000); // Move o objeto para uma posição fora da vista

        // Aguarda 3 segundos
        yield return new WaitForSeconds(3f);

        // Reposiciona e reativa visualmente o objeto
        transform.position = originalPosition;
        objectRenderer.enabled = true;
    }
}
