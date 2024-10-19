using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // For TextMeshPro components
using System.IO;  // For reading .txt files

public class TextFileLoader : MonoBehaviour
{
    public TextMeshProUGUI scrollText;  // Reference to the TextMeshPro component in the ScrollView

    // This is the name of the text file you want to load from the Resources folder
    public string fileName = "Avalanche_proofread";  // Do not include .txt here

    void Start()
    {
        LoadTextFile(fileName);
    }

    // Method to load a text file from the Resources folder
    void LoadTextFile(string fileName)
    {
        // Load the .txt file from Resources (make sure your file is placed in a Resources folder)
        TextAsset txtFile = Resources.Load<TextAsset>(fileName);

        if (txtFile != null)
        {
            scrollText.text = txtFile.text;  // Set the content of the TextMeshPro component
        }
        else
        {
            Debug.LogError("Text file not found in Resources.");
        }
    }
}
