namespace EF_Prescription_Manager.DTO;

public class PrescriptionDto
{
    public int IdPrescription { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }

    public List<MedicamentGetDto> Medicaments { get; set; }

    public DoctorDto Doctor { get; set; }
}