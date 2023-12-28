
using Medicines.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Medicines.DataProcessor.ImportDtos
{
    [XmlType("Pharmacy")]
    public class ImportPharmaciesDTO
    {
        [Required]
        [XmlAttribute("non-stop")]
        [RegularExpression(@"(true|false)\b")]
        public string IsNonStop { get; set; }

        [Required]
        [MaxLength(50)]
        [MinLength(2)]
        [XmlElement("Name")]
        public string Name { get; set; }

        [Required]
        [MaxLength(14)]
        [MinLength(14)]
        [XmlElement("PhoneNumber")]
        [RegularExpression(@"\(\d{3}\) \d{3}-\d{4}\b")]
        public string PhoneNumber { get; set; }

        [XmlArray("Medicines")]
        public ImportPharmaciesMedicinesDTO[] Medicines { get; set; }
    }
}
