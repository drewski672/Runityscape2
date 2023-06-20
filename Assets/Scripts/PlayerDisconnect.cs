using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.Networking;

public class PlayerDisconnect : MonoBehaviour
{
    private NetworkManager _networkManager;
    private string phpUpdateURL = "http://localhost/sqlconnect/updatefield.php";

    private void Awake()
    {
        _networkManager = GetComponent<NetworkManager>();
        _networkManager.OnClientDisconnectCallback += SavePlayerPosition;
    }

    private void SavePlayerPosition(ulong clientId)
    {
        var player = _networkManager.gameObject;
        Vector3 position = player.transform.position;

        StartCoroutine(PostPlayerPosition(player.name, position));
    }

    IEnumerator PostPlayerPosition(string playerName, Vector3 position)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", playerName);
        form.AddField("fieldName", "positionX");
        form.AddField("newValue", position.x.ToString());

        UnityWebRequest www = UnityWebRequest.Post(phpUpdateURL, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Player position X update complete!");
        }

        form = new WWWForm();
        form.AddField("username", playerName);
        form.AddField("fieldName", "positionY");
        form.AddField("newValue", position.y.ToString());

        www = UnityWebRequest.Post(phpUpdateURL, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Player position Y update complete!");
        }

        form = new WWWForm();
        form.AddField("username", playerName);
        form.AddField("fieldName", "positionZ");
        form.AddField("newValue", position.z.ToString());

        www = UnityWebRequest.Post(phpUpdateURL, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Player position Z update complete!");
        }
    }

    public void Logout()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }


}
