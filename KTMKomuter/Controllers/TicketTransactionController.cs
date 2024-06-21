using KTMKomuter.MailSettings;
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
                        IndexQuantity = reader.GetInt32(9),
                        TotalAmount = reader.GetDouble(10)
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
                cmd.Parameters.AddWithValue("@indexquantity", ticket.IndexQuantity);
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

        public IActionResult SearchIndex(string searchString = "")
        {
            IList<TicketTransaction> dbList = GetDbList();
            var result = dbList.Where(x => x.ViewId.ToLower().Contains(searchString.ToLower()) ||
            x.CustName.ToLower().Contains(searchString.ToLower()))
                .OrderBy(x => x.CustName).ThenByDescending(x => x.ViewDateTime);

            return View("AdminDashboard", result);
        }
        public IActionResult Details(string id)
        {
            IList<TicketTransaction> dbList = GetDbList();
            var result = dbList.First(x => x.ViewId == id);

            return View(result);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            IList<TicketTransaction> dbList = GetDbList();
            var result = dbList.First(x => x.ViewId == id);

            return View(result);
        }

        [HttpPost]
        public IActionResult Edit(string id, TicketTransaction parcel)
        {
            SqlConnection conn = new SqlConnection(configuration.GetConnectionString("KTMConnStr"));
            SqlCommand cmd = new SqlCommand("spUpdateTicket", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@custname", parcel.CustName);
            cmd.Parameters.AddWithValue("@custidentity", parcel.CustIdentity);
            cmd.Parameters.AddWithValue("@custemail", parcel.CustEmail);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
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
            return RedirectToAction("AdminDashboard");
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            IList<TicketTransaction> dbList = GetDbList();
            var result = dbList.First(x => x.ViewId == id);

            return View(result);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult ConfirmDelete(string id)
        {
            SqlConnection conn = new SqlConnection(configuration.GetConnectionString("KTMConnStr"));
            SqlCommand cmd = new SqlCommand("spDeleteTicket", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", id);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                return RedirectToAction("AdminDashboard");
            }

            catch
            {
                return View();
            }

            finally
            {
                conn.Close();
            }
        }
        public IActionResult SendMail(string id)
        {
            IList<TicketTransaction> dbList = GetDbList();
            var result = dbList.First(x => x.ViewId == id);

            var subject = "Parcel Information" + result.ViewId;
            var body = "Parcel id:" + result.ViewId + "<br>" +
                "Date and time: " + result.ViewDateTime + "<br>" +
                "Customer name: " + result.CustName + "<br>" +
                "Customer Identity : " + result.CustIdentity + "<br>" +
                "Departure : " + result.DictDeparture[result.IndexDeparture] + "<br>" +
                "Arrival : " + result.DictArrival[result.IndexArrival] + "<br>" +
                "Category : " + result.DictCategory[result.IndexCategory] + "<br>" +
                "Ticket Type: " + result.DictType[result.IndexTypeTicket] + "<br>" +
                "Number of Ticket: " + result.IndexQuantity + "<br>" +
                "Total Amount: " + result.TotalAmount.ToString("c2");

            var mail = new Mail(configuration);

            if (mail.Send(configuration["Gmail:Username"], result.CustEmail, subject, body))
            {
                ViewBag.Message = "Mail successfully send to " + result.CustEmail;
                ViewBag.Body = body;
            }
            else
            {
                ViewBag.Message = "Sent Mail Failed";
                ViewBag.Body = "";
            }

            return View(result);

        }
    }
}
