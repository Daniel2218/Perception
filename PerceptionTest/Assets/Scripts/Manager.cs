using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {

    public bool isPaused; // keeps track of pause state

    void Start () {
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
	}
	
	void Update () {
        
	}

    public void onPause()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = true;
            Time.timeScale = 0; // pause game
            SceneManager.LoadScene("PauseMenu");
        }  
    }

    public void onResume()
    {
        isPaused = false;
        Time.timeScale = 1;  // starts game
        SceneManager.LoadScene("Level 1");
    }
}
