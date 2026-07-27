using UnityEngine;

public class ChaliceBox : Obstacle
{
    private SpriteRenderer _doorsRenderer;

    // One logical ChaliceBox occupies four Board cells.
    public void Init(Sprite backgroundSprite, Sprite doorsSprite)
    {
        SetSprite(backgroundSprite);
        Renderer.sortingOrder = 0;

        GameObject doorsObject = new GameObject("Doors");
        doorsObject.transform.SetParent(transform, false);

        _doorsRenderer = doorsObject.AddComponent<SpriteRenderer>();
        _doorsRenderer.sprite = doorsSprite;
        _doorsRenderer.sortingOrder = 1;
    }

    // We will use this when the door phase is completed.
    public void OpenDoors()
    {
        if (_doorsRenderer != null)
        {
            _doorsRenderer.enabled = false;
        }
    }
}