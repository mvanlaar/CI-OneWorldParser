using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.IO.Compression;
using CsvHelper;
using Newtonsoft.Json;
using System.Threading;

namespace CI_OneWorldParser
{
    public class Program
    {
        public static readonly List<string> _ColombiaAirports = new List<string>() { "APO", "AUC", "AXM", "BSC", "EJA", "BAQ", "BOG", "BGA", "BUN", "CLO", "CTG", "CRC", "CZU", "CUC", "EYP", "FLA", "GIR", "GPI", "IBE", "LET", "MZL", "MQU", "EOH", "MDE", "MVP", "MTR", "NVA", "PSO", "PEI", "PPN", "PVA", "PUU", "PCR", "UIB", "RCH", "ADZ", "SJE", "SVI", "SMR", "RVE", "TME", "TLU", "TCO", "VUP", "VVC", "ACD", "AFI", "ACR", "ARQ", "NBB", "CPB", "CCO", "CUO", "CAQ", "CPL", "IGO", "CIM", "COG", "RAV", "BHF", "EBG", "ELB", "ECR", "LGT", "HTZ", "IPI", "JUO", "LMC", "LPD", "LPE", "MGN", "MCJ", "MFS", "MMP", "MTB", "NCI", "NQU", "OCV", "ORC", "RON", "PZA", "PTX", "PLT", "PBE", "PDA", "LQM", "NAR", "OTU", "SNT", "AYG", "SSL", "SOX", "TTM", "TCD", "TIB", "TBD", "TDA", "TRB", "URI", "URR", "VGZ", "LCR", "SQE", "SRS", "ULQ", "CVE", "PAL", "PYA", "TQS", "API" };
                
        [Serializable]
        public class CIFLight
        {
            // Auto-implemented properties. 

            public string FromIATA;            
            public string FromIATATerminal;
            public string ToIATA;           
            public string ToIATATerminal;
            public DateTime FromDate;
            public DateTime ToDate;
            public Boolean FlightMonday;
            public Boolean FlightTuesday;
            public Boolean FlightWednesday;
            public Boolean FlightThursday;
            public Boolean FlightFriday;
            public Boolean FlightSaterday;
            public Boolean FlightSunday;
            public string DepartTime;
            public string ArrivalTime;
            public String FlightNumber;
            public String FlightAirline;
            public String FlightOperator;
            public String FlightAircraft;
            public Boolean FlightCodeShare;
            public Boolean FlightNextDayArrival;
            public int FlightNextDays;
            public string FlightDuration;
            public Boolean FlightNonStop;
            public string FlightVia;
        }
        
        public class IATAAirline
        {
            public string Airline_Name;
            public string Airline_IATA;            
        }
       
