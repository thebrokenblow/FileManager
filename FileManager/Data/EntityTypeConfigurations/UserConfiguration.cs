using FileManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Data.EntityTypeConfigurations;

/// <summary>
/// Конфигурация сущности User для базы данных
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Настраивает сущность User для Entity Framework
    /// </summary>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
               .HasColumnName("id");

        builder.Property(user => user.Username)
               .HasColumnName("username")
               .IsRequired();

        builder.Property(user => user.PasswordHash)
               .HasColumnName("password_hash")
               .IsRequired();

        builder.HasIndex(user => user.Username)
               .IsUnique();

        builder.HasAlternateKey(user => user.Username);

        builder.HasMany(user => user.Files)
               .WithOne(infoFile => infoFile.User)
               .HasForeignKey(infoFile => infoFile.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Directories)
               .WithOne(infoDirectory => infoDirectory.User)
               .HasForeignKey(infoDirectory => infoDirectory.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.OperationsFiles)
               .WithOne(operationFile => operationFile.User)
               .HasForeignKey(operationFile => operationFile.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.OperationsDirectories)
               .WithOne(operationDirectory => operationDirectory.User)
               .HasForeignKey(operationDirectory => operationDirectory.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}