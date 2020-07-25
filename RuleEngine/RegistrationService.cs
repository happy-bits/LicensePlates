// First attempt with Rule Engine (this is no good solution
using System;
using System.Collections.Generic;
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

    class ValidatorEngine
    {
        public ValidatorEngine()
        {
            Rules = new List<Func<string, Validator, bool>> { IsNormalPlate, NoConflictWithTaxi, NoConflictWithAdvertisments };
        }
        public List<Func<string, Validator, bool>> Rules {get;set;}

        private bool IsNormalPlate(string plate, Validator validator) => Regex.IsMatch(plate, RegexForNormalPlate());
        private bool IsCorrectFormatForDiplomat(string plate) => Regex.IsMatch(plate, "[A-Z]{2} \\d\\d\\d [A-Z]");

        private bool NoConflictWithTaxi(string plate, Validator validator) => validator is TaxiValidator || plate.Last() != 'T';

        private bool NoConflictWithAdvertisments(string plate, Validator validator) => validator is AdvertismentValidator || !plate.StartsWith("MLB");

        private static string ExcludeLetters(string letters, string lettersToRemove) => string.Join("", letters.Where(c => !lettersToRemove.Contains(c)));

        private static string RegexForNormalPlate()
        {
            var allSwedishLetters = "ABCDEFGHIJKLMNOPQRSTWXYZÅÄÖ";
            var invalidLetters = "IQVÅÄÖ";
            var validLetters = ExcludeLetters(allSwedishLetters, invalidLetters);
            var validLastCharacter = validLetters + "0123456789";
            return "[" + validLetters + "]{3} [0-9][0-9][" + validLastCharacter + "]";
        }

        public bool Validate(string plate, Validator validator)
        {
            if (validator is DiplomatValidator && IsCorrectFormatForDiplomat(plate))
                return true;

            if (validator is DiplomatValidator && !IsCorrectFormatForDiplomat(plate))
                return false;

            foreach (var rule in Rules){
                if (!rule(plate, validator))
                    return false;
            }
            return true;
        }
    }

    abstract class Validator
    {
        protected string Plate { get; }

        public Validator(string plate)
        {
            Plate = plate;
        }

        public bool IsValid => new ValidatorEngine().Validate(Plate, this);
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
