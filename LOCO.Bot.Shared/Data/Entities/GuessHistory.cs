namespace LOCO.Bot.Shared.Data.Entities;

public class GuessHistory : IHaveId
{
    public int Id { get; set; }

    public ulong GuildId { get; set; }

    public virtual DateTime? StartDate { get; set; }
    public virtual DateTime? EndDate { get; set; }
    public virtual DateTime? ResultDate { get; set; }

    public virtual bool Valid { get; set; }

    public void Update(object guess)
    {
        if (guess is GuessHistory guessHistory)
        {
            GuildId = guessHistory.GuildId;
            StartDate = guessHistory.StartDate;
            EndDate = guessHistory.EndDate;
            ResultDate = guessHistory.ResultDate;
        }
    }
}
