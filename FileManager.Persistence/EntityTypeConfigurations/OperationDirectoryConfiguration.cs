using FileManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Persistence.EntityTypeConfigurations;

/// <summary>
/// Конфигурация сущности OperationDirectory для базы данных
/// </summary>
public class OperationDirectoryConfiguration : IEntityTypeConfiguration<OperationDirectory>
{
    /// <summary>
    /// Настраивает сущность OperationDirectory для Entity Framework
    /// </summary>
    public void Configure(EntityTypeBuilder<OperationDirectory> builder)
    {
        builder.ToTable("directory_operations");

        builder.HasKey(operationDirectory => operationDirectory.Id);

        builder.Property(operationDirectory => operationDirectory.Id)
               .HasColumnName("id");

        builder.Property(operationDirectory => operationDirectory.ExecutedAt)
               .HasColumnName("executed_at")
               .IsRequired();

        builder.Property(operationDirectory => operationDirectory.OperationType)
               .HasColumnName("operation_type")
               .IsRequired();

        builder.Property(operationDirectory => operationDirectory.DirectoryId)
               .HasColumnName("directory_id")
               .IsRequired();

        builder.Property(operationDirectory => operationDirectory.UserId)
               .HasColumnName("user_id")
               .IsRequired();

        builder.HasOne(operationDirectory => operationDirectory.Directory)
               .WithMany(infoDirectory => infoDirectory.Operations)
               .HasForeignKey(operationDirectory => operationDirectory.DirectoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(operationDirectory => operationDirectory.User)
                .WithMany(user => user.OperationsDirectories)
                .HasForeignKey(operationDirectory => operationDirectory.UserId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}