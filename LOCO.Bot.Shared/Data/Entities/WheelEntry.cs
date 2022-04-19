namespace LOCO.Bot.Shared.Data.Entities;

public class WheelEntry : IHaveId
{
    public int Id { get; set; }

    public Guid Guid { get; set; } = Guid.NewGuid();

    public string Text { get; set; }
    public string Color { get; set; }

    public int Qty { get; set; }

    public void Update(object wheelEntry)
    {
        if (wheelEntry is WheelEntry we)
        {
            Text = we.Text;
            Color = we.Color;
            Qty = we.Qty;
        }
    }
}
