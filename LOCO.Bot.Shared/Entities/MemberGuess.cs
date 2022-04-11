namespace LOCO.Bot.Shared.Entities;

public class MemberGuess : IHaveId
{
    public int Id { get; set; }

    public virtual string MemberName { get; set; }
    public virtual ulong MemberId { get; set; }
    public virtual double Guess { get; set; }

    public void Update(object guess)
    {
        if(guess is MemberGuess memberGuess)
        {
            MemberName = memberGuess.MemberName;
            MemberId = memberGuess.MemberId;
            Guess = memberGuess.Guess;
        }
    }
}
