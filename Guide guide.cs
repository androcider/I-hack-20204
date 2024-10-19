using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // Required for scene navigation
using UnityEngine.UI;  // Required for UI elements

public class SceneNavigation2 : MonoBehaviour
{
    public Button AvalanceButton;  // Button to navigate to SampleScene
    public Button guidesButton;  // Button to navigate to Guides

    void Start()
    {
        // Add listeners to the buttons for navigation
        AvalanceButton.onClick.AddListener(OnAvalancheButtonClicked);
        guidesButton.onClick.AddListener(OnGuidesButtonClicked);
    }

    // Method to handle the SampleScene button click event
    void OnAvalancheButtonClicked()
    {
        Debug.Log("Navigating to SampleScene.");
        SceneManager.LoadScene("Avalanche");  // Replace with the name of the SampleScene
    }

    // Method to handle the Guides button click event
    void OnGuidesButtonClicked()
    {
        Debug.Log("Navigating to Guides scene.");
        SceneManager.LoadScene("Hurricane");  // Replace with the name of the Guides scene
    }
}
