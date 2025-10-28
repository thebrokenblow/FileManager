using FileManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Persistence.EntityTypeConfigurations;

/// <summary>
/// Конфигурация сущности InfoFile для базы данных
/// </summary>
public class InfoFileConfiguration : IEntityTypeConfiguration<InfoFile>
{
    /// <summary>
    /// Настраивает сущность InfoFile для Entity Framework
    /// </summary>
    public void Configure(EntityTypeBuilder<InfoFile> builder)
    {
        builder.ToTable("files");

        builder.HasKey(file => file.Id);

        builder.Property(file => file.Id)
               .HasColumnName("id");

        builder.Property(file => file.Filename)
               .HasColumnName("filename")
               .IsRequired();

        builder.Property(file => file.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(file => file.Size)
               .HasColumnName("size")
               .IsRequired(false);

        builder.Property(file => file.Location)
               .HasColumnName("location")
               .IsRequired(false);

        builder.Property(file => file.UserId)
               .HasColumnName("owner_id")
               .IsRequired();

        builder.HasOne(file => file.User)
               .WithMany(user => user.Files)
               .HasForeignKey(file => file.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(file => file.Operations)
               .WithOne(operationFile => operationFile.File)
               .HasForeignKey(operationFile => operationFile.FileId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}