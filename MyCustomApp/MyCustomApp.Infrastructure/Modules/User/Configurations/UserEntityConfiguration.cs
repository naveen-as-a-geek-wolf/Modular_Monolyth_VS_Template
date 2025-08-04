using LanguageExt.Pipes;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MyCustomApp.Infrastructure.Modules.User.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<Domain.Modules.User.User>
    {
        public void Configure(EntityTypeBuilder<Domain.Modules.User.User> builder)
        {
            builder.ToTable("User");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("UserId")
                .ValueGeneratedOnAdd();

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

        }
    }
}
