using UnityEngine;

public class Vase : Obstacle
{
    private Sprite _damagedSprite;

    public int HitPoints { get; private set; }

    public override bool CanFall => true;

    public void Init(
        Sprite healthySprite,
        Sprite damagedSprite)
    {
        HitPoints = 2;
        _damagedSprite = damagedSprite;

        SetSprite(healthySprite);
    }

    public override bool ApplyDamage(
        ObstacleDamageSource source,
        int affectedCount)
    {
        if (IsCleared || affectedCount <= 0)
        {
            return IsCleared;
        }

        // Bir damage source, kaç hücre/küp ile vurursa vursun
        // Vase'e en fazla bir hasar verir.
        HitPoints--;

        if (HitPoints == 1)
        {
            if (_damagedSprite != null)
            {
                SetSprite(_damagedSprite);
            }

            Debug.Log(
                $"Vase damaged at ({Row}, {Column}).");
        }

        if (HitPoints <= 0)
        {
            HitPoints = 0;
            IsCleared = true;

            Debug.Log(
                $"Vase cleared at ({Row}, {Column}).");
        }

        return IsCleared;
    }
}