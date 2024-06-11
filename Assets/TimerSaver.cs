using UnityEngine;

public class TimeSaver : MonoBehaviour
{
    private Timer timer;

    private void Start()
    {
        // Procura pelo Timer no mesmo objeto ou em objetos filhos
        timer = GetComponentInParent<Timer>();

        // Se não encontrou, tenta achar no objeto "Timer" específico na cena
        if (timer == null)
        {
            timer = GameObject.FindObjectOfType<Timer>();
        }

        if (timer == null)
        {
            Debug.LogError("Timer não foi encontrado pelo TimeSaver.");
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("TimeSave"))
        {
            if (timer != null)
            {
                timer.currentTime += 3;
                Destroy(col.gameObject);
                print("Col");
            }
            else
            {
                Debug.LogError("Timer não foi atribuído no TimeSaver.");
            }
        }
    }
}
