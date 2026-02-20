using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int rows;
    public int columns;
    public int score;
    public int combo;
    public int moves;
    public int lives;
    public int turnNumber;
    public int matchNumber;
    public List<int> gridCardTypes;
    public List<bool> gridCardMatched;
}
