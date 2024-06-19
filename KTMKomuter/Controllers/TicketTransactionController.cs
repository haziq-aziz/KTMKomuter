using KTMKomuter.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace KTMKomuter.Controllers
{
    public class TicketTransactionController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<TicketTransactionController> logger;

        public TicketTransactionController(IConfiguration config, ILogger<TicketTransactionController> logger)
        {
            this.configuration = config;
            this.logger = logger;
        }

        // Method to get the database list
        IList<TicketTransaction> GetDbList()
        {
            IList<TicketTransaction> dbList = new List<TicketTransaction>();
            SqlConnection conn = new SqlConnection(configuration.GetConnectionString("KTMConnStr"));

            string sql = @"SELECT * FROM TicketTransactions";
            SqlCommand cmd = new SqlCommand(sql, conn);

            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    dbList.Add(new TicketTransaction()
                    {
                        ViewId = reader.GetString(0),
                        ViewDateTime = reader.GetDateTime(1),
                        CustName = reader.GetString(2),
                        CustIdentity = reader.GetString(3),
                        CustEmail = reader.GetString(4),
                        IndexDeparture = reader.GetInt32(5),
                        IndexArrival = reader.GetInt32(6),
                        IndexCategory = reader.GetInt32(7),
                        IndexTypeTicket = reader.GetInt32(8),
                        TotalAmount = reader.GetDouble(9)
                    });
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching the data.");
                RedirectToAction("Error");
            }

            finally
            {
                conn.Close();
            }

            return dbList;
        }

        //belum add view lagi so ikut la nak jadikan ni homepage or anything
        public IActionResult Index()
        {
            return View();
        }

        //To show error
        public IActionResult Error()
        {
            return View();
        }

        //To get data in the form
        [HttpGet]
        public IActionResult OrderTicket()
        {
            TicketTransaction ticket = new TicketTransaction();
            ticket.IndexDeparture = ticket.IndexArrival = -1;
            ticket.IndexCategory = ticket.IndexTypeTicket = -1;
            return View(ticket);
        }

        [HttpPost]
        public IActionResult OrderTicket(TicketTransaction ticket)
        {
            if (!ModelState.IsValid)
            {
                SqlConnection conn = new SqlConnection(configuration.GetConnectionString("KTMConnStr"));
                SqlCommand cmd = new SqlCommand("spInsertTicket", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ticketid", ticket.TicketId);
                cmd.Parameters.AddWithValue("@ticketdatetime", ticket.TicketDateTime);
                cmd.Parameters.AddWithValue("@custname", ticket.CustName);
                cmd.Parameters.AddWithValue("@custidentity", ticket.CustIdentity);
                cmd.Parameters.AddWithValue("@custemail", ticket.CustEmail);

                cmd.Parameters.AddWithValue("@indexdeparture", ticket.IndexDeparture);
                cmd.Parameters.AddWithValue("@indexarrival", ticket.IndexArrival);
                cmd.Parameters.AddWithValue("@indexcategory", ticket.IndexCategory);
                cmd.Parameters.AddWithValue("@indextypeticket", ticket.IndexTypeTicket);
                cmd.Parameters.AddWithValue("@totalamount", ticket.TotalAmount);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    return View(ticket);
                }
                finally
                {
                    conn.Close();
                }

                return View("TicketInvoice", ticket);
            }

            else
                return View(ticket);
        }

        public IActionResult AdminDashboard ()
        {
            IList<TicketTransaction> dbList = GetDbList();
            return View(dbList);
        }
    }
}
