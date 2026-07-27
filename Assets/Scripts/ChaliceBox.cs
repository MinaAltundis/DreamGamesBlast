using UnityEngine;

public class ChaliceBox : Obstacle
{
    private const int RequiredChalices = 10;

    private SpriteRenderer _doorsRenderer;

    public bool IsDoorOpen { get; private set; }
    public int CollectedChalices { get; private set; }

    public void Init(
        Sprite backgroundSprite,
        Sprite doorsSprite)
    {
        SetSprite(backgroundSprite);
        Renderer.sortingOrder = 0;

        GameObject doorsObject =
            new GameObject("Doors");

        doorsObject.transform.SetParent(
            transform,
            false);

        _doorsRenderer =
            doorsObject.AddComponent<SpriteRenderer>();

        _doorsRenderer.sprite = doorsSprite;
        _doorsRenderer.sortingOrder = 1;
    }

    public override bool ApplyDamage(
        ObstacleDamageSource source,
        int affectedCount)
    {
        if (IsCleared || affectedCount <= 0)
        {
            return IsCleared;
        }

        // Door phase: Kaynak kaç hücreye dokunursa dokunsun
        // yalnýzca kapýyý açar.
        if (!IsDoorOpen)
        {
            OpenDoors();

            Debug.Log(
                $"Chalice Box doors opened at " +
                $"({Row}, {Column}).");

            return false;
        }

        // Chalice phase:
        // Adjacent blast -> komþu patlayan cube sayýsý.
        // Special explosion -> etkilenen 2x2 hücre sayýsý.
        CollectedChalices += affectedCount;

        if (CollectedChalices >
            RequiredChalices)
        {
            CollectedChalices =
                RequiredChalices;
        }

        Debug.Log(
            $"Chalice Box at ({Row}, {Column}): " +
            $"{CollectedChalices}/{RequiredChalices}");

        if (CollectedChalices >=
            RequiredChalices)
        {
            IsCleared = true;

            Debug.Log(
                $"Chalice Box cleared at " +
                $"({Row}, {Column}).");
        }

        return IsCleared;
    }

    public void OpenDoors()
    {
        IsDoorOpen = true;

        if (_doorsRenderer != null)
        {
            _doorsRenderer.enabled = false;
        }
    }
}