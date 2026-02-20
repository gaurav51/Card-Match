using System.Collections.Generic;
using UnityEngine;

public class SaveSystem : ISaveService
{
    private CardgameManager ctx;
    private const string SaveKey = "CardGameSaveData";

    public SaveSystem(CardgameManager context)
    {
        ctx = context;
    }

    public bool TryLoadGame()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            
            if (data != null && data.gridCardTypes.Count == data.rows * data.columns)
            {
                ctx.score = data.score;
                ctx.combo = data.combo;
                ctx.moves = data.moves;
                ctx.lives = data.lives <= 0 ? 3 : data.lives; // Restores lives correctly 
                ctx.turnNumber = data.turnNumber;
                
                ctx.RestoreGrid(data);
                return true;
            }
        }
        return false;
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData
        {
            rows = ctx.rows,
            columns = ctx.columns,
            score = ctx.score,
            combo = ctx.combo,
            moves = ctx.moves,
            lives = ctx.lives,
            turnNumber = ctx.turnNumber,
            matchNumber = ctx.matchNumber,
            gridCardTypes = new List<int>(),
            gridCardMatched = new List<bool>()
        };

        foreach (var c in ctx.cards)
        {
            data.gridCardTypes.Add((int)c.cardType);
            data.gridCardMatched.Add(c.cardState == CardState.Matched);
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}
