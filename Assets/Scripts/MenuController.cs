using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    
    public InputField length,
        width,
        cell_width,
        wall_width;
    
    void Start(){
        // Initialize playerprefs values
        PlayerPrefs.SetInt("MAZE_LENGTH", 15);
        PlayerPrefs.SetInt("MAZE_HEIGHT", 15);
        PlayerPrefs.SetInt("MAZE_WIDTH", 9);
        PlayerPrefs.SetInt("WALL_WIDTH", 2);
    }

    public IEnumerator StartGame()
    {
        int maze_length = int.Parse(length.text.Length != 0 ? length.text : "15"),
            maze_width = int.Parse(width.text.Length != 0 ? width.text : "15"),
            cell_width_val = int.Parse(cell_width.text.Length != 0 ? cell_width.text : "9"),
            wall_width_val = int.Parse(wall_width.text.Length != 0 ? wall_width.text : "2");
        
        if (maze_length > 4 && maze_length < 101)
        {
            PlayerPrefs.SetInt("MAZE_LENGTH", maze_length);
        }

        if (maze_width > 4 && maze_width < 101)
        {
            PlayerPrefs.SetInt("MAZE_HEIGHT", maze_width);
        }

        if (cell_width_val > 8 && cell_width_val < 31)
        {
            PlayerPrefs.SetInt("MAZE_WIDTH", cell_width_val);
        }

        if (wall_width_val > 0 && wall_width_val < 31)
        {
            PlayerPrefs.SetInt("WALL_WIDTH", wall_width_val);
        }
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainGame");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
