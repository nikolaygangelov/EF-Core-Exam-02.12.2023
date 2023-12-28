using Medicines.Data.Models.Enums;
using Medicines.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Medicines.DataProcessor.ImportDtos
{
    public class ImportPatientDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        [MinLength(5)]
        [JsonProperty("FullName")]
        public string FullName { get; set; }

        [Required]
        [JsonProperty("AgeGroup")]
        [RegularExpression(@"(0|1|2)\b")]
        public string AgeGroup { get; set; }

        [Required]
        [JsonProperty("Gender")]
        [RegularExpression(@"(0|1|)\b")]
        public string Gender { get; set; }

        public int[] Medicines { get; set; }
    }
}
