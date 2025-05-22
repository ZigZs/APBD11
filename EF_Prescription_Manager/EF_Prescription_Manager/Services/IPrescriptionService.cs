using EF_Prescription_Manager.DTO;

namespace EF_Prescription_Manager.Services;

public interface IPrescriptionService
{
    Task AddPrescription(PerscriptionRequestDto prescriptionRequestDto);
    Task<PatientGetDto> GetPatient(int id);
}