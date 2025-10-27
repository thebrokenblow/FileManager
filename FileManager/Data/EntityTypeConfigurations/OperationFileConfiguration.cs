using FileManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Data.EntityTypeConfigurations;

/// <summary>
/// Конфигурация сущности OperationFile для базы данных
/// </summary>
public class OperationFileConfiguration : IEntityTypeConfiguration<OperationFile>
{
    /// <summary>
    /// Настраивает сущность OperationFile для Entity Framework
    /// </summary>
    public void Configure(EntityTypeBuilder<OperationFile> builder)
    {
        builder.ToTable("file_operations");

        builder.HasKey(operationFile => operationFile.Id);

        builder.Property(operationFile => operationFile.Id)
               .HasColumnName("id");

        builder.Property(operationFile => operationFile.Timestamp)
               .HasColumnName("timestamp")
               .IsRequired();

        builder.Property(operationFile => operationFile.OperationType)
               .HasColumnName("operation_type")
               .IsRequired();

        builder.Property(operationFile => operationFile.FileId)
               .HasColumnName("file_id")
               .IsRequired();

        builder.Property(operationFile => operationFile.UserId)
               .HasColumnName("user_id")
               .IsRequired();

        builder.HasOne(operationFile => operationFile.File)
               .WithMany(infoFile => infoFile.Operations)
               .HasForeignKey(operationFile => operationFile.FileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(operationFile => operationFile.User)
               .WithMany(user => user.OperationsFiles)
               .HasForeignKey(operationFile => operationFile.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}