using UnityEngine;

// Level tanýmlarýný Resources/Levels klasöründen okur.
public static class LevelLoader
{
    private const string LevelsFolder = "Levels";

    // Verilen numaralý level'ý (1, 2, 3...) yükleyip LevelData'ya çevirir.
    public static LevelData Load(int levelNumber)
    {
        // Dosyalar level_01, level_02 ... level_10 diye adlandýrýlmýþ.
        // {levelNumber:D2} sayýyý en az 2 haneli yazar (1 -> "01", 10 -> "10").
        string path = $"{LevelsFolder}/level_{levelNumber:D2}";

        TextAsset file = Resources.Load<TextAsset>(path);
        if (file == null)
        {
            Debug.LogError($"[LevelLoader] Dosya bulunamadý: Resources/{path}");
            return null;
        }

        return JsonUtility.FromJson<LevelData>(file.text);
    }

    // Resources/Levels içinde kaç level dosyasý olduðunu döndürür.
    public static int LevelCount()
    {
        return Resources.LoadAll<TextAsset>(LevelsFolder).Length;
    }
}