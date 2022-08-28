using Discord;
using Discord.Commands;
using Discord.Rest;

using FakeItEasy;

using LOCO.Bot.Data;
using LOCO.Bot.Modules;
using LOCO.Bot.Shared.Data.Entities;
using LOCO.Bot.Shared.Discord.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Xunit;

namespace LOCO.Bot.Tests.Modules;

public class MemberGuessModuleTest
{
    private Context _ctx;
    private ICommandContext _cctx;

    private GuessModule GetSut()
    {
        var options = new DbContextOptionsBuilder<Context>()
                            .UseInMemoryDatabase(databaseName: "Test")
                            .Options;

        _ctx = new Context(options);
        var ss = A.Fake<ISettingService>();
        A.CallTo(() => ss.GetSettings(A.Dummy<ulong>()))
         .Returns(Task.FromResult(new Setting { Id = 1, GuessChannelId = 1, GuessMemberRoleId = 1, GuessingsPossible = true }));

        var ch = A.Fake<ICommandHandler>();
        var l = A.Fake<ILogger<GuessModule>>();
        _cctx = A.Fake<ICommandContext>();
        var rc = A.Fake<DiscordRestClient>();

        var m = new GuessModule(_ctx, ss, ch, l, rc);

        //(m as IModuleBase).SetContext(_cctx);

        return m;
    }

    [Fact]
    public async Task SetChannelWithValidChannelAllGood()
    {
        var c = A.Dummy<IChannel>();
        var m = GetSut();

        var result = await m.SetChannel(c);

        Assert.True(result.IsSuccess);
    }

    //[Theory]
    //[InlineData("5", 5.00, 1)]
    //[InlineData("5.01", 5.01, 2)]
    //[InlineData("2000€", 2000.00, 3)]
    //[InlineData("454.23$", 454.23, 4)]
    //[InlineData("BLABLABLA", null, 5)]
    //public async Task GuessWithDifferentValues(string guess, double? expectedResult, ulong memberId)
    //{
    //    var m = GetSut();
    //    A.CallTo(() => _cctx.User.Id).Returns(memberId);

    //    var result = await m.Guess(guess);
    //    var guessDb = _ctx.MemberGuess.FirstOrDefault(x => x.MemberId == memberId);

    //    Assert.Equal(expectedResult, guessDb?.Guess);
    //}
}
