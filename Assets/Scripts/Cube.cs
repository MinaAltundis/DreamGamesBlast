using UnityEngine;

public enum CubeColor
{
    Red,
    Green,
    Blue,
    Yellow
}

public class Cube : GridItem
{
    public CubeColor Color { get; private set; }

    public override bool CanFall => true;

    public void Init(CubeColor color, Sprite sprite)
    {
        Color = color;
        SetSprite(sprite);
    }
}