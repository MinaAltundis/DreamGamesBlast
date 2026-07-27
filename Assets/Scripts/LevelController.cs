using UnityEngine;

// Sahne baþladýðýnda mevcut level için görünür grid'i inþa eder.
public class LevelController : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f;

    [Header("Küp sprite'larý (Inspector'dan ata)")]
    [SerializeField] private Sprite redCube;
    [SerializeField] private Sprite greenCube;
    [SerializeField] private Sprite blueCube;
    [SerializeField] private Sprite yellowCube;

    private Board _board;

    private void Start()
    {
        int levelNumber = ProgressService.GetCurrentLevel();
        LevelData data = LevelLoader.Load(levelNumber);
        if (data == null) return;

        _board = new Board(data.Width, data.Height, cellSize);
        BuildGrid(data);
        FitCameraToGrid(data);
    }

    private void BuildGrid(LevelData data)
    {
        for (int row = 0; row < data.Height; row++)
        {
            for (int column = 0; column < data.Width; column++)
            {
                // JSON listesi sol alttan baþlar, satýr satýr ilerler.
                string code = data.Grid[row * data.Width + column];

                // Þimdilik sadece küpleri çiziyoruz. Engel/özel itemler sonraki turda.
                if (TryGetCubeColor(code, out CubeColor color))
                {
                    SpawnCube(color, row, column);
                }
            }
        }
    }

    private void SpawnCube(CubeColor color, int row, int column)
    {
        GameObject go = new GameObject($"Cube_{row}_{column}");
        go.transform.SetParent(transform);

        Cube cube = go.AddComponent<Cube>();
        cube.Init(color, SpriteFor(color));
        cube.SetGridPosition(row, column);
        cube.transform.position = _board.CellToWorld(row, column);
        cube.FitToCellSize(cellSize);

        _board.SetItem(row, column, cube);
    }

    private bool TryGetCubeColor(string code, out CubeColor color)
    {
        switch (code)
        {
            case "r": color = CubeColor.Red; return true;
            case "g": color = CubeColor.Green; return true;
            case "b": color = CubeColor.Blue; return true;
            case "y": color = CubeColor.Yellow; return true;
            case "rand": color = (CubeColor)Random.Range(0, 4); return true;
            default: color = CubeColor.Red; return false; // küp kodu deðil
        }
    }

    private Sprite SpriteFor(CubeColor color)
    {
        switch (color)
        {
            case CubeColor.Red: return redCube;
            case CubeColor.Green: return greenCube;
            case CubeColor.Blue: return blueCube;
            case CubeColor.Yellow: return yellowCube;
            default: return redCube;
        }
    }

    private void FitCameraToGrid(LevelData data)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float gridWidth = data.Width * cellSize;
        float gridHeight = data.Height * cellSize;
        const float padding = 1.1f;

        // orthographicSize, kameranýn gördüðü YÜKSEKLÝÐÝN YARISIDIR (dünya birimi).
        float sizeForHeight = gridHeight / 2f;
        float sizeForWidth = gridWidth / (2f * cam.aspect);
        cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth) * padding;

        cam.transform.position = new Vector3(0f, 0f, cam.transform.position.z);
    }
}