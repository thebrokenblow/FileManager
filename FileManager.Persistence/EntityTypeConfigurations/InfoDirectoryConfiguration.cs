using FileManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Persistence.EntityTypeConfigurations;

/// <summary>
/// Конфигурация сущности InfoDirectory для базы данных
/// </summary>
public class InfoDirectoryConfiguration : IEntityTypeConfiguration<InfoDirectory>
{
    /// <summary>
    /// Настраивает сущность InfoDirectory для Entity Framework
    /// </summary>
    public void Configure(EntityTypeBuilder<InfoDirectory> builder)
    {
        builder.ToTable("directories");

        builder.HasKey(directory => directory.Id);

        builder.Property(directory => directory.Id)
               .HasColumnName("id");

        builder.Property(directory => directory.DirectoryName)
               .HasColumnName("directory_name")
               .IsRequired();

        builder.Property(directory => directory.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(directory => directory.Location)
               .HasColumnName("location")
               .IsRequired();

        builder.Property(directory => directory.UserId)
               .HasColumnName("user_id")
               .IsRequired();

        builder.HasOne(directory => directory.User)
               .WithMany(user => user.Directories)
               .HasForeignKey(d => d.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}