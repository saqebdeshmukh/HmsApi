using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccess.DBEntities
{
    public class PatientDiseases
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? PatientId { get; set; }
        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }
        public int? GeneticDiseaseId { get; set; }
        [ForeignKey(nameof(GeneticDiseaseId))]
        public GeneticDisease GeneticDisease { get; set; }
        public string? Medicines { get; set; }
    }
}
