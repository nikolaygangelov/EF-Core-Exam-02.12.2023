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
            //using Data Transfer Object Class to map it with patients
            var patientssArray = JsonConvert.DeserializeObject<ImportPatientDTO[]>(jsonString);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid patients can be kept
            List<Patient> patientList = new List<Patient>();           

            foreach (ImportPatientDTO patientDTO in patientssArray)
            {
                //validating info for patient from data
                if (!IsValid(patientDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //creating a valid patient
                Patient patientToAdd = new Patient()
                {
                    //using identical properties in order to map successfully
                    FullName = patientDTO.FullName,
                    AgeGroup = (AgeGroup)int.Parse(patientDTO.AgeGroup), //two transformations in order to reach needed format
                    Gender = (Gender)int.Parse(patientDTO.Gender)
                };



                foreach (int medicineId in patientDTO.Medicines)
                {
                    //checking for duplicates
                    if (patientToAdd.PatientsMedicines.Any(pm => pm.MedicineId == medicineId)) 
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //adding valid PatientMedicine
                    patientToAdd.PatientsMedicines.Add(new PatientMedicine()
                    {
                        MedicineId = medicineId
                    });


                }

                patientList.Add(patientToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedPatient, patientToAdd.FullName, patientToAdd.PatientsMedicines.Count));
            }

            context.AddRange(patientList);

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();
        }

        public static string ImportPharmacies(MedicinesContext context, string xmlString)
        {
            //using Data Transfer Object Class to map it with pharmacies
            var serializer = new XmlSerializer(typeof(ImportPharmaciesDTO[]), new XmlRootAttribute("Pharmacies"));

            //Deserialize method needs TextReader object to convert/map
            using StringReader inputReader = new StringReader(xmlString);
            var pharmaciesArrayDTOs = (ImportPharmaciesDTO[])serializer.Deserialize(inputReader);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid pharmacies can be kept
            List<Pharmacy> pharmaciesXML = new List<Pharmacy>();

            foreach (ImportPharmaciesDTO pharmacyDTO in pharmaciesArrayDTOs)
            {
                //validating info for pharmacy from data
                if (!IsValid(pharmacyDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //creating a valid pharmacy
                Pharmacy pharmacyToAdd = new Pharmacy
                {
                    //using identical properties in order to map successfully
                    IsNonStop = bool.Parse(pharmacyDTO.IsNonStop),
                    Name = pharmacyDTO.Name,
                    PhoneNumber = pharmacyDTO.PhoneNumber
                };

                foreach (var medicine in pharmacyDTO.Medicines)
                {
                    //validating info for medicine from data
                    if (!IsValid(medicine))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //validating dates
                    if (DateTime.ParseExact(medicine.ProductionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) >= //culture-independent format in order to reach needed format
                        DateTime.ParseExact(medicine.ExpiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //checking for duplicates
                    if (pharmacyToAdd.Medicines.Any(m => m.Name == medicine.Name && m.Producer == medicine.Producer))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //adding valid medicine
                    pharmacyToAdd.Medicines.Add(new Medicine()
                    {
                        //using identical properties in order to map successfully
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

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
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
