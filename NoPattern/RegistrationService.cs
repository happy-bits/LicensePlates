using System.Linq;
using System.Text.RegularExpressions;

namespace LicensePlates.NoPattern
{
    class RegistrationService
    {
        private readonly ILicensePlateRepository _repo;

        public RegistrationService(ILicensePlateRepository repo)
        {
            _repo = repo;
        }

        public int NrOfRegistredPlates => _repo.CountRegisteredPlates();

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

        public Result AddLicensePlate(string plate, CustomerType customer)
        {
            if (customer == CustomerType.Diplomat && !IsValidDiplomatPlate(plate))
                return Result.InvalidFormat;

            if (customer != CustomerType.Diplomat)
            {
                if (!IsNormalPlate(plate))
                    return Result.InvalidFormat;

                if (customer != CustomerType.Taxi && ReserveredForTaxi(plate))
                    return Result.InvalidFormat;

                if (customer != CustomerType.Advertisment && ReserveredForAdvertisments(plate))
                    return Result.InvalidFormat;
            }

            if (!_repo.IsAvailable(plate))
                return Result.NotAvailable;

            _repo.Save(plate);
            return Result.Success;
        }

    }

}
