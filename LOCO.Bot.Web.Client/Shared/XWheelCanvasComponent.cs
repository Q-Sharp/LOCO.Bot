using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;

using LOCO.Bot.Shared.Web.Models;
using LOCO.Bot.Shared.Web.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using static MudBlazor.CategoryTypes;

namespace LOCO.Bot.Web.Client.Shared;

public class XWheelCanvasComponent : LayoutComponentBase
{
    [Inject]
    public IWheelService WheelService { get; set; }

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    public bool Spinning { get; set; }

    protected XBoard xBoard = new();

    private Canvas2DContext _context;

    protected BECanvasComponent _canvas;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _context = await _canvas.CreateCanvas2DAsync();
        await JSRuntime.InvokeAsync<object>("initGame", DotNetObjectReference.Create(this));
        await AddMissingCards();
        await DrawBoard();
        await DrawCards();
        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public async ValueTask GameLoop(int screenWidth, int screenHeight)
    {
        // update & render
        if(Spinning)
            await Update(screenWidth, screenHeight);

        await DrawCards();   
    }

    private async ValueTask Update(int screenWidth, int screenHeight)
    {
        foreach (var card in xBoard.Cards)
        {
            await _context.ClearRectAsync(card.X, card.Y, card.Width, card.Height);
            card.Move(500);
        }
    }

    protected async Task DrawBoard()
    {
        await _context.SetFillStyleAsync("#27272f");

        await _context.FillRectAsync(0, 0, xBoard.Width, xBoard.Height);
    }

    protected async ValueTask DrawCards()
    {
        await _context.SetTextAlignAsync(TextAlign.Start);
        await _context.SetFontAsync("18px Lato");

        foreach (var card in xBoard.Cards)
        {
            await _context.SetFillStyleAsync(card.Color);

            await _context.FillRectAsync(card.X, card.Y, card.Width, card.Height);

            await _context.SetFillStyleAsync("white");
            await _context.FillTextAsync(card.Text, card.X + 30, card.Y + (card.Height / 3), card.Width + 60);
        }
    }

    public async Task Spin()
    {
        Spinning = true;
        await Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(x => Spinning = false);
    }

    private async Task AddMissingCards()
    {
        if (xBoard.Cards.Count < xBoard.NeededCards)
        {
            var wheelEntries = (await WheelService.GetNextWheelEntries(xBoard.NeededCards - xBoard.Cards.Count)).ToList();

            var i = xBoard.NeededCards / 2 * -1;
            var cards = wheelEntries.Select(x => new XCard((XCard.CardWidth + 10) * i++, 10, 2, x));

            xBoard.AddCards(cards);
        }
    }
}
