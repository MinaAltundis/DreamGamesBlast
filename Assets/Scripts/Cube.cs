using UnityEngine;

public enum CubeColor
{
    Red,
    Green,
    Blue,
    Yellow
}

public enum CubeHint
{
    None,
    Rocket,
    Tnt
}

public class Cube : GridItem
{
    public CubeColor Color { get; private set; }
    public CubeHint CurrentHint { get; private set; }

    public override bool CanFall => true;

    private Sprite _defaultSprite;
    private Sprite _rocketHintSprite;
    private Sprite _tntHintSprite;

    public void Init(
        CubeColor color,
        Sprite defaultSprite,
        Sprite rocketHintSprite,
        Sprite tntHintSprite)
    {
        Color = color;

        _defaultSprite = defaultSprite;
        _rocketHintSprite = rocketHintSprite;
        _tntHintSprite = tntHintSprite;

        SetHint(CubeHint.None);
    }

    public void SetHint(CubeHint hint)
    {
        CurrentHint = hint;

        switch (hint)
        {
            case CubeHint.Rocket:
                SetSprite(
                    _rocketHintSprite != null
                        ? _rocketHintSprite
                        : _defaultSprite);
                break;

            case CubeHint.Tnt:
                SetSprite(
                    _tntHintSprite != null
                        ? _tntHintSprite
                        : _defaultSprite);
                break;

            default:
                SetSprite(_defaultSprite);
                break;
        }
    }
}