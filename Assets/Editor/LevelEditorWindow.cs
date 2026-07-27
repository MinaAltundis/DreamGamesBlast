#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Oyunu baþtan oynamadan kayýtlý level'ý görüp deðiþtirmemizi saðlayan küçük bir
// özel editor penceresi. Case'in "level'ý ayarlayan editor menüsü" þartýný karþýlar.
// Üst menüden açýlýr: Dream Games > Level Settings.
public class LevelEditorWindow : EditorWindow
{
    private int _levelToSet = 1;

    // Unity'nin üst menü çubuðuna "Dream Games/Level Settings" ekler.
    [MenuItem("Dream Games/Level Settings")]
    public static void Open()
    {
        GetWindow<LevelEditorWindow>("Level Settings");
    }

    // Pencerenin içeriðini çizer.
    private void OnGUI()
    {
        GUILayout.Label("Oyuncu Ýlerlemesi", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Kayýtlý level:", ProgressService.GetCurrentLevel().ToString());
        EditorGUILayout.LabelField("Toplam level:", LevelLoader.LevelCount().ToString());

        EditorGUILayout.Space();

        _levelToSet = EditorGUILayout.IntField("Level'ý þuna ayarla:", _levelToSet);
        if (_levelToSet < 1) _levelToSet = 1;

        if (GUILayout.Button("Uygula"))
        {
            ProgressService.SetCurrentLevel(_levelToSet);
            Debug.Log($"[Level Settings] Level {_levelToSet} olarak ayarlandý.");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("1. Level'a Sýfýrla"))
        {
            ProgressService.SetCurrentLevel(1);
            _levelToSet = 1;
            Debug.Log("[Level Settings] Ýlerleme 1. level'a sýfýrlandý.");
        }

        EditorGUILayout.Space();

        // Level okumanýn çalýþtýðýný doðrulamak için: seçili level'ý yükleyip
        // detaylarýný Console'a yazar.
        if (GUILayout.Button("Test: Bu level'ý yükle ve yazdýr"))
        {
            LevelData data = LevelLoader.Load(_levelToSet);
            if (data != null)
            {
                Debug.Log($"[Level Settings] Level {data.LevelNumber} yüklendi: " +
                          $"{data.Width}x{data.Height} grid, {data.MoveCount} hamle, " +
                          $"{data.Grid.Count} hücre.");
            }
        }
    }
}
#endif