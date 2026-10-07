using BKMC.Pharmacy.Application.DTOs.Patients;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _context;

    public PatientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
    {
        return await _context.Patients
            .AsNoTracking()
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (patient == null)
            return null;

        return MapToDto(patient);
    }

    public async Task<PatientDto?> GetByMRNumberAsync(
        string mrNumber)
    {
        if (string.IsNullOrWhiteSpace(mrNumber))
            return null;

        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.MRNumber == mrNumber);

        if (patient == null)
            return null;

        return MapToDto(patient);
    }

    public async Task<PatientDto> CreateAsync(
        CreatePatientDto dto)
    {
        // MR Number required
        if (string.IsNullOrWhiteSpace(dto.MRNumber))
        {
            throw new InvalidOperationException(
                "MR Number is required.");
        }

        // Patient name required
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException(
                "Patient name is required.");
        }

        // Age validation
        if (dto.Age < 0)
        {
            throw new InvalidOperationException(
                "Patient age cannot be negative.");
        }

        // Check duplicate MR Number
        var mrExists = await _context.Patients
            .AnyAsync(x =>
                x.MRNumber == dto.MRNumber);

        if (mrExists)
        {
            throw new InvalidOperationException(
                "MR Number already exists.");
        }

        var patient = new Patient
        {
            MRNumber = dto.MRNumber.Trim(),
            Name = dto.Name.Trim(),
            Age = dto.Age,
            Gender = dto.Gender,
            AdmissionNumber =
                string.IsNullOrWhiteSpace(dto.AdmissionNumber)
                    ? null
                    : dto.AdmissionNumber.Trim(),
            ReferralId =
                string.IsNullOrWhiteSpace(dto.ReferralId)
                    ? null
                    : dto.ReferralId.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Patients.Add(patient);

        await _context.SaveChangesAsync();

        return MapToDto(patient);
    }

    public async Task<PatientDto?> UpdateAsync(
        int id,
        UpdatePatientDto dto)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(x => x.Id == id);

        if (patient == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.MRNumber))
        {
            throw new InvalidOperationException(
                "MR Number is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException(
                "Patient name is required.");
        }

        if (dto.Age < 0)
        {
            throw new InvalidOperationException(
                "Patient age cannot be negative.");
        }

        // Check duplicate MR Number
        var duplicateMR = await _context.Patients
            .AnyAsync(x =>
                x.Id != id &&
                x.MRNumber == dto.MRNumber);

        if (duplicateMR)
        {
            throw new InvalidOperationException(
                "MR Number already belongs to another patient.");
        }

        patient.MRNumber = dto.MRNumber.Trim();
        patient.Name = dto.Name.Trim();
        patient.Age = dto.Age;
        patient.Gender = dto.Gender;

        patient.AdmissionNumber =
            string.IsNullOrWhiteSpace(dto.AdmissionNumber)
                ? null
                : dto.AdmissionNumber.Trim();

        patient.ReferralId =
            string.IsNullOrWhiteSpace(dto.ReferralId)
                ? null
                : dto.ReferralId.Trim();

        await _context.SaveChangesAsync();

        return MapToDto(patient);
    }

    private static PatientDto MapToDto(
        Patient patient)
    {
        return new PatientDto
        {
            Id = patient.Id,
            MRNumber = patient.MRNumber,
            Name = patient.Name,
            Age = patient.Age,
            Gender = patient.Gender,
            AdmissionNumber =
                patient.AdmissionNumber,
            ReferralId =
                patient.ReferralId,
            CreatedAt = patient.CreatedAt
        };
    }
}