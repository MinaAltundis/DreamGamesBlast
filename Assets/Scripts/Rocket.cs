using UnityEngine;

public enum RocketDirection
{
    Horizontal,
    Vertical
}

public class Rocket : SpecialItem
{
    public RocketDirection Direction { get; private set; }

    public void Init(RocketDirection direction, Sprite sprite)
    {
        Direction = direction;
        SetSprite(sprite);
    }
}