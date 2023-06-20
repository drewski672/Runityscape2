using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    private MyNetworkManager _myNetworkManager;

    private void Start()
    {
        _myNetworkManager = MyNetworkManager.Instance;
    }

    public void SpawnPlayer(ulong clientId)
    {
        Debug.Log("_myNetworkManager: " + (_myNetworkManager != null ? "Exists" : "Is Null"));
        if (_myNetworkManager.TryGetPlayerDatabaseId(clientId, out int playerDatabaseId))
        {
            _myNetworkManager.GetPlayerSpawnPosition(playerDatabaseId, spawnPosition =>
            {
                Debug.Log("playerPrefab: " + (playerPrefab != null ? "Exists" : "Is Null"));
                GameObject newPlayerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                Debug.Log("newPlayerObject: " + (newPlayerObject != null ? "Exists" : "Is Null"));

                foreach (Transform child in newPlayerObject.transform)
                {
                    Debug.Log("Player Object Child: " + child.name);
                }

                // Create a new DBManager instance and associate it with the Player component
                //DBManager dbManager = new DBManager();
                DBManager dbManager = DBManager.Instance;
                Player playerComponent = newPlayerObject.GetComponent<Player>();
                playerComponent.AddDBManager(clientId, dbManager);

                NetworkObject newPlayerNetworkObject = newPlayerObject.GetComponent<NetworkObject>();
                Debug.Log("newPlayerNetworkObject: " + (newPlayerNetworkObject != null ? "Exists" : "Is Null"));
                newPlayerNetworkObject.SpawnAsPlayerObject(clientId);  // Modified line

                // Call the new method on MyNetworkManager to store the player object reference
                _myNetworkManager.SetPlayerObject(clientId, newPlayerObject);
            });
        }
        else
        {
            Debug.LogError("Failed to spawn player for client " + clientId + " because no database ID was found");
        }
    }
}
