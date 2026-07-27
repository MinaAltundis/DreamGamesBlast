using UnityEngine;

public class GridItemFactory : MonoBehaviour
{
    [Header("Cubes - Default")]
    [SerializeField] private Sprite redCube;
    [SerializeField] private Sprite greenCube;
    [SerializeField] private Sprite blueCube;
    [SerializeField] private Sprite yellowCube;

    [Header("Cubes - Rocket Hint")]
    [SerializeField] private Sprite redRocketHint;
    [SerializeField] private Sprite greenRocketHint;
    [SerializeField] private Sprite blueRocketHint;
    [SerializeField] private Sprite yellowRocketHint;

    [Header("Cubes - TNT Hint")]
    [SerializeField] private Sprite redTntHint;
    [SerializeField] private Sprite greenTntHint;
    [SerializeField] private Sprite blueTntHint;
    [SerializeField] private Sprite yellowTntHint;

    [Header("Special Items")]
    [SerializeField] private Sprite horizontalRocket;
    [SerializeField] private Sprite verticalRocket;
    [SerializeField] private Sprite tnt;

    [Header("Obstacles")]
    [SerializeField] private Sprite stone;
    [SerializeField] private Sprite vaseHealthy;
    [SerializeField] private Sprite vaseDamaged;
    [SerializeField] private Sprite chaliceBoxBackground;
    [SerializeField] private Sprite chaliceBoxDoors;

    public GridItem Create(
        string code,
        int row,
        int column,
        Board board,
        Transform parent)
    {
        switch (code)
        {
            case "r":
                return SpawnCube(
                    CubeColor.Red,
                    row,
                    column,
                    board,
                    parent);

            case "g":
                return SpawnCube(
                    CubeColor.Green,
                    row,
                    column,
                    board,
                    parent);

            case "b":
                return SpawnCube(
                    CubeColor.Blue,
                    row,
                    column,
                    board,
                    parent);

            case "y":
                return SpawnCube(
                    CubeColor.Yellow,
                    row,
                    column,
                    board,
                    parent);

            case "rand":
                return SpawnRandomCube(
                    row,
                    column,
                    board,
                    parent);

            case "hro":
                return SpawnRocket(
                    RocketDirection.Horizontal,
                    row,
                    column,
                    board,
                    parent);

            case "vro":
                return SpawnRocket(
                    RocketDirection.Vertical,
                    row,
                    column,
                    board,
                    parent);

            case "t":
                return SpawnTnt(
                    row,
                    column,
                    board,
                    parent);

            case "s":
                return SpawnStone(
                    row,
                    column,
                    board,
                    parent);

            case "v":
                return SpawnVase(
                    row,
                    column,
                    board,
                    parent);

            case "cbBL":
                return SpawnChaliceBox(
                    row,
                    column,
                    board,
                    parent);

            case "cbBR":
            case "cbTL":
            case "cbTR":
                Debug.LogError(
                    $"Chalice Box part at ({row}, {column}) " +
                    "was found without a valid cbBL anchor.");
                return null;

            default:
                Debug.LogWarning(
                    $"Unknown grid code '{code}' at " +
                    $"({row}, {column}).");
                return null;
        }
    }

    public Cube SpawnRandomCube(
        int row,
        int column,
        Board board,
        Transform parent)
    {
        CubeColor randomColor =
            (CubeColor)Random.Range(0, 4);

        return SpawnCube(
            randomColor,
            row,
            column,
            board,
            parent);
    }

    public Rocket SpawnRandomRocket(
        int row,
        int column,
        Board board,
        Transform parent)
    {
        RocketDirection direction =
            Random.value < 0.5f
                ? RocketDirection.Horizontal
                : RocketDirection.Vertical;

        return SpawnRocket(
            direction,
            row,
            column,
            board,
            parent);
    }

    public Rocket SpawnRocket(
        RocketDirection direction,
        int row,
        int column,
        Board board,
        Transform parent)
    {
        Sprite sprite =
            direction == RocketDirection.Horizontal
                ? horizontalRocket
                : verticalRocket;

        Rocket rocket = CreateAt<Rocket>(
            $"{direction}Rocket_{row}_{column}",
            row,
            column,
            board.CellToWorld(row, column),
            parent);

        rocket.Init(direction, sprite);
        rocket.FitToCellSize(board.CellSize);

        board.SetItem(row, column, rocket);

        return rocket;
    }

