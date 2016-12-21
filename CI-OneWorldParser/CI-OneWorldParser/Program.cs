using System;
using System.Collections.Generic;
//using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.IO.Compression;
using CsvHelper;
using Newtonsoft.Json.Linq;

namespace CI_OneWorldParser
{
    public class Program
    {
        public static readonly List<string> _ColombiaAirports = new List<string>() { "APO", "AUC", "AXM", "BSC", "EJA", "BAQ", "BOG", "BGA", "BUN", "CLO", "CTG", "CRC", "CZU", "CUC", "EYP", "FLA", "GIR", "GPI", "IBE", "LET", "MZL", "MQU", "EOH", "MDE", "MVP", "MTR", "NVA", "PSO", "PEI", "PPN", "PVA", "PUU", "PCR", "UIB", "RCH", "ADZ", "SJE", "SVI", "SMR", "RVE", "TME", "TLU", "TCO", "VUP", "VVC", "ACD", "AFI", "ACR", "ARQ", "NBB", "CPB", "CCO", "CUO", "CAQ", "CPL", "IGO", "CIM", "COG", "RAV", "BHF", "EBG", "ELB", "ECR", "LGT", "HTZ", "IPI", "JUO", "LMC", "LPD", "LPE", "MGN", "MCJ", "MFS", "MMP", "MTB", "NCI", "NQU", "OCV", "ORC", "RON", "PZA", "PTX", "PLT", "PBE", "PDA", "LQM", "NAR", "OTU", "SNT", "AYG", "SSL", "SOX", "TTM", "TCD", "TIB", "TBD", "TDA", "TRB", "URI", "URR", "VGZ", "LCR", "SQE", "SRS", "ULQ", "CVE", "PAL", "PYA", "TQS", "API" };

        public class Airport
        {
            public string STACODE { get; set; }
            public string CNTRYCODE { get; set; }
            public string REGIONCODE { get; set; }
        }
        [Serializable]
        public class CIFLight
        {
            // Auto-implemented properties. 

            public string FromIATA;
            public string FromIATACountry;
            public string FromIATARegion;
            public string FromIATATerminal;
            public string ToIATA;
            public string ToIATACountry;
            public string ToIATARegion;
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

        public class IATAAirport
        {
            public string stop_id;
            public string stop_name;
            public string stop_desc;
            public string stop_lat;
            public string stop_lon;
            public string zone_id;
            public string stop_url;
        }


        public class IATAAirline
        {
            public string Airline_Name;
            public string Airline_IATA;            
        }

        public class AirlinesDef
        {
            // Auto-implemented properties.  
            public string Name { get; set; }
            public string IATA { get; set; }
            public string DisplayName { get; set; }
            public string WebsiteUrl { get; set; }
        }

