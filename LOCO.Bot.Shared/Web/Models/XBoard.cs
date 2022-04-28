namespace LOCO.Bot.Shared.Web.Models;

public class XBoard
{
    public XBoard()
    {
    }

    public double Width { get; private set; } = 1250;
    public double Height { get; private set; } = 210;

    public ICollection<XCard> Cards { get; set; } = new List<XCard>();
    public double XVel { get; private set; }

    public void Spin() => Cards.ToList().ForEach(x => x.Move(XVel));
    internal void Add(XCard xCard) => Cards.Add(xCard);

    public void Resize(double width, double height) =>
        (Width, Height) = (width, height);

    public void AddCards(IEnumerable<XCard> cards)
    {
        foreach (var xcard in cards)
        {
            Cards.Add(xcard);
        }
    }

    public void Clean()
    {
       Cards.Where(x => Math.Abs(x.X) >= Width).ToList().ForEach(x => Cards.Remove(x));
    }
}
