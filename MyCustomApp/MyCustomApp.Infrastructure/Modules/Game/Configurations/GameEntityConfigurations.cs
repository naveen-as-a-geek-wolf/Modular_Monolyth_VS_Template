using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace MyCustomApp.Infrastructure.Modules.Game.Configurations
{
    internal class GameEntityConfigurations: IEntityTypeConfiguration<Domain.Modules.Game.Game>
    {
        public void Configure(EntityTypeBuilder<Domain.Modules.Game.Game> builder)
        {
            builder.ToTable("Game");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
               .ValueGeneratedOnAdd();
        }
    }
}
