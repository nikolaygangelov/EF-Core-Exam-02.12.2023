

using Medicines.Data.Models.Enums;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Medicines.DataProcessor.ExportDtos
{
    [XmlType("Patient")]
    public class ExportPatientsDTO
    {

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("AgeGroup")]
        public string AgeGroup { get; set; }

        [XmlAttribute("Gender")]
        public string Gender { get; set; }

        [XmlArray("Medicines")]
        public ExportPatientsWithMedicinesDTO[] Medicines { get; set; }
    }
}
