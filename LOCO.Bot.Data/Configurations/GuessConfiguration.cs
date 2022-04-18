using LOCO.Bot.Shared.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LOCO.Bot.Data.Configurations;

public class GuessConfiguration : IEntityTypeConfiguration<Guess>
{
    public void Configure(EntityTypeBuilder<Guess> builder) => builder.UseXminAsConcurrencyToken()
               .HasKey(c => c.Id);
}
