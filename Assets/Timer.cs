using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float startTime = 60f;
    public float currentTime;
    [SerializeField] private playerDeath playerDeath; // Refer�ncia ao playerDeath

    public bool IsGamePaused { get; set; } = false; // Propriedade para pausar o jogo

    void Start()
    {
        currentTime = startTime;
        UpdateTimerText();

      
    }

    void Update()
    {
        if (!IsGamePaused && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerText();

            if (currentTime <= 0)
            {
                currentTime = 0;
                TimerEnded();
            }
        }
    }

    public void ResetTime()
    {
        currentTime = startTime;
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        int seconds = Mathf.CeilToInt(currentTime);
        if (timerText != null)
        {
            timerText.text = seconds.ToString();
        }
    }

    void TimerEnded()
    {
        if (playerDeath != null)
        {
            playerDeath.Death = true; // Marca como morte ao terminar o tempo
        }
        else
        {
            Debug.LogError("playerDeath n�o foi atribu�do no Timer.");
        }
    }
}
