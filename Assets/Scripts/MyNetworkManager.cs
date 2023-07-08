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

    public void LogOutButton()
    {
        ulong clientId = LocalClientId;
        Debug.Log("Logging Out Client: " + clientId);
        LogOut(clientId);
    }

    public void LogOut(ulong clientId)
    {
        OnClientDisconnected(clientId);
        _playerData.LogOut(clientId); // Log out the correct client
    }

    public void OnClientConnected(ulong clientId)
    {
        Debug.Log("Client connected. Processing...");

        PlayerData playerData = DBManager.Instance.CurrentPlayerData;
        if (playerData == null)
        {
            Debug.LogError("Failed to fetch PlayerData from DBManager. Aborting client connection.");
            return;
        }

        PlayerDataDictionary[clientId] = playerData;
        Debug.Log("PlayerData stored for clientId " + clientId);

        int playerDatabaseId = GetPlayerDatabaseIDFromLogin(clientId);

        if (playerDatabaseId == 0)
        {
            Debug.LogError("Failed to fetch playerDatabaseId for clientId " + clientId);
            return;
        }

        Debug.Log("OnClientConnected: clientId = " + clientId + ", playerDatabaseId = " + playerDatabaseId);

        _clientToDatabaseID[clientId] = playerDatabaseId;
        Debug.Log("Client to Database Id mapped for clientId " + clientId);

        PrintDebugInfo();

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
        Debug.Log("Trying to access ID: " + playerDatabaseId.ToString() + " in UpdatePlayerField");
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
        Debug.Log("Client disconnected. Processing...");

        //if (_clientToDatabaseID.TryGetValue(clientId, out int playerDatabaseId))
        if (TryGetPlayerDatabaseId(clientId, out int playerDatabaseId))
        {
            Vector3 playerPosition = GetPlayerPosition(clientId);
            if (playerPosition == Vector3.zero)
            {
                Debug.LogError("Failed to fetch player position for clientId " + clientId);
                return;
            }

            Debug.Log("Player position for clientId " + clientId + ": " + playerPosition);

            Debug.Log("Attempting to save player location for player id: " + playerDatabaseId);

            StartCoroutine(UpdatePlayerField(playerDatabaseId, "positionX", playerPosition.x.ToString()));
            StartCoroutine(UpdatePlayerField(playerDatabaseId, "positionY", playerPosition.y.ToString()));
            StartCoroutine(UpdatePlayerField(playerDatabaseId, "positionZ", playerPosition.z.ToString()));

            _clientToPlayerObject.Remove(clientId);
            Debug.Log("Removed PlayerObject for clientId " + clientId);

            PlayerDataDictionary.Remove(clientId);
            Debug.Log("Removed PlayerData for clientId " + clientId);

            _clientToDatabaseID.Remove(clientId);
            Debug.Log("Removed Client to Database Id mapping for clientId " + clientId);
        }
        else
        {
            Debug.LogError("Failed to fetch playerDatabaseId for clientId " + clientId);
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
