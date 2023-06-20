using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System;

public class MyNetworkManager : NetworkManager
{
    public PlayerSpawner _spawner;
    public DBManager _dbManager;
    public PlayerData _playerData;
    public static MyNetworkManager Instance;

    // This is where we'll store our ID mapping
    public Dictionary<ulong, int> _clientToDatabaseID = new Dictionary<ulong, int>();
    public Dictionary<ulong, PlayerData> PlayerDataDictionary = new Dictionary<ulong, PlayerData>();

    // Dictionary to store player GameObjects
    private Dictionary<ulong, GameObject> _clientToPlayerObject = new Dictionary<ulong, GameObject>();

    // Database url
    private string _databaseUrl = "http://localhost/sqlconnect/updatefield.php";

    private void Awake()
    {
        if (Instance == null) // Singleton implementation
        {
            Instance = this;
            _dbManager = DBManager.Instance;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        _spawner = FindObjectOfType<PlayerSpawner>();
        if (_spawner == null)
        {
            Debug.LogError("PlayerSpawner not found in the scene");
        }

        OnClientConnectedCallback += OnClientConnected;
        OnClientDisconnectCallback += OnClientDisconnected;
    }

    // Method to print debug information
    private void PrintDebugInfo()
    {
        foreach (var entry in _clientToDatabaseID)
        {
            Debug.Log("Client ID: " + entry.Key + ", Database ID: " + entry.Value);
        }
    }

    public void StartTestClient()
    {
        StartClient();
    }

    public void StartTestHost()
    {
        Debug.Log("Start Host was clicked...");
        StartHost();
    }

    public void StartTestServer()
    {
        StartServer();
    }

    public void LogOut(ulong clientId)
    {
        OnClientDisconnected(clientId);
        _playerData.LogOut(clientId); // Log out the correct client
    }

    public void OnClientConnected(ulong clientId)
    {
        PlayerData playerData = DBManager.Instance.CurrentPlayerData;
        PlayerDataDictionary[clientId] = playerData;
        // Get player's database ID directly, not dependent on clientId anymore
        int playerDatabaseId = GetPlayerDatabaseIDFromLogin(clientId);

        Debug.Log("OnClientConnected: clientId = " + clientId + ", playerDatabaseId = " + playerDatabaseId);

        // Add the mapping to our dictionary
        _clientToDatabaseID[clientId] = playerDatabaseId;

        PrintDebugInfo(); // Print debug info

        _spawner.SpawnPlayer(clientId);
    }


    // Method to retrieve player's database ID
    public bool TryGetPlayerDatabaseId(ulong clientId, out int playerDatabaseId)
    {
        foreach (var entry in _clientToDatabaseID)
        {
            Debug.Log("Client ID: " + entry.Key + ", Database ID: " + entry.Value);
        }


        return _clientToDatabaseID.TryGetValue(clientId, out playerDatabaseId);
    }

    // Method to set player object
    public void SetPlayerObject(ulong clientId, GameObject playerObject)
    {
        Debug.Log("SetPlayerObject successfully called...");
        _clientToPlayerObject[clientId] = playerObject;
    }

    private IEnumerator UpdatePlayerField(int playerDatabaseId, string fieldName, string newValue)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", playerDatabaseId.ToString()); // Change this to fetch the actual username if necessary
        form.AddField("fieldName", fieldName);
        form.AddField("newValue", newValue);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/sqlconnect/updatefield.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Field update failed. HTTP Request error: " + www.error);
            }
            else
            {
                Debug.Log("Field " + fieldName + " should have changed to " + newValue);
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Here you can look up the database ID based on the clientId
        if (_clientToDatabaseID.TryGetValue(clientId, out int playerDatabaseId))
        {
            // Assuming you have a reference to the player object, you can fetch its position.
            Vector3 playerPosition = GetPlayerPosition(clientId);
            Debug.Log(GetPlayerPosition(clientId));
            foreach (var entry in _clientToDatabaseID)
            {
                Debug.Log("Client ID: " + entry.Key + ", Database ID: " + entry.Value);
            }
            Debug.Log("Attempting to save player location for player id: " + playerDatabaseId);
            // Save player data to database.
            StartCoroutine(UpdatePlayerField(playerDatabaseId, "positionX", playerPosition.x.ToString()));
            StartCoroutine(UpdatePlayerField(playerDatabaseId, "positionY", playerPosition.y.ToString()));
            StartCoroutine(UpdatePlayerField(playerDatabaseId, "positionZ", playerPosition.z.ToString()));

            // Remove the player's GameObject reference from the dictionary
            _clientToPlayerObject.Remove(clientId);

            PlayerDataDictionary.Remove(clientId);

            // Optionally, you can now remove this clientId from the mapping if it's no longer needed
            _clientToDatabaseID.Remove(clientId);
        }
    }

    private Vector3 GetPlayerPosition(ulong clientId)
    {
        if (_clientToPlayerObject.TryGetValue(clientId, out GameObject playerObject))
        {
            if (playerObject == null)
            {
                Debug.LogError("Player object is null for client " + clientId);
                return Vector3.zero;
            }
            else
            {
                Debug.Log("Player position for client " + clientId + " is " + playerObject.transform.position);
                return playerObject.transform.position;
            }
        }
        else
        {
            Debug.LogError("No player object found for client " + clientId);
            return Vector3.zero;
        }
    }


    // Stub method to demonstrate getting a player's database ID from login data
    private int GetPlayerDatabaseIDFromLogin(ulong clientId)
    {
        if (PlayerDataDictionary.TryGetValue(clientId, out PlayerData playerData))
        {
            return playerData.Id;
        }
        else
        {
            Debug.LogError("No player data found for client " + clientId);
            return 0;  // or some other value indicating an error
        }
    }


    // Method to get player spawn position from the database
    public void GetPlayerSpawnPosition(int playerId, Action<Vector3> callback)
    {
        _dbManager.GetPlayerData(this, playerId, playerData =>
        {
            if (playerData != null)
            {
                // Create a Vector3 from the playerData position fields
                Vector3 spawnPosition = new Vector3(playerData.PositionX, playerData.PositionY, playerData.PositionZ);
                callback(spawnPosition);
            }
            else
            {
                Debug.LogError("Failed to retrieve player data for player ID " + playerId);
            }
        });
    }

}
