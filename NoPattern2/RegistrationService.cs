/*
 
Similair to NoPattern but with a class "Validator"

*/

using System.Linq;
using System.Text.RegularExpressions;

namespace LicensePlates.NoPattern2
{
    class Validator
    {
        private readonly string _plate;
        private readonly CustomerType _customer;

        public Validator(string plate, CustomerType customer)
        {
            _plate = plate;
            _customer = customer;
        }

        private static bool ReserveredForTaxi(string plate) => plate.Last() == 'T';

        private static bool ReserveredForAdvertisments(string plate) => plate.StartsWith("MLB");

        private static string ExcludeLetters(string letters, string lettersToRemove) => string.Join("", letters.Where(c => !lettersToRemove.Contains(c)));

        private static bool IsValidDiplomatPlate(string plate) => Regex.IsMatch(plate, "[A-Z]{2} \\d\\d\\d [A-Z]");

        public static bool IsNormalPlate(string plate)
        {
            var allSwedishLetters = "ABCDEFGHIJKLMNOPQRSTWXYZÅÄÖ";
            var invalidLetters = "IQVÅÄÖ";
            var validLetters = ExcludeLetters(allSwedishLetters, invalidLetters);
            var validLastCharacter = validLetters + "0123456789";
            var regex = "[" + validLetters + "]{3} [0-9][0-9][" + validLastCharacter + "]";
            return Regex.IsMatch(plate, regex);
        }

        public bool IsValid()
        {
            if (_customer == CustomerType.Diplomat && !IsValidDiplomatPlate(_plate))
                return false;

            if (_customer != CustomerType.Diplomat)
            {
                if (!IsNormalPlate(_plate))
                    return false;

                if (_customer != CustomerType.Taxi && ReserveredForTaxi(_plate))
                    return false;

                if (_customer != CustomerType.Advertisment && ReserveredForAdvertisments(_plate))
                    return false;
            }
            return true;

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
            if (!new Validator(plate, customer).IsValid())
                return Result.InvalidFormat;

            if (!_repo.IsAvailable(plate))
                return Result.NotAvailable;

            _repo.Save(plate);
            return Result.Success;
        }

    }

}
