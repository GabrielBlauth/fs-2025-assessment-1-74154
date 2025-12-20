using System.ComponentModel.DataAnnotations;

namespace fs_2025_assessment_1_74154_App.Models
{
    public class StationEditModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number must be a positive integer.")]
        public int Number { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [RegularExpression("OPEN|CLOSED", ErrorMessage = "Status must be OPEN or CLOSED.")]
        public string Status { get; set; } = "OPEN";

        [Range(0, int.MaxValue)]
        public int BikeStands { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableBikeStands { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableBikes { get; set; }
    }
}
