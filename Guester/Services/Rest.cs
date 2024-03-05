
using Newtonsoft.Json;
using System.Text;


namespace Guester.Services
{
    public  class RestService
    {

        //private static string RestUrl= "http://test.4dev.kz:8889/guester_odata/odata/Validates";

        //private static string RestUrl= "https://guester.gosu.kz/odata/Validates";
        //private static string RestUrl = "https://gtest.gosu.kz/odata/Validates";
      //  private static string RestUrl= "http://test.4dev.kz:8889/guester_odata/odata/Validates";
        private static string RestUrl= "https://gtest.gosu.kz/odata/Validates";


        public static async Task<Dictionary<string,string>> LoginAsync(string name, string password) // DevBrand.arte,@gmail.com
        {
            var result = new  Dictionary<string, string>();
            try
            {
            
                //var user = new Dictionary<string, string>()
                //{
                //    {"name",name },
                //    {"password",password},
                //};
                //var request = JsonConvert.SerializeObject(user);


                var client = GetClient();
                var response = await client.PostAsync(string.Format(RestUrl+ $"?Login={name}&Password={password}"), new StringContent("", Encoding.Default, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var responseData= await response.Content.ReadAsStringAsync();
                    //result = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<Dictionary<string,string>>(responseData);
                  //  result = responseData.Replace("\"", "");
                }
                else
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return null;
                }
            }
            catch (Exception ex)
            {
                // TODO Нужно сделать обработку на плохой интернетт и выводить нормальные алёрты
                Console.WriteLine($"Ошибка при авторизации устройства : {ex.Message} time : {DateTime.Now}");
                return null;
            }
            return result;
        }




  

        private static HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var token = Preferences.Get("token", $"");
            var token_type = Preferences.Get("token_type", $"bearer");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"{token_type} {token}");
            }

            return client;
        }
    }
}
