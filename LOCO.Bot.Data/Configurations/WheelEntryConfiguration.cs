using LOCO.Bot.Shared.Data.Entities;
using LOCO.Bot.Shared.Data.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.Drawing;

namespace LOCO.Bot.Data.Configurations;

public class WheelEntryConfiguration : IEntityTypeConfiguration<WheelEntry>
{
    public void Configure(EntityTypeBuilder<WheelEntry> builder)
    {
        builder.UseXminAsConcurrencyToken()
               .HasKey(c => c.Id);

        builder.Ignore(c => c.Guid);

        builder.HasData(new WheelEntry { Id = 1, Text = "1k RLB", Color = Color.DarkRed.ToHex(), Qty = 5 },
            new WheelEntry { Id = 2, Text = "5K RLB", Color = Color.OrangeRed.ToHex(), Qty = 3 },
            new WheelEntry { Id = 3, Text = "20$ bonus buy", Color = Color.Green.ToHex(), Qty = 2 },
            new WheelEntry { Id = 4, Text = "40$ bonus buy", Color = Color.GreenYellow.ToHex(), Qty = 1 },
            new WheelEntry { Id = 5, Text = "100$ bonus buy", Color = Color.Gold.ToHex(), Qty = 1 });
    }
}
