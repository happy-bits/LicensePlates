using System.Linq;
using System.Text.RegularExpressions;

namespace LicensePlates.Factory
{
    class ValidatorFactory
    {
        // Simple factory
        public static IValidator Create(CustomerType customerType) => customerType switch
        {
            CustomerType.Diplomat => new DiplomatValidator(),
            CustomerType.Advertisment => new AdvertismentValidator(),
            CustomerType.Normal => new NormalValidator(),
            CustomerType.Taxi => new TaxiValidator(),
            _ => throw new System.Exception()
        };
    }

    interface IValidator
    {
        bool IsValid(string plate);
    }

    class Helper
    {
        public static bool IsNormalPlate(string plate)
        {
            var allSwedishLetters = "ABCDEFGHIJKLMNOPQRSTWXYZÅÄÖ";
            var invalidLetters = "IQVÅÄÖ";
            var validLetters = ExcludeLetters(allSwedishLetters, invalidLetters);
            var validLastCharacter = validLetters + "0123456789";
            var regex = "[" + validLetters + "]{3} [0-9][0-9][" + validLastCharacter + "]";
            return Regex.IsMatch(plate, regex);
        }

        public static bool ReserveredForTaxi(string plate) => plate.Last() == 'T';

        public static bool ReserveredForAdvertisments(string plate) => plate.StartsWith("MLB");

        private static string ExcludeLetters(string letters, string lettersToRemove) => string.Join("", letters.Where(c => !lettersToRemove.Contains(c)));
    }

    class TaxiValidator: IValidator
    {
        public bool IsValid(string plate) => Helper.IsNormalPlate(plate) && !Helper.ReserveredForAdvertisments(plate);
    }

    class NormalValidator : IValidator
    {
        public bool IsValid(string plate) => Helper.IsNormalPlate(plate) && !Helper.ReserveredForAdvertisments(plate) && !Helper.ReserveredForTaxi(plate);
    }

    class DiplomatValidator : IValidator
    {
        public bool IsValid(string plate) => Regex.IsMatch(plate, "[A-Z]{2} \\d\\d\\d [A-Z]");
    }

    class AdvertismentValidator : IValidator
    {
        public bool IsValid(string plate) => Helper.IsNormalPlate(plate) && !Helper.ReserveredForTaxi(plate);
    }

    class RegistrationService
    {
        private readonly ILicensePlateRepository _repo;

        public RegistrationService(ILicensePlateRepository repo)
        {
            _repo = repo;
        }

        public int NrOfRegistredPlates => _repo.CountRegisteredPlates();

        public Result AddLicensePlate(string number, CustomerType customer)
        {
            IValidator validator = ValidatorFactory.Create(customer);

            if (!validator.IsValid(number))
                return Result.InvalidFormat;

            if (!_repo.IsAvailable(number))
                return Result.NotAvailable;

            _repo.Save(number);
            return Result.Success;
        }

    }

}
