namespace EF_Prescription_Manager.DTO;

public class MedicamentGetDto
{
        public int IdMedicament { get; set; }
        public string Name { get; set; }
        public int Dose { get; set; }
        public string Description { get; set; }
}