using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Rocket ve TNT patlamalarýnýn animasyonunu ve zincirleme
// special-item tetiklenmesini yönetir.
public class SpecialExplosionController : MonoBehaviour
{
    [Header("Rocket Part Sprites")]
    [SerializeField] private Sprite horizontalLeftPart;
    [SerializeField] private Sprite horizontalRightPart;
    [SerializeField] private Sprite verticalTopPart;
    [SerializeField] private Sprite verticalBottomPart;

    [Header("Timings")]
    [SerializeField] private float rocketStepDuration = 0.06f;
    [SerializeField] private float tntPulseDuration = 0.18f;
    [SerializeField] private float itemDestroyDuration = 0.12f;

    private Board _board;

    // Ayný cube'un iki farklý patlama tarafýndan iki kez
    // silinmesini engeller.
    private readonly HashSet<GridItem> _removingItems =
        new HashSet<GridItem>();

    public void Initialize(Board board)
    {
        _board = board;
    }

    public IEnumerator Resolve(SpecialItem firstItem)
    {
        if (_board == null || firstItem == null)
        {
            yield break;
        }

        Queue<SpecialItem> pendingSpecials =
            new Queue<SpecialItem>();

        HashSet<SpecialItem> triggeredSpecials =
            new HashSet<SpecialItem>();

        EnqueueSpecial(
            firstItem,
            pendingSpecials,
            triggeredSpecials);

        while (pendingSpecials.Count > 0)
        {
            SpecialItem current =
                pendingSpecials.Dequeue();

            if (current == null)
            {
                continue;
            }

            Debug.Log(
                $"Exploding {current.GetType().Name} at " +
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

        // Son vurulan cube animasyonlarýnýn bitmesini bekle.
        if (itemDestroyDuration > 0f)
        {
            yield return new WaitForSeconds(
                itemDestroyDuration);
        }
    }

    private void EnqueueSpecial(
        SpecialItem specialItem,
        Queue<SpecialItem> pendingSpecials,
        HashSet<SpecialItem> triggeredSpecials)
    {
        if (specialItem == null ||
            !triggeredSpecials.Add(specialItem))
        {
            return;
        }

        // Board açýsýndan hücre hemen boþalýr; fakat nesne kendi
        // patlama sýrasý gelene kadar ekranda kalýr.
        _board.ClearItem(
            specialItem.Row,
            specialItem.Column,
            specialItem);

        pendingSpecials.Enqueue(specialItem);
    }

    private IEnumerator ResolveRocket(
        Rocket rocket,
        Queue<SpecialItem> pendingSpecials,
        HashSet<SpecialItem> triggeredSpecials)
    {
        int centerRow = rocket.Row;
        int centerColumn = rocket.Column;

        SpriteRenderer originalRenderer =
            rocket.GetComponent<SpriteRenderer>();

        Sprite fallbackSprite =
            originalRenderer != null
                ? originalRenderer.sprite
                : null;

        Sprite positiveSprite;
        Sprite negativeSprite;

        int rowStep;
        int columnStep;

        if (rocket.Direction ==
            RocketDirection.Horizontal)
        {
            positiveSprite =
                horizontalRightPart != null
                    ? horizontalRightPart
                    : fallbackSprite;

            negativeSprite =
                horizontalLeftPart != null
                    ? horizontalLeftPart
                    : fallbackSprite;

            rowStep = 0;
            columnStep = 1;
        }
        else
        {
            positiveSprite =
                verticalTopPart != null
                    ? verticalTopPart
                    : fallbackSprite;

            negativeSprite =
                verticalBottomPart != null
                    ? verticalBottomPart
                    : fallbackSprite;

            rowStep = 1;
            columnStep = 0;
        }

        Vector3 centerPosition =
            rocket.transform.position;

        Transform itemParent =
            rocket.transform.parent;

        GameObject positivePart =
            CreateRocketPart(
                "RocketPositivePart",
                positiveSprite,
                centerPosition,
                itemParent,
                originalRenderer);

        GameObject negativePart =
            CreateRocketPart(
                "RocketNegativePart",
                negativeSprite,
                centerPosition,
                itemParent,
                originalRenderer);

        if (originalRenderer != null)
        {
            originalRenderer.enabled = false;
        }

        Vector3 positivePosition =
            centerPosition;

        Vector3 negativePosition =
            centerPosition;

        for (int distance = 1; ; distance++)
        {
            int positiveRow =
                centerRow + rowStep * distance;

            int positiveColumn =
                centerColumn + columnStep * distance;

            int negativeRow =
                centerRow - rowStep * distance;

            int negativeColumn =
                centerColumn - columnStep * distance;

            bool positiveInside =
                _board.IsInside(
                    positiveRow,
                    positiveColumn);

            bool negativeInside =
                _board.IsInside(
                    negativeRow,
                    negativeColumn);

            if (!positiveInside &&
                !negativeInside)
            {
                break;
            }

            if (!positiveInside &&
                positivePart != null)
            {
                positivePart.SetActive(false);
            }

            if (!negativeInside &&
                negativePart != null)
            {
                negativePart.SetActive(false);
            }

            Vector3 positiveTarget =
                positiveInside
                    ? (Vector3)_board.CellToWorld(
                        positiveRow,
                        positiveColumn)
                    : positivePosition;

            Vector3 negativeTarget =
                negativeInside
                    ? (Vector3)_board.CellToWorld(
                        negativeRow,
                        negativeColumn)
                    : negativePosition;

            yield return MoveRocketParts(
                positivePart,
                positivePosition,
                positiveTarget,
                positiveInside,
                negativePart,
                negativePosition,
                negativeTarget,
                negativeInside);

            if (positiveInside)
            {
                positivePosition =
                    positiveTarget;

                HitCell(
                    positiveRow,
                    positiveColumn,
                    pendingSpecials,
                    triggeredSpecials);
            }

            if (negativeInside)
            {
                negativePosition =
                    negativeTarget;

                HitCell(
                    negativeRow,
                    negativeColumn,
                    pendingSpecials,
                    triggeredSpecials);
            }
        }

        if (positivePart != null)
        {
            Destroy(positivePart);
        }

        if (negativePart != null)
        {
            Destroy(negativePart);
        }

        Destroy(rocket.gameObject);
    }

    private IEnumerator ResolveTnt(
        Tnt tnt,
        Queue<SpecialItem> pendingSpecials,
        HashSet<SpecialItem> triggeredSpecials)
    {
        int centerRow = tnt.Row;
        int centerColumn = tnt.Column;

        Vector3 originalScale =
            tnt.transform.localScale;

        float elapsed = 0f;

        // Küçük bir büyüyüp küçülme animasyonu.
        while (elapsed < tntPulseDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / tntPulseDuration);

            float pulse =
                1f +
                Mathf.Sin(progress * Mathf.PI) *
                0.4f;

            tnt.transform.localScale =
                originalScale * pulse;

            yield return null;
        }

        tnt.transform.localScale =
            originalScale;

        // Merkezden iki hücre her yöne: toplam 5x5.
        for (int row = centerRow - 2;
             row <= centerRow + 2;
             row++)
        {
            for (int column = centerColumn - 2;
                 column <= centerColumn + 2;
                 column++)
            {
                if (!_board.IsInside(
                        row,
                        column))
                {
                    continue;
                }

                HitCell(
                    row,
                    column,
                    pendingSpecials,
                    triggeredSpecials);
            }
        }

        yield return ScaleToZero(
            tnt.transform,
            originalScale);

        Destroy(tnt.gameObject);
    }

    private void HitCell(
        int row,
        int column,
        Queue<SpecialItem> pendingSpecials,
        HashSet<SpecialItem> triggeredSpecials)
    {
        GridItem item =
            _board.GetItem(row, column);

        if (item == null)
        {
            return;
        }

        // Special item baþka special item tarafýndan vurulursa
        // kendi patlamasýný zincirleme olarak gerçekleþtirir.
        if (item is SpecialItem specialItem)
        {
            EnqueueSpecial(
                specialItem,
                pendingSpecials,
                triggeredSpecials);

            return;
        }

        if (item is Cube cube)
        {
            RemoveCube(cube);
            return;
        }

        if (item is Obstacle obstacle)
        {
            // Bir sonraki aþamada burada gerçek Stone, Vase ve
            // Chalice Box damage sistemi çaðrýlacak.
            Debug.Log(
                $"Special explosion hit " +
                $"{obstacle.GetType().Name} at " +
                $"({row}, {column}).");
        }
    }

    private void RemoveCube(Cube cube)
    {
        if (cube == null ||
            !_removingItems.Add(cube))
        {
            return;
        }

        _board.ClearItem(
            cube.Row,
            cube.Column,
            cube);

        StartCoroutine(
            AnimateAndDestroyCube(cube));
    }

    private IEnumerator AnimateAndDestroyCube(
        Cube cube)
    {
        if (cube == null)
        {
            yield break;
        }

        Vector3 startScale =
            cube.transform.localScale;

        float duration =
            Mathf.Max(
                0.01f,
                itemDestroyDuration);

        float elapsed = 0f;

        while (elapsed < duration &&
               cube != null)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration);

            cube.transform.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    progress);

