using EF_Prescription_Manager.Data;
using EF_Prescription_Manager.DTO;
using EF_Prescription_Manager.Exceptions;
using EF_Prescription_Manager.Models;
using Microsoft.EntityFrameworkCore;

namespace EF_Prescription_Manager.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly DatabaseContext _context;
    private const int MaxMedicamentsPerPrescription = 10;

    public PrescriptionService(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddPrescription(PerscriptionRequestDto dto)
    {
        ValidatePrescriptionDates(dto);
        ValidateMedicamentsCount(dto);

        var patient = await FindOrCreatePatientAsync(dto.Patient);
        var doctor = await GetDoctorAsync(dto.IdDoctor);
        var medicaments = await GetValidatedMedicamentsAsync(dto.Medicaments);

        var prescription = CreatePrescription(dto, patient, medicaments);
        
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();
    }

    public async Task<PatientGetDto> GetPatient(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.Prescriptions)
                .ThenInclude(p => p.Doctor)
            .Include(p => p.Prescriptions)
                .ThenInclude(p => p.PrescriptionMedicaments)
                    .ThenInclude(pm => pm.Medicament)
            .FirstOrDefaultAsync(p => p.IdPatient == id);

        if (patient == null)
        {
            throw new NotFoundException($"Patient with ID {id} not found");
        }

        return MapPatientToDto(patient);
    }

    private void ValidatePrescriptionDates(PerscriptionRequestDto dto)
    {
        if (dto.DueDate < dto.Date)
        {
            throw new ConflictException("Due date cannot be earlier than prescription date");
        }
    }

    private void ValidateMedicamentsCount(PerscriptionRequestDto dto)
    {
        if (dto.Medicaments.Count > MaxMedicamentsPerPrescription)
        {
            throw new ConflictException($"Maximum {MaxMedicamentsPerPrescription} medicaments allowed per prescription");
        }
    }

    private async Task<Patient> FindOrCreatePatientAsync(PatientDto patientDto)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p =>
                p.FirstName == patientDto.FirstName &&
                p.LastName == patientDto.LastName &&
                p.Birthdate == patientDto.Birthdate);

        if (patient == null)
        {
            patient = new Patient
            {
                FirstName = patientDto.FirstName,
                LastName = patientDto.LastName,
                Birthdate = patientDto.Birthdate
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
        }

        return patient;
    }

    private async Task<Doctor> GetDoctorAsync(int doctorId)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);
        return doctor ?? throw new NotFoundException($"Doctor with ID {doctorId} not found");
    }

    private async Task<List<Medicament>> GetValidatedMedicamentsAsync(List<MedicamentDto> medicamentDtos)
    {
        var medicamentIds = medicamentDtos.Select(m => m.IdMedicament).ToList();
        var medicaments = await _context.Medicaments
            .Where(m => medicamentIds.Contains(m.IdMedicament))
            .ToListAsync();

        if (medicaments.Count != medicamentDtos.Count)
        {
            var missingIds = medicamentIds.Except(medicaments.Select(m => m.IdMedicament));
            throw new NotFoundException($"Medicaments not found: {string.Join(", ", missingIds)}");
        }

        return medicaments;
    }

    private static Prescription CreatePrescription(
        PerscriptionRequestDto dto,
        Patient patient,
        List<Medicament> medicaments)
    {
        return new Prescription
        {
            Date = dto.Date,
            DueDate = dto.DueDate,
            IdDoctor = dto.IdDoctor,
            IdPatient = patient.IdPatient,
            PrescriptionMedicaments = dto.Medicaments.Select(m => new PrescriptionMedicament
            {
                IdMedicament = m.IdMedicament,
                Dose = m.Dose,
                Details = m.Details
            }).ToList()
        };
    }

    private static PatientGetDto MapPatientToDto(Patient patient)
    {
        return new PatientGetDto
        {
            IdPatient = patient.IdPatient,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Birthdate = patient.Birthdate,
            Prescriptions = patient.Prescriptions
                .OrderBy(p => p.DueDate)
                .Select(p => new PrescriptionDto
                {
                    IdPrescription = p.IdPrescription,
                    Date = p.Date,
                    DueDate = p.DueDate,
                    Doctor = new DoctorDto
                    {
                        IdDoctor = p.Doctor.IdDoctor,
                        FirstName = p.Doctor.FirstName,
                    },
                    Medicaments = p.PrescriptionMedicaments
                        .Select(pm => new MedicamentGetDto
                        {
                            IdMedicament = pm.IdMedicament,
                            Name = pm.Medicament.Name,
                            Description = pm.Medicament.Description,
                            Dose = pm.Dose,
                        }).ToList()
                }).ToList()
        };
    }
}