        static void Main(string[] args)
        {
            string DataDir = AppDomain.CurrentDomain.BaseDirectory + "\\data";
            System.IO.Directory.CreateDirectory(DataDir);
            string sqldb = Path.Combine(DataDir, "cm.sqlite");
            string APIPathAirport = "airport/iata/";
            string APIPathAirline = "airline/iata/";
            const string ua = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            const string HeaderAccept = "text/html,application/xhtml+xml,application/xml;q=0.9,*;q=0.8";
            List<CIFLight> CIFLights = new List<CIFLight> { };            
            Console.WriteLine("Requesting Latest update...");
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            string dateformat = "hh:mm";

            
            var request = (HttpWebRequest)WebRequest.Create("http://mtk.innovataw3svc.com/MapDataToolKitServices.asmx/GetLastUpdate?");

            var postData = @"_sSearchXML=<GetLastUpdate_Input customerCode=""ONW-CTK"" customerSubCode="""" productCode=""DskMap8.0"" lang=""EN""/>";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            string b = WebUtility.HtmlDecode(responseString);

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(b);
            XmlNodeList xmlnode;
            XmlNodeList lastupdate;
            XmlNodeList dataValidFrom;
            XmlNodeList dataValidTo;
            //int i = 0;
            xmlnode = xmlDoc.GetElementsByTagName("dbFile");
            lastupdate = xmlDoc.GetElementsByTagName("lastUpdatedDate");
            string lastupdateMM = lastupdate[0].Attributes["MM"].Value;
            string lastupdateDD = lastupdate[0].Attributes["DD"].Value;
            string lastupdateYYYY = lastupdate[0].Attributes["YYYY"].Value;
            dataValidFrom = xmlDoc.GetElementsByTagName("dataValidFrom");
            string dataValidFromMM = dataValidFrom[0].Attributes["MM"].Value;
            string dataValidFromDD = dataValidFrom[0].Attributes["DD"].Value;
            string dataValidFromYYYY = dataValidFrom[0].Attributes["YYYY"].Value;
            dataValidTo = xmlDoc.GetElementsByTagName("dataValidTo");
            string dataValidToMM = dataValidTo[0].Attributes["MM"].Value;
            string dataValidToDD = dataValidTo[0].Attributes["DD"].Value;
            string dataValidToYYYY = dataValidTo[0].Attributes["YYYY"].Value;

            // Generate Filename
            string dbdownload = Path.Combine(DataDir, "ONW-CTK_" + lastupdateYYYY + lastupdateMM + lastupdateDD + "db.gz");
            // Check if File Exist.
            if (!File.Exists(dbdownload))
            {
                // XMLNode 0 = url, 1 = md5checksum               
                string downloadurl = xmlnode[0].ChildNodes.Item(0).InnerText.Trim();
                string md5checksum = xmlnode[0].ChildNodes.Item(1).InnerText.Trim();
                using (var client = new WebClient())
                {
                    Console.WriteLine("Downloading {0} bytes from {1}", xmlnode[0].Attributes["zippedSize"].Value, downloadurl);
                    client.DownloadFile(downloadurl, dbdownload);
                    Console.WriteLine("Checking MD5 of File...");
                    string filemd5 = Utils.checkMD5(filename: dbdownload);
                    if (filemd5 == md5checksum) { Console.WriteLine("Checksum is good...."); }
                }                
                Utils.ExtractFile(filename: dbdownload, extractfile: sqldb);
                //Stream inStream = File.OpenRead(dbdownload);                
            }            
            else
            {
                Console.WriteLine("File Exists on disk is the same as the downloadable version. Skipping download...");
            }
            // File Downloaded or reused. 
            Console.WriteLine("Loading List of airports");            

            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=" + sqldb + ";Version=3;");
            m_dbConnection.Open();

            
            // Start Loading the Flights
            Console.WriteLine("Loading Flights...");
            string sqlflights = "select * from GSEC_MODIFIED where CODESHARE = 0;";
            SQLiteCommand commandflights = new SQLiteCommand(sqlflights, m_dbConnection);
            SQLiteDataReader readerflights = commandflights.ExecuteReader();
            while (readerflights.Read())
            {
                string TEMP_FromIATA = null;
                string TEMP_FromIATACountry = null;
                string TEMP_FromIATARegion = null;
                string TEMP_ToIATA = null;
                string TEMP_ToIATACountry = null;
                string TEMP_ToIATARegion = null;
                DateTime TEMP_ValidFrom = new DateTime();
                DateTime TEMP_ValidTo = new DateTime();
                Boolean TEMP_FlightMonday = false;
                Boolean TEMP_FlightTuesday = false;
                Boolean TEMP_FlightWednesday = false;
                Boolean TEMP_FlightThursday = false;
                Boolean TEMP_FlightFriday = false;
                Boolean TEMP_FlightSaterday = false;
                Boolean TEMP_FlightSunday = false;
                string TEMP_DepartTime = null;
                string TEMP_ArrivalTime = null;
                Boolean TEMP_FlightCodeShare = false;
                string TEMP_FlightNumber = null;
                string TEMP_Aircraftcode = null;
                TimeSpan TEMP_DurationTime = TimeSpan.MinValue;
                Boolean TEMP_FlightNextDayArrival = false;
                int TEMP_FlightNextDays = 0;
                string TEMP_FlightOperator = null;
                string TEMP_Airline = null;
                Boolean TEMP_FlightNonStop = true;
                string TEMP_FlightVia = null;
                string TEMP_FromIATATerminal = null;
                string TEMP_ToIATATerminal = null;

                TEMP_FromIATA = readerflights["CODE_DEP"].ToString();
                TEMP_ToIATA = readerflights["CODE_ARR"].ToString();

                
                TEMP_ValidFrom = DateTime.Parse(readerflights["DATE_FROM"].ToString());
                TEMP_ValidTo = DateTime.Parse(readerflights["DATE_TO"].ToString());
                TEMP_FlightMonday = Boolean.Parse(readerflights["op1"].ToString());
                TEMP_FlightTuesday = Boolean.Parse(readerflights["op2"].ToString());
                TEMP_FlightWednesday = Boolean.Parse(readerflights["op3"].ToString());
                TEMP_FlightThursday = Boolean.Parse(readerflights["op4"].ToString());
                TEMP_FlightFriday = Boolean.Parse(readerflights["op5"].ToString());
                TEMP_FlightSaterday = Boolean.Parse(readerflights["op6"].ToString());
                TEMP_FlightSunday = Boolean.Parse(readerflights["op7"].ToString());
                TEMP_DepartTime = readerflights["TIME_PASS_DEP"].ToString();
                TEMP_ArrivalTime = readerflights["TIME_PASS_ARR"].ToString();
                TEMP_FlightNumber = readerflights["CARRIER"].ToString() + readerflights["FLIGHTNO"].ToString();
                TEMP_FromIATATerminal = readerflights["DEP_TERM"].ToString();
                TEMP_ToIATATerminal = readerflights["ARR_TERM"].ToString();

                if (readerflights["CODESHARE"].ToString() == "1")
                {
                    TEMP_FlightOperator = readerflights["CODE_SHARE_CARRIER"].ToString();
                    TEMP_FlightCodeShare = true;
                }
                else
                {
                    TEMP_FlightOperator = readerflights["CARRIER"].ToString();
                    TEMP_FlightCodeShare = false;
                }
                if (readerflights["EXTRA_DAY"].ToString() == "1")
                {
                    TEMP_FlightNextDayArrival = true;
                }
                if (readerflights["STOPS"].ToString() == "1")
                {
                    TEMP_FlightNonStop = false;
                    TEMP_FlightVia = readerflights["STOP_CODE"].ToString();                    
                }
                TEMP_Airline = readerflights["CARRIER"].ToString();
                TEMP_FlightNextDays = int.Parse(readerflights["EXTRA_DAY"].ToString());
                TEMP_DurationTime = TimeSpan.FromMinutes(double.Parse(readerflights["FLIGHTDURATION"].ToString()));

                if (_ColombiaAirports.Contains(TEMP_FromIATA, StringComparer.OrdinalIgnoreCase) & !_ColombiaAirports.Contains(TEMP_ToIATA, StringComparer.OrdinalIgnoreCase) || (_ColombiaAirports.Contains(TEMP_ToIATA, StringComparer.OrdinalIgnoreCase) & !_ColombiaAirports.Contains(TEMP_FromIATA, StringComparer.OrdinalIgnoreCase)) || (_ColombiaAirports.Contains(TEMP_ToIATA, StringComparer.OrdinalIgnoreCase) & _ColombiaAirports.Contains(TEMP_FromIATA, StringComparer.OrdinalIgnoreCase)))
                {

                    CIFLights.Add(new CIFLight
                    {
                        FromIATA = TEMP_FromIATA,                        
                        FromIATATerminal = TEMP_FromIATATerminal,
                        ToIATA = TEMP_ToIATA,                        
                        ToIATATerminal = TEMP_ToIATATerminal,
                        FromDate = TEMP_ValidFrom,
                        ToDate = TEMP_ValidTo,
                        ArrivalTime = TEMP_ArrivalTime,
                        DepartTime = TEMP_DepartTime,
                        FlightAircraft = TEMP_Aircraftcode,
                        FlightAirline = TEMP_Airline,
                        FlightMonday = TEMP_FlightMonday,
                        FlightTuesday = TEMP_FlightTuesday,
                        FlightWednesday = TEMP_FlightWednesday,
                        FlightThursday = TEMP_FlightThursday,
                        FlightFriday = TEMP_FlightFriday,
                        FlightSaterday = TEMP_FlightSaterday,
                        FlightSunday = TEMP_FlightSunday,
                        FlightNumber = TEMP_FlightNumber,
                        FlightOperator = TEMP_FlightOperator,
                        FlightDuration = TEMP_DurationTime.ToString().Replace("-", ""),
                        FlightCodeShare = TEMP_FlightCodeShare,
                        FlightNextDayArrival = TEMP_FlightNextDayArrival,
                        FlightNextDays = TEMP_FlightNextDays,
                        FlightNonStop = TEMP_FlightNonStop,
                        FlightVia = TEMP_FlightVia
                    });
                }
            }            
            m_dbConnection.Close();

            //CIFLights.RemoveAll(s => s.FromIATACountry != "CO" | s.ToIATACountry != "CO");


            // Write XML File
            Console.WriteLine("Writeing XML File...");
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(CIFLights.GetType());
            string myDirOut = AppDomain.CurrentDomain.BaseDirectory + "\\output";
            System.IO.Directory.CreateDirectory(myDirOut);
            System.IO.StreamWriter file =
               new System.IO.StreamWriter("output\\output.xml");

            writer.Serialize(file, CIFLights);
            file.Close();
            
            Console.WriteLine("Generate GTFS Files...");
                                    
            string gtfsDir = AppDomain.CurrentDomain.BaseDirectory + "\\gtfs";
            System.IO.Directory.CreateDirectory(gtfsDir);
            Console.WriteLine("Creating GTFS File agency.txt...");

            using (var gtfsagency = new StreamWriter(@"gtfs\\agency.txt"))
            {
                var csv = new CsvWriter(gtfsagency);
                csv.Configuration.Delimiter = ",";
                csv.Configuration.Encoding = Encoding.UTF8;
                csv.Configuration.TrimFields = true;
                // header 
                csv.WriteField("agency_id");
                csv.WriteField("agency_name");
                csv.WriteField("agency_url");
                csv.WriteField("agency_timezone");
                csv.WriteField("agency_lang");
                csv.WriteField("agency_phone");
                csv.WriteField("agency_fare_url");
                csv.WriteField("agency_email");
                csv.NextRecord();

                var airlines = CIFLights.Select(m => new {m.FlightAirline}).Distinct().ToList();

                foreach (var compagny in airlines) {
                    string urlapi = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirline + compagny.FlightAirline.Trim();
                    string RequestAirlineJson = String.Empty;
                    HttpWebRequest requestAirline = (HttpWebRequest)WebRequest.Create(urlapi);

                    requestAirline.Method = "GET";
                    requestAirline.UserAgent = ua;
                    requestAirline.Accept = HeaderAccept;
                    requestAirline.Proxy = null;
                    requestAirline.KeepAlive = false;
                    using (HttpWebResponse Airlineresponse = (HttpWebResponse)requestAirline.GetResponse())
                    using (StreamReader reader = new StreamReader(Airlineresponse.GetResponseStream()))
                    {
                        RequestAirlineJson = reader.ReadToEnd();
                    }

                    dynamic AirlineResponseJson = JsonConvert.DeserializeObject(RequestAirlineJson);
                    csv.WriteField(Convert.ToString(AirlineResponseJson[0].code));
                    csv.WriteField(Convert.ToString(AirlineResponseJson[0].name));
                    csv.WriteField(Convert.ToString(AirlineResponseJson[0].website));
                    csv.WriteField("America/Bogota");
                    csv.WriteField("ES");
                    csv.WriteField(Convert.ToString(AirlineResponseJson[0].phone));
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.NextRecord();
                }                    
            }

            Console.WriteLine("Creating GTFS File routes.txt ...");

            using (var gtfsroutes = new StreamWriter(@"gtfs\\routes.txt"))
            {
                // Route record
                var csvroutes = new CsvWriter(gtfsroutes);
                csvroutes.Configuration.Delimiter = ",";
                csvroutes.Configuration.Encoding = Encoding.UTF8;
                csvroutes.Configuration.TrimFields = true;
                // header 
                csvroutes.WriteField("route_id");
                csvroutes.WriteField("agency_id");
                csvroutes.WriteField("route_short_name");
                csvroutes.WriteField("route_long_name");
                csvroutes.WriteField("route_desc");
                csvroutes.WriteField("route_type");
                csvroutes.WriteField("route_url");
                csvroutes.WriteField("route_color");
                csvroutes.WriteField("route_text_color");
                csvroutes.NextRecord();

                var routes = CIFLights.Select(m => new { m.FromIATA, m.ToIATA, m.FlightAirline }).Distinct().ToList();

                foreach (var route in routes) {
                    string FromAirportName = null;
                    string ToAirportName = null;
                    string FromAirportCountry = null;
                    string FromAirportContinent = null;
                    string ToAirportCountry = null;
                    string ToAirportContinent = null;

                    using (var clientFrom = new WebClient())
                    {
                        clientFrom.Encoding = Encoding.UTF8;
                        clientFrom.Headers.Add("user-agent", ua);
                        string urlapiFrom = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + route.FromIATA;
                        var jsonapiFrom = clientFrom.DownloadString(urlapiFrom);
                        dynamic AirportResponseJsonFrom = JsonConvert.DeserializeObject(jsonapiFrom);
                        FromAirportName = Convert.ToString(AirportResponseJsonFrom[0].name);
                        FromAirportCountry = Convert.ToString(AirportResponseJsonFrom[0].country_code);
                        FromAirportContinent = Convert.ToString(AirportResponseJsonFrom[0].continent);
                    }

                    using (var clientTo = new WebClient())
                    {
                        clientTo.Encoding = Encoding.UTF8;
                        clientTo.Headers.Add("user-agent", ua);
                        string urlapiTo = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + route.ToIATA;
                        var jsonapiTo = clientTo.DownloadString(urlapiTo);
                        dynamic AirportResponseJsonTo = JsonConvert.DeserializeObject(jsonapiTo);
                        ToAirportName = Convert.ToString(AirportResponseJsonTo[0].name);
                        ToAirportCountry = Convert.ToString(AirportResponseJsonTo[0].country_code);
                        ToAirportContinent = Convert.ToString(AirportResponseJsonTo[0].continent);
                    }

                    csvroutes.WriteField(route.FromIATA + route.ToIATA + route.FlightAirline);
                    csvroutes.WriteField(route.FlightAirline);
                    csvroutes.WriteField(route.FromIATA + route.ToIATA);
                    csvroutes.WriteField(FromAirportName + " - " + ToAirportName);                                   
                    csvroutes.WriteField(""); // routes[i].FlightAircraft + ";" + CIFLights[i].FlightAirline + ";" + CIFLights[i].FlightOperator + ";" + CIFLights[i].FlightCodeShare
                    if (FromAirportCountry == ToAirportCountry)
                    {
                        // Colombian internal flight domestic
                        csvroutes.WriteField(1102);
                    }
                    else
                    {
                        csvroutes.WriteField(FromAirportContinent == ToAirportContinent ? 1101 : 1103);
                    }
                    csvroutes.WriteField("");
                    csvroutes.WriteField("");
                    csvroutes.WriteField("");
                    csvroutes.NextRecord();
                }
            }

            // stops.txt

            List<string> agencyairportsiata =
                CIFLights.SelectMany(m => new string[] { m.FromIATA, m.ToIATA })
                        .Distinct()
                        .ToList();

            using (var gtfsstops = new StreamWriter(@"gtfs\\stops.txt"))
            {
                // Route record
                var csvstops = new CsvWriter(gtfsstops);
                csvstops.Configuration.Delimiter = ",";
                csvstops.Configuration.Encoding = Encoding.UTF8;
                csvstops.Configuration.TrimFields = true;
                //csvstops.Configuration.QuoteNoFields = true;

                // header                                 
                csvstops.WriteField("stop_id");
                csvstops.WriteField("stop_name");
                csvstops.WriteField("stop_desc");
                csvstops.WriteField("stop_lat");
                csvstops.WriteField("stop_lon");
                csvstops.WriteField("zone_id");
                csvstops.WriteField("stop_url");
                csvstops.WriteField("stop_timezone");
                csvstops.NextRecord();

                foreach (string airportiata in agencyairportsiata) {
// Using API for airport Data.
                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        string url = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + airportiata.Trim();
                        var json = client.DownloadString(url);
                        dynamic AirportResponseJson = JsonConvert.DeserializeObject(json);

                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].code));
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].name));
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].city)); 
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].lat));
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].lng));
                        csvstops.WriteField("");
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].website));
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].timezone));
                        csvstops.NextRecord();
                    }
                }
            }                

            Console.WriteLine("Creating GTFS File trips.txt and stop_times.txt...");
            using (var gtfscalendar = new StreamWriter(@"gtfs\\calendar.txt"))
            {                    
                using (var gtfstrips = new StreamWriter(@"gtfs\\trips.txt"))
                {
                    using (var gtfsstoptimes = new StreamWriter(@"gtfs\\stop_times.txt"))
                    {
                        // Headers 
                        var csvstoptimes = new CsvWriter(gtfsstoptimes);
                        csvstoptimes.Configuration.Delimiter = ",";
                        csvstoptimes.Configuration.Encoding = Encoding.UTF8;
                        csvstoptimes.Configuration.TrimFields = true;
                        // header 
                        csvstoptimes.WriteField("trip_id");
                        csvstoptimes.WriteField("arrival_time");
                        csvstoptimes.WriteField("departure_time");
                        csvstoptimes.WriteField("stop_id");
                        csvstoptimes.WriteField("stop_sequence");
                        csvstoptimes.WriteField("stop_headsign");
                        csvstoptimes.WriteField("pickup_type");
                        csvstoptimes.WriteField("drop_off_type");
                        csvstoptimes.WriteField("shape_dist_traveled");
                        csvstoptimes.WriteField("timepoint");
                        csvstoptimes.NextRecord();

                        var csvtrips = new CsvWriter(gtfstrips);
                        csvtrips.Configuration.Delimiter = ",";
                        csvtrips.Configuration.Encoding = Encoding.UTF8;
                        csvtrips.Configuration.TrimFields = true;
                        // header 
                        csvtrips.WriteField("route_id");
                        csvtrips.WriteField("service_id");
                        csvtrips.WriteField("trip_id");
                        csvtrips.WriteField("trip_headsign");
                        csvtrips.WriteField("trip_short_name");
                        csvtrips.WriteField("direction_id");
                        csvtrips.WriteField("block_id");
                        csvtrips.WriteField("shape_id");
                        csvtrips.WriteField("wheelchair_accessible");
                        csvtrips.WriteField("bikes_allowed ");
                        csvtrips.NextRecord();
                                
                        var csvcalendar = new CsvWriter(gtfscalendar);
                        csvcalendar.Configuration.Delimiter = ",";
                        csvcalendar.Configuration.Encoding = Encoding.UTF8;
                        csvcalendar.Configuration.TrimFields = true;
                        // header 
                        csvcalendar.WriteField("service_id");
                        csvcalendar.WriteField("monday");
                        csvcalendar.WriteField("tuesday");
                        csvcalendar.WriteField("wednesday");
                        csvcalendar.WriteField("thursday");
                        csvcalendar.WriteField("friday");
                        csvcalendar.WriteField("saturday");
                        csvcalendar.WriteField("sunday");
                        csvcalendar.WriteField("start_date");
                        csvcalendar.WriteField("end_date");
                        csvcalendar.NextRecord();

                        //1101 International Air Service
                        //1102 Domestic Air Service
                        //1103 Intercontinental Air Service
                        //1104 Domestic Scheduled Air Service
                                
                        foreach (CIFLight flight in CIFLights) {
                            // Calender
                            csvcalendar.WriteField(flight.FromIATA + flight.ToIATA + flight.FlightNumber.Replace(" ", "") +
                                                   $"{flight.FromDate:yyyyMMdd}" + $"{flight.ToDate:yyyyMMdd}" + Convert.ToInt32(flight.FlightMonday) + Convert.ToInt32(flight.FlightTuesday) + Convert.ToInt32(flight.FlightWednesday) + Convert.ToInt32(flight.FlightThursday) + Convert.ToInt32(flight.FlightFriday) + Convert.ToInt32(flight.FlightSaterday) + Convert.ToInt32(flight.FlightSunday));
                            csvcalendar.WriteField(Convert.ToInt32(flight.FlightMonday));
                            csvcalendar.WriteField(Convert.ToInt32(flight.FlightTuesday));
                            csvcalendar.WriteField(Convert.ToInt32(flight.FlightWednesday));
                            csvcalendar.WriteField(Convert.ToInt32(flight.FlightThursday));
                            csvcalendar.WriteField(Convert.ToInt32(flight.FlightFriday));
                            csvcalendar.WriteField(Convert.ToInt32(flight.FlightSaterday));
                            csvcalendar.WriteField(Convert.ToInt32(flight.FlightSunday));
                            csvcalendar.WriteField($"{flight.FromDate:yyyyMMdd}");
                            csvcalendar.WriteField($"{flight.ToDate:yyyyMMdd}");
                            csvcalendar.NextRecord();

                            // Trips
                            string FromAirportName = null;
                            string ToAirportName = null;
                            using (var client = new WebClient())
                            {
                                client.Encoding = Encoding.UTF8;
                                string url = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + flight.FromIATA;
                                var json = client.DownloadString(url);
                                dynamic AirportResponseJson = JsonConvert.DeserializeObject(json);
                                FromAirportName = Convert.ToString(AirportResponseJson[0].name);
                            }
                            using (var client = new WebClient())
                            {
                                client.Encoding = Encoding.UTF8;
                                string url = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + flight.ToIATA;
                                var json = client.DownloadString(url);
                                dynamic AirportResponseJson = JsonConvert.DeserializeObject(json);
                                ToAirportName = Convert.ToString(AirportResponseJson[0].name);
                            }

                            csvtrips.WriteField(flight.FromIATA + flight.ToIATA + flight.FlightAirline);
                            csvtrips.WriteField(flight.FromIATA + flight.ToIATA + flight.FlightNumber.Replace(" ", "") +
                                                $"{flight.FromDate:yyyyMMdd}" + $"{flight.ToDate:yyyyMMdd}" + Convert.ToInt32(flight.FlightMonday) + Convert.ToInt32(flight.FlightTuesday) + Convert.ToInt32(flight.FlightWednesday) + Convert.ToInt32(flight.FlightThursday) + Convert.ToInt32(flight.FlightFriday) + Convert.ToInt32(flight.FlightSaterday) + Convert.ToInt32(flight.FlightSunday));
                            csvtrips.WriteField(flight.FromIATA + flight.ToIATA + flight.FlightNumber.Replace(" ", "") +
                                                $"{flight.FromDate:yyyyMMdd}" + $"{flight.ToDate:yyyyMMdd}" + Convert.ToInt32(flight.FlightMonday) + Convert.ToInt32(flight.FlightTuesday) + Convert.ToInt32(flight.FlightWednesday) + Convert.ToInt32(flight.FlightThursday) + Convert.ToInt32(flight.FlightFriday) + Convert.ToInt32(flight.FlightSaterday) + Convert.ToInt32(flight.FlightSunday));
                            csvtrips.WriteField(ToAirportName);
                            csvtrips.WriteField(flight.FlightNumber);
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("1");
                            csvtrips.WriteField("");
                            csvtrips.NextRecord();

                            // Depart Record
                            csvstoptimes.WriteField(flight.FromIATA + flight.ToIATA + flight.FlightNumber.Replace(" ", "") +
                                                    $"{flight.FromDate:yyyyMMdd}" + $"{flight.ToDate:yyyyMMdd}" + Convert.ToInt32(flight.FlightMonday) + Convert.ToInt32(flight.FlightTuesday) + Convert.ToInt32(flight.FlightWednesday) + Convert.ToInt32(flight.FlightThursday) + Convert.ToInt32(flight.FlightFriday) + Convert.ToInt32(flight.FlightSaterday) + Convert.ToInt32(flight.FlightSunday));
                            csvstoptimes.WriteField(flight.DepartTime + ":00");
                            csvstoptimes.WriteField(flight.DepartTime + ":00");
                            csvstoptimes.WriteField(flight.FromIATA);
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField(ToAirportName);
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("");
                            csvstoptimes.WriteField("");
                            csvstoptimes.NextRecord();
                            // Arrival Record
                            //if(CIFLights[i].DepartTime.TimeOfDay < System.TimeSpan.Parse("23:59:59") && CIFLights[i].ArrivalTime.TimeOfDay > System.TimeSpan.Parse("00:00:00"))
                            if (!flight.FlightNextDayArrival)
                            {
                                csvstoptimes.WriteField(flight.FromIATA + flight.ToIATA + flight.FlightNumber.Replace(" ", "") +
                                                        $"{flight.FromDate:yyyyMMdd}" + $"{flight.ToDate:yyyyMMdd}" + Convert.ToInt32(flight.FlightMonday) + Convert.ToInt32(flight.FlightTuesday) + Convert.ToInt32(flight.FlightWednesday) + Convert.ToInt32(flight.FlightThursday) + Convert.ToInt32(flight.FlightFriday) + Convert.ToInt32(flight.FlightSaterday) + Convert.ToInt32(flight.FlightSunday));
                                csvstoptimes.WriteField(flight.ArrivalTime + ":00");
                                csvstoptimes.WriteField(flight.ArrivalTime + ":00");
                                csvstoptimes.WriteField(flight.ToIATA);
                                csvstoptimes.WriteField("2");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.NextRecord();
                            }
                            else
                            {
                                //add 24 hour for the gtfs time
                                TimeSpan ts = TimeSpan.Parse(flight.ArrivalTime);
                                int hour = ts.Hours;
                                hour = hour + 24;
                                int minute = ts.Minutes;
                                string strminute = minute.ToString();
                                if (strminute.Length == 1) { strminute = "0" + strminute; }
                                csvstoptimes.WriteField(flight.FromIATA + flight.ToIATA + flight.FlightNumber.Replace(" ", "") +
                                                        $"{flight.FromDate:yyyyMMdd}" + $"{flight.ToDate:yyyyMMdd}" + Convert.ToInt32(flight.FlightMonday) + Convert.ToInt32(flight.FlightTuesday) + Convert.ToInt32(flight.FlightWednesday) + Convert.ToInt32(flight.FlightThursday) + Convert.ToInt32(flight.FlightFriday) + Convert.ToInt32(flight.FlightSaterday) + Convert.ToInt32(flight.FlightSunday));
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(flight.ToIATA);
                                csvstoptimes.WriteField("2");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.NextRecord();
                            }
                        }
                    }
                }
            }
            // Create Zip File
            string startPath = gtfsDir;
            string zipPath = DataDir + "\\OneWorld.zip";
            if (File.Exists(zipPath)) { File.Delete(zipPath); }
            ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.Fastest, false);
        }

    }
}
