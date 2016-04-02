using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public Manager manager;

    public void onButtonResume()
    {
        manager.onResume();

        //Manager.onResume();
        Debug.Log("Worked");
    }
    public void onMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Worked");
    }
}