    public Tnt SpawnTnt(
        int row,
        int column,
        Board board,
        Transform parent)
    {
        Tnt item = CreateAt<Tnt>(
            $"TNT_{row}_{column}",
            row,
            column,
            board.CellToWorld(row, column),
            parent);

        item.Init(tnt);
        item.FitToCellSize(board.CellSize);

        board.SetItem(row, column, item);

        return item;
    }

    private Cube SpawnCube(
        CubeColor color,
        int row,
        int column,
        Board board,
        Transform parent)
    {
        Cube cube = CreateAt<Cube>(
            $"Cube_{row}_{column}",
            row,
            column,
            board.CellToWorld(row, column),
            parent);

        cube.Init(
            color,
            DefaultSpriteFor(color),
            RocketHintSpriteFor(color),
            TntHintSpriteFor(color));

        cube.FitToCellSize(board.CellSize);

        board.SetItem(row, column, cube);

        return cube;
    }

    private Stone SpawnStone(
        int row,
        int column,
        Board board,
        Transform parent)
    {
        Stone item = CreateAt<Stone>(
            $"Stone_{row}_{column}",
            row,
            column,
            board.CellToWorld(row, column),
            parent);

        item.Init(stone);
        item.FitToCellSize(board.CellSize);

        board.SetItem(row, column, item);

        return item;
    }

    private Vase SpawnVase(
        int row,
        int column,
        Board board,
        Transform parent)
    {
        Vase item = CreateAt<Vase>(
            $"Vase_{row}_{column}",
            row,
            column,
            board.CellToWorld(row, column),
            parent);

        item.Init(vaseHealthy, vaseDamaged);
        item.FitToCellSize(board.CellSize);

        board.SetItem(row, column, item);

        return item;
    }

    private ChaliceBox SpawnChaliceBox(
        int row,
        int column,
        Board board,
        Transform parent)
    {
        if (!board.IsInside(row + 1, column + 1))
        {
            Debug.LogError(
                $"Chalice Box at ({row}, {column}) " +
                "does not fit inside the board.");

            return null;
        }

        Vector2 bottomLeft =
            board.CellToWorld(row, column);

        Vector2 topRight =
            board.CellToWorld(
                row + 1,
                column + 1);

        Vector2 center =
            (bottomLeft + topRight) * 0.5f;

        ChaliceBox box = CreateAt<ChaliceBox>(
            $"ChaliceBox_{row}_{column}",
            row,
            column,
            center,
            parent);

        box.Init(
            chaliceBoxBackground,
            chaliceBoxDoors);

        box.FitToSize(
            board.CellSize * 2f,
            board.CellSize * 2f,
            0.96f);

        board.SetItem(row, column, box);
        board.SetItem(row, column + 1, box);
        board.SetItem(row + 1, column, box);
        board.SetItem(row + 1, column + 1, box);

        return box;
    }

    private T CreateAt<T>(
        string objectName,
        int row,
        int column,
        Vector2 worldPosition,
        Transform parent)
        where T : GridItem
    {
        GameObject itemObject =
            new GameObject(objectName);

        itemObject.transform.SetParent(parent);

        T item =
            itemObject.AddComponent<T>();

        item.SetGridPosition(row, column);
        item.transform.position = worldPosition;

        return item;
    }

    private Sprite DefaultSpriteFor(CubeColor color)
    {
        switch (color)
        {
            case CubeColor.Red:
                return redCube;

            case CubeColor.Green:
                return greenCube;

            case CubeColor.Blue:
                return blueCube;

            case CubeColor.Yellow:
                return yellowCube;

            default:
                return redCube;
        }
    }

    private Sprite RocketHintSpriteFor(CubeColor color)
    {
        switch (color)
        {
            case CubeColor.Red:
                return redRocketHint;

            case CubeColor.Green:
                return greenRocketHint;

            case CubeColor.Blue:
                return blueRocketHint;

            case CubeColor.Yellow:
                return yellowRocketHint;

            default:
                return redRocketHint;
        }
    }

    private Sprite TntHintSpriteFor(CubeColor color)
    {
        switch (color)
        {
            case CubeColor.Red:
                return redTntHint;

            case CubeColor.Green:
                return greenTntHint;

            case CubeColor.Blue:
                return blueTntHint;

            case CubeColor.Yellow:
                return yellowTntHint;

            default:
                return redTntHint;
        }
    }
}