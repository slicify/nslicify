using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nslicify
{
    /// <summary>
    /// C# wrapper that allows Slicify REST libraries to be easily
    /// called from .Net.
    /// 
    /// </summary>
    public class SlicifyNode
    {        
        private string Username;
        private string Password;

        public static string API_BASE = "https://secure.slicify.com/Service/BookingService.asmx/";

        /// <summary>
        /// REST API wrapper
        /// </summary>
        /// <param name="username">Slicify username</param>
        /// <param name="password">Slicify password</param>
        public SlicifyNode(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Add a new bid
        /// </summary>
        /// <param name="active"></param>
        /// <param name="maxPrice"></param>
        /// <param name="minECU"></param>
        /// <param name="minRam"></param>
        /// <param name="country"></param>
        /// <param name="bits"></param>
        /// <returns>bid ID of new bid</returns>
        public int AddBid(bool active, decimal maxPrice, int minECU, int minRam, string country, int bits)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("active", active.ToString());
            parameters.Add("maxPrice", maxPrice.ToString());
            parameters.Add("minECU", minECU.ToString());
            parameters.Add("minRam", minRam.ToString());
            if(country != null)
                parameters.Add("country", country.ToString());
            parameters.Add("bits", bits.ToString());

            return HttpGetInt("BidAdd", parameters);
        }

        /// <summary>
        /// Delete a bid
        /// </summary>
        /// <param name="bidid"></param>
        public void DeleteBid(int bidid)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bidid", bidid.ToString());
            HttpGetString("BidDelete", parameters);
        }

        /// <summary>
        /// Activate bid
        /// </summary>
        /// <param name="bidid"></param>
        /// <returns></returns>
        public int ActivateBid(int bidid)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bidid", bidid.ToString());
            return HttpGetInt("BidActivate", parameters);
        }

        /// <summary>
        /// Inactivate bid. Also terminates any associated booking.
        /// </summary>
        /// <param name="bidid"></param>
        /// <returns></returns>
        public int InactivateBid(int bidid)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bidid", bidid.ToString());
            return HttpGetInt("BidInactivate", parameters);
        }

        /// <summary>
        /// Inactivate all bids for the current user. Also terminates
        /// all current bookings.
        /// </summary>
        /// <returns></returns>
        public int InactivateAllBids()
        {
            NameValueCollection parameters = new NameValueCollection();
            return HttpGetInt("BidInactivateAll", parameters);
        }

        /// <summary>
        /// Delete all bids for the current user. Also terminates all
        /// current bookings
        /// </summary>
        /// <returns></returns>
        public int DeleteAllBids()
        {
            NameValueCollection parameters = new NameValueCollection();
            return HttpGetInt("BidDeleteAll", parameters);
        }

        /// <summary>
        /// Get all the info for the specified bid.
        /// </summary>
        /// <param name="bidid"></param>
        /// <returns></returns>
        public BidInfo GetBidInfo(int bidid)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bidid", bidid.ToString());
            return (BidInfo)HttpGet("BidGetInfo", parameters, BidInfoParser);
        }

        /// <summary>
        /// Get the status description for the current booking e.g.
        /// Launching, Ready
        /// </summary>
        /// <param name="bookingID"></param>
        /// <returns></returns>
        public string GetBookingStatus(int bookingID)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bookingID", bookingID.ToString());
            return HttpGetString("BookingGetStatus", parameters);
        }

        /// <summary>
        /// Get the current rate being charged for this booking.
        /// </summary>
        /// <param name="bookingID"></param>
        /// <returns></returns>
        public decimal GetCurrentRate(int bookingID)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bookingID", bookingID.ToString());
            return HttpGetDecimal("BookingGetCurrentRate", parameters);
        }

        /// <summary>
        /// Get total billed so far for this booking
        /// </summary>
        /// <param name="bookingID"></param>
        /// <returns></returns>
        public decimal GetTotalBilled(int bookingID)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bookingID", bookingID.ToString());
            return HttpGetDecimal("BookingGetTotalBilled", parameters);
        }

        /// <summary>
        /// Get the bid ID associated with this booking
        /// </summary>
        /// <param name="bookingID"></param>
        /// <returns></returns>
        public int GetBookingBidID(int bookingID)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bookingID", bookingID.ToString());
            return HttpGetInt("BookingGetBidID", parameters);
        }

        /// <summary>
        /// Get the account balance for the user.
        /// </summary>
        /// <returns></returns>
        public decimal GetAccountBalance()
        {
            NameValueCollection parameters = new NameValueCollection();
            return HttpGetDecimal("AccountBalance", parameters);
        }

        /// <summary>
        /// Get the average cost of the specified booking over the specified
        /// time range.
        /// </summary>
        /// <param name="bookingID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public decimal BookingGetCost(int bookingID, DateTime startTime, DateTime endTime)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bookingID", bookingID.ToString());
            parameters.Add("startTime", startTime.ToSlicifyDateFormat());
            parameters.Add("endTime", endTime.ToSlicifyDateFormat());
            return HttpGetDecimal("BookingGetCost", parameters);
        }

        /// <summary>
        /// Get the booking password.
        /// </summary>
        /// <param name="bookingID"></param>
        /// <returns></returns>
        public string GetBookingOTP(int bookingID)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("bookingID", bookingID.ToString());
            return HttpGetString("BookingGetPassword", parameters);
        }


        private string HttpGetString(string URL, NameValueCollection parameters)
        {
            return HttpGet(URL, parameters, SingleStringParser).ToString();
        }

        private decimal HttpGetDecimal(string URL, NameValueCollection parameters)
        {
            string reply = HttpGet(URL, parameters, SingleStringParser).ToString();
            return Convert.ToDecimal(reply);
        }

        private int HttpGetInt(string URL, NameValueCollection parameters)
        {
            string reply = HttpGet(URL, parameters, SingleStringParser).ToString();
            return Convert.ToInt32(reply);
        }

        private object HttpGet(string url, NameValueCollection parameters, ParserDelegate parser)
        {
            //create web request with params
            WebClient webClient = new WebClient();
            webClient.QueryString.Add(parameters);

            //add basic auth token
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(Username + ":" + Password));
            webClient.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;

            //call REST API and return result
            while (true)
            {
                //retry if there is any timeout etc
                try
                {
                    Stream replyStream = webClient.OpenRead(API_BASE + url);
                    XDocument replyDoc = XDocument.Load(replyStream);

                    return parser(replyDoc);
                }
                catch (WebException e)
                {
                    if (e.HResult == -2146233079)
                        Thread.Sleep(1000); //retry on timeout
                    else
                        throw e;
                }
            }
        }

        //parsers - these take a XML document and extract
        //the relevant info
        private delegate object ParserDelegate(XDocument doc);

        /// <summary>
        /// Parse a single string from the XML doc
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private string SingleStringParser(XDocument doc)
        {
            return ((XElement)(doc.FirstNode)).Value;
        }

        /// <summary>
        /// Parse bid information from the XML doc
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private BidInfo BidInfoParser(XDocument doc)
        {
            BidInfo info = null;

            //get the first record in the result set
            XContainer c1 = (XContainer)(doc);
            XContainer c2 = (XContainer)(c1.FirstNode);

            //loop through fields
            foreach (XNode node in c2.Nodes())
            {
                XElement el = (XElement)node;
                string key = el.Name.LocalName;
                string value = el.Value;

                //little hack to only set BidInfo if there are some values
                if(info == null)
                    info = new BidInfo();

                if (key.Equals("Active"))
                    info.Active = Convert.ToBoolean(value);
                else if (key.Equals("BidID"))
                    info.BidID = Convert.ToInt32(value);
                else if (key.Equals("Bits"))
                    info.Bits = Convert.ToInt32(value);
                else if (key.Equals("BookingID"))
                    info.BookingID = Convert.ToInt32(value);
                else if (key.Equals("Country"))
                    info.Country = value;
                else if (key.Equals("MaxPrice"))
                    info.MaxPrice = Convert.ToDecimal(value);
                else if (key.Equals("MinECU"))
                    info.MinECU = Convert.ToInt32(value);
                else if (key.Equals("MinRAM"))
                    info.MinRAM = Convert.ToInt32(value);
                else if (key.Equals("Deleted"))
                    info.Deleted = Convert.ToBoolean(value);
            }
            return info;
        }

        /// <summary>
        /// Contains information about this bid.
        /// </summary>
        public class BidInfo
        {
            public int BidID { get; set; }
            public int BookingID { get; set; }
            public bool Active { get; set; }
            public decimal MaxPrice { get; set; }
            public int MinECU { get; set; }
            public int MinRAM { get; set; }
            public string Country { get; set; }
            public int Bits { get; set; }
            public bool Deleted { get; set; }
        }

    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Convert a DateTime value into the text format
        /// required by Slicify.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static String ToSlicifyDateFormat(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss.ff");
        }
    }
}
