namespace MKR.Models
{
    public class DadataResult
    {
        public string Result { get; set; }
        public string CityWithType { get; set; }
        public string CityDistrictWithType	{ get; set; }
        public string SettlementWithType	{get; set; }
        public string StreetWithType	{ get; set; }
        public string House	{ get; set; }
        public string Block { get; set; }
        public string Entrance	{ get; set; }
        public string Floor { get; set; }  
        public string Flat { get; set; }
        public string QC { get; set; }
        public string QCDescription { get {
                string result = "";

                switch (QC)
                {
                    case "0":
                        result = "Адрес распознан уверенно";
                        break;
                    case "1":
                        result = "Адрес пустой или заведомо «мусорный»";
                        break;
                    case "2":
                        result = "Остались «лишние» части";
                        break;
                    case "3":
                        result = "Есть альтернативные варианты";
                        break;
                }
                return result;
            } 
        }
    }
}
