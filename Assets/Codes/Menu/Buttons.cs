using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
   [SerializeField] private Button ButtonPlay;
    // Start is called before the first frame update
    void Awake()
    {
        ButtonPlay.onClick.AddListener(OnButtonPlay);
    }

    // Update is called once per frame
    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex +1);
    }
    private void OnButtonPlay()
    {
        Play();
    }
}
