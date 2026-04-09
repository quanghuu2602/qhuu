using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // MenuItem
        builder.Entity<MenuItem>(e => {
            e.Property(m => m.Price).HasColumnType("decimal(18,2)");
            e.Property(m => m.Name).HasMaxLength(200).IsRequired();
            e.HasOne(m => m.Category)
             .WithMany(c => c.MenuItems)
             .HasForeignKey(m => m.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderDetail
        builder.Entity<OrderDetail>(e => {
            e.Property(d => d.UnitPrice).HasColumnType("decimal(18,2)");
            e.HasOne(d => d.Order)
             .WithMany(o => o.Details)
             .HasForeignKey(d => d.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.MenuItem)
             .WithMany()
             .HasForeignKey(d => d.MenuItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment — 1 Order chỉ có 1 Payment
        builder.Entity<Payment>(e => {
            e.Property(p => p.Subtotal).HasColumnType("decimal(18,2)");
            e.Property(p => p.VatAmount).HasColumnType("decimal(18,2)");
            e.Property(p => p.ServiceFee).HasColumnType("decimal(18,2)");
            e.Property(p => p.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Order)
             .WithOne(o => o.Payment)
             .HasForeignKey<Payment>(p => p.OrderId);
        });

        // Order — StaffId là string
        builder.Entity<Order>(e => {
            e.HasOne(o => o.Staff)
             .WithMany()
             .HasForeignKey(o => o.StaffId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        // 1. Phải chèn Categories TRƯỚC (Cha)
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Khai vị", SortOrder = 1 },
            new Category { Id = 2, Name = "Món chính", SortOrder = 2 },
            new Category { Id = 3, Name = "Lẩu & Combo", SortOrder = 3 },
            new Category { Id = 4, Name = "Tráng miệng", SortOrder = 4 },
            new Category { Id = 5, Name = "Đồ uống", SortOrder = 5 }
        );
        // 2. MenuItems (Đa dạng món hơn)
        builder.Entity<MenuItem>().HasData(
     // --- KHAI VỊ (Category 1) ---
            new MenuItem { Id = 1, CategoryId = 1, Name = "Salad Ức Gà", Price = 65000, IsAvailable = true, TagsJson = "[\"Healthy\"]" },
            new MenuItem { Id = 2, CategoryId = 1, Name = "Súp Bào Ngư", Price = 120000, IsAvailable = true, TagsJson = "[\"Special\"]" },
            new MenuItem { Id = 11, CategoryId = 1, Name = "Khoai Tây Chiên Bơ Tỏi", Price = 45000, IsAvailable = true },
            new MenuItem { Id = 12, CategoryId = 1, Name = "Nộm Ngó Sen Tôm Thịt", Price = 85000, IsAvailable = true },

            // --- MÓN CHÍNH (Category 2) ---
            new MenuItem { Id = 3, CategoryId = 2, Name = "Bò Bít Tết Mỹ", Price = 250000, IsAvailable = true, TagsJson = "[\"Best Seller\"]" },
            new MenuItem { Id = 4, CategoryId = 2, Name = "Cá Hồi Áp Chảo", Price = 185000, IsAvailable = true },
            new MenuItem { Id = 5, CategoryId = 2, Name = "Sườn Nướng BBQ", Price = 210000, IsAvailable = true },
            new MenuItem { Id = 13, CategoryId = 2, Name = "Cơm Chiên Hải Sản", Price = 95000, IsAvailable = true },
            new MenuItem { Id = 14, CategoryId = 2, Name = "Mỳ Ý Sốt Bò Bằm", Price = 115000, IsAvailable = true },

            // --- LẨU & COMBO (Category 3) ---
            new MenuItem { Id = 6, CategoryId = 3, Name = "Lẩu Thái Hải Sản", Price = 450000, IsAvailable = true },
            new MenuItem { Id = 7, CategoryId = 3, Name = "Combo Gia Đình (4 người)", Price = 899000, IsAvailable = true },
            new MenuItem { Id = 15, CategoryId = 3, Name = "Lẩu Nấm Chim Câu", Price = 550000, IsAvailable = true },

            // --- TRÁNG MIỆNG (Category 4) - MỚI ---
            new MenuItem { Id = 16, CategoryId = 4, Name = "Chè Dưỡng Nhan", Price = 35000, IsAvailable = true },
            new MenuItem { Id = 17, CategoryId = 4, Name = "Bánh Tiramisu", Price = 55000, IsAvailable = true, TagsJson = "[\"Must Try\"]" },
            new MenuItem { Id = 18, CategoryId = 4, Name = "Kem Trái Dừa", Price = 45000, IsAvailable = true },
            new MenuItem { Id = 19, CategoryId = 4, Name = "Trái Cây Thập Cẩm", Price = 60000, IsAvailable = true },
            new MenuItem { Id = 20, CategoryId = 4, Name = "Bánh Pudding Xoài", Price = 40000, IsAvailable = true },

            // --- ĐỒ UỐNG (Category 5) ---
            new MenuItem { Id = 8, CategoryId = 5, Name = "Nước Ép Cam", Price = 35000, IsAvailable = true },
            new MenuItem { Id = 9, CategoryId = 5, Name = "Bia Heineken", Price = 25000, IsAvailable = true },
            new MenuItem { Id = 10, CategoryId = 5, Name = "Trà Đào Cam Sả", Price = 45000, IsAvailable = true },
            new MenuItem { Id = 21, CategoryId = 5, Name = "Cà Phê Muối", Price = 35000, IsAvailable = true },
            new MenuItem { Id = 22, CategoryId = 5, Name = "Soda Việt Quất", Price = 40000, IsAvailable = true }
        );

        // 3. Areas
        builder.Entity<Area>().HasData(
            new Area { Id = 1, Name = "Tầng trệt (Trong nhà)" },
            new Area { Id = 2, Name = "Tầng 2 (VIP)" },
            new Area { Id = 3, Name = "Sân thượng (View hồ)" }
        );

        // 4. Tables (Nhiều bàn hơn để test quản lý luồng khách)
        var tables = new List<Table>();

        // Add 10 bàn cho Tầng 1 (Id 1-10)
        for (int i = 1; i <= 10; i++)
            tables.Add(new Table { Id = i, Name = $"Bàn T1.{i:D2}", Capacity = (i % 3 == 0) ? 6 : 4, AreaId = 1 });

        // Add 5 bàn VIP cho Tầng 2 (Id 11-15)
        for (int i = 11; i <= 15; i++)
            tables.Add(new Table { Id = i, Name = $"Phòng VIP {i - 10}", Capacity = 8, AreaId = 2 });

        // Add 5 bàn Sân thượng (Id 16-20)
        for (int i = 16; i <= 20; i++)
            tables.Add(new Table { Id = i, Name = $"Bàn ST.{i - 15}", Capacity = 2, AreaId = 3 });

        builder.Entity<Table>().HasData(tables);
    }
}