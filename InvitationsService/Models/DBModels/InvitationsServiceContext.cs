using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invitations>(entity =>
            {
                entity.HasKey(e => e.InvitationId).HasName("PRIMARY");

                entity.ToTable("invitations");

                entity.Property(e => e.InvitationId).HasColumnName("invitation_id");

                entity.Property(e => e.RecipientName)
                    .HasColumnName("recipient_name")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Data)
                    .HasColumnName("data")
                    .HasMaxLength(255);

                entity.Property(e => e.OfficerId).HasColumnName("officer_id");

                entity.Property(e => e.InstitutionId).HasColumnName("institution_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
