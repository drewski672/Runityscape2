using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Login : NetworkBehaviour
{
    public TMP_InputField nameField;
    public TMP_InputField passwordField;
    public string username;
    public string password;
    public Button submitButton;

    public void CallLogin()
    {
        username = nameField.text;
        password = passwordField.text;
        Debug.Log($"OnLoginButton - Username: {username}, Password: {password}");
        StartCoroutine(LoginPlayer());
    }

    IEnumerator LoginPlayer()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", nameField.text);
        form.AddField("password", passwordField.text);
        Debug.Log($"Login - Username: {username}, Password: {password}");
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/sqlconnect/login.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Login failed. HTTP Request error: " + www.error);
            }
            else
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.text);
                if (responseData.status == "0")
                {
                    // Access the DBManager singleton instance
                    DBManager dbManager = DBManager.Instance;
                    Debug.Log("Data received from database:");
                    

                    // Convert PlayerDataRaw to PlayerData before assigning it
                    dbManager.CurrentPlayerData = responseData.playerData.ToPlayerData();

                    Debug.Log("Login successful.");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
                else
                {
                    Debug.Log("Login failed. Error # " + responseData.error);
                }
            }
        }
    }


    public void VerifyInputs()
    {
        submitButton.interactable = (nameField.text.Length >= 8 && passwordField.text.Length >= 8);
    }
}
