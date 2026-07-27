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

    public void ClearItem(
        int row,
        int column,
        GridItem expectedItem = null)
    {
        if (!IsInside(row, column))
        {
            return;
        }

        if (expectedItem == null ||
            _items[row, column] == expectedItem)
        {
            _items[row, column] = null;
        }
    }

    public void ClearAllReferences(
    GridItem item)
    {
        if (item == null)
        {
            return;
        }

        for (int row = 0;
             row < Height;
             row++)
        {
            for (int column = 0;
                 column < Width;
                 column++)
            {
                if (_items[row, column] == item)
                {
                    _items[row, column] = null;
                }
            }
        }
    }

    public Vector2 CellToWorld(int row, int column)
    {
        float x =
            (column - (Width - 1) / 2f) * CellSize;

        float y =
            (row - (Height - 1) / 2f) * CellSize;

        return new Vector2(x, y);
    }

    public bool TryWorldToCell(
        Vector2 worldPosition,
        out int row,
        out int column)
    {
        float leftEdge =
            -(Width * CellSize) / 2f;

        float bottomEdge =
            -(Height * CellSize) / 2f;

        column = Mathf.FloorToInt(
            (worldPosition.x - leftEdge) / CellSize);

        row = Mathf.FloorToInt(
            (worldPosition.y - bottomEdge) / CellSize);

        return IsInside(row, column);
    }
}