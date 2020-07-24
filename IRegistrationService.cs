namespace LicensePlates
{
    interface IRegistrationService
    {
        int NrOfRegistredPlates { get; }

        Result AddLicensePlate(string plate, CustomerType customer);
    }
}
