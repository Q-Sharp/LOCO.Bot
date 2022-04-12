namespace LOCO.Bot.Shared.Entities;

public class Setting : IHaveId
{
    public int Id { get; set; }

    public virtual ulong GuessChannelId { get; set; }
    public virtual ulong GuessMemberRoleId { get; set; }
    public virtual bool GuessingsPossible { get; set; }
    public virtual ulong GuildId { get; set; }

    public void Update(object guess)
    {
        if (guess is Setting setting)
        {
            GuessChannelId = setting.GuessChannelId;
            GuessMemberRoleId = setting.GuessMemberRoleId;
            GuessingsPossible = setting.GuessingsPossible;
            GuildId = setting.GuildId;
        }
    }
}