using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LicensePlates.ChainOfResponsibility
{

    abstract class Validator
    {
        protected Validator _next;

        public void SetNext(Validator next) => _next = next;

        public abstract bool IsValid(PlateInfo plateInfo);

        protected bool ProcessNext(PlateInfo plateInfo)
        {
            if (_next == null)
                return true;
            return _next.IsValid(plateInfo);
        }
    }

    class PlateInfo
    {
        public CustomerType CustomerType { get; }
        public string Plate { get; }

        public PlateInfo(string plate, CustomerType customerType)
        {
            Plate = plate;
            CustomerType = customerType;
        }
    }

    class DiplomatValidator : Validator
    {
        public override bool IsValid(PlateInfo plateInfo)
        {
            if (plateInfo.CustomerType == CustomerType.Diplomat)
                return Regex.IsMatch(plateInfo.Plate, "[A-Z]{2} \\d\\d\\d [A-Z]");

            return ProcessNext(plateInfo);
        }
    }

    class NormalPlateValidator : Validator
    {
        private static string ExcludeLetters(string letters, string lettersToRemove) => string.Join("", letters.Where(c => !lettersToRemove.Contains(c)));

        public static bool IsNormalPlate(string plate)
        {
            var allSwedishLetters = "ABCDEFGHIJKLMNOPQRSTWXYZÅÄÖ";
            var invalidLetters = "IQVÅÄÖ";
            var validLetters = ExcludeLetters(allSwedishLetters, invalidLetters);
            var validLastCharacter = validLetters + "0123456789";
            var regex = "[" + validLetters + "]{3} [0-9][0-9][" + validLastCharacter + "]";
            return Regex.IsMatch(plate, regex);
        }

        public override bool IsValid(PlateInfo plateInfo)
        {
            if (plateInfo.CustomerType != CustomerType.Diplomat && !IsNormalPlate(plateInfo.Plate))
                return false;
            return ProcessNext(plateInfo);
        }
    }

    class ReservedForTaxiValidator : Validator
    {
        public override bool IsValid(PlateInfo plateInfo)
        {
            if (plateInfo.CustomerType != CustomerType.Taxi && plateInfo.Plate.Last() == 'T')
                return false;

            return ProcessNext(plateInfo);
        }
    }

    class ReservedForAdvertismentValidator : Validator
    {
        public override bool IsValid(PlateInfo plateInfo)
        {
            if (plateInfo.CustomerType != CustomerType.Advertisment && plateInfo.Plate.StartsWith("MLB"))
                return false;

            return ProcessNext(plateInfo);
        }
    }

    class RegistrationService : IRegistrationService
    {
        private readonly ILicensePlateRepository _repo;

        public RegistrationService(ILicensePlateRepository repo)
        {
            _repo = repo;
        }

        public int NrOfRegistredPlates => _repo.CountRegisteredPlates();

        public Result AddLicensePlate(string plate, CustomerType customer)
        {
            if (!IsValid(new PlateInfo(plate, customer)))
                return Result.InvalidFormat;

            if (!_repo.IsAvailable(plate))
                return Result.NotAvailable;

            _repo.Save(plate);
            return Result.Success;
        }

        private bool IsValid(PlateInfo plateInfo)
        {
            Validator validator = new DiplomatValidator();
            Validator normal = new NormalPlateValidator();
            Validator taxi = new ReservedForTaxiValidator();
            Validator advertisment = new ReservedForAdvertismentValidator();

            validator.SetNext(normal);
            normal.SetNext(taxi);
            taxi.SetNext(advertisment);

            return validator.IsValid(plateInfo);
        }
    }

}
