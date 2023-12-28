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

            XmlSerializer serializer = new XmlSerializer(typeof(ExportPatientsDTO[]), new XmlRootAttribute("Patients"));

            StringBuilder sb = new StringBuilder();

            using var writer = new StringWriter(sb);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                //OmitXmlDeclaration = true
            };
            using var xmlwriter = XmlWriter.Create(writer, settings);

            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);

            DateTime givenDate;
            DateTime.TryParse(date, out givenDate);

            var patientsAndMedicines = context.Patients
                .Where(p => p.PatientsMedicines.Any(pm => pm.Medicine.ProductionDate >=
                givenDate))
                .Select(p => new ExportPatientsDTO
                {
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


            serializer.Serialize(xmlwriter, patientsAndMedicines, xns);
            writer.Close();

            return sb.ToString();


        }

        public static string ExportMedicinesFromDesiredCategoryInNonStopPharmacies(MedicinesContext context, int medicineCategory)
        {
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

            return JsonConvert.SerializeObject(medicinesAndPharmacies, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
