using LOCO.Bot.Shared.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LOCO.Bot.Data.Configurations;

public class RestartConfiguration : IEntityTypeConfiguration<Restart>
{
    public void Configure(EntityTypeBuilder<Restart> builder) => builder.UseXminAsConcurrencyToken()
               .HasKey(c => c.Id);
}
