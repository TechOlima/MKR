using Microsoft.AspNetCore.Mvc;
using Dadata;
using Dadata.Model;
using MKR.Models;

namespace MKR.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DadataAdress : ControllerBase
    { 
        private readonly ILogger<DadataAdress> _logger;

        public DadataAdress(ILogger<DadataAdress> logger)
        {
            _logger = logger;
        }        

        [HttpGet]
        public async Task<DadataResult> ClearAddress(string adress)
        {
            var token = "03fab14d0555d6f7a9ea39754dbdc4771b4c3a8a";
            var secret = "234c0ae331f316d156103bd279573ae5aab3fcd1";

            var api = new CleanClientAsync(token, secret);
            Dadata.Model.Address dadataaddress = await api.Clean<Address>(adress);

            //мск сухонская 11 89

            DadataResult dadataResult = new DadataResult();
            dadataResult.Result = dadataaddress.result;
            dadataResult.CityWithType = dadataaddress.city_with_type;
            dadataResult.CityDistrictWithType = dadataaddress.city_district_with_type;
            dadataResult.SettlementWithType = dadataaddress.settlement_with_type;
            dadataResult.StreetWithType = dadataaddress.street_with_type;
            dadataResult.House = dadataaddress.house;
            dadataResult.Block = dadataaddress.block;
            dadataResult.Entrance = dadataaddress.entrance;
            dadataResult.Floor = dadataaddress.floor;
            dadataResult.Flat = dadataaddress.flat;
            dadataResult.QC = dadataaddress.qc;

            return dadataResult;
        }       
    
    }
}
