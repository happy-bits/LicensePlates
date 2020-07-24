
/*

    This exercise is about creating a "RegistrationService" with a method "AddLicensePlate".
    The service add a (swedish) license plates to a repository and handle problems.
    The license plates have to be on a special format depending on the type of customer.

    Question: what do you think about the two solutions of this problem?

    Especially these files:
    - Factory/RegistrationService.cs
    - NoPattern/RegistrationService.cs

    Pros and cons?

 */
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LicensePlates
{
    [TestClass]
    public class Tests
    {
        private IRegistrationService[] _services;

        public Tests()
        {
            _services = new IRegistrationService[] {
                new NoPattern.RegistrationService(new FakeLicensePlateRepository()) ,
                new Factory.RegistrationService(new FakeLicensePlateRepository()) ,
            };
        }

        // NORMAL CUSTOMER

        [TestMethod]
        [DataRow("ABC 123")]
        [DataRow("ABC 12B")] // (last character may be a letter)
        public void return_Success_when_plate_has_correct_format(string plate)
        {
            foreach (var service in _services)
            {
                Assert.AreEqual(Result.Success, service.AddLicensePlate(plate, CustomerType.Normal));
            }
        }

        [TestMethod]
        [DataRow("ABC X23")]
        [DataRow("ÅBC 123")] // Å is a swedish letter but not a valid letter anyway
        [DataRow("QBC 123")] // Q is not a valid letter (too similair to O)
        [DataRow("1BC 123")]
        [DataRow("ABC  123")]
        public void return_InvalidFormat_when_format_is_incorrect(string plate)
        {
            foreach (var service in _services)
            {
                var result = service.AddLicensePlate(plate, CustomerType.Normal);
                Assert.AreEqual(Result.InvalidFormat, result);
            }
        }

        [TestMethod]
        [DataRow("MLB 123")]
        [DataRow("MLB 456")]
        public void return_OnlyForAdvertisment_when_non_advertisment_try_register_plate_starting_with_MLB(string plate)
        {
            foreach (var service in _services)
            {
                var result = service.AddLicensePlate(plate, CustomerType.Normal);
                Assert.AreEqual(Result.InvalidFormat, result);
            }
        }

        [TestMethod]
        [DataRow("ABC 12T")]
        [DataRow("CCC 77T")]
        public void return_OnlyForTaxi_when_plate_ends_with_T_and_customer_isnot_taxi(string plate)
        {
            foreach (var service in _services)
            {
                var result = service.AddLicensePlate(plate, CustomerType.Normal);
                Assert.AreEqual(Result.InvalidFormat, result);
            }
        }

        [TestMethod]
        public void return_NotAvailable_when_plate_is_already_registered()
        {
            foreach (var service in _services)
            {
                service.AddLicensePlate("ABC 123", CustomerType.Normal);

                var result = service.AddLicensePlate("ABC 123", CustomerType.Normal);
                Assert.AreEqual(Result.NotAvailable, result);
            }
        }

        // ADVERTISMENT CUSTOMER

        [TestMethod]
        [DataRow("ABC 123")]
        [DataRow("ABC 12B")]
        [DataRow("MLB 123")]
        [DataRow("MLB 456")]
        public void return_Success_when_plate_has_correct_format_for_advertisment(string plate)
        {
            foreach (var service in _services)
            {
                var result = service.AddLicensePlate(plate, CustomerType.Advertisment);

                Assert.AreEqual(Result.Success, result);
            }
        }

        [TestMethod]
        [DataRow("ABC X23")]
        [DataRow("ÅBC 123")]
        [DataRow("QBC 123")]
        [DataRow("1BC 123")]
        [DataRow("ABC  123")]
        public void return_InvalidFormat_when_format_is_incorrect_for_advertisment(string plate)
        {
            foreach (var service in _services)
            {
                var result = service.AddLicensePlate(plate, CustomerType.Normal);
                Assert.AreEqual(Result.InvalidFormat, result);
            }
        }

        // TAXI CUSTOMER

        [DataRow("ABC 12T")]
        [DataRow("CCC 77T")]
        public void return_Success_when_plate_has_correct_format_for_taxi(string plate)
        {
            foreach (var service in _services)
            {
                Assert.AreEqual(Result.Success, service.AddLicensePlate(plate, CustomerType.Taxi));
            }
        }

        [TestMethod]
        [DataRow("ABC X23")]
        [DataRow("ÅBC 123")]
        [DataRow("QBC 123")]
        [DataRow("1BC 123")]
        [DataRow("ABC  123")]
        public void return_InvalidFormat_when_format_is_incorrect_for_taxi(string plate)
        {
            foreach (var service in _services)
            {
                var result = service.AddLicensePlate(plate, CustomerType.Taxi);
                Assert.AreEqual(Result.InvalidFormat, result);
            }
        }

        // DIPLOMAT

        /*
            First two letters: country code
            Three numbers: the embassy's serial number 
            Last letter: the ambassadors rank
        */

        [TestMethod]
        [DataRow("AA 111 A")]
        [DataRow("CD 777 X")]
        public void return_Success_when_plate_has_correct_format_for_diplomats(string plate)
        {
            foreach (var service in _services)
            {
                Assert.AreEqual(Result.Success, service.AddLicensePlate(plate, CustomerType.Diplomat));
            }
        }

        [TestMethod]
        [DataRow("ABC 123")]
        [DataRow("ABC 12B")]
        [DataRow("A8 111 A")]
        [DataRow("CD A77 X")]
        public void return_InvalidFormat_when_format_is_incorrect_for_diplomat(string plate)
        {
            foreach (var service in _services)
            {
                var result = service.AddLicensePlate(plate, CustomerType.Diplomat);
                Assert.AreEqual(Result.InvalidFormat, result);
            }
        }

        // EXCEPTIONS

        [TestMethod]
        public void throw_RepositoryException_when_unexpected_problem_with_database()
        {
            foreach (var service in _services)
            {

                Assert.ThrowsException<RepositoryException>(() =>
                {
                    service.AddLicensePlate("XXX 666", CustomerType.Normal);
                });

                Assert.ThrowsException<RepositoryException>(() =>
                {
                    service.AddLicensePlate("YYY 666", CustomerType.Normal);
                });
            }
        }

    }
}
