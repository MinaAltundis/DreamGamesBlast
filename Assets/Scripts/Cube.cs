using UnityEngine;

// Dört küp rengi. (Cube'la sýký iliþkili olduðu için ayný dosyada tutuyoruz.)
public enum CubeColor
{
    Red,
    Green,
    Blue,
    Yellow
}

// Board üzerindeki renkli bir küp. Ortak davranýþý GridItem'dan miras alýr.
public class Cube : GridItem
{
    public CubeColor Color { get; private set; }

    public void Init(CubeColor color, Sprite sprite)
    {
        Color = color;
        SetSprite(sprite);
    }
}