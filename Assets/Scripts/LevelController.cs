using UnityEngine;

[RequireComponent(typeof(GridItemFactory))]
public class LevelController : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f;

    private Board _board;
    private GridItemFactory _itemFactory;

    private void Awake()
    {
        _itemFactory = GetComponent<GridItemFactory>();
    }

    private void Start()
    {
        int levelNumber = ProgressService.GetCurrentLevel();
        LevelData data = LevelLoader.Load(levelNumber);

        if (data == null)
        {
            return;
        }

        _board = new Board(
            data.Width,
            data.Height,
            cellSize);

        BuildGrid(data);
        FitCameraToGrid(data);
    }

    private void BuildGrid(LevelData data)
    {
        for (int row = 0; row < data.Height; row++)
        {
            for (int column = 0;
                 column < data.Width;
                 column++)
            {
                // Chalice Box may already have occupied this cell.
                if (_board.GetItem(row, column) != null)
                {
                    continue;
                }

                int index = row * data.Width + column;
                string code = data.Grid[index];

                _itemFactory.Create(
                    code,
                    row,
                    column,
                    _board,
                    transform);
            }
        }
    }

    private void FitCameraToGrid(LevelData data)
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            return;
        }

        float gridWidth = data.Width * cellSize;
        float gridHeight = data.Height * cellSize;

        const float padding = 1.1f;

        float sizeForHeight = gridHeight / 2f;
        float sizeForWidth =
            gridWidth / (2f * camera.aspect);

        camera.orthographicSize =
            Mathf.Max(sizeForHeight, sizeForWidth) * padding;

        camera.transform.position = new Vector3(
            0f,
            0f,
            camera.transform.position.z);
    }
}