            yield return null;
        }

        if (cube != null)
        {
            _removingItems.Remove(cube);
            Destroy(cube.gameObject);
        }
    }

    private IEnumerator ScaleToZero(
        Transform target,
        Vector3 startScale)
    {
        float duration =
            Mathf.Max(
                0.01f,
                itemDestroyDuration);

        float elapsed = 0f;

        while (elapsed < duration &&
               target != null)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration);

            target.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    progress);

            yield return null;
        }
    }

    private IEnumerator MoveRocketParts(
        GameObject positivePart,
        Vector3 positiveStart,
        Vector3 positiveTarget,
        bool positiveActive,
        GameObject negativePart,
        Vector3 negativeStart,
        Vector3 negativeTarget,
        bool negativeActive)
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

            if (positiveActive &&
                positivePart != null)
            {
                positivePart.transform.position =
                    Vector3.Lerp(
                        positiveStart,
                        positiveTarget,
                        smoothProgress);
            }

            if (negativeActive &&
                negativePart != null)
            {
                negativePart.transform.position =
                    Vector3.Lerp(
                        negativeStart,
                        negativeTarget,
                        smoothProgress);
            }

            yield return null;
        }
    }

    private GameObject CreateRocketPart(
        string objectName,
        Sprite sprite,
        Vector3 position,
        Transform parent,
        SpriteRenderer sourceRenderer)
    {
        GameObject part =
            new GameObject(objectName);

        part.transform.SetParent(parent);
        part.transform.position = position;

        SpriteRenderer renderer =
            part.AddComponent<SpriteRenderer>();

        renderer.sprite = sprite;

        if (sourceRenderer != null)
        {
            renderer.sortingLayerID =
                sourceRenderer.sortingLayerID;

            renderer.sortingOrder =
                sourceRenderer.sortingOrder + 10;
        }

        FitSpriteToCell(
            part.transform,
            sprite);

        return part;
    }

    private void FitSpriteToCell(
        Transform target,
        Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Vector2 spriteSize =
            sprite.bounds.size;

        if (spriteSize.x <= 0f ||
            spriteSize.y <= 0f)
        {
            return;
        }

        float targetSize =
            _board.CellSize * 0.8f;

        float scale =
            Mathf.Min(
                targetSize / spriteSize.x,
                targetSize / spriteSize.y);

        target.localScale =
            new Vector3(
                scale,
                scale,
                1f);
    }
}