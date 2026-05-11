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

        public DbSet<RetrainingRequest> RetrainingRequests { get; set; }

        public DbSet<AnalysisResult> AnalysisResults { get; set; }
        public DbSet<DetectionBox> DetectionBoxes { get; set; }

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

                entity.HasMany(mi => mi.AnalysisResults)
                      .WithOne(a => a.MedicalImage)
                      .HasForeignKey(a => a.MedicalImageId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(mi => mi.RetrainingRequests)
                      .WithOne(r => r.MedicalImage)
                      .HasForeignKey(r => r.MedicalImageId)
                      .OnDelete(DeleteBehavior.Restrict);
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

            builder.Entity<RetrainingRequest>(entity =>
            {
                // Вказуємо первинний ключ (хоча EF і так би здогадався)
                entity.HasKey(r => r.Id);

                // Зберігаємо статус як текст (наприклад "Pending"), а не як цифру (0)
                entity.Property(r => r.Status)
                      .IsRequired()
                      .HasConversion<string>()
                      .HasMaxLength(20);

                // Обмежуємо довжину коментаря (щоб не писали цілі поеми в БД)
                entity.Property(r => r.Comment)
                      .HasMaxLength(1000);

                // Явно вказуємо зв'язок з лікарем (User)
                entity.HasOne(r => r.RequestByUser)
                      .WithMany() // Якщо в класі User немає List<RetrainingRequest>, залишаємо порожнім
                      .HasForeignKey(r => r.RequestByUserId)
                      // Забороняємо видаляти лікаря, якщо в нього є створені запити (захист від помилок)
                      .OnDelete(DeleteBehavior.Restrict);

                // Явно вказуємо зв'язок з обстеженням (Examination)
                entity.HasOne(r => r.Examination)
                      .WithMany()
                      .HasForeignKey(r => r.ExaminationId)
                      // Якщо видалили обстеження - видаляємо і запит на його перевірку
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(r => r.RequestType)
                      .IsRequired()
                      .HasConversion<string>()
                      .HasMaxLength(30);
            });
            builder.Entity<AnalysisResult>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.ModelName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(a => a.ModelVersion)
                      .HasMaxLength(100);

                entity.Property(a => a.ModelPath)
                      .HasMaxLength(500);

                entity.Property(a => a.Status)
                      .IsRequired()
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(a => a.DoctorComment)
                      .HasMaxLength(1000);

                entity.HasOne(a => a.Examination)
                      .WithMany()
                      .HasForeignKey(a => a.ExaminationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<DetectionBox>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.ClassName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(d => d.Source)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasOne(d => d.AnalysisResult)
                      .WithMany(a => a.DetectionBoxes)
                      .HasForeignKey(d => d.AnalysisResultId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
