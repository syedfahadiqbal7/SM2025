using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerTransferController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<CustomerTransferController> _logger;

        public CustomerTransferController(MyDbContext context, ILogger<CustomerTransferController> logger)
        {
            _context = context;
            _logger = logger;
        }
        // GET: api/Cart/GetCartItems/{customerId}
        [HttpGet("TransferList")]
        public async Task<List<CustomerTransferViewModel>> TransferList()
        {
            try
            {
                List<CustomerTransferViewModel> transfers = new List<CustomerTransferViewModel>();

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "Report_GetCustomerTransferDetails";
                    command.CommandType = CommandType.StoredProcedure;
                    _context.Database.OpenConnection();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transfers.Add(new CustomerTransferViewModel
                            {
                                ID = reader.GetInt32(0),
                                PaymentType = reader.GetString(1),
                                Amount = Convert.ToDecimal(reader.GetString(2)),
                                MerchantID = reader.GetInt32(3),
                                CompanyName = reader.GetString(4),
                                IsPaymentSuccess = Convert.ToBoolean(reader.GetInt32(5)),
                                ServiceID = reader.GetInt32(6),
                                PaymentDateTime = reader.GetDateTime(7),
                                Quantity = reader.GetInt32(8),
                                RFDFU = reader.GetInt32(9),
                                ServiceImage = reader.GetString(15),
                                CustomerName = reader.GetString(16),
                                ServiceName = reader.GetString(17)
                            });
                        }
                    }
                }

                return transfers;
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet("GetSalesTransactions")]
        public async Task<ActionResult<List<SalesTransactionViewModel>>> GetSalesTransactions()
        {
            List<SalesTransactionViewModel> transactions = new List<SalesTransactionViewModel>();
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "GetSalesTransactions";
                    command.CommandType = CommandType.StoredProcedure;
                    _context.Database.OpenConnection();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transactions.Add(new SalesTransactionViewModel
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                PaymentType = reader["PAYMENTTYPE"].ToString(),
                                Amount = Convert.ToDecimal(reader["AMOUNT"]),
                                ProviderName = reader["ProviderName"].ToString(),
                                IsPaymentSuccess = Convert.ToBoolean(reader["ISPAYMENTSUCCESS"]),
                                ServiceID = Convert.ToInt32(reader["SERVICEID"]),
                                PaymentDateTime = Convert.ToDateTime(reader["PAYMENTDATETIME"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                ServiceImage = reader["ServiceImage"].ToString(),
                                CustomerName = reader["CustomerName"].ToString(),
                                ServiceName = reader["ServiceName"].ToString(),
                                Discount = Convert.ToDecimal(reader["Discount"]), // Discount from FINALPRICE
                                Tax = Convert.ToDecimal(reader["Tax"]), // VAT Percentage Applied
                                ServiceCharge = Convert.ToDecimal(reader["ServiceCharge"]), // Service Charge Calculation
                                PaymentStatus = Convert.ToBoolean(reader["ISPAYMENTSUCCESS"]) ? "Successful" : "Pending"
                            });
                        }
                    }
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sales transactions.");
                return StatusCode(500, "Internal server error.");
            }
        }

        public class SalesTransactionViewModel
        {
            public int ID { get; set; }
            public string PaymentType { get; set; }
            public decimal Amount { get; set; }
            public string ProviderName { get; set; }
            public bool IsPaymentSuccess { get; set; }
            public int ServiceID { get; set; }
            public DateTime PaymentDateTime { get; set; }
            public int Quantity { get; set; }
            public string ServiceImage { get; set; }
            public string CustomerName { get; set; }
            public string ServiceName { get; set; }
            public string PaymentStatus { get; set; }
            public decimal Discount { get; set; }
            public decimal Tax { get; set; }
            public decimal ServiceCharge { get; set; }
        }
    }
}
