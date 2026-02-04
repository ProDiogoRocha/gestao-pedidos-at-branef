
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Orders.Models;
using Orders.Domain.Customers;
using Orders.Domain.Orders;
using Orders.Domain.Products;

namespace Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Email).HasMaxLength(200).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("Products");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("Orders");
            b.HasKey(x => x.Id);
            b.Property(x => x.CustomerId).IsRequired();
            b.Property(x => x.OrderDate).IsRequired();
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(x => x.Status).IsRequired();

            b.HasMany(x => x.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.ToTable("OrderItems");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            b.Property(x => x.Quantity).IsRequired();
            b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)").IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).HasMaxLength(250).IsRequired();
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.OccurredOnUtc).IsRequired();
            b.Property(x => x.RetryCount).IsRequired();
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var outboxMessages = new List<OutboxMessage>();

        var orderEntries = ChangeTracker.Entries<Order>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in orderEntries)
        {
            if (entry.State == EntityState.Deleted)
            {
                var payload = JsonSerializer.Serialize(new { Id = entry.Entity.Id });
                outboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredOnUtc = DateTime.UtcNow,
                    Type = "OrderDeleted",
                    Payload = payload,
                    RetryCount = 0
                });
                continue;
            }

            var o = entry.Entity;
            var snapshot = new OrderReadDto(
                o.Id,
                o.CustomerId,
                o.OrderDate,
                o.TotalAmount,
                (int)o.Status,
                o.Items.Select(i => new OrderItemReadDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()
            );
            outboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = entry.State == EntityState.Added ? "OrderCreated" : "OrderUpdated",
                Payload = JsonSerializer.Serialize(snapshot),
                RetryCount = 0
            });
        }

        if (outboxMessages.Count > 0)
            Outbox.AddRange(outboxMessages);

        return await base.SaveChangesAsync(cancellationToken);
    }
}
