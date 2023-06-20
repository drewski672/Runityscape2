using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Dictionary<ulong, DBManager> dbManagers = new Dictionary<ulong, DBManager>();

    public void AddDBManager(ulong clientId, DBManager dbManager)
    {
        dbManagers[clientId] = dbManager;
    }

    public DBManager GetDBManager(ulong clientId)
    {
        if (dbManagers.TryGetValue(clientId, out var dbManager))
        {
            return dbManager;
        }
        else
        {
            // handle the error - there's no DBManager for the given clientId
            return null;
        }
    }
}

