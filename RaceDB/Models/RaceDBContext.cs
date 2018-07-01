using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RaceDB.Models
{
    public partial class RaceDBContext : DbContext
    {
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<League> League { get; set; }
        public virtual DbSet<Match> Match { get; set; }

        public RaceDBContext(DbContextOptions<RaceDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.CategoryKey)
                    .IsRequired()
                    .HasColumnType("nchar(100)");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasColumnType("nchar(100)");
            });

            modelBuilder.Entity<League>(entity =>
            {
                entity.Property(e => e.LeagueId).HasColumnName("LeagueID");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.LeagueKey)
                    .IsRequired()
                    .HasColumnType("nchar(100)");

                entity.Property(e => e.LeagueName)
                    .IsRequired()
                    .HasColumnType("nchar(100)");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.League)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_League_Category");
            });

            modelBuilder.Entity<Match>(entity =>
            {
                entity.Property(e => e.MatchId).HasColumnName("MatchID");

                entity.Property(e => e.AwayCompetitorName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.HomeCompetitorName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.LeagueId).HasColumnName("LeagueID");

                entity.Property(e => e.MatchKey)
                    .IsRequired()
                    .HasColumnType("nchar(200)");

                entity.Property(e => e.SportId).HasColumnName("SportID");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Match)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Match_Category");

                entity.HasOne(d => d.League)
                    .WithMany(p => p.Match)
                    .HasForeignKey(d => d.LeagueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Match_League");
            });
        }
    }
}
