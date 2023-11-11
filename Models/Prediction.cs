using System.ComponentModel.DataAnnotations;

namespace Lab5.Models
{
    public enum Question
    {
        Earth, Computer
    }
    public class Prediction
    {
        [Key]
        public int PredictionId { get; set; }

        [MinLength(20)]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [MinLength(20)]
        [Display(Name = "Url")]
        public string Url { get; set; }

        [Required]
        public Question Question { get; set; }
    }
}
