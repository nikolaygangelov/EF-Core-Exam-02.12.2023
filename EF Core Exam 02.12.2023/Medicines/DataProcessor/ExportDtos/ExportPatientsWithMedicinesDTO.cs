using Medicines.Data.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Medicines.DataProcessor.ExportDtos
{
    [XmlType("Medicine")]
    public class ExportPatientsWithMedicinesDTO
    {

        [XmlAttribute("Category")]
        public string Category { get; set; }


        [XmlElement("Name")]
        public string Name { get; set; }


        [XmlElement("Price")]
        public string Price { get; set; } //using string format in order to map with needed export format

        [XmlElement("Producer")]
        public string Producer { get; set; }

        [XmlElement("BestBefore")]
        public string BestBefore { get; set; } //using string format in order to map with needed export format



    }
}
