namespace EF_Prescription_Manager.DTO;

public class PerscriptionRequestDto
{
    public PatientDto Patient { get; set; }
    public List<MedicamentDto> Medicaments { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }

    public int IdDoctor { get; set; }
}