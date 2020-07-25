using System.Linq;
using System.Text.RegularExpressions;

namespace LicensePlates.RuleEngine
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

    /*
     Good: Plate is reused so e.g "ReservedForTaxi" is just a prop
     Bad: This class have too many responsibilities. IsNormalPlate, ReservedForTaxi etc don't belong here
         */
    abstract class Validator
    {
        protected string Plate { get; }

        public Validator(string plate)
        {
            Plate = plate;
        }

        public abstract bool IsValid { get; }

        protected bool IsNormalPlate => Regex.IsMatch(Plate, RegexForNormalPlate());

        protected bool ReserveredForTaxi => Plate.Last() == 'T';

        protected bool ReserveredForAdvertisments => Plate.StartsWith("MLB");

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

    /*
     Good: these validators is short and simple
     Bad: if a new validator is inserted (e.g VIP) we probably need to change IsValid for many of the validators

   */
    class TaxiValidator : Validator
    {
        public TaxiValidator(string plate) : base(plate)
        {
        }

        public override bool IsValid => IsNormalPlate && !ReserveredForAdvertisments;
    }

    class NormalValidator : Validator
    {
        public NormalValidator(string plate) : base(plate)
        {
        }

        public override bool IsValid => IsNormalPlate && !ReserveredForAdvertisments && !ReserveredForTaxi;
    }

    class AdvertismentValidator : Validator
    {
        public AdvertismentValidator(string plate) : base(plate)
        {
        }
        public override bool IsValid => IsNormalPlate && !ReserveredForTaxi;
    }

    class DiplomatValidator : Validator
    {
        public DiplomatValidator(string plate) : base(plate)
        {
        }
        public override bool IsValid => Regex.IsMatch(Plate, "[A-Z]{2} \\d\\d\\d [A-Z]");
    }

    // Good: this class is simpler than NoPattern

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
