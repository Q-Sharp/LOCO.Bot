namespace LOCO.Bot.Shared.Data;

public interface IHaveId
{
    int Id { get; set; }
    void Update(object HaveId);
}
