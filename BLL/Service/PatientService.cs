using BLL.DTOs.Patients;
using BLL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Interfaces;
using DAL.Entity;
using DAL.Repositories;


namespace BLL.Service
{
    public class PatientService : IPatientService
    {
        private readonly IPatient _patientRepository;
        public PatientService(IPatient patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<List<PatientsListDto>> GetAllPatientsAsync()
        {
            // Оскільки в репозиторії метод GetAll() синхронний, 
            // ми обгортаємо його в Task.Run, щоб вікно WPF не "зависало" під час загрузки
            var patients = await _patientRepository.GetAllAsync();

            // Перетворюємо список Patient на список PatientListDto
            var dtoList = patients.Select(p => new PatientsListDto
            {
                Id = p.Id,
                FullName = p.FullNamePatient,
                Status = p.Status,

                // Форматуємо дати, щоб вони виглядали гарно (напр. "24.10.2023")
                CreatingDate = p.CreatingDate.ToString("dd.MM.yyyy"),
                DateOfBirth = p.DateOfBirth.ToString("dd.MM.yyyy"),

                // Перекладаємо стать через наш метод
                Sex = p.Sex.ToString(),

                // МАГІЯ ПІДРАХУНКУ ОБСТЕЖЕНЬ:
                // Знаки питання (?.) захищають нас від помилки, якщо медкартки ще немає
                ExaminationsCount = p.MedicalCard?.Examinations?.Count ?? 0
            }).ToList();

            return dtoList;
        }
        public async Task<EditPatientDto> GetPatientByIdAsync(string id)
        {
            var p = await _patientRepository.GetByIdAsync(id);

            if (p == null)
            {
                throw new Exception("Пацієнта не знайдено!");
            }

            // Перекладаємо з бази в DTO для вікна редагування
            return new EditPatientDto
            {
                Id = p.Id,
                FullName = p.FullNamePatient,
                Phone = p.Phone,
                Address = p.Address,
                DateOfBirth = p.DateOfBirth,
                // Ідеальна зворотна операція для твого Enum.Parse!
                Sex = p.Sex.ToString()
            };
        }

        public async Task CreatePatientAsync(CreatePatientDto dto)
        {
            var existingPatient = await _patientRepository.GetByIdAsync(dto.Id);
            if (existingPatient != null)
            {
                throw new Exception($"Пацієнт з номером паспорта {dto.Id} вже існує!");
            }

            var newPatient = new Patient
            {
                Id = dto.Id,
                FullNamePatient = dto.FullName,
                DateOfBirth = dto.DateOfBirth.Value,
                Phone = dto.Phone,
                Address = dto.Address,
                Status = "Зареєстровано",
                CreatingDate = DateTime.Now,

                // Твій улюблений підхід з Enum.Parse!
                Sex = (ApplicationUser.TypeGender)Enum.Parse(typeof(ApplicationUser.TypeGender), dto.Sex)
            };

            await  _patientRepository.AddAsync(newPatient);
        }
        public async Task UpdatePatientAsync(EditPatientDto dto)
        {
            var patientToUpdate = await _patientRepository.GetByIdAsync(dto.Id);

            if (patientToUpdate == null)
            {
                throw new Exception("Пацієнта не знайдено в базі!");
            }

            patientToUpdate.FullNamePatient = dto.FullName;
            patientToUpdate.DateOfBirth = dto.DateOfBirth.Value;
            patientToUpdate.Phone = dto.Phone;
            patientToUpdate.Address = dto.Address;
            

            // Знову ідеально чистий парсинг
            patientToUpdate.Sex = (ApplicationUser.TypeGender)Enum.Parse(typeof(ApplicationUser.TypeGender), dto.Sex);

            await _patientRepository.UpdateAsync(patientToUpdate);
        }

        public async Task DeletePatientAsync(string id)
        {
            await _patientRepository.DeleteAsync(id);
        }
    }
}
