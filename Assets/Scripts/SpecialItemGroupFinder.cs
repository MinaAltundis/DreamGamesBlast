using System.Collections.Generic;
using UnityEngine;

// Týklanan special item'a yatay veya dikey olarak baðlý
// bütün special item'larý bulur.
public static class SpecialItemGroupFinder
{
    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    public static List<SpecialItem> FindGroup(
        Board board,
        SpecialItem startingItem)
    {
        List<SpecialItem> group =
            new List<SpecialItem>();

        if (board == null || startingItem == null)
        {
            return group;
        }

        bool[,] visited =
            new bool[board.Height, board.Width];

        Queue<Vector2Int> pendingCells =
            new Queue<Vector2Int>();

        pendingCells.Enqueue(
            new Vector2Int(
                startingItem.Column,
                startingItem.Row));

        while (pendingCells.Count > 0)
        {
            Vector2Int cell =
                pendingCells.Dequeue();

            int column = cell.x;
            int row = cell.y;

            if (!board.IsInside(row, column) ||
                visited[row, column])
            {
                continue;
            }

            visited[row, column] = true;

            if (!(board.GetItem(row, column)
                  is SpecialItem specialItem))
            {
                continue;
            }

            group.Add(specialItem);

            foreach (Vector2Int direction in Directions)
            {
                pendingCells.Enqueue(
                    new Vector2Int(
                        column + direction.x,
                        row + direction.y));
            }
        }

        return group;
    }
}