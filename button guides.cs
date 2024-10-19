using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Required for UI elements
using UnityEngine.SceneManagement;  // Required for scene navigation

public class ButtonManager : MonoBehaviour
{
    // Array to hold references to all 15 buttons in the scene
    public Button[] buttons;  // Assign in the inspector, should hold exactly 15 buttons

    void Start()
    {
        if (buttons.Length != 15)
        {
            Debug.LogError("You need to assign exactly 15 buttons in the Inspector.");
            return;
        }

        // Assign listeners to each button
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;  // Capture the loop variable to avoid closure issues
            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    // Method triggered when any of the 15 buttons is clicked
    void OnButtonClicked(int buttonIndex)
    {
        Debug.Log("Button " + buttonIndex + " clicked.");

        // Perform action based on which button was clicked
        switch (buttonIndex)
        {
            case 0:
                SceneManager.LoadScene("Scene1");  // Replace with actual scene names
                break;
            case 1:
                SceneManager.LoadScene("Scene2");
                break;
            case 2:
                SceneManager.LoadScene("Scene3");
                break;
            case 3:
                SceneManager.LoadScene("Scene4");
                break;
            case 4:
                SceneManager.LoadScene("Scene5");
                break;
            case 5:
                SceneManager.LoadScene("Scene6");
                break;
            case 6:
                SceneManager.LoadScene("Scene7");
                break;
            case 7:
                SceneManager.LoadScene("Scene8");
                break;
            case 8:
                SceneManager.LoadScene("Scene9");
                break;
            case 9:
                SceneManager.LoadScene("Scene10");
                break;
            case 10:
                SceneManager.LoadScene("Scene11");
                break;
            case 11:
                SceneManager.LoadScene("Scene12");
                break;
            case 12:
                SceneManager.LoadScene("Scene13");
                break;
            case 13:
                SceneManager.LoadScene("Scene14");
                break;
            case 14:
                SceneManager.LoadScene("Scene15");
                break;
            default:
                Debug.LogError("Invalid button index.");
                break;
        }
    }
}
