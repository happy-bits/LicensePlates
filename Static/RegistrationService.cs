/*
 Ingen bra lösning

 Allt är kopplat till Helper-klassen som inte är utbyggdbar alls

 Om programmet utvecklas så är det inte bra att ha all viktig businesslogik i en enskild "god class" som hela världen är beroende av

 Denna lösning är procedural, inte objektorienterad. Den är global, inte encapsulated.
 
 */
using System.Linq;
using System.Text.RegularExpressions;

namespace LicensePlates.Static
{
    class ValidatorFactory
    {
        public static Validator Create(string plate, CustomerType customerType) => customerType switch
        {
            CustomerType.Diplomat => new DiplomatValidator(plate),
            CustomerType.Advertisment => new AdvertismentValidator(plate),
            CustomerType.Normal => new NormalValidator(plate),
            CustomerType.Taxi => new TaxiValidator(plate),
            _ => throw new System.Exception()
        };
    }

    // Question: I guess this is a really bad solution, but if someone asked I wouln't be able to answer why it's bad. So why is this solution not good?

    static class Helper
    {
        public static bool IsNormalPlate(string Plate) => Regex.IsMatch(Plate, RegexForNormalPlate());
        public static bool ReserveredForTaxi(string Plate) => Plate.Last() == 'T';
        public static bool ReserveredForAdvertisments(string Plate) => Plate.StartsWith("MLB");
        private static string ExcludeLetters(string letters, string lettersToRemove) => string.Join("", letters.Where(c => !lettersToRemove.Contains(c)));
        private static string RegexForNormalPlate()
        {
            var allSwedishLetters = "ABCDEFGHIJKLMNOPQRSTWXYZÅÄÖ";
            var invalidLetters = "IQVÅÄÖ";
            var validLetters = ExcludeLetters(allSwedishLetters, invalidLetters);
            var validLastCharacter = validLetters + "0123456789";
            return "[" + validLetters + "]{3} [0-9][0-9][" + validLastCharacter + "]";
        }

    }

    abstract class Validator
    {
        protected string Plate { get; }

        public Validator(string plate)
        {
            Plate = plate;
        }

        public abstract bool IsValid { get; }
    }

    class TaxiValidator : Validator
    {
        public TaxiValidator(string plate) : base(plate)
        {
        }

        public override bool IsValid => Helper.IsNormalPlate(Plate) && !Helper.ReserveredForAdvertisments(Plate);
    }

    class NormalValidator : Validator
    {
        public NormalValidator(string plate) : base(plate)
        {
        }

        public override bool IsValid => Helper.IsNormalPlate(Plate) && !Helper.ReserveredForAdvertisments(Plate) && !Helper.ReserveredForTaxi(Plate);
    }

    class AdvertismentValidator : Validator
    {
        public AdvertismentValidator(string plate) : base(plate)
        {
        }
        public override bool IsValid => Helper.IsNormalPlate(Plate) && !Helper.ReserveredForTaxi(Plate);
    }

    class DiplomatValidator : Validator
    {
        public DiplomatValidator(string plate) : base(plate)
        {
        }
        public override bool IsValid => Regex.IsMatch(Plate, "[A-Z]{2} \\d\\d\\d [A-Z]");
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
            Validator validator = ValidatorFactory.Create(plate, customer);

            if (!validator.IsValid)
                return Result.InvalidFormat;

            if (!_repo.IsAvailable(plate))
                return Result.NotAvailable;

            _repo.Save(plate);
            return Result.Success;
        }

    }

}
