using UnityEngine;

public class GridItemFactory : MonoBehaviour
{
    [Header("Cubes")]
    [SerializeField] private Sprite redCube;
    [SerializeField] private Sprite greenCube;
    [SerializeField] private Sprite blueCube;
    [SerializeField] private Sprite yellowCube;

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

    public void Create(
        string code,
        int row,
        int column,
        Board board,
        Transform parent)
    {
        switch (code)
        {
            case "r":
                CreateCube(CubeColor.Red, row, column, board, parent);
                break;

            case "g":
                CreateCube(CubeColor.Green, row, column, board, parent);
                break;

            case "b":
                CreateCube(CubeColor.Blue, row, column, board, parent);
                break;

            case "y":
                CreateCube(CubeColor.Yellow, row, column, board, parent);
                break;

            case "rand":
                CubeColor randomColor =
                    (CubeColor)Random.Range(0, 4);

                CreateCube(
                    randomColor,
                    row,
                    column,
                    board,
                    parent);
                break;

            case "hro":
                CreateRocket(
                    RocketDirection.Horizontal,
                    horizontalRocket,
                    row,
                    column,
                    board,
                    parent);
                break;

            case "vro":
                CreateRocket(
                    RocketDirection.Vertical,
                    verticalRocket,
                    row,
                    column,
                    board,
                    parent);
                break;

            case "t":
                CreateTnt(row, column, board, parent);
                break;

            case "s":
                CreateStone(row, column, board, parent);
                break;

            case "v":
                CreateVase(row, column, board, parent);
                break;

            case "cbBL":
                CreateChaliceBox(row, column, board, parent);
                break;

            case "cbBR":
            case "cbTL":
            case "cbTR":
                // Normally these cells are already occupied by the
                // ChaliceBox created from cbBL.
                Debug.LogError(
                    $"Chalice Box part at ({row}, {column}) " +
                    "was found without a valid cbBL anchor.");
                break;

            default:
                Debug.LogWarning(
                    $"Unknown grid code '{code}' at " +
                    $"({row}, {column}).");
                break;
        }
    }

    private void CreateCube(
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

        cube.Init(color, SpriteFor(color));
        cube.FitToCellSize(board.CellSize);

        board.SetItem(row, column, cube);
    }

    private void CreateRocket(
        RocketDirection direction,
        Sprite sprite,
        int row,
        int column,
        Board board,
        Transform parent)
    {
        Rocket rocket = CreateAt<Rocket>(
            $"{direction}Rocket_{row}_{column}",
            row,
            column,
            board.CellToWorld(row, column),
            parent);

        rocket.Init(direction, sprite);
        rocket.FitToCellSize(board.CellSize);

        board.SetItem(row, column, rocket);
    }

    private void CreateTnt(
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
    }

    private void CreateStone(
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
    }

    private void CreateVase(
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
    }

    private void CreateChaliceBox(
        int row,
        int column,
        Board board,
        Transform parent)
    {
        // cbBL is the bottom-left cell of a 2x2 area.
        if (!board.IsInside(row + 1, column + 1))
        {
            Debug.LogError(
                $"Chalice Box at ({row}, {column}) " +
                "does not fit inside the board.");

            return;
        }

        Vector2 bottomLeft =
            board.CellToWorld(row, column);

        Vector2 topRight =
            board.CellToWorld(row + 1, column + 1);

        Vector2 center = (bottomLeft + topRight) * 0.5f;

        ChaliceBox box = CreateAt<ChaliceBox>(
            $"ChaliceBox_{row}_{column}",
            row,
            column,
            center,
            parent);

        box.Init(chaliceBoxBackground, chaliceBoxDoors);

        box.FitToSize(
            board.CellSize * 2f,
            board.CellSize * 2f,
            0.96f);

        // All four cells refer to the same logical object.
        board.SetItem(row, column, box);
        board.SetItem(row, column + 1, box);
        board.SetItem(row + 1, column, box);
        board.SetItem(row + 1, column + 1, box);
    }

    private T CreateAt<T>(
        string objectName,
        int row,
        int column,
        Vector2 worldPosition,
        Transform parent)
        where T : GridItem
    {
        GameObject itemObject = new GameObject(objectName);
        itemObject.transform.SetParent(parent);

        T item = itemObject.AddComponent<T>();

        item.SetGridPosition(row, column);
        item.transform.position = worldPosition;

        return item;
    }

    private Sprite SpriteFor(CubeColor color)
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
}