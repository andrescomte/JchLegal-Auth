using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Models;
namespace AuthService.Infrastructure.Context;

public partial class AuthDbContext : DbContext
{
    public AuthDbContext()
    {
    }

    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Tenants> Tenants { get; set; }

    public virtual DbSet<AuditLogs> AuditLogs { get; set; }

    public virtual DbSet<LoginAttempts> LoginAttempts { get; set; }

    public virtual DbSet<Menus> Menus { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<Tokens> Tokens { get; set; }

    public virtual DbSet<UserPasswords> UserPasswords { get; set; }

    public virtual DbSet<UserRoles> UserRoles { get; set; }

    public virtual DbSet<UserStatus> UserStatus { get; set; }

    public virtual DbSet<UserStatusHistory> UserStatusHistory { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("citext")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Tenants>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tenants_pkey");
            entity.ToTable("tenants");
            entity.HasIndex(e => e.Code, "tenants_code_key").IsUnique();
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        });

        modelBuilder.Entity<AuditLogs>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audit_logs_pkey");

            entity.ToTable("audit_logs");

            entity.HasIndex(e => new { e.Action, e.LoggedAt }, "idx_audit_logs_action_time").IsDescending(false, true);

            entity.HasIndex(e => new { e.UserId, e.LoggedAt }, "idx_audit_logs_user_time").IsDescending(false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.LoggedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("logged_at");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("audit_logs_user_id_fkey");
        });

        modelBuilder.Entity<LoginAttempts>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("login_attempts_pkey");

            entity.ToTable("login_attempts");

            entity.HasIndex(e => new { e.Ip, e.AttemptedAt }, "idx_login_attempts_ip_time").IsDescending(false, true);

            entity.HasIndex(e => new { e.UserId, e.AttemptedAt }, "idx_login_attempts_user_time").IsDescending(false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("attempted_at");
            entity.Property(e => e.Email)
                .HasColumnType("citext")
                .HasColumnName("email");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.Succeeded).HasColumnName("succeeded");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.LoginAttempts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("login_attempts_user_id_fkey");
        });

        modelBuilder.Entity<Menus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("menus_pkey");

            entity.ToTable("menus");

            entity.HasIndex(e => new { e.ParentId, e.SortOrder }, "idx_menus_parent_sort");

            entity.HasIndex(e => new { e.ParentId, e.Label }, "menus_parent_id_label_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IconClass)
                .HasMaxLength(64)
                .HasColumnName("icon_class");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Label)
                .HasMaxLength(100)
                .HasColumnName("label");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Path)
                .HasMaxLength(200)
                .HasColumnName("path");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("menus_parent_id_fkey");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Code, "roles_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.HomeMenuId).HasColumnName("home_menu_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.HomeMenu).WithMany(p => p.Roles)
                .HasForeignKey(d => d.HomeMenuId)
                .HasConstraintName("roles_home_menu_id_fkey");

            entity.HasMany(d => d.Menu).WithMany(p => p.Role)
                .UsingEntity<Dictionary<string, object>>(
                    "RoleMenus",
                    r => r.HasOne<Menus>().WithMany()
                        .HasForeignKey("MenuId")
                        .HasConstraintName("role_menus_menu_id_fkey"),
                    l => l.HasOne<Roles>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("role_menus_role_id_fkey"),
                    j =>
                    {
                        j.HasKey("RoleId", "MenuId").HasName("role_menus_pkey");
                        j.ToTable("role_menus");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                        j.IndexerProperty<long>("MenuId").HasColumnName("menu_id");
                    });
        });

        modelBuilder.Entity<Tokens>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tokens_pkey");

            entity.ToTable("tokens");

            entity.HasIndex(e => new { e.UserId, e.ExpiresAt }, "idx_tokens_user_expires");

            entity.HasIndex(e => e.TokenHash, "tokens_token_hash_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("issued_at");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasColumnName("token_hash");
            entity.Property(e => e.TokenType)
                .HasMaxLength(32)
                .HasColumnName("token_type");
            entity.Property(e => e.Used)
                .HasDefaultValue(false)
                .HasColumnName("used");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("tokens_user_id_fkey");
        });

        modelBuilder.Entity<UserPasswords>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_passwords_pkey");

            entity.ToTable("user_passwords");

            entity.HasIndex(e => e.UserId, "user_passwords_user_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FailedAttempts)
                .HasDefaultValue(0)
                .HasColumnName("failed_attempts");
            entity.Property(e => e.PasswordHash).HasColumnType("character varying").HasColumnName("password_hash");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.UserPasswords)
                .HasForeignKey<UserPasswords>(d => d.UserId)
                .HasConstraintName("user_passwords_user_id_fkey");
        });

        modelBuilder.Entity<UserRoles>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("user_roles_pkey");

            entity.ToTable("user_roles");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("user_roles_role_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_roles_user_id_fkey");
        });

        modelBuilder.Entity<UserStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_status_pkey");

            entity.ToTable("user_status");

            entity.HasIndex(e => e.Code, "user_status_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasColumnName("name");
        });

        modelBuilder.Entity<UserStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_status_history_pkey");

            entity.ToTable("user_status_history");

            entity.HasIndex(e => new { e.UserId, e.StartedAt }, "user_status_history_user_id_started_at_idx").IsDescending(false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndedAt).HasColumnName("ended_at");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("started_at");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Status).WithMany(p => p.UserStatusHistory)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_status_history_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserStatusHistory)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_status_history_user_id_fkey");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => new { e.TenantId, e.Email }, "users_tenant_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasColumnType("citext")
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Username)
                .HasMaxLength(64)
                .HasColumnName("username");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Users)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("users_tenant_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
