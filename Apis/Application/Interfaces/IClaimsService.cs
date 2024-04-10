namespace Application.Interfaces
{
    public interface IClaimsService
    {
        public Guid GetCurrentUserId { get; }
        public bool GetIsAdmin { get; }
        public bool GetIsCustomer { get; }
    }
}
