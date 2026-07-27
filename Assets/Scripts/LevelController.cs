using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(
    typeof(GridItemFactory),
    typeof(SpecialExplosionController))]
public class LevelController : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f;

    [Header("Animations")]
    [SerializeField] private float blastDuration = 0.18f;
    [SerializeField] private float specialCreationDuration = 0.14f;
    [SerializeField] private float fallDuration = 0.30f;

    private Board _board;
    private GridItemFactory _itemFactory;
    private Camera _mainCamera;
    private SpecialExplosionController
    _specialExplosionController;

    private int _remainingMoves;
    private bool _isResolving;

    public int RemainingMoves => _remainingMoves;

    private static readonly Vector2Int[]
        OrthogonalDirections =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private class FallAnimation
    {
        public GridItem Item;
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
    }

    private void Awake()
    {
        _itemFactory =
            GetComponent<GridItemFactory>();

        _specialExplosionController =
            GetComponent<SpecialExplosionController>();

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

        _specialExplosionController.Initialize(
            _board);

        BuildGrid(data);
        FitCameraToGrid(data);
        UpdateCubeHints();

        Debug.Log(
            $"Level {data.LevelNumber} started. " +
            $"Remaining moves: {_remainingMoves}");
    }

    private void Update()
    {
        if (_isResolving ||
            _board == null ||
            _remainingMoves <= 0)
        {
            return;
        }

        if (!TryGetPointerDown(
                out Vector2 screenPosition))
        {
            return;
        }

        HandlePointerDown(screenPosition);
    }

    private void DamageAdjacentObstacles(
    List<Cube> blastedGroup)
    {
        // Her obstacle için ona komþu olan farklý
        // blasted cube'larý topluyoruz.
        Dictionary<Obstacle, HashSet<Cube>>
            adjacentCubesByObstacle =
                new Dictionary<
                    Obstacle,
                    HashSet<Cube>>();

        foreach (Cube cube in blastedGroup)
        {
            if (cube == null)
            {
                continue;
            }

            foreach (Vector2Int direction
                     in OrthogonalDirections)
            {
                int neighbourRow =
                    cube.Row + direction.y;

                int neighbourColumn =
                    cube.Column + direction.x;

                GridItem neighbour =
                    _board.GetItem(
                        neighbourRow,
                        neighbourColumn);

                if (!(neighbour is
                      Obstacle obstacle))
                {
                    continue;
                }

                if (!adjacentCubesByObstacle
                        .TryGetValue(
                            obstacle,
                            out HashSet<Cube>
                                adjacentCubes))
                {
                    adjacentCubes =
                        new HashSet<Cube>();

                    adjacentCubesByObstacle.Add(
                        obstacle,
                        adjacentCubes);
                }

                // Ayný cube, ayný 2x2 Chalice Box'a
                // iki kez sayýlmasýn.
                adjacentCubes.Add(cube);
            }
        }

        foreach (
            KeyValuePair<
                Obstacle,
                HashSet<Cube>> entry
            in adjacentCubesByObstacle)
        {
            Obstacle obstacle = entry.Key;

            if (obstacle == null)
            {
                continue;
            }

            bool cleared =
                obstacle.ApplyDamage(
                    ObstacleDamageSource
                        .AdjacentCubeBlast,
                    entry.Value.Count);

            if (cleared)
            {
                RemoveObstacle(obstacle);
            }
        }
    }

    private void RemoveObstacle(
        Obstacle obstacle)
    {
        if (obstacle == null)
        {
            return;
        }

        _board.ClearAllReferences(obstacle);
        Destroy(obstacle.gameObject);
    }

    private bool TryGetPointerDown(
        out Vector2 screenPosition)
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition =
                Mouse.current.position.ReadValue();

            return true;
        }

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

        if (tappedItem is Cube tappedCube)
        {
            List<Cube> group =
                CubeGroupFinder.FindGroup(
                    _board,
                    tappedCube);

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

            return;
        }

        if (tappedItem is SpecialItem specialItem)
        {
            List<SpecialItem> specialGroup =
                SpecialItemGroupFinder.FindGroup(
                    _board,
                    specialItem);

            StartCoroutine(
                ResolveSpecialTap(
                    specialItem,
                    specialGroup));

            return;
        }
    }

    private IEnumerator ResolveCubeBlast(
        Cube tappedCube,
        List<Cube> group)
    {
        _isResolving = true;
        _remainingMoves--;

        int tappedRow = tappedCube.Row;
        int tappedColumn = tappedCube.Column;

        Vector3 tappedPosition =
            tappedCube.transform.position;

        bool createsSpecialItem =
            group.Count >= 4;

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

        DamageAdjacentObstacles(group);

        foreach (Cube cube in group)
        {
            if (cube != null)
            {
                Destroy(cube.gameObject);
            }
        }

        GridItem createdSpecialItem = null;

        if (group.Count >= 6)
        {
            createdSpecialItem =
                _itemFactory.SpawnTnt(
                    tappedRow,
                    tappedColumn,
                    _board,
                    transform);

            Debug.Log(
                $"TNT created at " +
                $"({tappedRow}, {tappedColumn}).");
        }
        else if (group.Count >= 4)
        {
            createdSpecialItem =
                _itemFactory.SpawnRandomRocket(
                    tappedRow,
                    tappedColumn,
                    _board,
                    transform);

            Debug.Log(
                $"Rocket created at " +
                $"({tappedRow}, {tappedColumn}).");
        }

        if (createdSpecialItem != null)
        {
            yield return AnimateSpecialCreation(
                createdSpecialItem);
        }

        yield return ResolveGravityAndRefill();

        UpdateCubeHints();

        Debug.Log(
            $"Blasted {group.Count} cubes. " +
            $"Remaining moves: {_remainingMoves}");

        _isResolving = false;
    }

    private IEnumerator ResolveSpecialTap(
    SpecialItem specialItem,
    List<SpecialItem> specialGroup)
    {
        _isResolving = true;

        // Normal patlama, combo ve zincirleme patlama
        // toplamda yalnýzca bir move harcar.
        _remainingMoves--;

        if (specialGroup != null &&
            specialGroup.Count >= 2)
        {
            yield return
                _specialExplosionController.ResolveCombo(
                    specialItem,
                    specialGroup);
        }
        else
        {
            yield return
                _specialExplosionController.Resolve(
                    specialItem);
        }

        yield return ResolveGravityAndRefill();

        UpdateCubeHints();

        Debug.Log(
            $"Special resolution finished. " +
            $"Remaining moves: {_remainingMoves}");

        _isResolving = false;
    }

    private IEnumerator AnimateSpecialCreation(
        GridItem specialItem)
    {
        Vector3 targetScale =
            specialItem.transform.localScale;

        specialItem.transform.localScale =
            Vector3.zero;

        float elapsed = 0f;

        while (elapsed < specialCreationDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    specialCreationDuration);

            float overshoot =
                Mathf.Sin(progress * Mathf.PI) *
                0.15f;

            specialItem.transform.localScale =
                Vector3.Lerp(
                    Vector3.zero,
                    targetScale,
                    progress) *
                (1f + overshoot);

            yield return null;
        }

        specialItem.transform.localScale =
            targetScale;
    }

    private IEnumerator ResolveGravityAndRefill()
    {
        List<FallAnimation> animations =
            new List<FallAnimation>();

        for (int column = 0;
             column < _board.Width;
             column++)
        {
            int nextAvailableRow = 0;

            for (int row = 0;
                 row < _board.Height;
                 row++)
            {
                GridItem item =
                    _board.GetItem(row, column);

                if (item == null)
                {
                    continue;
                }

                // Stone ve Chalice Box gibi sabit nesneler
                // kolonun aþaðýsý ile yukarýsýný ayýrýr.
                if (!item.CanFall)
                {
                    nextAvailableRow = row + 1;
                    continue;
                }

                if (row != nextAvailableRow)
                {
                    Vector3 startPosition =
                        item.transform.position;

                    Vector3 targetPosition =
                        _board.CellToWorld(
                            nextAvailableRow,
                            column);

                    _board.ClearItem(
                        row,
                        column,
                        item);

                    _board.SetItem(
                        nextAvailableRow,
                        column,
                        item);

                    item.SetGridPosition(
                        nextAvailableRow,
                        column);

                    animations.Add(
                        new FallAnimation
                        {
                            Item = item,
                            StartPosition =
                                startPosition,
                            TargetPosition =
                                targetPosition
                        });
                }

                nextAvailableRow++;
            }

            // Yalnýzca kolonun en üstteki açýk segmenti
            // yeni küplerle doldurulur. Sabit engellerin
            // içinden yeni küp geçemez.
            int spawnOrder = 0;

            for (int row = nextAvailableRow;
                 row < _board.Height;
                 row++)
            {
                Cube newCube =
                    _itemFactory.SpawnRandomCube(
                        row,
                        column,
                        _board,
                        transform);

                Vector3 targetPosition =
                    _board.CellToWorld(
                        row,
                        column);

                float spawnDistance =
                    (_board.Height -
                     row +
                     spawnOrder +
                     1) *
                    _board.CellSize;

                Vector3 startPosition =
                    targetPosition +
                    Vector3.up * spawnDistance;

                newCube.transform.position =
                    startPosition;

                animations.Add(
                    new FallAnimation
                    {
                        Item = newCube,
                        StartPosition =
                            startPosition,
                        TargetPosition =
                            targetPosition
                    });

                spawnOrder++;
            }
        }

        if (animations.Count == 0)
        {
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / fallDuration);

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress);

            foreach (FallAnimation animation
                     in animations)
            {
                if (animation.Item == null)
                {
                    continue;
                }

                animation.Item.transform.position =
                    Vector3.Lerp(
                        animation.StartPosition,
                        animation.TargetPosition,
                        smoothProgress);
            }

            yield return null;
        }

        foreach (FallAnimation animation
                 in animations)
        {
            if (animation.Item != null)
            {
                animation.Item.transform.position =
                    animation.TargetPosition;
            }
        }
    }

    private void UpdateCubeHints()
    {
        bool[,] visited =
            new bool[
                _board.Height,
                _board.Width];

        for (int row = 0;
             row < _board.Height;
             row++)
        {
            for (int column = 0;
                 column < _board.Width;
                 column++)
            {
                if (visited[row, column])
                {
                    continue;
                }

                GridItem item =
                    _board.GetItem(row, column);

                if (!(item is Cube cube))
                {
                    visited[row, column] = true;
                    continue;
                }

                List<Cube> group =
                    CubeGroupFinder.FindGroup(
                        _board,
                        cube);

                CubeHint hint;

                if (group.Count >= 6)
                {
                    hint = CubeHint.Tnt;
                }
                else if (group.Count >= 4)
                {
                    hint = CubeHint.Rocket;
                }
                else
                {
                    hint = CubeHint.None;
                }

                foreach (Cube groupCube in group)
                {
                    groupCube.SetHint(hint);

                    visited[
                        groupCube.Row,
                        groupCube.Column] = true;
                }
            }
        }
    }

    private void BuildGrid(LevelData data)
    {
        for (int row = 0;
             row < data.Height;
             row++)
        {
            for (int column = 0;
                 column < data.Width;
                 column++)
            {
                if (_board.GetItem(
                        row,
                        column) != null)
                {
                    continue;
                }

                int index =
                    row * data.Width +
                    column;

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

    private void FitCameraToGrid(
        LevelData data)
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
                sizeForWidth) *
            padding;

        _mainCamera.transform.position =
            new Vector3(
                0f,
                0f,
                _mainCamera.transform.position.z);
    }
}