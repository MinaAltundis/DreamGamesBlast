using UnityEngine;

public class Vase : Obstacle
{
    private Sprite _damagedSprite;

    public int HitPoints { get; private set; }

    public override bool CanFall => true;

    public void Init(Sprite healthySprite, Sprite damagedSprite)
    {
        HitPoints = 2;
        _damagedSprite = damagedSprite;

        SetSprite(healthySprite);
    }

    // We will call this from the blast system later.
    public bool TakeOneDamage()
    {
        if (HitPoints <= 0)
        {
            return true;
        }

        HitPoints--;

        if (HitPoints == 1 && _damagedSprite != null)
        {
            SetSprite(_damagedSprite);
        }

        return HitPoints == 0;
    }
}