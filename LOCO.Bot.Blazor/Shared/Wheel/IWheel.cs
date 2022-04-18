namespace LOCO.Bot.Blazor.Shared.Wheel;

public interface IWheel
{
    ICollection<WheelEntry> WheelEntries { get; set; }

    void AddEntries(string text, string color, int qty);
}
