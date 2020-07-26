/*
 Question: 
 
 What do you think about this solution? 

 I'm not so fond of the solution :) but maybe one good thing is that the order of validatorns in BuildChain doesn't matter so it's possible to later change the order for performance reasons.
 
 */

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

        // Question: should this method be in this class? If no: where?

        private bool IsValid(PlateInfo plateInfo)
        {
            var validator = BuildChain(
                new DiplomatValidator(),
                new NormalPlateValidator(),
                new ReservedForTaxiValidator(),
                new ReservedForAdvertismentValidator()
                );

            return validator.IsValid(plateInfo);
        }

        // Question: I guess this method should belong to another class. Maybe be generic. If yes what would you call that class?

        private static Validator BuildChain(params Validator[] validators)
        {
            var validator = validators[0];

            for (int i = 0; i < validators.Length-1; i++)
            {
                var v = validators[i];
                v.SetNext(validators[i + 1]);
            }

            return validator;
        }
    }

}
