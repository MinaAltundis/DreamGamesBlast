using UnityEngine;

// Bir grid hücresinde durabilecek HER nesnenin temel sýnýfý: küpler, engeller,
// özel itemler. Kendi hücre konumunu bilir ve bir sprite gösterebilir. Alt sýnýflar
// kendi davranýþlarýný ekler. "abstract" = düz bir GridItem oluþturamazsýn,
// sadece Cube gibi somut çocuklarý oluþturabilirsin. Ýþte kalýtýmýn temeli bu.
[RequireComponent(typeof(SpriteRenderer))]
public abstract class GridItem : MonoBehaviour
{
    public int Row { get; private set; }
    public int Column { get; private set; }

    private SpriteRenderer _spriteRenderer;

    // SpriteRenderer'ý ilk ihtiyaç anýnda bulur (tembel yükleme).
    protected SpriteRenderer Renderer
    {
        get
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
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

    // Bu nesneyi, sprite'ý bir hücreye güzelce sýðacak þekilde ölçekler.
    public void FitToCellSize(float cellSize)
    {
        if (Renderer.sprite == null) return;
        float spriteWorldSize = Renderer.sprite.bounds.size.x;
        if (spriteWorldSize <= 0f) return;

        float fill = 0.9f; // hücreler arasýnda küçük boþluk býrakýr
        float scale = (cellSize * fill) / spriteWorldSize;
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}