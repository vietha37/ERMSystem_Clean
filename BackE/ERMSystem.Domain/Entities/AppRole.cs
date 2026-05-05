namespace ERMSystem.Domain.Entities
{
    public static class AppRole
    {
        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Receptionist = "Receptionist";
        public const string Patient = "Patient";

        public static readonly string[] All = { Admin, Doctor, Receptionist, Patient };
        public static readonly string[] Internal = { Admin, Doctor, Receptionist };
    }
}
