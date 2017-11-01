using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{

    public UIManager UI;

    /*void Start()
    {

    }*/
    //void GenerateMap(GameSettings settings) {  }
    public void StartNewGame() {  }

    public void TogglePauseMenu()
    {
        if (UI.GetComponentInChildren<Canvas>().enabled)
        {
            UI.GetComponentInChildren<Canvas>().enabled = false;
            Time.timeScale = 1.0f;
        }
        else
        {
            UI.GetComponentInChildren<Canvas>().enabled = true;
            Time.timeScale = 0f;
        }

        //Debug.Log("GAMEMANAGER:: TimeScale: " + Time.timeScale);
    }

}
