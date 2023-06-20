using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DBManager
{
    private static DBManager _instance;
    public static DBManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DBManager();
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    public PlayerData CurrentPlayerData { get; set; }

    private DBManager()
    {
        if (_instance != null)
        {
            throw new Exception("Cannot create another instance of singleton DBManager.");
        }
    }

    public void GetPlayerData(MonoBehaviour monoBehaviour, int playerId, Action<PlayerData> callback)
    {
        monoBehaviour.StartCoroutine(GetPlayerDataCoroutine(playerId, callback));
    }

    private IEnumerator GetPlayerDataCoroutine(int playerId, Action<PlayerData> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerId", playerId);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/sqlconnect/getplayerdata.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Database fetch failed: " + www.error);
                callback(null);
            }
            else
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);
                PlayerData playerData = responseData.playerData.ToPlayerData(); // Convert PlayerDataRaw to PlayerData here
                Debug.Log("Received player data for player ID " + playerId);
                callback(playerData);
            }
        }
    }

}


public class PlayerData
{
    public enum Skill
    {
        Attack, Strength, Defense, Hits, Magic, Ranged, Prayer, Cooking,
        Woodcutting, Fletching, Fishing, Firemaking, Crafting, Smithing,
        Mining, Herblore, Agility, Thieving, Slayer, Farming, Runecrafting
    }

    public enum IronmanStatus
    {
        None, Ironman, UltimateIronman, HardcoreIronman
    }

    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; } // added
    public string Email { get; set; } // added
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public IronmanStatus PlayerIronmanStatus { get; set; }
    public int QuestPoints { get; set; } // added
    public int CombatLevel { get; set; } // added
    public int TotalLevel { get; set; } // added
    public long TotalExperience { get; set; }
    public bool MembershipStatus { get; set; } // added
    public bool BanStatus { get; set; } // added
    public bool MuteStatus { get; set; } // added
    public DateTime RegistrationDate { get; set; } // added
    public DateTime LastLogin { get; set; } // added
    public bool LoggedIn { get; set; }

    private Dictionary<Skill, int> _levels;
    public Dictionary<Skill, int> Levels { get { return _levels; } }

    private Dictionary<Skill, int> _experience;
    public Dictionary<Skill, int> Experience { get { return _experience; } }

    public event Action<long> OnTotalExperienceChanged = delegate { };
    private event Action<Skill, int> OnSkillLevelChanged = delegate { };
    private event Action<Skill, int> OnSkillExperienceChanged = delegate { };

    public PlayerData()
    {
        _levels = new Dictionary<Skill, int>();
        _experience = new Dictionary<Skill, int>();

        foreach (Skill skill in Enum.GetValues(typeof(Skill)))
        {
            _levels[skill] = 0;
            _experience[skill] = 0;
        }

        Id = -1;
        Username = null;
        Password = null;
        Email = null;
        PositionX = 0;
        PositionY = 0;
        PositionZ = 0;
        PlayerIronmanStatus = IronmanStatus.None;
        QuestPoints = 0;
        CombatLevel = 3;
        TotalLevel = 32;
        TotalExperience = 1154;
        MembershipStatus = false;
        BanStatus = false;
        MuteStatus = false;
        RegistrationDate = DateTime.Now;
        LastLogin = DateTime.Now;
        LoggedIn = false;
    }

    public void LogOut(ulong clientId)
    {
        Id = -1;
        Username = null;
        Password = null;
        Email = null;
        PositionX = 0;
        PositionY = 0;
        PositionZ = 0;
        PlayerIronmanStatus = IronmanStatus.None;
        QuestPoints = 0;
        CombatLevel = 3;
        TotalLevel = 32;
        TotalExperience = 1154;
        MembershipStatus = false;
        BanStatus = false;
        MuteStatus = false;
        RegistrationDate = DateTime.Now;
        LastLogin = DateTime.Now;
        LoggedIn = false;
        _levels.Clear();
        _experience.Clear();
        TotalExperience = 0;
    }

    public void SetLevel(Skill skill, int level)
    {
        if (_levels[skill] != level)
        {
            _levels[skill] = level;
            OnSkillLevelChanged.Invoke(skill, level);
        }
    }

    public void SetExperience(Skill skill, int experience)
    {
        if (_experience[skill] != experience)
        {
            _experience[skill] = experience;
            OnSkillExperienceChanged.Invoke(skill, experience);
        }
    }
}


[System.Serializable]
public class ResponseData
{
    public string status;
    public string error;
    public PlayerDataRaw playerData;
}

