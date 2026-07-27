using System.Collections.Generic;
using UnityEngine;

// Bir küpten baþlayarak yatay/dikey baðlý,
// ayný renkteki bütün küpleri bulur.
public static class CubeGroupFinder
{
    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    public static List<Cube> FindGroup(
        Board board,
        Cube startingCube)
    {
        List<Cube> group = new List<Cube>();

        if (board == null || startingCube == null)
        {
            return group;
        }

        bool[,] visited =
            new bool[board.Height, board.Width];

        Queue<Vector2Int> cellsToVisit =
            new Queue<Vector2Int>();

        cellsToVisit.Enqueue(
            new Vector2Int(
                startingCube.Column,
                startingCube.Row));

        while (cellsToVisit.Count > 0)
        {
            Vector2Int cell = cellsToVisit.Dequeue();

            int column = cell.x;
            int row = cell.y;

            if (!board.IsInside(row, column))
            {
                continue;
            }

            if (visited[row, column])
            {
                continue;
            }

            visited[row, column] = true;

            GridItem item =
                board.GetItem(row, column);

            if (!(item is Cube cube))
            {
                continue;
            }

            if (cube.Color != startingCube.Color)
            {
                continue;
            }

            group.Add(cube);

            foreach (Vector2Int direction in Directions)
            {
                cellsToVisit.Enqueue(
                    new Vector2Int(
                        column + direction.x,
                        row + direction.y));
            }
        }

        return group;
    }
}