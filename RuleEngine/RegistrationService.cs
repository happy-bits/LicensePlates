using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LicensePlates.RuleEngine
{
    class ValidatorEngine
    {
        private readonly List<Rule> _rules;

        public ValidatorEngine(string plate, CustomerType customerType)
        {
            _rules = new List<Rule> { 
                new DiplomatRule(customerType, plate),
                new NormalPlateRule(customerType, plate),
                new ReservedForTaxiRule(customerType, plate),
                new ReservedForAdvertismentRule(customerType, plate),
            };
        }

        public bool Validate() => _rules.All(rule => rule.Pass());
    }

    abstract class Rule
    {
        public Rule(CustomerType customerType, string plate)
        {
            CustomerType = customerType;
            Plate = plate;
        }

        protected CustomerType CustomerType { get; }
        protected string Plate { get; }

        public abstract bool Pass();
    }

    class DiplomatRule : Rule
    {
        public DiplomatRule(CustomerType customerType, string plate) : base(customerType, plate)
        {
        }

        public override bool Pass()
        {
            if (CustomerType != CustomerType.Diplomat)
                return true;
                
            return Regex.IsMatch(Plate, "[A-Z]{2} \\d\\d\\d [A-Z]");
        }
    }

    class NormalPlateRule : Rule
    {
        public NormalPlateRule(CustomerType customerType, string plate) : base(customerType, plate)
        {
        }

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

        public override bool Pass()
        {
            if (CustomerType == CustomerType.Diplomat)
                return true;

            return IsNormalPlate(Plate);
        }
    }

    class ReservedForTaxiRule : Rule
    {
        public ReservedForTaxiRule(CustomerType customerType, string plate) : base(customerType, plate)
        {
        }

        public override bool Pass()
        {
            if (CustomerType == CustomerType.Taxi)
                return true;

            return Plate.Last() != 'T';
        }
    }

    class ReservedForAdvertismentRule : Rule
    {
        public ReservedForAdvertismentRule(CustomerType customerType, string plate) : base(customerType, plate)
        {
        }

        public override bool Pass()
        {
            if (CustomerType == CustomerType.Advertisment)
                return true;

            return !Plate.StartsWith("MLB");
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
            if (!new ValidatorEngine(plate, customer).Validate())
                return Result.InvalidFormat;

            if (!_repo.IsAvailable(plate))
                return Result.NotAvailable;

            _repo.Save(plate);
            return Result.Success;
        }

    }

}
