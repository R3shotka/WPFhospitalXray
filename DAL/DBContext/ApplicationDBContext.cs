using DAL.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DAL.DBContext
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        // Конструктор для Dependency Injection
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        // Конструктор для міграцій (без параметрів)
        public ApplicationDBContext()
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<MedicalCard> MedicalCards { get; set; }        // ← Додати
        public DbSet<Examination> Examinations { get; set; }        // ← Додати  
        public DbSet<MedicalImage> MedicalImages { get; set; }      // ← Додати
        public DbSet<Conclusion> Conclusions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-EG334R7\SQLEXPRESS; Initial Catalog=HospitalXrayDb; Integrated Security=true; TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Для Identity

            // Patient конфігурація
            builder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.FullNamePatient)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Phone)
                      .HasMaxLength(20);

                entity.Property(p => p.Address)
                      .HasMaxLength(200);

                entity.Property(p => p.Status)
                      .HasMaxLength(50);

                // Enum для Sex
                entity.Property(p => p.Sex)
                      .IsRequired()
                      .HasConversion<string>() // Зберігаємо як string в БД
                      .HasMaxLength(1);
            });

            // MedicalCard конфігурація
            builder.Entity<MedicalCard>(entity =>
            {
                entity.HasKey(m => m.Id);

                // One-to-One з Patient
                entity.HasOne(m => m.Patient)
                      .WithOne(p => p.MedicalCard)
                      .HasForeignKey<MedicalCard>(m => m.PatientId)
                      .OnDelete(DeleteBehavior.Cascade); // При видаленні пацієнта - видаляється картка
            });

            // Examination конфігурація
            builder.Entity<Examination>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Зв'язок з MedicalCard
                entity.HasOne(e => e.MedicalCard)
                      .WithMany(m => m.Examinations)
                      .HasForeignKey(e => e.MedicalCardId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Conclusion>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.ConclusionText)
                      .IsRequired()
                      .HasMaxLength(2000); // Дамо лікарям місце для тексту

                // Зберігаємо Enum як текст
                entity.Property(c => c.Type)
                      .HasConversion<string>()
                      .IsRequired();

                // Зв'язок: один висновок належить одному обстеженню
                entity.HasOne(c => c.Examination)
                      .WithMany(e => e.Conclusions)
                      .HasForeignKey(c => c.ExaminationId)
                      .OnDelete(DeleteBehavior.Cascade); // Якщо видаляємо обстеження - видаляються і його висновки

                // Зв'язок: який лікар написав
                entity.HasOne(c => c.Doctor)
                      .WithMany()
                      .HasForeignKey(c => c.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict); // Знову захищаємо від помилки каскадного видалення юзерів
            });

            // MedicalImage конфігурація
            builder.Entity<MedicalImage>(entity =>
            {
                entity.HasKey(mi => mi.Id);

                entity.Property(mi => mi.FileName)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(mi => mi.FilePath)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(mi => mi.ContentType)
                      .HasMaxLength(100);

                // Зв'язок з Examination
                entity.HasOne(mi => mi.Examination)
                      .WithMany(e => e.Images)
                      .HasForeignKey(mi => mi.ExaminationId)
                      .OnDelete(DeleteBehavior.Cascade); // При видаленні обстеження - видаляються зображення
            });

            // ApplicationUser конфігурація (якщо потрібно)
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName)
                      .HasMaxLength(100);

                // Enum для Sex
                entity.Property(u => u.Sex)
                      .HasConversion<string>() // Зберігаємо як string в БД
                      .HasMaxLength(1);

                entity.Property(u => u.CreatingDate)
                      .IsRequired();
            });
        }
    }
}
