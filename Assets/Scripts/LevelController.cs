using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GridItemFactory))]
public class LevelController : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f;

    [Header("Animation")]
    [SerializeField] private float blastDuration = 0.18f;

    private Board _board;
    private GridItemFactory _itemFactory;
    private Camera _mainCamera;

    private int _remainingMoves;
    private bool _isResolving;

    public int RemainingMoves => _remainingMoves;

    private void Awake()
    {
        _itemFactory = GetComponent<GridItemFactory>();
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        int levelNumber =
            ProgressService.GetCurrentLevel();

        LevelData data =
            LevelLoader.Load(levelNumber);

        if (data == null)
        {
            return;
        }

        _remainingMoves = data.MoveCount;

        _board = new Board(
            data.Width,
            data.Height,
            cellSize);

        BuildGrid(data);
        FitCameraToGrid(data);

        Debug.Log(
            $"Level {data.LevelNumber} started. " +
            $"Remaining moves: {_remainingMoves}");
    }

    private void Update()
    {
        if (_isResolving || _board == null)
        {
            return;
        }

        if (!TryGetPointerDown(out Vector2 screenPosition))
        {
            return;
        }

        HandlePointerDown(screenPosition);
    }

    private bool TryGetPointerDown(
        out Vector2 screenPosition)
    {
        // Unity Editor / desktop mouse.
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition =
                Mouse.current.position.ReadValue();

            return true;
        }

        // Mobile touch support.
        if (Touchscreen.current != null &&
            Touchscreen.current
                .primaryTouch
                .press
                .wasPressedThisFrame)
        {
            screenPosition =
                Touchscreen.current
                    .primaryTouch
                    .position
                    .ReadValue();

            return true;
        }

        screenPosition = default;
        return false;
    }

    private void HandlePointerDown(
        Vector2 screenPosition)
    {
        if (_mainCamera == null)
        {
            return;
        }

        Vector3 worldPosition =
            _mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    0f));

        if (!_board.TryWorldToCell(
                worldPosition,
                out int row,
                out int column))
        {
            return;
        }

        GridItem tappedItem =
            _board.GetItem(row, column);

        if (!(tappedItem is Cube tappedCube))
        {
            return;
        }

        List<Cube> group =
            CubeGroupFinder.FindGroup(
                _board,
                tappedCube);

        // 1 küplük gruplar geçersizdir ve move harcamaz.
        if (group.Count < 2)
        {
            Debug.Log(
                $"Invalid tap: only {group.Count} cube.");

            return;
        }

        StartCoroutine(
            ResolveCubeBlast(
                tappedCube,
                group));
    }

    private IEnumerator ResolveCubeBlast(
        Cube tappedCube,
        List<Cube> group)
    {
        _isResolving = true;

        // Yalnýzca geçerli tap sonrasýnda move harcanýr.
        _remainingMoves--;

        bool createsSpecialItem =
            group.Count >= 4;

        Vector3 tappedPosition =
            tappedCube.transform.position;

        Vector3[] startPositions =
            new Vector3[group.Count];

        Vector3[] startScales =
            new Vector3[group.Count];

        for (int i = 0; i < group.Count; i++)
        {
            Cube cube = group[i];

            startPositions[i] =
                cube.transform.position;

            startScales[i] =
                cube.transform.localScale;

            // Animasyon sýrasýnda hücreler artýk boþ kabul edilir.
            _board.ClearItem(
                cube.Row,
                cube.Column,
                cube);
        }

        float elapsed = 0f;

        while (elapsed < blastDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / blastDuration);

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress);

            for (int i = 0; i < group.Count; i++)
            {
                Cube cube = group[i];

                if (cube == null)
                {
                    continue;
                }

                Vector3 targetPosition =
                    createsSpecialItem
                        ? tappedPosition
                        : startPositions[i];

                cube.transform.position =
                    Vector3.Lerp(
                        startPositions[i],
                        targetPosition,
                        smoothProgress);

                cube.transform.localScale =
                    Vector3.Lerp(
                        startScales[i],
                        Vector3.zero,
                        smoothProgress);
            }

            yield return null;
        }

        foreach (Cube cube in group)
        {
            if (cube != null)
            {
                Destroy(cube.gameObject);
            }
        }

        LogSpecialItemResult(
            group.Count,
            tappedCube.Row,
            tappedCube.Column);

        Debug.Log(
            $"Blasted {group.Count} cubes. " +
            $"Remaining moves: {_remainingMoves}");

        // Gravity ve yeni cube üretimini sonraki aþamada
        // burada çaðýracaðýz.
        _isResolving = false;
    }

    private void LogSpecialItemResult(
        int groupCount,
        int row,
        int column)
    {
        if (groupCount >= 6)
        {
            Debug.Log(
                $"TNT should be created at " +
                $"({row}, {column}).");

            return;
        }

        if (groupCount >= 4)
        {
            Debug.Log(
                $"Random Rocket should be created at " +
                $"({row}, {column}).");
        }
    }

    private void BuildGrid(LevelData data)
    {
        for (int row = 0; row < data.Height; row++)
        {
            for (int column = 0;
                 column < data.Width;
                 column++)
            {
                // Chalice Box daha önce bu hücreyi
                // kaplamýþ olabilir.
                if (_board.GetItem(row, column) != null)
                {
                    continue;
                }

                int index =
                    row * data.Width + column;

                string code =
                    data.Grid[index];

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
        if (_mainCamera == null)
        {
            return;
        }

        float gridWidth =
            data.Width * cellSize;

        float gridHeight =
            data.Height * cellSize;

        const float padding = 1.1f;

        float sizeForHeight =
            gridHeight / 2f;

        float sizeForWidth =
            gridWidth /
            (2f * _mainCamera.aspect);

        _mainCamera.orthographicSize =
            Mathf.Max(
                sizeForHeight,
                sizeForWidth) * padding;

        _mainCamera.transform.position =
            new Vector3(
                0f,
                0f,
                _mainCamera.transform.position.z);
    }
}