        public static List<AirlinesDef> _Airlines = new List<AirlinesDef>
        {
            new AirlinesDef { IATA = "DA", Name="AEROLINEA DE ANTIOQUIA S.A.", DisplayName="ADA",WebsiteUrl="https://www.ada-aero.com/" },
            new AirlinesDef { IATA = "EF", Name="EASYFLY S.A", DisplayName="Easyfly",WebsiteUrl="http://www.easyfly.com.co" },
            new AirlinesDef { IATA = "2K", Name="AEROGAL", DisplayName="Avianca Ecuador",WebsiteUrl="http://www.avianca.com" },
            new AirlinesDef { IATA = "9H", Name="DUTCH ANTILLES EXPRESS SUCURSAL COLOMBIA", DisplayName="Dutch Antilles Express",WebsiteUrl="https://nl.wikipedia.org/wiki/Dutch_Antilles_Express" },
            new AirlinesDef { IATA = "AR", Name="AEROLINEAS ARGENTINAS", DisplayName="Aerolíneas Argentinas",WebsiteUrl="http://www.aerolineas.com.ar/" },
            new AirlinesDef { IATA = "AM", Name="AEROMEXICO SUCURSAL COLOMBIA", DisplayName="Aeroméxico",WebsiteUrl="http://www.aeromexico.com/" },
            new AirlinesDef { IATA = "P5", Name="AEROREPUBLICA", DisplayName="Copa Airlines",WebsiteUrl="http://www.copa.com" },
            new AirlinesDef { IATA = "AC", Name="Air Canada", DisplayName="Air Canada",WebsiteUrl="http://www.aircanada.com" },
            new AirlinesDef { IATA = "AF", Name="AIR FRANCE", DisplayName="Air France",WebsiteUrl="http://www.airfrance.com" },
            new AirlinesDef { IATA = "4C", Name="AIRES", DisplayName="LATAM Colombia",WebsiteUrl="http://www.latam.com/" },
            new AirlinesDef { IATA = "AA", Name="AMERICAN", DisplayName="American Airlines",WebsiteUrl="http://www.aa.com" },
            new AirlinesDef { IATA = "AV", Name="Avianca", DisplayName="Avianca",WebsiteUrl="http://www.avianca.com" },
            new AirlinesDef { IATA = "V0", Name="CONVIASA", DisplayName="Conviasa",WebsiteUrl="http://www.conviasa.aero/" },
            new AirlinesDef { IATA = "CM", Name="COPA", DisplayName="Copa Airlines",WebsiteUrl="http://www.copaair.com/" },
            new AirlinesDef { IATA = "CU", Name="CUBANA", DisplayName="Cubana de Aviación",WebsiteUrl="http://www.cubana.cu/home/?lang=en" },
            new AirlinesDef { IATA = "DL", Name="DELTA", DisplayName="Delta",WebsiteUrl="http://www.delta.com" },
            new AirlinesDef { IATA = "4O", Name="INTERJET", DisplayName="Interjet",WebsiteUrl="http://www.interjet.com/" },
            new AirlinesDef { IATA = "5Z", Name="FAST COLOMBIA SAS", DisplayName="ViVaColombia",WebsiteUrl="http://www.vivacolombia.co/" },
            new AirlinesDef { IATA = "IB", Name="IBERIA", DisplayName="Iberia",WebsiteUrl="http://www.iberia.com" },
            new AirlinesDef { IATA = "B6", Name="JETBLUE AIRWAYS CORPORATION", DisplayName="Jetblue",WebsiteUrl="http://www.jetblue.com" },
            new AirlinesDef { IATA = "LR", Name="LACSA", DisplayName="Avianca Costa Rica",WebsiteUrl="http://www.avianca.com" },
            new AirlinesDef { IATA = "LA", Name="LAN AIRLINES S.A.", DisplayName="LAN Airlines",WebsiteUrl="http://www.lan.com/" },
            new AirlinesDef { IATA = "LP", Name="LAN PERU", DisplayName="LAN Airlines",WebsiteUrl="http://www.lan.com/" },
            new AirlinesDef { IATA = "LH", Name="Lufthansa", DisplayName="Lufthansa",WebsiteUrl="http://www.lufthansa.com" },
            new AirlinesDef { IATA = "9R", Name="SERVICIO AEREO A TERRITORIOS NACIONALES SATENA", DisplayName="Satena",WebsiteUrl="http://www.satena.com/" },
            new AirlinesDef { IATA = "NK", Name="SPIRIT AIRLINES", DisplayName="Spirit",WebsiteUrl="http://www.spirit.com" },
            new AirlinesDef { IATA = "TA", Name="TACA INTERNATIONAL", DisplayName="TACA Airlines",WebsiteUrl="http://www.taca.com/" },
            new AirlinesDef { IATA = "EQ", Name="TAME", DisplayName="TAME",WebsiteUrl="http://www.tame.com.ec/" },
            new AirlinesDef { IATA = "3P", Name="TIARA", DisplayName="Tiara Air Aruba",WebsiteUrl="http://www.tiara-air.com/" },
            new AirlinesDef { IATA = "T0", Name="TRANS AMERICAN AIR LINES S.A. SUCURSAL COL.", DisplayName="Trans American Airlines",WebsiteUrl="http://www.avianca.com/" },
            new AirlinesDef { IATA = "UA", Name="United Airlines", DisplayName="United",WebsiteUrl="http://www.united.com" },
            new AirlinesDef { IATA = "4C", Name="LATAM AIRLINES GROUP S.A SUCURSAL COLOMBIA", DisplayName="LATAM",WebsiteUrl="http://www.latam.com/" },
            new AirlinesDef { IATA = "TP", Name="TAP PORTUGAL SUCURSAL COLOMBIA", DisplayName="TAP",WebsiteUrl="http://www.flytap.com" },
            new AirlinesDef { IATA = "7P", Name="AIR PANAMA", DisplayName="Air Panama",WebsiteUrl="http://www.airpanama.com/" },
            new AirlinesDef { IATA = "O6", Name="OCEANAIR", DisplayName="Avianca Brazil",WebsiteUrl="http://www.avianca.com" },
            new AirlinesDef { IATA = "8I", Name="INSELAIR ARUBA", DisplayName="Insel Air Aruba",WebsiteUrl="http://www.fly-inselair.com/"},
            new AirlinesDef { IATA = "7I", Name="INSEL AIR", DisplayName="Insel Air",WebsiteUrl="http://www.fly-inselair.com/"},
            new AirlinesDef { IATA = "TK", Name="Turkish Airlines", DisplayName="Turkish Airlines",WebsiteUrl="http://www.turkishairlines.com"},
            new AirlinesDef { IATA = "UX", Name="AIR EUROPA", DisplayName="Air Europe",WebsiteUrl="http://www.aireurope.com"},
            new AirlinesDef { IATA = "9V", Name="AVIOR AIRLINES,C.A.", DisplayName="Avior Airlines",WebsiteUrl="http://www.avior.com.ve/"},
            new AirlinesDef { IATA = "KL", Name="KLM", DisplayName="KLM",WebsiteUrl="http://www.klm.nl"},
            new AirlinesDef { IATA = "JJ", Name="TAM", DisplayName="TAM Linhas Aéreas",WebsiteUrl="http://www.latam.com/"},
            new AirlinesDef { IATA = "BA", Name="BA", DisplayName="Britsch Airways",WebsiteUrl="http://www.ba.com/"}
        };

