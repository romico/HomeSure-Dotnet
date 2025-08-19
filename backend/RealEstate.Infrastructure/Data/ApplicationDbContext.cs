using Microsoft.EntityFrameworkCore;
using RealEstate.Core.Entities; // 엔티티 네임스페이스를 명시적으로 사용합니다.

namespace RealEstate.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // 엔티티 DbSet들
    public DbSet<User> Users { get; set; }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => (e.Entity is User || e.Entity is Property || e.Entity is Transaction) && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                switch (entry.Entity)
                {
                    case User user:
                        user.CreatedAt = now;
                        user.UpdatedAt = now;
                        break;
                    case Property property:
                        property.CreatedAt = now;
                        property.UpdatedAt = now;
                        break;
                    case Transaction transaction:
                        transaction.CreatedAt = now;
                        transaction.UpdatedAt = now;
                        break;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                switch (entry.Entity)
                {
                    case User user:
                        user.UpdatedAt = now;
                        break;
                    case Property property:
                        property.UpdatedAt = now;
                        break;
                    case Transaction transaction:
                        transaction.UpdatedAt = now;
                        break;
                }
                // CreatedAt은 수정 시 변경하지 않음
                entry.Property("CreatedAt").IsModified = false;
            }
        }

        // PropertyImage와 RefreshToken은 CreatedAt만 있음
        var createdOnlyEntries = ChangeTracker.Entries()
            .Where(e => (e.Entity is PropertyImage || e.Entity is RefreshToken) && 
                       e.State == EntityState.Added);

        foreach (var entry in createdOnlyEntries)
        {
            var now = DateTime.UtcNow;

            switch (entry.Entity) // 다른 네임스페이스의 모델은 전체 경로로 참조합니다.
            {
                case PropertyImage image:
                    image.CreatedAt = now;
                    break;
                case RefreshToken token:
                    token.CreatedAt = now;
                    break;
            }
        }
    }
    public DbSet<Property> Properties { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<PropertyImage> PropertyImages { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 엔티티 구성
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            // 인덱스 설정
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();

            // 속성 설정
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.FirstName)
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .HasMaxLength(100);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasDefaultValue("User")
                .HasMaxLength(50);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsEmailVerified)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .ValueGeneratedOnAddOrUpdate();

            entity.Property(e => e.WalletAddress)
                .HasMaxLength(42); // Ethereum 주소 길이
        });

        // 엔티티 구성들
        ConfigurePropertyEntity(modelBuilder);
        ConfigureTransactionEntity(modelBuilder);
        ConfigurePropertyImageEntity(modelBuilder);
        ConfigureRefreshTokenEntity(modelBuilder);

        // 향후 시드 데이터가 필요한 경우 여기에 추가
    }

    // 엔티티 구성 메소드들
    private void ConfigurePropertyEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id);

            // 속성 설정
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.Address)
                .HasMaxLength(500);

            entity.Property(e => e.PropertyType)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("House");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Available");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsFeatured)
                .HasDefaultValue(false);

            entity.Property(e => e.IsTokenized)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .ValueGeneratedOnAddOrUpdate();

            // 관계 설정
            entity.HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 인덱스 설정
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PropertyType);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.Price);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsFeatured);
        });
    }

    private void ConfigureTransactionEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            // 속성 설정
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.TransactionType)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Sale");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.Property(e => e.IsOnChain)
                .HasDefaultValue(false);

            entity.Property(e => e.Notes)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .ValueGeneratedOnAddOrUpdate();

            // 관계 설정
            entity.HasOne(t => t.Property)
                .WithMany(p => p.Transactions)
                .HasForeignKey(t => t.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Buyer)
                .WithMany()
                .HasForeignKey(t => t.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Seller)
                .WithMany()
                .HasForeignKey(t => t.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 인덱스 설정
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.TransactionType);
            entity.HasIndex(e => e.BlockchainTxHash);
            entity.HasIndex(e => e.IsOnChain);
        });
    }

    private void ConfigurePropertyImageEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasKey(e => e.Id);

            // 속성 설정
            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.AltText)
                .HasMaxLength(200);

            entity.Property(e => e.Caption)
                .HasMaxLength(500);

            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0);

            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.ContentType)
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .ValueGeneratedOnAdd();

            // 관계 설정
            entity.HasOne(pi => pi.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // 인덱스 설정
            entity.HasIndex(e => new { e.PropertyId, e.DisplayOrder });
            entity.HasIndex(e => e.IsPrimary);
            entity.HasIndex(e => e.IsActive);
        });
    }

    private void ConfigureRefreshTokenEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            // 속성 설정
            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Expires)
                .IsRequired();

            entity.Property(e => e.IsRevoked)
                .HasDefaultValue(false);

            entity.Property(e => e.RevokedBy)
                .HasMaxLength(200);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .ValueGeneratedOnAdd();

            // 관계 설정
            entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 인덱스 설정
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Expires);
            entity.HasIndex(e => e.IsRevoked);
        });
    }
}
