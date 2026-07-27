using UnityEngine;

// Item'larýn 2 boyutlu ýzgarasýný tutar ve hücre koordinatlarý ile dünya konumlarý
// arasýnda çeviri yapar. Düz C# sýnýfý (MonoBehaviour DEÐÝL) — saf mantýk, sahnede
// karþýlýðý olan bir nesne deðil. Bu da bir tasarým tercihi: mantýðý görselden ayýrmak.
public class Board
{
    public int Width { get; }
    public int Height { get; }

    private readonly GridItem[,] _items;
    private readonly float _cellSize;

    public Board(int width, int height, float cellSize)
    {
        Width = width;
        Height = height;
        _cellSize = cellSize;
        _items = new GridItem[height, width]; // [satýr, sütun] olarak indekslenir
    }

    public void SetItem(int row, int column, GridItem item)
    {
        _items[row, column] = item;
    }

    public GridItem GetItem(int row, int column)
    {
        return _items[row, column];
    }

    // Bir hücreyi (satýr, sütun) dünya konumuna çevirir, (0,0) merkezli.
    public Vector2 CellToWorld(int row, int column)
    {
        float x = (column - (Width - 1) / 2f) * _cellSize;
        float y = (row - (Height - 1) / 2f) * _cellSize;
        return new Vector2(x, y);
    }
}