        static void Main(string[] args)
        {
            string DataDir = AppDomain.CurrentDomain.BaseDirectory + "\\data";
            System.IO.Directory.CreateDirectory(DataDir);
            string sqldb = Path.Combine(DataDir, "cm.sqlite");
            List<CIFLight> CIFLights = new List<CIFLight> { };
            List<Airport> Airports = new List<Airport> { };
            Console.WriteLine("Requesting Latest update...");
            CultureInfo ci = new CultureInfo("en-US");
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

            string sqlairports = "select STACODE, CNTRYCODE,REGIONCODE from STATION_GEO_XREF;";

            SQLiteCommand commandairport = new SQLiteCommand(sqlairports, m_dbConnection);
            SQLiteDataReader readerairport = commandairport.ExecuteReader();
            while (readerairport.Read())
            {
                var Airport = new Airport();
                Airport.STACODE = readerairport["STACODE"].ToString();
                Airport.CNTRYCODE = readerairport["CNTRYCODE"].ToString();
                Airport.REGIONCODE = readerairport["REGIONCODE"].ToString();
                Airports.Add(Airport);
            }
            readerairport.Close();
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

                // Check Region & Country
                var FromAirportInfo = Airports.Find(q => q.STACODE == TEMP_FromIATA);
                var ToAirportInfo = Airports.Find(q => q.STACODE == TEMP_ToIATA);
                if (FromAirportInfo != null)
                {
                    TEMP_FromIATARegion = FromAirportInfo.REGIONCODE;
                    TEMP_FromIATACountry = FromAirportInfo.CNTRYCODE;
                }
                if (ToAirportInfo != null)
                {
                    TEMP_ToIATACountry = ToAirportInfo.CNTRYCODE;
                    TEMP_ToIATARegion = ToAirportInfo.REGIONCODE;
                }
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
                    var ViaAirportInfo = Airports.Find(q => q.STACODE == TEMP_FlightVia);
                }
                TEMP_Airline = readerflights["CARRIER"].ToString();
                TEMP_FlightNextDays = int.Parse(readerflights["EXTRA_DAY"].ToString());
                TEMP_DurationTime = TimeSpan.FromMinutes(double.Parse(readerflights["FLIGHTDURATION"].ToString()));

