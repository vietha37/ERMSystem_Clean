namespace ERMSystem.Domain.Entities
{
    public static class AppRole
    {
        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Receptionist = "Receptionist";

        public static readonly string[] All = { Admin, Doctor, Receptionist };
    }
}
