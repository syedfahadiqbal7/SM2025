namespace AFFZ_Admin.Models
{
    public class BankTransferAccount
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string? BankName { get; set; }
        public string? AccountHolderName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IBANNUMBER { get; set; }
        public string? BranchName { get; set; }
        public bool IsActive { get; set; }
    }
}
