using LOCO.Bot.Shared.Data.Entities;

namespace LOCO.Bot.Shared.Web.Models;

public class XCard : WheelEntry
{
    public const double CardWidth = 175.00;
    public const double CardHeight = 240.00;

    public double X { get; private set; }
    public double Y { get; private set; }
    public double XVel { get; private set; }
    public double Width { get; private set; } = CardWidth;
    public double Height { get; private set; } = CardHeight;

    public XCard(double x, double y, double xVel, string color)
        => (X, Y, XVel, Color) = (x, y, xVel, color);

    public XCard(double x, double y, double xVel, WheelEntry we)
        => (X, Y, XVel, Color, Text, Id) = (x, y, xVel, we.Color, we.Text, we.Id);

    public void Move(double xVel) => X += XVel + xVel;
}
