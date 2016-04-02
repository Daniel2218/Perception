using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public void onStart()
    {
        SceneManager.LoadScene("Level1");
        Debug.Log("Worked");
    }
    public void onQuit()
    {
        Application.Quit();
        Debug.Log("Worked");
    }
}
