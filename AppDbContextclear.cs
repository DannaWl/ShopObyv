using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShopObyv.Models;

public partial class AppDbContextclear : DbContext
{
    public AppDbContextclear()
    {
    }

    public AppDbContextclear(DbContextOptions<AppDbContextclear> options)
        : base(options)
    {
    }

    public virtual DbSet<AdressPvz> AdressPvzs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Manufactre> Manufactres { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SostavZakaza> SostavZakazas { get; set; }

    public virtual DbSet<Staty> Statys { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Tovar> Tovars { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Zakaz> Zakazs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DN;Database=ShopShoes;User Id=sa;Password=1234;TrustServerCertificate=true;");
                                     // надо добавить потом в шараге \\SQLEXPRESS
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdressPvz>(entity =>
        {
            entity.HasKey(e => e.IdPvz);

            entity.ToTable("adressPVz");

            entity.Property(e => e.IdPvz).HasColumnName("idPVZ");
            entity.Property(e => e.Dom).HasColumnName("dom");
            entity.Property(e => e.Gorod)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("gorod");
            entity.Property(e => e.IndexPvz).HasColumnName("indexPvz");
            entity.Property(e => e.Street)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("street");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategory);

            entity.ToTable("category");

            entity.Property(e => e.IdCategory)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("idCategory");
        });

        modelBuilder.Entity<Manufactre>(entity =>
        {
            entity.HasKey(e => e.IdManufacture);

            entity.ToTable("manufactre");

            entity.Property(e => e.IdManufacture)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("idManufacture");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole);

            entity.ToTable("role");

            entity.Property(e => e.IdRole)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("idRole");
        });

        modelBuilder.Entity<SostavZakaza>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("sostavZakaza");

            entity.Property(e => e.ArticleZakaa)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("articleZakaa");
            entity.Property(e => e.IdSostavaZakaza).HasColumnName("idSostavaZakaza");
            entity.Property(e => e.KolvoVzakaze).HasColumnName("kolvoVzakaze");

            entity.HasOne(d => d.ArticleZakaaNavigation).WithMany()
                .HasForeignKey(d => d.ArticleZakaa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_sostavZakaza_tovar");

            entity.HasOne(d => d.IdSostavaZakazaNavigation).WithMany()
                .HasForeignKey(d => d.IdSostavaZakaza)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_sostavZakaza_zakaz");
        });

        modelBuilder.Entity<Staty>(entity =>
        {
            entity.HasKey(e => e.IdStatys);

            entity.ToTable("statys");

            entity.Property(e => e.IdStatys)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("idStatys");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.IdSupplier);

            entity.ToTable("supplier");

            entity.Property(e => e.IdSupplier)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("idSupplier");
        });

        modelBuilder.Entity<Tovar>(entity =>
        {
            entity.HasKey(e => e.Article);

            entity.ToTable("tovar");

            entity.Property(e => e.Article)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("article");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.Description)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("image");
            entity.Property(e => e.Manufacture)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("manufacture");
            entity.Property(e => e.NameTovar)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("nameTovar");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("price");
            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("supplier");
            entity.Property(e => e.Unit)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("unit");
            entity.Property(e => e.UnitStorage).HasColumnName("unitStorage");

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.Tovars)
                .HasForeignKey(d => d.Category)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tovar_category1");

            entity.HasOne(d => d.ManufactureNavigation).WithMany(p => p.Tovars)
                .HasForeignKey(d => d.Manufacture)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tovar_manufactre");

            entity.HasOne(d => d.SupplierNavigation).WithMany(p => p.Tovars)
                .HasForeignKey(d => d.Supplier)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tovar_supplier");

            entity.HasOne(d => d.UnitNavigation).WithMany(p => p.Tovars)
                .HasForeignKey(d => d.Unit)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tovar_unit");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.IdUnit);

            entity.ToTable("unit");

            entity.Property(e => e.IdUnit)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("idUnit");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Login);

            entity.ToTable("users");

            entity.Property(e => e.Login)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("login");
            entity.Property(e => e.Fcs)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("FCs");
            entity.Property(e => e.Password)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("role");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Role)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_users_role");
        });

        modelBuilder.Entity<Zakaz>(entity =>
        {
            entity.HasKey(e => e.NomZakaza);

            entity.ToTable("zakaz");

            entity.Property(e => e.NomZakaza).HasColumnName("nomZakaza");
            entity.Property(e => e.ArticleZakaa).HasColumnName("articleZakaa");
            entity.Property(e => e.DarteDilivery)
                .HasColumnType("datetime")
                .HasColumnName("darteDilivery");
            entity.Property(e => e.DateOrder)
                .HasColumnType("datetime")
                .HasColumnName("dateOrder");
            entity.Property(e => e.LoginUsers)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("loginUsers");
            entity.Property(e => e.Pvz).HasColumnName("PVZ");
            entity.Property(e => e.Qr).HasColumnName("QR");
            entity.Property(e => e.Statys)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("statys");

            entity.HasOne(d => d.LoginUsersNavigation).WithMany(p => p.Zakazs)
                .HasForeignKey(d => d.LoginUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_zakaz_users");

            entity.HasOne(d => d.PvzNavigation).WithMany(p => p.Zakazs)
                .HasForeignKey(d => d.Pvz)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_zakaz_adressPVz");

            entity.HasOne(d => d.StatysNavigation).WithMany(p => p.Zakazs)
                .HasForeignKey(d => d.Statys)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_zakaz_statys");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
