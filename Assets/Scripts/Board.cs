using UnityEngine;

public class Board
{
    public int Width { get; }
    public int Height { get; }
    public float CellSize { get; }

    private readonly GridItem[,] _items;

    public Board(int width, int height, float cellSize)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;

        _items = new GridItem[height, width];
    }

    public bool IsInside(int row, int column)
    {
        return row >= 0 &&
               row < Height &&
               column >= 0 &&
               column < Width;
    }

    public void SetItem(int row, int column, GridItem item)
    {
        if (!IsInside(row, column))
        {
            throw new System.IndexOutOfRangeException(
                $"Cell ({row}, {column}) is outside the board.");
        }

        _items[row, column] = item;
    }

    public GridItem GetItem(int row, int column)
    {
        if (!IsInside(row, column))
        {
            return null;
        }

        return _items[row, column];
    }

    public Vector2 CellToWorld(int row, int column)
    {
        float x = (column - (Width - 1) / 2f) * CellSize;
        float y = (row - (Height - 1) / 2f) * CellSize;

        return new Vector2(x, y);
    }
}