                if (_ColombiaAirports.Contains(TEMP_FromIATA, StringComparer.OrdinalIgnoreCase) & !_ColombiaAirports.Contains(TEMP_ToIATA, StringComparer.OrdinalIgnoreCase) || (_ColombiaAirports.Contains(TEMP_ToIATA, StringComparer.OrdinalIgnoreCase) & !_ColombiaAirports.Contains(TEMP_FromIATA, StringComparer.OrdinalIgnoreCase)) || (_ColombiaAirports.Contains(TEMP_ToIATA, StringComparer.OrdinalIgnoreCase) & _ColombiaAirports.Contains(TEMP_FromIATA, StringComparer.OrdinalIgnoreCase)))
                {

                    CIFLights.Add(new CIFLight
                    {
                        FromIATA = TEMP_FromIATA,
                        FromIATARegion = TEMP_FromIATARegion,
                        FromIATACountry = TEMP_FromIATACountry,
                        FromIATATerminal = TEMP_FromIATATerminal,
                        ToIATA = TEMP_ToIATA,
                        ToIATACountry = TEMP_ToIATACountry,
                        ToIATARegion = TEMP_ToIATARegion,
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
            readerairport.Close(); 
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

            Console.WriteLine("Reading IATA Airports....");

            //StreamReader fileairports = new StreamReader(@"airports.csv");
            //var csvairport = new CsvReader(fileairports);
            //csvairport.Configuration.Delimiter = ",";
            //csvairport.Configuration.Encoding = Encoding.UTF8;
            ////var generatedMap = csvairport.Configuration.AutoMap<IATAAirport>();
            ////csv.Configuration.RegisterClassMap<IATAAirport>();
            //var IATAAirports = csvairport.GetRecords<IATAAirport>().ToList();




            string IATAAirportsFile = AppDomain.CurrentDomain.BaseDirectory + "IATAAirports.json";
            JArray o1 = JArray.Parse(File.ReadAllText(IATAAirportsFile));
            IList<IATAAirport> TempIATAAirports = o1.ToObject<IList<IATAAirport>>();
            var IATAAirports = TempIATAAirports as List<IATAAirport>;

            //Console.WriteLine("Reading IATA Airlines....");
            //string IATAAirlineFile = AppDomain.CurrentDomain.BaseDirectory + "IATAAirlines.json";
            //JArray o2 = JArray.Parse(File.ReadAllText(IATAAirlineFile));
            //IList<IATAAirline> TempIATAAirline = o2.ToObject<IList<IATAAirline>>();
            //var IATAAirlines = TempIATAAirline as List<IATAAirline>;
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

                for (int i = 0; i < airlines.Count; i++) // Loop through List with for)
                {
                    string temp_airline = airlines[i].FlightAirline;
                    var item4 = _Airlines.Find(q => q.IATA == airlines[i].FlightAirline);
                    string TEMP_Name = item4.DisplayName;
                    string TEMP_Url = item4.WebsiteUrl;
                    string TEMP_IATA = item4.IATA;
                    csv.WriteField(TEMP_IATA);
                    csv.WriteField(TEMP_Name);
                    csv.WriteField(TEMP_Url);
                    csv.WriteField("America/Bogota");
                    csv.WriteField("ES");
                    csv.WriteField("");
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

                for (int i = 0; i < routes.Count; i++) // Loop through List with for)
                {
                    //var item4 = _Airlines.Find(q => q.Name == routes[i].FlightAirline);
                    //string TEMP_Name = item4.DisplayName;
                    //string TEMP_Url = item4.WebsiteUrl;
                    //string TEMP_IATA = item4.IATA;

                    var FromAirportInfo = IATAAirports.Find(q => q.stop_id == routes[i].FromIATA);
                    var ToAirportInfo = IATAAirports.Find(q => q.stop_id == routes[i].ToIATA);
                    // Info used to determine if its a domestic, international, or intercontinental flight
                    var FromAirportInfo2 = Airports.Find(q => q.STACODE == routes[i].FromIATA);
                    var ToAirportInfo2 = Airports.Find(q => q.STACODE == routes[i].ToIATA);

                    csvroutes.WriteField(routes[i].FromIATA + routes[i].ToIATA + routes[i].FlightAirline);
                    csvroutes.WriteField(routes[i].FlightAirline);
                    csvroutes.WriteField("");
                    if (FromAirportInfo != null & ToAirportInfo != null)
                    {
                        csvroutes.WriteField(FromAirportInfo.stop_name + " - " + ToAirportInfo.stop_name);
                    }
                    else
                    {
                        csvroutes.WriteField(routes[i].FromIATA + routes[i].ToIATA + routes[i].FlightAirline);
                    }                    
                    csvroutes.WriteField(""); // routes[i].FlightAircraft + ";" + CIFLights[i].FlightAirline + ";" + CIFLights[i].FlightOperator + ";" + CIFLights[i].FlightCodeShare
                    if (FromAirportInfo2 != null & ToAirportInfo2 != null)
                    {
                        if (FromAirportInfo2.CNTRYCODE == ToAirportInfo2.CNTRYCODE)
                        {
                            // Colombian internal flight domestic
                            csvroutes.WriteField(1102);
                        }
                        else
                        {
                            if (FromAirportInfo2.REGIONCODE == ToAirportInfo2.REGIONCODE)
                            {
                                // International Flight
                                csvroutes.WriteField(1101);
                            }
                            else
                            {
                                // Intercontinental Flight
                                csvroutes.WriteField(1103);
                            }
                        }                        
                    }
                    else
                    {
                        csvroutes.WriteField(1102);
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

                for (int i = 0; i < agencyairportsiata.Count; i++) // Loop through List with for)
                {
                    //int result1 = IATAAirports.FindIndex(T => T.stop_id == 9458)
                    var airportinfo = IATAAirports.Find(q => q.stop_id == agencyairportsiata[i]);
                    csvstops.WriteField(airportinfo.stop_id);
                    csvstops.WriteField(airportinfo.stop_name);
                    csvstops.WriteField(airportinfo.stop_desc);
                    csvstops.WriteField(airportinfo.stop_lat);
                    csvstops.WriteField(airportinfo.stop_lon);
                    csvstops.WriteField(airportinfo.zone_id);
                    csvstops.WriteField(airportinfo.stop_url);
                    csvstops.NextRecord();
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
                                
                        for (int i = 0; i < CIFLights.Count; i++) // Loop through List with for)
                        {
                                    
                            // Calender

                            csvcalendar.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightMonday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightTuesday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightWednesday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightThursday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightFriday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightSaterday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvcalendar.WriteField(String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate));
                            csvcalendar.WriteField(String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                            csvcalendar.NextRecord();

                            // Trips

                            //var item4 = _Airlines.Find(q => q.Name == CIFLights[i].FlightAirline);
                            //string TEMP_IATA = item4.IATA;
                            var FromAirportInfo = IATAAirports.Find(q => q.stop_id == CIFLights[i].FromIATA);
                            var ToAirportInfo = IATAAirports.Find(q => q.stop_id == CIFLights[i].ToIATA);

                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline);
                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvtrips.WriteField(ToAirportInfo.stop_name);
                            csvtrips.WriteField(CIFLights[i].FlightNumber);
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("1");
                            csvtrips.WriteField("");
                            csvtrips.NextRecord();

                            // Depart Record
                            csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvstoptimes.WriteField(CIFLights[i].DepartTime + ":00");
                            csvstoptimes.WriteField(CIFLights[i].DepartTime + ":00");
                            csvstoptimes.WriteField(CIFLights[i].FromIATA);
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField(ToAirportInfo.stop_name);
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("");
                            csvstoptimes.WriteField("");
                            csvstoptimes.NextRecord();
                            // Arrival Record
                            //if(CIFLights[i].DepartTime.TimeOfDay < System.TimeSpan.Parse("23:59:59") && CIFLights[i].ArrivalTime.TimeOfDay > System.TimeSpan.Parse("00:00:00"))
                            if (!CIFLights[i].FlightNextDayArrival)
                            {
                                csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                                csvstoptimes.WriteField(CIFLights[i].ArrivalTime + ":00");
                                csvstoptimes.WriteField(CIFLights[i].ArrivalTime + ":00");
                                csvstoptimes.WriteField(CIFLights[i].ToIATA);
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
                                TimeSpan ts = TimeSpan.Parse(CIFLights[i].ArrivalTime);
                                int hour = ts.Hours;
                                hour = hour + 24;
                                int minute = ts.Minutes;
                                string strminute = minute.ToString();
                                if (strminute.Length == 1) { strminute = "0" + strminute; }
                                csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(CIFLights[i].ToIATA);
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
