using System.Drawing;

namespace LOCO.Bot.Blazor.Shared.Wheel;

public class Wheel : IWheel
{
    public ICollection<WheelEntry> WheelEntries { get; set; } = new List<WheelEntry>();

    public void AddEntries(string text, string color, int qty)
    {
        for (int i = 0; i < qty; i++)
            WheelEntries.Add(new WheelEntry(text, color));
    }

    public void AddEntries(string text, Color color, int qty)
    {
        for (int i = 0; i < qty; i++)
            WheelEntries.Add(new WheelEntry(text, color));
    }

    public void Shuffle() => WheelEntries = WheelEntries.Shuffle().ToList();
}
