
using BKMC.Pharmacy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Medicine> Medicines { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
   
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }

    public DbSet<PurchaseItem> PurchaseItems { get; set; }

    public DbSet<Batch> Batches { get; set; }
    public DbSet<StockTransfer> StockTransfers { get; set; }

    public DbSet<StockTransferItem> StockTransferItems { get; set; }

    public DbSet<StockTransaction> StockTransactions { get; set; }                              // add new table//
    public DbSet<StockAdjustment> StockAdjustments { get; set; }                                //add new table//

    public DbSet<Patient> Patients { get; set; }

    public DbSet<Dispensing> Dispensings { get; set; }

    public DbSet<DispensingItem> DispensingItems { get; set; }

    public DbSet<Bill> Bills { get; set; }                                              //add new table//
    public DbSet<BillItem> BillItems { get; set; }                                     //add new table//

    public DbSet<Return> Returns { get; set; }                                          //add new table//
    public DbSet<ReturnItem> ReturnItems { get; set; }                                  //add new table//

    public DbSet<AuditLog> AuditLogs { get; set; }                                       //add new table//
    public DbSet<AdminUser> AdminUsers { get; set; }

    public DbSet<BackupHistory> BackupHistories { get; set; }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Medicine>()
            .HasKey(m => m.Id);

        modelBuilder.Entity<Medicine>()           //this is new code//
    .Property(m => m.ReorderLevel)
    .HasPrecision(18, 2);

        modelBuilder.Entity<Supplier>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<PurchaseInvoice>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<PurchaseItem>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Batch>()
            .HasKey(b => b.Id);

        // Supplier → PurchaseInvoices
        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.PurchaseInvoices)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // PurchaseInvoice → PurchaseItems
        modelBuilder.Entity<PurchaseItem>()
            .HasOne(p => p.PurchaseInvoice)
            .WithMany(i => i.PurchaseItems)
            .HasForeignKey(p => p.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Medicine → PurchaseItems
        modelBuilder.Entity<PurchaseItem>()
            .HasOne(p => p.Medicine)
            .WithMany()
            .HasForeignKey(p => p.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        // Batch → PurchaseItems
        modelBuilder.Entity<PurchaseItem>()
            .HasOne(p => p.Batch)
            .WithMany(b => b.PurchaseItems)
            .HasForeignKey(p => p.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Medicine → Batches
        modelBuilder.Entity<Batch>()
            .HasOne(b => b.Medicine)
            .WithMany()
            .HasForeignKey(b => b.MedicineId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // Decimal precision to avoid silent truncation warnings
        modelBuilder.Entity<Batch>()
            .Property(b => b.Quantity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Stock>()
    .HasOne(x => x.Medicine)
    .WithMany()
    .HasForeignKey(x => x.MedicineId)
    .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<Stock>()
            .HasOne(x => x.Batch)
            .WithMany(x => x.Stocks)
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Stock>()
            .Property(s => s.Quantity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<StockTransfer>()
    .HasKey(x => x.Id);

        modelBuilder.Entity<StockTransferItem>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<StockTransferItem>()
            .HasOne(x => x.StockTransfer)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockTransferItem>()
            .HasOne(x => x.Medicine)
            .WithMany()
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransferItem>()
            .HasOne(x => x.Batch)
            .WithMany()
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransferItem>()
            .Property(x => x.Quantity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Patient>()
    .HasKey(x => x.Id);

        modelBuilder.Entity<Patient>()
            .HasIndex(x => x.MRNumber)
            .IsUnique();

        modelBuilder.Entity<Patient>()
            .Property(x => x.MRNumber)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Patient>()
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Dispensing>()
    .HasKey(x => x.Id);

        modelBuilder.Entity<DispensingItem>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Dispensing>()
            .HasOne(x => x.Patient)
            .WithMany(x => x.Dispensings)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DispensingItem>()
            .HasOne(x => x.Dispensing)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.DispensingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DispensingItem>()
            .HasOne(x => x.Medicine)
            .WithMany()
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DispensingItem>()
            .HasOne(x => x.Batch)
            .WithMany()
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DispensingItem>()
            .Property(x => x.Quantity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Return>()
    .HasKey(x => x.Id);

        modelBuilder.Entity<ReturnItem>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Return>()
            .HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Return>()
            .HasOne(x => x.Dispensing)
            .WithMany()
            .HasForeignKey(x => x.DispensingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReturnItem>()
            .HasOne(x => x.Return)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReturnItem>()
            .HasOne(x => x.DispensingItem)
            .WithMany()
            .HasForeignKey(x => x.DispensingItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReturnItem>()
            .HasOne(x => x.Medicine)
            .WithMany()
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReturnItem>()
            .HasOne(x => x.Batch)
            .WithMany()
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReturnItem>()
            .Property(x => x.Quantity)
            .HasPrecision(18, 2);
    }
}
