using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class GridItem : MonoBehaviour
{
    public int Row { get; private set; }
    public int Column { get; private set; }

    // Cubes, rockets, TNT and vases will override this.
    public virtual bool CanFall => false;

    private SpriteRenderer _spriteRenderer;

    protected SpriteRenderer Renderer
    {
        get
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            return _spriteRenderer;
        }
    }

    public void SetGridPosition(int row, int column)
    {
        Row = row;
        Column = column;
    }

    protected void SetSprite(Sprite sprite)
    {
        Renderer.sprite = sprite;
    }

    public void FitToCellSize(float cellSize)
    {
        FitToSize(cellSize, cellSize);
    }

    // Used by both 1x1 items and the 2x2 Chalice Box.
    public void FitToSize(
        float targetWidth,
        float targetHeight,
        float fill = 0.9f)
    {
        if (Renderer.sprite == null)
        {
            return;
        }

        Vector3 spriteSize = Renderer.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        float widthScale = targetWidth / spriteSize.x;
        float heightScale = targetHeight / spriteSize.y;
        float scale = Mathf.Min(widthScale, heightScale) * fill;

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}