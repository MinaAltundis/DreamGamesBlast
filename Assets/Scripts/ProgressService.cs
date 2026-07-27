using UnityEngine;

// Oyuncunun ilerlemesini (hangi levelde olduðunu) kaydeder/okur.
// Bu bizim "save sistemi": Unity'nin PlayerPrefs'ini kullanýr. PlayerPrefs küçük
// deðerleri cihazda saklar, yani oyun kapansa bile kaybolmaz.
public static class ProgressService
{
    private const string CurrentLevelKey = "current_level";
    private const int FirstLevel = 1;

    // Oyuncunun þu an olduðu level. Yeni oyuncu için varsayýlan 1.
    public static int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt(CurrentLevelKey, FirstLevel);
    }

    // Kayýtlý level'ý deðiþtirir ve hemen diske yazar.
    public static void SetCurrentLevel(int level)
    {
        PlayerPrefs.SetInt(CurrentLevelKey, level);
        PlayerPrefs.Save();
    }

    // Bir sonraki level'a geçer. Oyuncu kazanýnca çaðrýlacak.
    public static void AdvanceToNextLevel()
    {
        SetCurrentLevel(GetCurrentLevel() + 1);
    }

    // Tüm leveller bittiyse true döner (LevelButton'da "Finished" göstermek için).
    public static bool HasFinishedAllLevels()
    {
        return GetCurrentLevel() > LevelLoader.LevelCount();
    }
}