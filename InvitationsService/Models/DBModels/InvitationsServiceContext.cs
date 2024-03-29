﻿using Microsoft.EntityFrameworkCore;

namespace InvitationsService.Models.DBModels
{
    public partial class InvitationsServiceContext : DbContext
    {
        public InvitationsServiceContext()
        {
        }

        public InvitationsServiceContext(DbContextOptions<InvitationsServiceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Invitations> Invitations { get; set; }
        public virtual DbSet<EmailInvitations> EmailInvitations { get; set; }
        public virtual DbSet<RegistrationForms> RegistrationForms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invitations>(entity =>
            {
                entity.HasKey(e => e.InvitationId).HasName("PRIMARY");

                entity.ToTable("invitations");

                entity.Property(e => e.InvitationId).HasColumnName("invitation_id");

                entity.Property(e => e.RecipientName)
                    .HasColumnName("recipient_name")
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.PrivilageId).HasColumnName("privilage_id");

                entity.Property(e => e.Method)
                    .HasColumnName("method")
                    .HasColumnType("enum('email','phone_number')")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OfficerId).HasColumnName("officer_id");

                entity.Property(e => e.InstitutionId).HasColumnName("institution_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");
            });

            modelBuilder.Entity<EmailInvitations>(entity =>
            {
                entity.HasKey(e => e.EmailInvitationId).HasName("PRIMARY");

                entity.ToTable("email_invitations");

                entity.Property(e => e.EmailInvitationId).HasColumnName("email_invitation_id");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasColumnType("varchar(40)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.InvitationId).HasColumnName("invitation_id");

                entity.HasOne(d => d.Invitation)
                    .WithOne(p => p.EmailInvitation)
                    .HasForeignKey<EmailInvitations>(d => d.InvitationId)
                    .HasConstraintName("invitations_email_invitations_ibfk_1");
            });

            modelBuilder.Entity<RegistrationForms>(entity =>
            {
                entity.HasKey(e => e.RegistrationFormId).HasName("PRIMARY");

                entity.ToTable("registration_forms");

                entity.Property(e => e.RegistrationFormId).HasColumnName("registration_form_id");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
