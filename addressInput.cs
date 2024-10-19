using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;  // Required for scene navigation

public class GeocodeSearch : MonoBehaviour
{
    public TMP_InputField inputField;  // Input field for user's address
    public TextMeshProUGUI resultText; // Text field to display the search results
    public Button gpsButton;  // Button to start GPS location search
    public Button searchButton;  // Button to search based on user input
    public Button navigateButton; // Button to navigate to the new scene
    private string apiKey = "67132a338c9e7761924848lhv6517fb";  // Your API key

    // CSV file data
    Dictionary<string, Dictionary<string, Dictionary<string, string>>> disasterData;

    void Start()
    {
      

        // Add listeners to the buttons
        gpsButton.onClick.AddListener(OnGPSButtonClicked);
        searchButton.onClick.AddListener(OnSearchButtonClicked);
        navigateButton.onClick.AddListener(OnNavigateButtonClicked);  // Add listener for scene navigation
    }

    // Triggered when the user clicks the "Search" button for manual address input
    public void OnSearchButtonClicked()
    {      // Load disaster data from the CSV file when the script starts
        LoadDisasterData();
        string searchText = inputField.text;  // Get the text entered by the user
        StartCoroutine(GetCoordinatesFromAddress(searchText));
    }

    // Coroutine to get lat/lon from the search API based on user input
    IEnumerator GetCoordinatesFromAddress(string address)
    {
        string searchUrl = "https://geocode.maps.co/search?q=" + UnityWebRequest.EscapeURL(address) + "&api_key=" + apiKey;

        UnityWebRequest searchRequest = UnityWebRequest.Get(searchUrl);
        yield return searchRequest.SendWebRequest();

        if (searchRequest.result == UnityWebRequest.Result.ConnectionError || searchRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(searchRequest.error);
            resultText.text = "Error: " + searchRequest.error;
        }
        else
        {
            string responseText = searchRequest.downloadHandler.text;
            GeocodeResponse[] results = JsonUtility.FromJson<GeocodeResponseArrayWrapper>("{\"results\":" + responseText + "}").results;

            if (results.Length > 0)
            {
                float lat = results[0].lat;
                float lon = results[0].lon;

                // Use the latitude and longitude to reverse geocode
                StartCoroutine(ReverseGeocodeAndSearch(lat, lon));
            }
            else
            {
                resultText.text = "No results found for the entered address.";
            }
        }
    }

    // Triggered when the GPS button is clicked to obtain location
    public void OnGPSButtonClicked()
    {
        // Request location permission before accessing GPS
        RequestLocationPermission();

        // If permission is granted, get the GPS location
        StartCoroutine(GetGPSLocation());
    }

