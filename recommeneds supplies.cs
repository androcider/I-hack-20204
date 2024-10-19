using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;  // Required for scene navigation
using UnityEngine.UI;  // Required for UI elements

public class DisasterDetailLoader : MonoBehaviour
{
    public TextMeshProUGUI itemListText;  // Text component to display the item list
    public Button HomeButton;  // Button to navigate back to the previous scene

    private List<string[]> itemData;  // List to store item and type data

    private Dictionary<string, string> disasterCSVMap = new Dictionary<string, string>()
    {
        { "Flooding", "bug_out_flood" },
        { "Wildfire", "volcanic" },
        { "Winter Weather", "stay_at_home_winter_weather" },
        { "Cold Wave", "stay_at_home_winter_weather" },
        { "Earthquake", "bug_out_bag" },
        { "Tornado", "Tornado" },
        { "Heat Wave", "Bug_out_heat_wave" },
        { "Hurricane", "bug_out_hurricane" },
        { "Storms", "stay_at_home_supplies" },
        { "Hail", "stay_at_home_supplies" },
        { "Lighting", "stay_at_home_supplies" },
        { "Strong Wind", "stay_at_home_supplies" },
    };

    void Start()
    {
        // Get the highest ranked disaster from the previous scene
        string highestDisaster = DisasterData.highestDisaster;

        if (disasterCSVMap.ContainsKey(highestDisaster))
        {
            string csvFilename = disasterCSVMap[highestDisaster];
            // Load the specific CSV file for the list of items
            LoadItemData(csvFilename);
            // Display the item list (item and type)
            DisplayItemList();
        }
        else
        {
            Debug.LogError("No matching disaster found.");
            itemListText.text = "No CSV data available.";
        }

        // Add listener for backButton to navigate back to Scene 1
        HomeButton.onClick.AddListener(OnHomeButtonClicked);
    }

    // Method to load item data from the specified CSV file located in Resources
    void LoadItemData(string csvFilename)
    {
        // Load the CSV file as a TextAsset from the Resources folder
        TextAsset csvFile = Resources.Load<TextAsset>(csvFilename);

        if (csvFile == null)
        {
            Debug.LogError($"CSV file {csvFilename} not found in Resources folder.");
            return;
        }

        string csvData = csvFile.text;
        string[] lines = csvData.Split('\n');

        itemData = new List<string[]>();  // Initialize the list to store items and types

        for (int i = 1; i < lines.Length; i++)  // Skip the header (i = 1)
        {
            string[] fields = lines[i].Split(',');

            if (fields.Length < 2) // Check if there are at least two fields (Item and Type)
                continue;

            // Add only fields 0 (Item) and 1 (Type) to the list
            itemData.Add(new string[] { fields[0], fields[1] });
        }

        Debug.Log($"Item data from {csvFilename} successfully loaded.");
    }

    // Method to display the list of items and types
    void DisplayItemList()
    {
        // Build a string containing the item and type information
        string details = "Items and Types:\n\n";

        foreach (var entry in itemData)
        {
            string item = entry[0];
            string type = entry[1];

            details += $"{item}: {type}\n";
        }

        // Display the details in the UI
        itemListText.text = details;
    }

    // Method to handle the "Back" button click event
    void OnHomeButtonClicked()
    {
        Debug.Log("Back button clicked. Returning to the main scene.");
        SceneManager.LoadScene("home");  // Replace "MainScene" with the name of your Scene 1
    }
}
