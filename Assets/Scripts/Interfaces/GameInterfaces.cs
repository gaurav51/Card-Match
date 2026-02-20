using UnityEngine;

public interface IAudioService
{
    void PlaySound(AudioClip clip);
}

public interface ISaveService
{
    bool TryLoadGame();
    void SaveGame();
    void ClearSave();
}

public interface IGridService
{
    void GenerateGrid(int r, int c);
    void RestoreGrid(GameSaveData data);
    Vector2Int GetGridSizeForLevel(int currentLevel);
}
