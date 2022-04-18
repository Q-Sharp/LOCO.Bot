namespace LOCO.Bot.Shared.Data.Entities;

public class WheelEntry : IHaveId
{
    public int Id { get; set; }

    public Guid Guid { get; set; } = Guid.NewGuid();

    public string Text { get; set; }
    public string Color { get; set; }

    public int Qty { get; set; }

    public void Update(object guess)
    {
        if (guess is WheelEntry setting)
        {
            Text = setting.Text;
            Color = setting.Color;
        }
    }
}
