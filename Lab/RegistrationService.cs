/*
 Good: much private in Validator + validators is simplified
 Bad: ReserveredForOtherCustomers have to be updated when new validator appears
 
 */
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LicensePlates.Lab
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

    abstract class Validator
    {
        protected string Plate { get; }

        public Validator(string plate)
        {
            Plate = plate;
        }

        public virtual bool IsValid => IsNormalPlate && !ReserveredForOtherCustomers();

        private bool IsNormalPlate => Regex.IsMatch(Plate, RegexForNormalPlate());

        private bool ReserveredForTaxi => Plate.Last() == 'T';

        private bool ReserveredForAdvertisments => Plate.StartsWith("MLB");

        private bool ReserveredForOtherCustomers()
        {
            if (this is TaxiValidator && ReserveredForTaxi)
                return true;

            if (this is NormalValidator && (ReserveredForTaxi || ReserveredForAdvertisments))
                return true;

            if (this is AdvertismentValidator && ReserveredForTaxi)
                return true;

            return false;

        }

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

    class TaxiValidator : Validator
    {
        public TaxiValidator(string plate) : base(plate)
        {
        }
    }

    class NormalValidator : Validator
    {
        public NormalValidator(string plate) : base(plate)
        {
        }
    }

    class AdvertismentValidator : Validator
    {
        public AdvertismentValidator(string plate) : base(plate)
        {
        }
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
