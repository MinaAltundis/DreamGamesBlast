using UnityEngine;

public class Stone : Obstacle
{
    public void Init(Sprite sprite)
    {
        SetSprite(sprite);
    }

    public override bool ApplyDamage(
        ObstacleDamageSource source,
        int affectedCount)
    {
        if (IsCleared || affectedCount <= 0)
        {
            return IsCleared;
        }

        // Stone adjacent cube blast'ten etkilenmez.
        if (source !=
            ObstacleDamageSource.SpecialItemExplosion)
        {
            return false;
        }

        IsCleared = true;

        Debug.Log(
            $"Stone cleared at ({Row}, {Column}).");

        return true;
    }
}