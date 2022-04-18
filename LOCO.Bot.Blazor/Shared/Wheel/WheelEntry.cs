using System.Drawing;

namespace LOCO.Bot.Blazor.Shared.Wheel;

public class WheelEntry
{
    public Guid Id { get; }

    public string Text { get; }
    public string Color { get; }

    public WheelEntry(string text, Color color) : this(text, ToHex(color))
    {

    }

    public WheelEntry(string text, string hexColor)
    {
        Id = Guid.NewGuid();
        Text = text;
        Color = hexColor;
    }

    private static string ToHex(Color c) 
            => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

}
