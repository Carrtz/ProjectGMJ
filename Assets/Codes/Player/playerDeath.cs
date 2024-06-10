using UnityEngine;

public class playerDeath : MonoBehaviour
{
    public bool Death = false;
    [SerializeField] private GameObject inicialPos;
    [SerializeField] private GameObject PlayerPos;
    [SerializeField] private Timer timer; // Referência ao Timer

    void Start()
    {
      
    }

    void Update()
    {
        if (Death)
        {
            if (timer != null)
            {
                timer.ResetTime();
            }

            // Resetando a posição do jogador
            if (inicialPos != null && PlayerPos != null)
            {
                PlayerPos.transform.position = inicialPos.transform.position;
            }

            Death = false; // Reseta a flag de morte
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Death"))
        {
            Death = true;
        }
    }
}