    // Request location permission at runtime for Android 6.0+
    void RequestLocationPermission()
    {
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
        }
    }

    // Coroutine to get the GPS location of the device
    IEnumerator GetGPSLocation()
    {
        // Check if location services are enabled
        if (!Input.location.isEnabledByUser)
        {
            resultText.text = "Location services are not enabled.";
            yield break;
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in time
        if (maxWait < 1)
        {
            resultText.text = "Timed out while initializing location services.";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            resultText.text = "Unable to determine device location.";
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            float latitude = Input.location.lastData.latitude;
            float longitude = Input.location.lastData.longitude;

            // Use the latitude and longitude to reverse geocode
            StartCoroutine(ReverseGeocodeAndSearch(latitude, longitude));
        }

        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }

    // Coroutine to reverse geocode the latitude and longitude and search disaster risk
    IEnumerator ReverseGeocodeAndSearch(float lat, float lon)
    {
        string reverseUrl = $"https://geocode.maps.co/reverse?lat={lat}&lon={lon}&api_key={apiKey}";

        UnityWebRequest reverseRequest = UnityWebRequest.Get(reverseUrl);
        yield return reverseRequest.SendWebRequest();

        if (reverseRequest.result == UnityWebRequest.Result.ConnectionError || reverseRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Reverse geocoding failed: " + reverseRequest.error);
            resultText.text = "Error: " + reverseRequest.error;
        }
        else
        {
            string reverseResponseText = reverseRequest.downloadHandler.text;
            ReverseGeocodeResponse reverseResult = JsonUtility.FromJson<ReverseGeocodeResponse>(reverseResponseText);

            string state = reverseResult.address.state;
            string county = reverseResult.address.county;

            // Clear the resultText before displaying the new data
            resultText.text = "";  // Clear the previous result

            // Fetch disaster risks from CSV based on state and county
            string disasterRisks = GetDisasterRiskForStateAndCounty(state, county);
            resultText.text = $"State: {state}\nCounty: {county}\n\nDisaster Risks:\n{disasterRisks}";
        }
    }

    // Navigation: Triggered when the "Navigate" button is clicked to load a new scene
    public void OnNavigateButtonClicked()
    {
        // Get the highest-ranked disaster (assuming the risk sorting already happened)
        string highestDisaster = GetHighestRankedDisaster();

        // Store the highest-ranked disaster in a static class for the next scene
        DisasterData.highestDisaster = highestDisaster;

        // Navigate to the new scene (make sure to add this scene to your Build Settings)
        SceneManager.LoadScene("Recommended_supplies");
    }

    // Method to load disaster data from the CSV file located in Resources
    void LoadDisasterData()
    {
        // Load the CSV file as a TextAsset from the Resources folder
        TextAsset csvFile = Resources.Load<TextAsset>("NRI_Table_Counties");

        if (csvFile == null)
        {
            Debug.LogError("CSV file not found in Resources folder.");
            return;
        }

        string csvData = csvFile.text;
        string[] lines = csvData.Split('\n');

        string[] headers = lines[0].Split(',');

        disasterData = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');

            if (fields.Length < headers.Length) // Check for incomplete lines
                continue;

            string state = fields[0];
            string county = fields[1];

            Dictionary<string, string> disasterRatings = new Dictionary<string, string>();

            for (int j = 3; j < 19; j++)
            {
                disasterRatings[headers[j]] = fields[j];
            }

            if (!disasterData.ContainsKey(state))
                disasterData[state] = new Dictionary<string, Dictionary<string, string>>();

            disasterData[state][county] = disasterRatings;
        }

        Debug.Log("Disaster data successfully loaded from Resources.");
    }

    // Main method to fetch disaster risk ratings for a specific state and county
    string GetDisasterRiskForStateAndCounty(string state, string county)
    {
        // Define the mapping of risk levels to numerical values
        Dictionary<string, int> riskLevelMap = new Dictionary<string, int>()
        {
            { "Very High", 5 },
            { "Relatively High", 4 },
            { "Relatively Moderate", 3 },
            { "Relatively Low", 2 },
            { "Very Low", 1 },
            { "No Rating", 0 },
            { "Not Applicable", 0 },
            { "Insufficient Data", 0 }
        };

        // Define the disaster groups based on your specifications
        Dictionary<string, List<string>> disasterGroups = new Dictionary<string, List<string>>()
        {
            { "Winter Weather", new List<string> { "Avalanche", "Cold Wave", "Ice Storm", "Winter Weather" } },
            { "Wildfire", new List<string> { "Volcanic Activity", "Wildfire" } },
            { "Flooding", new List<string> { "Coastal Flooding", "Riverine Flooding" } },
            { "Storms", new List<string> { "Strong Wind", "Lightning", "Hail" } }
        };

        // List of disasters that will remain as individual items
        List<string> individualDisasters = new List<string> { "Drought", "Earthquake", "Tornado", "Hurricane", "Tsunami", "Heat Wave" };

        if (disasterData.ContainsKey(state) && disasterData[state].ContainsKey(county))
        {
            Dictionary<string, string> risks = disasterData[state][county];

            // Dictionary to store the highest risk for each group
            Dictionary<string, int> highestRiskPerGroup = new Dictionary<string, int>();

            // Process each disaster group
            foreach (var group in disasterGroups)
            {
                string groupName = group.Key;
                List<string> disastersInGroup = group.Value;

                int highestRisk = 0;  // Initialize the highest risk for this group

                foreach (var disaster in disastersInGroup)
                {
                    if (risks.ContainsKey(disaster))
                    {
                        string riskLevel = risks[disaster];

                        // Compare the numerical value of the risk
                        if (riskLevelMap.ContainsKey(riskLevel))
                        {
                            int riskValue = riskLevelMap[riskLevel];
                            if (riskValue > highestRisk)
                            {
                                highestRisk = riskValue;  // Update to the highest risk found
                            }
                        }
                    }
                }

                // Store the highest risk for this group
                highestRiskPerGroup[groupName] = highestRisk;
            }

            // Process individual disasters
            foreach (var disaster in individualDisasters)
            {
                if (risks.ContainsKey(disaster))
                {
                    string riskLevel = risks[disaster];

                    if (riskLevelMap.ContainsKey(riskLevel))
                    {
                        int riskValue = riskLevelMap[riskLevel];

                        // Store individual risks directly
                        highestRiskPerGroup[disaster] = riskValue;
                    }
                }
            }

            // Sort the groups by their risk value (highest risk first)
            var sortedRiskGroups = highestRiskPerGroup.OrderByDescending(group => group.Value);

            // Convert the highest risk values back to textual representation and build the result
            string result = "";
            foreach (var group in sortedRiskGroups)
            {
                string riskText = GetRiskText(group.Value);  // Use the manual function to get the text

                if (group.Value > 0)  // Only display groups with a valid risk
                {
                    result += $"{group.Key}: {riskText}\n";
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                return "No applicable disaster risks for this location.";
            }

            return result;
        }
        else
        {
            return "No disaster data found for this location.";
        }
    }

    // Method to convert numerical risk back to textual risk
    string GetRiskText(int riskValue)
    {
        Dictionary<string, int> riskLevelMap = new Dictionary<string, int>()
        {
            { "Very High", 5 },
            { "Relatively High", 4 },
            { "Relatively Moderate", 3 },
            { "Relatively Low", 2 },
            { "Very Low", 1 },
            { "No Rating", 0 }
        };

        foreach (var entry in riskLevelMap)
        {
            if (entry.Value == riskValue)
            {
                return entry.Key;
            }
        }
        return "Unknown";  // Default fallback in case no match is found
    }

    // Example method to get the highest-ranked disaster group (based on risk values)
 string GetHighestRankedDisaster()
{
    // Define the mapping of risk levels to numerical values
    Dictionary<string, int> riskLevelMap = new Dictionary<string, int>()
    {
        { "Very High", 5 },
        { "Relatively High", 4 },
        { "Relatively Moderate", 3 },
        { "Relatively Low", 2 },
        { "Very Low", 1 },
        { "No Rating", 0 },
        { "Not Applicable", 0 },
        { "Insufficient Data", 0 }
    };

    // Loop through the disaster data, map risk strings to numerical values, and find the highest-ranked disaster
    KeyValuePair<string, string> highestRisk = disasterData
        .SelectMany(state => state.Value)  // Get all counties
        .SelectMany(county => county.Value)  // Get all disasters for the county
        .OrderByDescending(risk => riskLevelMap.ContainsKey(risk.Value) ? riskLevelMap[risk.Value] : 0)  // Convert risk value to int
        .FirstOrDefault();  // Get the highest risk

    return highestRisk.Key;  // Return the name of the highest-ranked disaster
}


}

// Wrapping the array to allow JSON deserialization with JsonUtility
[System.Serializable]
public class GeocodeResponseArrayWrapper
{
    public GeocodeResponse[] results;
}

[System.Serializable]
public class GeocodeResponse
{
    public float lat;  // Latitude field from the search API
    public float lon;  // Longitude field from the search API
}

[System.Serializable]
public class ReverseGeocodeResponse
{
    public ReverseAddress address;
}

[System.Serializable]
public class ReverseAddress
{
    public string state;
    public string county;
}

// Static class to pass data between scenes
public static class DisasterData
{
    public static string highestDisaster = "";  // To store the highest-ranked disaster
}
