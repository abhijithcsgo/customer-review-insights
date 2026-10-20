using Microsoft.EntityFrameworkCore;
using CustomerReviewInsights.Api.Models;

namespace CustomerReviewInsights.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CustomerReview> CustomerReviews => Set<CustomerReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var review = modelBuilder.Entity<CustomerReview>();

        review.ToTable("CustomerReviews");
        review.HasKey(r => r.Id);

        review.Property(r => r.ReviewText)
            .IsRequired();

        review.Property(r => r.Sentiment)
            .IsRequired()
            .HasMaxLength(20);

        review.Property(r => r.Category)
            .IsRequired()
            .HasMaxLength(50);

        review.Property(r => r.Summary)
            .IsRequired()
            .HasMaxLength(200);

        review.Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        review.HasIndex(r => r.CreatedAt);
    }
}
