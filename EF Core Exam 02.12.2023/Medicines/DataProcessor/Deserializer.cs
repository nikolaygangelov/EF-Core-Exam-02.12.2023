namespace Medicines.DataProcessor
{
    using Medicines.Data;
    using Medicines.Data.Models;
    using Medicines.Data.Models.Enums;
    using Medicines.DataProcessor.ImportDtos;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid Data!";
        private const string SuccessfullyImportedPharmacy = "Successfully imported pharmacy - {0} with {1} medicines.";
        private const string SuccessfullyImportedPatient = "Successfully imported patient - {0} with {1} medicines.";

        public static string ImportPatients(MedicinesContext context, string jsonString)
        {
            var patientssArray = JsonConvert.DeserializeObject<ImportPatientDTO[]>(jsonString);

            StringBuilder sb = new StringBuilder();
            List<Patient> patientList = new List<Patient>();           

            foreach (ImportPatientDTO patientDTO in patientssArray)
            {

                if (!IsValid(patientDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                Patient patientToAdd = new Patient()
                {
                    FullName = patientDTO.FullName,
                    AgeGroup = (AgeGroup)int.Parse(patientDTO.AgeGroup),
                    Gender = (Gender)int.Parse(patientDTO.Gender)
                };



                foreach (int medicineId in patientDTO.Medicines)
                {

                    if (patientToAdd.PatientsMedicines.Any(pm => pm.MedicineId == medicineId)) 
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    patientToAdd.PatientsMedicines.Add(new PatientMedicine()
                    {
                        //Patient = patientToAdd,
                        MedicineId = medicineId
                    });


                }

                patientList.Add(patientToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedPatient, patientToAdd.FullName, patientToAdd.PatientsMedicines.Count));
            }

            context.AddRange(patientList);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPharmacies(MedicinesContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportPharmaciesDTO[]), new XmlRootAttribute("Pharmacies"));
            using StringReader inputReader = new StringReader(xmlString);
            var pharmaciesArrayDTOs = (ImportPharmaciesDTO[])serializer.Deserialize(inputReader);

            StringBuilder sb = new StringBuilder();
            List<Pharmacy> pharmaciesXML = new List<Pharmacy>();

            foreach (ImportPharmaciesDTO pharmacyDTO in pharmaciesArrayDTOs)
            {

                if (!IsValid(pharmacyDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Pharmacy pharmacyToAdd = new Pharmacy
                {
                    IsNonStop = bool.Parse(pharmacyDTO.IsNonStop),
                    Name = pharmacyDTO.Name,
                    PhoneNumber = pharmacyDTO.PhoneNumber
                };

                foreach (var medicine in pharmacyDTO.Medicines)
                {
                    if (!IsValid(medicine))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    // проверка за валидност на датите ?

                    if (DateTime.ParseExact(medicine.ProductionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) >=
                        DateTime.ParseExact(medicine.ExpiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (pharmacyToAdd.Medicines.Any(m => m.Name == medicine.Name && m.Producer == medicine.Producer))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    pharmacyToAdd.Medicines.Add(new Medicine()
                    {
                        Name = medicine.Name,
                        Price = medicine.Price,
                        Category = (Category)medicine.Category,
                        ProductionDate = DateTime.ParseExact(medicine.ProductionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        ExpiryDate = DateTime.ParseExact(medicine.ExpiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Producer = medicine.Producer
                    });

                }

                pharmaciesXML.Add(pharmacyToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedPharmacy, pharmacyToAdd.Name,
                    pharmacyToAdd.Medicines.Count));
            }

            context.Pharmacies.AddRange(pharmaciesXML);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
