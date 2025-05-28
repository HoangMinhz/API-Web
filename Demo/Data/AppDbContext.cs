using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Demo.Models;

namespace Demo.Data
{
    /// <summary>
    /// Database context for the MyShop application, integrating ASP.NET Core Identity.
    /// </summary>
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure AppUser
            builder.Entity<AppUser>(entity =>
            {
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Province).HasMaxLength(10);
                entity.Property(e => e.District).HasMaxLength(10);
            });

            // Configure Order
            builder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.User)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(o => o.TotalAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(o => o.DiscountAmount)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(o => o.ShippingAddress)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(o => o.PhoneNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(o => o.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(o => o.Notes)
                    .HasMaxLength(500);

                entity.Property(o => o.VoucherCode)
                    .HasMaxLength(50);
            });

            // Configure OrderItem
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(oi => oi.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(oi => oi.TotalPrice)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure Cart
            builder.Entity<Cart>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Review
            builder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Voucher
            builder.Entity<Voucher>(entity =>
            {
                entity.Property(v => v.Code)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(v => v.Code)
                    .IsUnique();

                entity.Property(v => v.DiscountValue)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(v => v.MaxDiscount)
                    .HasColumnType("decimal(10,2)");

                entity.Property(v => v.MinOrderValue)
                    .HasColumnType("decimal(10,2)");
            });

            // Configure UserVoucher
            builder.Entity<UserVoucher>(entity =>
            {
                entity.HasOne(uv => uv.User)
                    .WithMany()
                    .HasForeignKey(uv => uv.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uv => uv.Voucher)
                    .WithMany()
                    .HasForeignKey(uv => uv.VoucherId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uv => uv.Order)
                    .WithMany()
                    .HasForeignKey(uv => uv.OrderId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Ensure a user can only use a voucher once
                entity.HasIndex(uv => new { uv.UserId, uv.VoucherId })
                    .IsUnique()
                    .HasFilter("[UsedAt] IS NOT NULL");
            });
        }
    }
}