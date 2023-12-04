namespace Evntd.Bogus
{
    public class ScenarioCustomerOptions
    {
        /// <summary>
        /// The min number of initial customers to register.
        /// </summary>
        public int MinInitialCustomerBase { get; set; }

        /// <summary>
        /// The max number of initial customers to register.
        /// </summary>
        public int MaxInitialCustomerBase { get; set; }

        /// <summary>
        /// The maximum time between customer registrations.
        /// </summary>
        public TimeSpan MaxTimeBetweenRegistrations { get; set; }

        /// <summary>
        /// The earliest date which a customer registers.
        /// </summary>
        public DateTimeOffset EarliestCustomerRegistration { get; set; }

        /// <summary>
        /// The earliest possible birth date to generate for a customer.
        /// </summary>
        public DateTime MinBirthDate { get; set; }

        /// <summary>
        /// The latest possible birth date to generate for a customer.
        /// </summary>
        public DateTime MaxBirthDate { get; set; }

        /// <summary>
        /// A percentage of customers which supply their birthdate during registration.
        /// </summary>
        public float PercentBirthDateProvided { get; set; }

        /// <summary>
        /// A percentage of customers which set their default address.
        /// </summary>
        public float PercentSetDefaultAddress { get; set; }

        /// <summary>
        /// The format of the phone number.
        /// </summary>
        public string PhoneNumberFormat { get; set; }

        /// <summary>
        /// The format of address zip codes.
        /// </summary>
        public string ZipCodeFormat { get; set; }

        /// <summary>
        /// The length of the password hash.
        /// </summary>
        public int PasswordHashLength { get; set; }

        /// <summary>
        /// The length of the password salt.
        /// </summary>
        public int PasswordSaltLength { get; set; }

        public string[] DefaultAddressLabels { get; set; }

        public TimeSpan RegistrationTime { get; set; }

        public TimeSpan CarryoutTime { get; set; }
    }
}