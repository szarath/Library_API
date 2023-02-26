using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Policy;

namespace Library_API.Models
{
    public partial class LibraryDBContext :DbContext
    {

        public LibraryDBContext(DbContextOptions options)
    : base(options)
        {
        }


        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=LibraryDBContext");
               
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.BookId)
                 .HasName("PK_book_id_2")
                 .IsClustered(false);
                entity.ToTable("Book");

                entity.Property(e => e.BookId).HasColumnName("book_id");

                entity.Property(e => e.PublishedDate)
                    .HasColumnName("published_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");


                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("('UNDECIDED')");

            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);

                entity.ToTable("RefreshToken");

                entity.Property(e => e.TokenId).HasColumnName("token_id");

                entity.Property(e => e.ExpiryDate)
                    .HasColumnName("expiry_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasColumnName("token")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.MemberId)
                    .HasConstraintName("FK__RefreshTo__member___60FC61CA");
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.ReservationId)
                .HasName("PK_reservation_id_2")
                .IsClustered(false);

                entity.ToTable("Reservation");

                entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

                entity.Property(e => e.BookId).IsRequired().HasColumnName("book_id");

                entity.Property(e => e.MemberId).IsRequired().HasColumnName("member_id");

                entity.Property(e => e.DateReserved).IsRequired()
                    .HasColumnName("date_reserved")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.MemberId)
                    .HasName("PK_member_id_2")
                    .IsClustered(false);

                entity.ToTable("Member");

                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email_address")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(20)
                    .IsUnicode(false);

               
                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);


                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                    entity.Property(e => e.DateOfBirth)
                .HasColumnName("DOB")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");


                    entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasColumnName("phone_number")
                .HasMaxLength(100)
                .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
    
}
