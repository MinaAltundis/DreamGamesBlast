using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SpecialExplosionController
{
    [Header("Combo Timings")]
    [SerializeField]
    private float comboGatherDuration = 0.16f;

    [SerializeField]
    private float comboPulseDuration = 0.14f;

    private enum SpecialComboType
    {
        RocketRocket,
        TntRocket,
        TntTnt
    }

    private sealed class ComboRocketLane
    {
        public int OriginRow;
        public int OriginColumn;

        public int RowStep;
        public int ColumnStep;

        public GameObject PositivePart;
        public GameObject NegativePart;

        public Vector3 PositivePosition;
        public Vector3 NegativePosition;
    }

    public IEnumerator ResolveCombo(
        SpecialItem tappedItem,
        List<SpecialItem> comboGroup)
    {
        if (_board == null || tappedItem == null)
        {
            yield break;
        }

        if (comboGroup == null ||
            comboGroup.Count < 2)
        {
            yield return Resolve(tappedItem);
            yield break;
        }

        int centerRow = tappedItem.Row;
        int centerColumn = tappedItem.Column;

        Vector3 centerPosition =
            _board.CellToWorld(
                centerRow,
                centerColumn);

        SpriteRenderer sourceRenderer =
            tappedItem.GetComponent<SpriteRenderer>();

        Sprite fallbackSprite =
            sourceRenderer != null
                ? sourceRenderer.sprite
                : null;

        Queue<SpecialItem> pendingSpecials =
            new Queue<SpecialItem>();

        HashSet<SpecialItem> triggeredSpecials =
            new HashSet<SpecialItem>();

        // Combo üyeleri artýk Board üzerinde boþ kabul edilir.
        // Ayrýca bireysel patlama kuyruðuna tekrar eklenemezler.
        foreach (SpecialItem specialItem
                 in comboGroup)
        {
            if (specialItem == null)
            {
                continue;
            }

            triggeredSpecials.Add(specialItem);

            _board.ClearItem(
                specialItem.Row,
                specialItem.Column,
                specialItem);
        }

        SpecialComboType comboType =
            DetermineComboType(comboGroup);

        Debug.Log(
            $"{comboType} combo started at " +
            $"({centerRow}, {centerColumn}) " +
            $"with {comboGroup.Count} items.");

        // Bütün combo üyeleri týklanan hücreye hareket eder.
        yield return GatherComboItems(
            comboGroup,
            centerPosition);

        yield return PulseComboItems(comboGroup);

        // Combo üyeleri kendi bireysel Rocket/TNT
        // patlamalarýný gerçekleþtirmeyecek.
        foreach (SpecialItem specialItem
                 in comboGroup)
        {
            if (specialItem != null)
            {
                Destroy(specialItem.gameObject);
            }
        }

        Dictionary<Obstacle, int> obstacleHits =
            new Dictionary<Obstacle, int>();

        HashSet<Vector2Int> processedCells =
            new HashSet<Vector2Int>();

        switch (comboType)
        {
            case SpecialComboType.RocketRocket:
                yield return ResolveRocketLinesCombo(
                    centerRow,
                    centerColumn,
                    horizontalRadius: 0,
                    verticalRadius: 0,
                    fallbackSprite,
                    sourceRenderer,
                    pendingSpecials,
                    triggeredSpecials,
                    obstacleHits,
                    processedCells);
                break;

            case SpecialComboType.TntTnt:
                ResolveAreaCombo(
                    centerRow,
                    centerColumn,
                    radius: 3,
                    pendingSpecials,
                    triggeredSpecials,
                    obstacleHits,
                    processedCells);
                break;

            case SpecialComboType.TntRocket:
                // Üç yatay ve üç dikey Rocket çizgisi.
                yield return ResolveRocketLinesCombo(
                    centerRow,
                    centerColumn,
                    horizontalRadius: 1,
                    verticalRadius: 1,
                    fallbackSprite,
                    sourceRenderer,
                    pendingSpecials,
                    triggeredSpecials,
                    obstacleHits,
                    processedCells);
                break;
        }

        // Bütün combo tek bir obstacle damage source olarak uygulanýr.
        ApplyObstacleHits(obstacleHits);

        // Combo dýþýnda patlama tarafýndan vurulan special item'lar
        // bireysel zincirleme patlamalarýný yapar.
        yield return ResolveComboChainReactions(
            pendingSpecials,
            triggeredSpecials);

        if (itemDestroyDuration > 0f)
        {
            yield return new WaitForSeconds(
                itemDestroyDuration);
        }

        Debug.Log(
            $"{comboType} combo finished.");
    }

    private SpecialComboType DetermineComboType(
        List<SpecialItem> comboGroup)
    {
        int tntCount = 0;

        foreach (SpecialItem specialItem
                 in comboGroup)
        {
            if (specialItem is Tnt)
            {
                tntCount++;
            }
        }

        // En az iki TNT varsa diðer Rocket'lar olsa bile
        // TNT-TNT kuralý önceliklidir.
        if (tntCount >= 2)
        {
            return SpecialComboType.TntTnt;
        }

        if (tntCount == 1)
        {
            return SpecialComboType.TntRocket;
        }

        return SpecialComboType.RocketRocket;
    }

    private IEnumerator GatherComboItems(
        List<SpecialItem> comboGroup,
        Vector3 targetPosition)
    {
        Vector3[] startPositions =
            new Vector3[comboGroup.Count];

        for (int i = 0;
             i < comboGroup.Count;
             i++)
        {
            if (comboGroup[i] != null)
            {
                startPositions[i] =
                    comboGroup[i].transform.position;
            }
        }

        float duration =
            Mathf.Max(
                0.01f,
                comboGatherDuration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration);

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress);

            for (int i = 0;
                 i < comboGroup.Count;
                 i++)
            {
                SpecialItem specialItem =
                    comboGroup[i];

                if (specialItem == null)
                {
                    continue;
                }

                specialItem.transform.position =
                    Vector3.Lerp(
                        startPositions[i],
                        targetPosition,
                        smoothProgress);
            }

            yield return null;
        }

        foreach (SpecialItem specialItem
                 in comboGroup)
        {
            if (specialItem != null)
            {
                specialItem.transform.position =
                    targetPosition;
            }
        }
    }

    private IEnumerator PulseComboItems(
        List<SpecialItem> comboGroup)
    {
        Vector3[] startScales =
            new Vector3[comboGroup.Count];

        for (int i = 0;
             i < comboGroup.Count;
             i++)
        {
            if (comboGroup[i] != null)
            {
                startScales[i] =
                    comboGroup[i]
                        .transform
                        .localScale;
            }
        }

        float duration =
            Mathf.Max(
                0.01f,
                comboPulseDuration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration);

            float pulse =
                1f +
                Mathf.Sin(progress * Mathf.PI) *
                0.35f;

            for (int i = 0;
                 i < comboGroup.Count;
                 i++)
            {
                SpecialItem specialItem =
                    comboGroup[i];

                if (specialItem == null)
                {
                    continue;
                }

                specialItem.transform.localScale =
                    startScales[i] * pulse;
            }

            yield return null;
        }
    }

    private void ResolveAreaCombo(
        int centerRow,
        int centerColumn,
        int radius,
        Queue<SpecialItem> pendingSpecials,
        HashSet<SpecialItem> triggeredSpecials,
        Dictionary<Obstacle, int> obstacleHits,
        HashSet<Vector2Int> processedCells)
    {
        for (int row = centerRow - radius;
             row <= centerRow + radius;
             row++)
        {
            for (int column = centerColumn - radius;
                 column <= centerColumn + radius;
                 column++)
            {
                HitComboCell(
                    row,
                    column,
                    pendingSpecials,
                    triggeredSpecials,
                    obstacleHits,
                    processedCells);
            }
        }
    }

    private IEnumerator ResolveRocketLinesCombo(
        int centerRow,
        int centerColumn,
        int horizontalRadius,
        int verticalRadius,
        Sprite fallbackSprite,
        SpriteRenderer sourceRenderer,
        Queue<SpecialItem> pendingSpecials,
        HashSet<SpecialItem> triggeredSpecials,
        Dictionary<Obstacle, int> obstacleHits,
        HashSet<Vector2Int> processedCells)
    {
        List<ComboRocketLane> lanes =
            new List<ComboRocketLane>();

        // Yatay Rocket çizgileri.
        for (int rowOffset = -horizontalRadius;
             rowOffset <= horizontalRadius;
             rowOffset++)
        {
            int row = centerRow + rowOffset;

            if (!_board.IsInside(
                    row,
                    centerColumn))
            {
                continue;
            }

            lanes.Add(
                CreateComboRocketLane(
                    row,
                    centerColumn,
                    rowStep: 0,
                    columnStep: 1,
                    fallbackSprite,
                    sourceRenderer));
        }

        // Dikey Rocket çizgileri.
        for (int columnOffset = -verticalRadius;
             columnOffset <= verticalRadius;
             columnOffset++)
        {
            int column =
                centerColumn + columnOffset;

            if (!_board.IsInside(
                    centerRow,
                    column))
            {
                continue;
            }

            lanes.Add(
                CreateComboRocketLane(
                    centerRow,
                    column,
                    rowStep: 1,
                    columnStep: 0,
                    fallbackSprite,
                    sourceRenderer));
        }

        // Rocket'larýn baþladýðý hücreler de patlamadan etkilenir.
        foreach (ComboRocketLane lane in lanes)
        {
            HitComboCell(
                lane.OriginRow,
                lane.OriginColumn,
                pendingSpecials,
                triggeredSpecials,
                obstacleHits,
                processedCells);
        }

        int maximumDistance =
            Mathf.Max(
                _board.Width,
                _board.Height);

        for (int distance = 1;
             distance <= maximumDistance;
             distance++)
        {
            int laneCount = lanes.Count;

            Vector3[] positiveStarts =
                new Vector3[laneCount];

            Vector3[] negativeStarts =
                new Vector3[laneCount];

            Vector3[] positiveTargets =
                new Vector3[laneCount];

            Vector3[] negativeTargets =
                new Vector3[laneCount];

            bool[] positiveActive =
                new bool[laneCount];

            bool[] negativeActive =
                new bool[laneCount];

            int[] positiveRows =
                new int[laneCount];

            int[] positiveColumns =
                new int[laneCount];

            int[] negativeRows =
                new int[laneCount];

            int[] negativeColumns =
                new int[laneCount];

            bool anyActive = false;

            for (int i = 0;
                 i < laneCount;
                 i++)
            {
                ComboRocketLane lane =
                    lanes[i];

                positiveStarts[i] =
                    lane.PositivePosition;

                negativeStarts[i] =
                    lane.NegativePosition;

                positiveRows[i] =
                    lane.OriginRow +
                    lane.RowStep * distance;

                positiveColumns[i] =
                    lane.OriginColumn +
                    lane.ColumnStep * distance;

                negativeRows[i] =
                    lane.OriginRow -
                    lane.RowStep * distance;

                negativeColumns[i] =
                    lane.OriginColumn -
                    lane.ColumnStep * distance;

                positiveActive[i] =
                    _board.IsInside(
                        positiveRows[i],
                        positiveColumns[i]);

                negativeActive[i] =
                    _board.IsInside(
                        negativeRows[i],
                        negativeColumns[i]);

                if (positiveActive[i])
                {
                    positiveTargets[i] =
                        _board.CellToWorld(
                            positiveRows[i],
                            positiveColumns[i]);

                    anyActive = true;
                }
                else
                {
                    positiveTargets[i] =
                        lane.PositivePosition;

                    if (lane.PositivePart != null)
                    {
                        lane.PositivePart.SetActive(false);
                    }
                }

                if (negativeActive[i])
                {
                    negativeTargets[i] =
                        _board.CellToWorld(
                            negativeRows[i],
                            negativeColumns[i]);

                    anyActive = true;
                }
                else
                {
                    negativeTargets[i] =
                        lane.NegativePosition;

                    if (lane.NegativePart != null)
                    {
                        lane.NegativePart.SetActive(false);
                    }
                }
            }

            if (!anyActive)
            {
                break;
            }

            yield return AnimateComboRocketStep(
                lanes,
                positiveStarts,
                positiveTargets,
                positiveActive,
                negativeStarts,
                negativeTargets,
                negativeActive);

            for (int i = 0;
                 i < laneCount;
                 i++)
            {
                ComboRocketLane lane =
                    lanes[i];

                if (positiveActive[i])
                {
                    lane.PositivePosition =
                        positiveTargets[i];

                    HitComboCell(
                        positiveRows[i],
                        positiveColumns[i],
                        pendingSpecials,
                        triggeredSpecials,
                        obstacleHits,
                        processedCells);
                }

                if (negativeActive[i])
                {
                    lane.NegativePosition =
                        negativeTargets[i];

                    HitComboCell(
                        negativeRows[i],
                        negativeColumns[i],
                        pendingSpecials,
                        triggeredSpecials,
                        obstacleHits,
                        processedCells);
                }
            }
        }

        foreach (ComboRocketLane lane in lanes)
        {
            if (lane.PositivePart != null)
            {
                Destroy(lane.PositivePart);
            }

            if (lane.NegativePart != null)
            {
                Destroy(lane.NegativePart);
            }
        }
    }

    private ComboRocketLane CreateComboRocketLane(
        int originRow,
        int originColumn,
        int rowStep,
        int columnStep,
        Sprite fallbackSprite,
        SpriteRenderer sourceRenderer)
    {
        bool horizontal =
            rowStep == 0;

        Sprite positiveSprite =
            horizontal
                ? horizontalRightPart
                : verticalTopPart;

        Sprite negativeSprite =
            horizontal
                ? horizontalLeftPart
                : verticalBottomPart;

        if (positiveSprite == null)
        {
            positiveSprite = fallbackSprite;
        }

        if (negativeSprite == null)
        {
            negativeSprite = fallbackSprite;
        }

        Vector3 originPosition =
            _board.CellToWorld(
                originRow,
                originColumn);

        return new ComboRocketLane
        {
            OriginRow = originRow,
            OriginColumn = originColumn,
            RowStep = rowStep,
            ColumnStep = columnStep,

            PositivePart = CreateRocketPart(
                "ComboRocketPositivePart",
                positiveSprite,
                originPosition,
                transform,
                sourceRenderer),

            NegativePart = CreateRocketPart(
                "ComboRocketNegativePart",
                negativeSprite,
                originPosition,
                transform,
                sourceRenderer),

            PositivePosition = originPosition,
            NegativePosition = originPosition
        };
    }

    private IEnumerator AnimateComboRocketStep(
        List<ComboRocketLane> lanes,
        Vector3[] positiveStarts,
        Vector3[] positiveTargets,
        bool[] positiveActive,
        Vector3[] negativeStarts,
        Vector3[] negativeTargets,
        bool[] negativeActive)
    {
        float duration =
            Mathf.Max(
                0.01f,
                rocketStepDuration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration);

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress);

            for (int i = 0;
                 i < lanes.Count;
                 i++)
            {
                ComboRocketLane lane =
                    lanes[i];

                if (positiveActive[i] &&
                    lane.PositivePart != null)
                {
                    lane.PositivePart
                        .transform
                        .position =
                        Vector3.Lerp(
                            positiveStarts[i],
                            positiveTargets[i],
                            smoothProgress);
                }

                if (negativeActive[i] &&
                    lane.NegativePart != null)
                {
                    lane.NegativePart
                        .transform
                        .position =
                        Vector3.Lerp(
                            negativeStarts[i],
                            negativeTargets[i],
                            smoothProgress);
                }
            }

            yield return null;
        }
    }

    private void HitComboCell(
        int row,
        int column,
        Queue<SpecialItem> pendingSpecials,
        HashSet<SpecialItem> triggeredSpecials,
        Dictionary<Obstacle, int> obstacleHits,
        HashSet<Vector2Int> processedCells)
    {
        if (!_board.IsInside(row, column))
        {
            return;
        }

        Vector2Int cell =
            new Vector2Int(column, row);

        // Yatay ve dikey Rocket çizgilerinin kesiþtiði hücre
        // ayný combo kaynaðýnda yalnýzca bir kez sayýlýr.
        if (!processedCells.Add(cell))
        {
            return;
        }

        HitCell(
            row,
            column,
            pendingSpecials,
            triggeredSpecials,
            obstacleHits);
    }

    private IEnumerator ResolveComboChainReactions(
        Queue<SpecialItem> pendingSpecials,
        HashSet<SpecialItem> triggeredSpecials)
    {
        while (pendingSpecials.Count > 0)
        {
            SpecialItem current =
                pendingSpecials.Dequeue();

            if (current == null)
            {
                continue;
            }

            Debug.Log(
                $"Combo triggered " +
                $"{current.GetType().Name} at " +
                $"({current.Row}, {current.Column}).");

            if (current is Rocket rocket)
            {
                yield return ResolveRocket(
                    rocket,
                    pendingSpecials,
                    triggeredSpecials);
            }
            else if (current is Tnt tnt)
            {
                yield return ResolveTnt(
                    tnt,
                    pendingSpecials,
                    triggeredSpecials);
            }
        }
    }
}