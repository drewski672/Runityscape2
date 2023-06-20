using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class Game : MonoBehaviour
{
    public TMP_Text playerDisplay;
    public TMP_Text attackDisplay;
    public TMP_Text strengthDisplay;
    public TMP_Text defenseDisplay;
    public TMP_Text hitsDisplay;

    private DBManager _dbManager;
    private MyNetworkManager _myNetworkManager;
    private PlayerData _playerData;

    private void Awake()
    {
        _dbManager = DBManager.Instance;
        _myNetworkManager = FindObjectOfType<MyNetworkManager>();

        ulong clientId = _myNetworkManager.LocalClientId;


        if (_myNetworkManager._clientToDatabaseID.TryGetValue(clientId, out int playerDatabaseId))
        {
            _dbManager.GetPlayerData(this, playerDatabaseId, playerData =>
            {
                if (playerData != null)
                {
                    _playerData = playerData;
                    UpdateUI();
                }
                else
                {
                    Debug.Log("DBManager couldn't fetch player data");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
            });
        }
        // Use the following if you have a specific playerId to use
    }

    private void UpdateUI()
    {
        playerDisplay.text = "Player: " + _playerData.Username;
        attackDisplay.text = "Attack Level: " + _playerData.Levels[PlayerData.Skill.Attack];
        strengthDisplay.text = "Strength Level: " + _playerData.Levels[PlayerData.Skill.Strength];
        defenseDisplay.text = "Defense Level: " + _playerData.Levels[PlayerData.Skill.Defense];
        hitsDisplay.text = "Hits Level: " + _playerData.Levels[PlayerData.Skill.Hits];

        _playerData.OnTotalExperienceChanged += value => UpdateDatabase("totalExperience", value);
    }

    void UpdateDatabase(string fieldName, object value)
    {
        StartCoroutine(UpdateFieldInDatabase(fieldName, value));
    }

    IEnumerator UpdateFieldInDatabase(string fieldName, object value)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", _playerData.Username);
        form.AddField("fieldName", fieldName);
        form.AddField("newValue", value.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/sqlconnect/updatefield.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Update failed for {fieldName}. HTTP Request error: " + www.error);
            }
            else
            {
                if (www.downloadHandler.text == "0")
                {
                    Debug.Log($"Field {fieldName} updated successfully.");
                }
                else
                {
                    Debug.Log($"Update failed for {fieldName}. Error # " + www.downloadHandler.text);
                }
            }
        }
    }


    public void IncreaseAttack()
    {
        _playerData.SetLevel(PlayerData.Skill.Attack, _playerData.Levels[PlayerData.Skill.Attack] + 1);
        attackDisplay.text = "Attack Level: " + _playerData.Levels[PlayerData.Skill.Attack];
    }

    public void IncreaseStrength()
    {
        _playerData.SetLevel(PlayerData.Skill.Strength, _playerData.Levels[PlayerData.Skill.Strength] + 1);
        strengthDisplay.text = "Strength Level: " + _playerData.Levels[PlayerData.Skill.Strength];
    }

    public void IncreaseDefense()
    {
        _playerData.SetLevel(PlayerData.Skill.Defense, _playerData.Levels[PlayerData.Skill.Defense] + 1);
        defenseDisplay.text = "Defense Level: " + _playerData.Levels[PlayerData.Skill.Defense];
    }

    public void IncreaseHits()
    {
        _playerData.SetLevel(PlayerData.Skill.Hits, _playerData.Levels[PlayerData.Skill.Hits] + 1);
        hitsDisplay.text = "Hits Level: " + _playerData.Levels[PlayerData.Skill.Hits];
    }
}



