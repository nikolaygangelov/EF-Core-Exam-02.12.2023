namespace Medicines.DataProcessor
{
    using Medicines.Data;
    using Medicines.Data.Models;
    using Medicines.Data.Models.Enums;
    using Medicines.DataProcessor.ExportDtos;
    using Medicines.DataProcessor.ImportDtos;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPatientsWithTheirMedicines(MedicinesContext context, string date)
        {
            //using Data Transfer Object Class to map it with patients
            XmlSerializer serializer = new XmlSerializer(typeof(ExportPatientsDTO[]), new XmlRootAttribute("Patients"));

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //"using" automatically closes opened connections
            using var writer = new StringWriter(sb);

            //setting identation with tabulation
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                //OmitXmlDeclaration = true
            };
            using var xmlwriter = XmlWriter.Create(writer, settings);

            var xns = new XmlSerializerNamespaces();

            //one way to display empty namespace in resulted file
            xns.Add(string.Empty, string.Empty);

            //validating date
            DateTime givenDate;
            DateTime.TryParse(date, out givenDate);

            var patientsAndMedicines = context.Patients
                .Where(p => p.PatientsMedicines.Any(pm => pm.Medicine.ProductionDate >=
                givenDate))
                .Select(p => new ExportPatientsDTO
                {
                    //using identical properties in order to map successfully
                    Gender = p.Gender.ToString().ToLower(),
                    Name = p.FullName,
                    AgeGroup = p.AgeGroup.ToString(),
                    Medicines = p.PatientsMedicines
                    .Where(pm => pm.Medicine.ProductionDate >=
                    givenDate)
                    .OrderByDescending(m => m.Medicine.ExpiryDate)
                    .ThenBy(m => m.Medicine.Price)
                    .Select(pm => new ExportPatientsWithMedicinesDTO
                    {
                        Name = pm.Medicine.Name,
                        Category = pm.Medicine.Category.ToString().ToLower(),
                        Price = pm.Medicine.Price.ToString("f2"),
                        Producer = pm.Medicine.Producer,
                        BestBefore = pm.Medicine.ExpiryDate.ToString("yyyy-MM-dd")
                    })
                    .ToArray()
                })
                .OrderByDescending(p => p.Medicines.Length)
                .ThenBy(p => p.Name)
                .ToArray();


            //Serialize method needs file, TextReader object and namespace to convert/map
            serializer.Serialize(xmlwriter, patientsAndMedicines, xns);

            //explicitly closing connection in terms of reaching edge cases
            writer.Close();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();


        }

        public static string ExportMedicinesFromDesiredCategoryInNonStopPharmacies(MedicinesContext context, int medicineCategory)
        {
            //turning needed info about medicines into a collection using anonymous object
            //using less data
            var medicinesAndPharmacies = context.Medicines
                .Where(m => m.Category == (Category)medicineCategory &&
                m.Pharmacy.IsNonStop == true)
                .OrderBy(m => m.Price)
                .ThenBy(m => m.Name)
                .Select(m => new
                {
                    Name = m.Name,
                    Price = m.Price.ToString("f2"),
                    Pharmacy = new
                    {
                        Name = m.Pharmacy.Name,
                        PhoneNumber = m.Pharmacy.PhoneNumber
                    }
                })
                 .ToArray();

            //Serialize method needs object to convert/map
	        //adding Formatting for better reading 
            return JsonConvert.SerializeObject(medicinesAndPharmacies, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
