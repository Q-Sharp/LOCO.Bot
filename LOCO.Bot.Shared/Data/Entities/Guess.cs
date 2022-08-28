namespace LOCO.Bot.Shared.Data.Entities;

public class Guess : IHaveId
{
    public int Id { get; set; }

    public virtual string MemberName { get; set; }
    public virtual ulong MemberId { get; set; }
    public virtual double GuessAmount { get; set; }

    public void Update(object guess)
    {
        if (guess is Guess memberGuess)
        {
            MemberName = memberGuess.MemberName;
            MemberId = memberGuess.MemberId;
            GuessAmount = memberGuess.GuessAmount;
        }
    }
}
