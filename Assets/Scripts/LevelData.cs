using System.Collections.Generic;

// Bir level JSON dosyasýnýn C#'taki karþýlýðý. Sadece veri tutar, davranýþý yok.
[System.Serializable]
public class LevelData
{
    // Bu alan isimleri neden küçük harf + alt çizgi? Çünkü Unity'nin JsonUtility'si
    // JSON anahtarlarýný C# alanlarýna ADINA GÖRE eþler. JSON dosyalarý "grid_width"
    // gibi yazdýðý için, alanlarýn da birebir ayný isimde olmasý gerekiyor.
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;

    // Kodun geri kalaný yukarýdaki çirkin isimlerle uðraþmasýn diye temiz,
    // sadece-okunur özellikler. "=>" ifadesi "bunu döndür" demenin kýsa yolu.
    public int LevelNumber => level_number;
    public int Width => grid_width;
    public int Height => grid_height;
    public int MoveCount => move_count;
    public IReadOnlyList<string> Grid => grid;
}