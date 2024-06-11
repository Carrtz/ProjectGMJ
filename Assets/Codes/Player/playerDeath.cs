using TarodevController;
using UnityEngine;

public class playerDeath : MonoBehaviour
{
    public bool Death = false;
    [SerializeField] private GameObject inicialPos;
    [SerializeField] private GameObject PlayerPos;
    [SerializeField] private Timer timer; // Referência ao Timer
    [SerializeField] private ScriptableStats stats; // Assign this in the Inspector
    [SerializeField] private PlayerController playerController; // Assign this in the Inspector

    void Start()
    {
        // Ensure that stats and playerController are assigned in the Inspector
        if (stats == null)
        {
            Debug.LogError("ScriptableStats is not assigned in the Inspector.");
        }

        if (playerController == null)
        {
            Debug.LogError("PlayerController is not assigned in the Inspector.");
        }
    }

    void Update()
    {
        if (Death)
        {
            if (stats != null)
            {
                stats.MaxSpeed = 10;
            }

            if (playerController != null)
            {
                playerController.canDoubleJump = true;
            }

            if (timer != null)
            {
                timer.ResetTime();
            }
            else
            {
                Debug.LogError("Timer is not assigned");
            }

            // Resetando a posição do jogador
            if (inicialPos != null && PlayerPos != null)
            {
                PlayerPos.transform.position = inicialPos.transform.position;
            }
            else
            {
                if (inicialPos == null)
                {
                    Debug.LogError("inicialPos is not assigned");
                }

                if (PlayerPos == null)
                {
                    Debug.LogError("PlayerPos is not assigned");
                }
            }

            Death = false;
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
