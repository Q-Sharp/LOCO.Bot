using LOCO.Bot.Shared.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LOCO.Bot.Data.Configurations;

public class MemberGuessConfiguration : IEntityTypeConfiguration<MemberGuess>
{
    public void Configure(EntityTypeBuilder<MemberGuess> builder)
    {
        builder.UseXminAsConcurrencyToken()
               .HasKey(c => c.Id);
    }
}
