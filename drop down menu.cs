using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Required for UI components
using UnityEngine.SceneManagement;  // Required for scene navigation

public class DropdownMenuController : MonoBehaviour
{
    public Dropdown sceneDropdown;  // Dropdown menu in the UI

    void Start()
    {
        // Add listener for when the value of the dropdown changes, and call method to handle it
        sceneDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(sceneDropdown);
        });

        // Initialize dropdown options (if you want to do it programmatically)
        PopulateDropdownOptions();
    }

    // Method to populate dropdown options programmatically (optional)
    void PopulateDropdownOptions()
    {
        List<string> options = new List<string>() { "Select Scene", "SampleScene", "Guides" };
        sceneDropdown.ClearOptions();  // Clear any existing options
        sceneDropdown.AddOptions(options);  // Add the new options
    }

    // Method that runs when dropdown value is changed
    void DropdownValueChanged(Dropdown dropdown)
    {
        int index = dropdown.value;  // Get the index of the selected option

        // Based on the index or option, load different scenes
        switch (index)
        {
            case 1:  // Option 1: "SampleScene"
                Debug.Log("SampleScene selected.");
                SceneManager.LoadScene("SampleScene");
                break;
            case 2:  // Option 2: "Guides"
                Debug.Log("Guides selected.");
                SceneManager.LoadScene("Guides");
                break;
            default:
                Debug.Log("No valid option selected.");
                break;
        }
    }
}
