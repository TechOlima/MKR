using Microsoft.AspNetCore.Mvc;
using MKR.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MKR.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class VKController : ControllerBase
    {
        private readonly ILogger<VKController> _logger;

        public VKController(ILogger<VKController> logger)
        {
            _logger = logger;
        }
        //метод получения токена
        [HttpPost]
        public async Task<ActionResult<GetTokenResponse>> GetToken([FromForm] string code, [FromForm] string client_id = "51653671",
            [FromForm] string client_secret= "F3jxyY0jQj4qdyaUpNHl", [FromForm] string redirect_uri= "https://frontend-v2-20230520121235.azurewebsites.net")
        {
            HttpClient httpClient = new();
            string uri = $"https://oauth.vk.com/access_token?client_id={client_id}&client_secret={client_secret}&redirect_uri={redirect_uri}&code={code}";            
            using HttpResponseMessage response = await httpClient.GetAsync(uri);
            GetTokenResponse token = new GetTokenResponse();
            string request = response.RequestMessage != null ? response.RequestMessage.ToString() : "";
            var jsonResponse = await response.Content.ReadAsStringAsync();
            try
            {
                token = await response.Content.ReadFromJsonAsync<GetTokenResponse>();
                if (token?.access_token != null) {
                    return token;
                } 
                else return BadRequest(new { request = request, response = jsonResponse });
            }
            catch
            {
                return BadRequest(new { request = request, response = jsonResponse });
            }
        }
        //метод получения размещенных товаров в вк
        [HttpPost]
        public async Task<ActionResult<object>> GetMarket([FromForm] string access_token, [FromForm] string owner_id= "-220761511", [FromForm] string version="5.131")
        {
            HttpClient httpClient = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.vk.com/method/market.get");
            
            Dictionary<string, string> data = new Dictionary<string, string>
            {                
                ["owner_id"] = owner_id,
                ["v"] = version
            };
            // создаем объект HttpContent
            HttpContent contentForm = new FormUrlEncodedContent(data);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            request.Content = contentForm;            

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            string requestStr = response.RequestMessage.ToString();
            var jsonResponse = await response.Content.ReadAsStringAsync();                      

            return Ok(new { request = requestStr, response = jsonResponse});
        }
        //метод получения адреса для загрузки фото
        [HttpPost]
        public async Task<ActionResult<GetUploadServerResponse>> GetUploadServer([FromForm] string access_token, [FromForm] string group_id = "220761511",
            [FromForm] string version = "5.131")
        {
            HttpClient httpClient = new HttpClient();

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.vk.com/method/photos.getMarketUploadServer");
                        
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["group_id"] = group_id,
                ["v"] = version
            };
            // создаем объект HttpContent
            HttpContent contentForm = new FormUrlEncodedContent(data);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            request.Content = contentForm;            

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            string requestStr = response.RequestMessage.ToString();

            GetUploadServerResponse vKResponse = await response.Content.ReadFromJsonAsync<GetUploadServerResponse>();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (vKResponse.response.upload_url != null) return Ok(vKResponse);
            else return BadRequest(new { request = requestStr, response = jsonResponse });
        }
        //метод загрузки фотографий на сервер
        [HttpPost]
        public async Task<ActionResult<UploadImageToServerResponse>> UploadImageToServer([FromForm] string access_token, [FromForm] string upload_url, [FromForm] string photo_url)
        {
            HttpClient httpClient = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, upload_url);

            using var multipartFormContent = new MultipartFormDataContent(); 

            Stream myStream = await httpClient.GetStreamAsync(new Uri(photo_url));            

            var fileStreamContent = new StreamContent(myStream);

            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            multipartFormContent.Add(fileStreamContent, name: "file", fileName: "forest.jpg");

            //HttpContent contentForm = new FormUrlEncodedContent(data);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            request.Content = multipartFormContent;

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            //создаем поток чтобы преобразовать результат
            Stream stream = response.Content.ReadAsStream();
            StreamReader reader = new StreamReader(stream);
            var textResponse = await reader.ReadToEndAsync();

            //получаем текст запроса
            string requestStr = response.RequestMessage.ToString();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                StringContent strcontent = new StringContent(textResponse);

                //получаем результат в виде объекта
                UploadImageToServerResponse? uploadResult = new UploadImageToServerResponse();
                uploadResult = await strcontent.ReadFromJsonAsync<UploadImageToServerResponse>();
                return uploadResult;
            }
            else return BadRequest(new { request = requestStr, response = textResponse });            
        }
        //метод сохранения фотографии на сервере
        [HttpPost]
        public async Task<ActionResult<SaveImageToServerResponse>> SaveImageToServer([FromForm] string access_token, [FromForm] string photo, [FromForm] string hash,
            [FromForm] string group_id = "220761511", [FromForm] string server= "516636", [FromForm] string version = "5.131")
        {
            HttpClient httpClient = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.vk.com/method/photos.saveMarketPhoto");

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["group_id"] = group_id,
                ["v"] = version,
                ["server"]= server,
                ["photo"]= Regex.Unescape(photo),
                ["hash"]= hash
            };
            // создаем объект HttpContent
            HttpContent contentForm = new FormUrlEncodedContent(data);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            request.Content = contentForm;

            using HttpResponseMessage response = await httpClient.SendAsync(request);
            
            //получаем текст запроса
            string requestStr = response.RequestMessage.ToString();
            string textResponse = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                SaveImageToServerResponse? result = new SaveImageToServerResponse();
                result = await response.Content.ReadFromJsonAsync<SaveImageToServerResponse>();
                if (result != null && result.response.Count() > 0) return result;
                else return Ok(new { request = requestStr, response = textResponse });
            }
            else return BadRequest(new { request = requestStr, response = textResponse });
        }
        //метод добавления товара
        [HttpPost]
        public async Task<ActionResult<MarketAddResponse>> MarketAdd([FromForm] string access_token, 
            [FromForm] string name, [FromForm] string description,
            [FromForm] string? price, [FromForm] string main_photo_id,
            [FromForm] string? photo_ids, 
            [FromForm] string category_id = "5005", [FromForm] string owner_id= "-220761511", [FromForm] string version = "5.131")
        {
            HttpClient httpClient = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.vk.com/method/market.add");

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["owner_id"] = owner_id,
                ["name"] = name,
                ["description"] = description,
                ["category_id"] = category_id,
                ["price"] = price,
                ["main_photo_id"] = main_photo_id,
                ["photo_ids"] = photo_ids,
                ["v"] = version
            };
            // создаем объект HttpContent
            HttpContent contentForm = new FormUrlEncodedContent(data);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            request.Content = contentForm;

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            //получаем текст запроса
            string requestStr = response.RequestMessage.ToString();
            string textResponse = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                MarketAddResponse? result = new MarketAddResponse();
                result = await response.Content.ReadFromJsonAsync<MarketAddResponse>();
                if (result != null && result.response.market_item_id > 0) return result;
                else return Ok(new { request = requestStr, response = textResponse });
            }
            else return BadRequest(new { request = requestStr, response = textResponse });
        }

        [HttpPost]
        public async Task<ActionResult<MarketEditResponse>> MarketEdit(
            [FromForm] string access_token, [FromForm] string name, [FromForm] string description, 
            [FromForm] string? price, [FromForm] string main_photo_id, 
            [FromForm] string? photo_ids, [FromForm] string item_id, [FromForm] string category_id = "5005",
            [FromForm] string owner_id = "-220761511", [FromForm] string version = "5.131")
        {
            HttpClient httpClient = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.vk.com/method/market.edit");

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["owner_id"] = owner_id,
                ["name"] = name,
                ["item_id"] = item_id,
                ["description"] = description,
                ["category_id"] = category_id,
                ["price"] = price,
                ["main_photo_id"] = main_photo_id,
                ["photo_ids"] = photo_ids,
                ["v"] = version
            };
            // создаем объект HttpContent
            HttpContent contentForm = new FormUrlEncodedContent(data);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            request.Content = contentForm;

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            //получаем текст запроса
            string requestStr = response.RequestMessage.ToString();
            string textResponse = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                MarketEditResponse? result = new MarketEditResponse();
                result = await response.Content.ReadFromJsonAsync<MarketEditResponse>();
                if (result != null && result.response > 0) return result;
                else return Ok(new { request = requestStr, response = textResponse });
            }
            else return BadRequest(new { request = requestStr, response = textResponse });
        }
    }
}