[System.Serializable]
public class PlayerDataRaw
{
    public string id;
    public string username;
    public string password; // added
    public string email; // added
    public string positionX;
    public string positionY;
    public string positionZ;
    public string ironmanStatus; // added
    public string questPoints; // added
    public string combatLevel; // added
    public string totalLevel; // added
    public string totalExperience;
    public string skillAttack;
    public string experienceAttack;
    public string skillDefense;
    public string experienceDefense;
    public string skillStrength;
    public string experienceStrength;
    public string skillHits;
    public string experienceHits;
    public string skillRanged;
    public string experienceRanged;
    public string skillPrayer;
    public string experiencePrayer;
    public string skillMagic;
    public string experienceMagic;
    public string skillCooking;
    public string experienceCooking;
    public string skillWoodcutting;
    public string experienceWoodcutting;
    public string skillFletching;
    public string experienceFletching;
    public string skillFishing;
    public string experienceFishing;
    public string skillFiremaking;
    public string experienceFiremaking;
    public string skillCrafting;
    public string experienceCrafting;
    public string skillSmithing;
    public string experienceSmithing;
    public string skillMining;
    public string experienceMining;
    public string skillHerblore;
    public string experienceHerblore;
    public string skillAgility;
    public string experienceAgility;
    public string skillThieving;
    public string experienceThieving;
    public string skillSlayer;
    public string experienceSlayer;
    public string skillFarming;
    public string experienceFarming;
    public string skillRunecrafting;
    public string experienceRunecrafting;
    public string membershipStatus; // added
    public string banStatus; // added
    public string muteStatus; // added
    public string registrationDate; // added
    public string lastLogin; // added

    
    public PlayerData ToPlayerData()
    {
        PlayerData playerData = new PlayerData();
        Debug.Log("Raw ID: " + id);
        Debug.Log("ID before parsing: " + playerData.Id);

        playerData.Id = int.Parse(id);
        Debug.Log("ID after parsing: " + playerData.Id);
        playerData.Username = username;
        playerData.Password = password; // added
        playerData.Email = email; // added
        playerData.PositionX = float.Parse(positionX);
        playerData.PositionY = float.Parse(positionY);
        playerData.PositionZ = float.Parse(positionZ);
        playerData.PlayerIronmanStatus = (PlayerData.IronmanStatus)Enum.Parse(typeof(PlayerData.IronmanStatus), ironmanStatus); // added
        playerData.QuestPoints = int.Parse(questPoints); // added
        playerData.CombatLevel = int.Parse(combatLevel); // added
        playerData.TotalLevel = int.Parse(totalLevel); // added
        playerData.TotalExperience = long.Parse(totalExperience);

        playerData.SetLevel(PlayerData.Skill.Attack, int.Parse(skillAttack));
        playerData.SetExperience(PlayerData.Skill.Attack, int.Parse(experienceAttack));

        playerData.SetLevel(PlayerData.Skill.Defense, int.Parse(skillDefense));
        playerData.SetExperience(PlayerData.Skill.Defense, int.Parse(experienceDefense));

        playerData.SetLevel(PlayerData.Skill.Strength, int.Parse(skillStrength));
        playerData.SetExperience(PlayerData.Skill.Strength, int.Parse(experienceStrength));

        playerData.SetLevel(PlayerData.Skill.Hits, int.Parse(skillHits));
        playerData.SetExperience(PlayerData.Skill.Hits, int.Parse(experienceHits));

        playerData.SetLevel(PlayerData.Skill.Magic, int.Parse(skillMagic));
        playerData.SetExperience(PlayerData.Skill.Magic, int.Parse(experienceMagic));

        playerData.SetLevel(PlayerData.Skill.Ranged, int.Parse(skillRanged));
        playerData.SetExperience(PlayerData.Skill.Ranged, int.Parse(experienceRanged));

        playerData.SetLevel(PlayerData.Skill.Prayer, int.Parse(skillPrayer));
        playerData.SetExperience(PlayerData.Skill.Prayer, int.Parse(experiencePrayer));

        playerData.SetLevel(PlayerData.Skill.Cooking, int.Parse(skillCooking));
        playerData.SetExperience(PlayerData.Skill.Cooking, int.Parse(experienceCooking));

        playerData.SetLevel(PlayerData.Skill.Woodcutting, int.Parse(skillWoodcutting));
        playerData.SetExperience(PlayerData.Skill.Woodcutting, int.Parse(experienceWoodcutting));

        playerData.SetLevel(PlayerData.Skill.Fletching, int.Parse(skillFletching));
        playerData.SetExperience(PlayerData.Skill.Fletching, int.Parse(experienceFletching));

        playerData.SetLevel(PlayerData.Skill.Fishing, int.Parse(skillFishing));
        playerData.SetExperience(PlayerData.Skill.Fishing, int.Parse(experienceFishing));

        playerData.SetLevel(PlayerData.Skill.Firemaking, int.Parse(skillFiremaking));
        playerData.SetExperience(PlayerData.Skill.Firemaking, int.Parse(experienceFiremaking));

        playerData.SetLevel(PlayerData.Skill.Crafting, int.Parse(skillCrafting));
        playerData.SetExperience(PlayerData.Skill.Crafting, int.Parse(experienceCrafting));

        playerData.SetLevel(PlayerData.Skill.Smithing, int.Parse(skillSmithing));
        playerData.SetExperience(PlayerData.Skill.Smithing, int.Parse(experienceSmithing));

        playerData.SetLevel(PlayerData.Skill.Mining, int.Parse(skillMining));
        playerData.SetExperience(PlayerData.Skill.Mining, int.Parse(experienceMining));

        playerData.SetLevel(PlayerData.Skill.Herblore, int.Parse(skillHerblore));
        playerData.SetExperience(PlayerData.Skill.Herblore, int.Parse(experienceHerblore));

        playerData.SetLevel(PlayerData.Skill.Agility, int.Parse(skillAgility));
        playerData.SetExperience(PlayerData.Skill.Agility, int.Parse(experienceAgility));

        playerData.SetLevel(PlayerData.Skill.Thieving, int.Parse(skillThieving));
        playerData.SetExperience(PlayerData.Skill.Thieving, int.Parse(experienceThieving));

        playerData.SetLevel(PlayerData.Skill.Slayer, int.Parse(skillSlayer));
        playerData.SetExperience(PlayerData.Skill.Slayer, int.Parse(experienceSlayer));

        playerData.SetLevel(PlayerData.Skill.Farming, int.Parse(skillFarming));
        playerData.SetExperience(PlayerData.Skill.Farming, int.Parse(experienceFarming));

        playerData.SetLevel(PlayerData.Skill.Runecrafting, int.Parse(skillRunecrafting));
        playerData.SetExperience(PlayerData.Skill.Runecrafting, int.Parse(experienceRunecrafting));

        return playerData;
    }
}

