using RestSharp;
using Sha256Hashser;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


const string clientId = "";
const string clientSecret = "";

var authResponse = await PerformAuth(clientId, clientSecret);

//expires in 3600 seconds by default so need to handle renewal of token
var accessToken = authResponse?.access_token;


string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input.xml");

string fileContent = File.ReadAllText(filePath);

var hash = ComputeHash(fileContent);


Console.WriteLine("--------------------------------------------------------------");

Console.WriteLine(hash);

Console.WriteLine("-----------------------------------------------------------------------");


var base64FileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

Console.WriteLine("--------------------------------------------------------------");

Console.WriteLine(base64FileContent);

Console.WriteLine("-----------------------------------------------------------------------");


var submissionResponse = await SubmitInvoice(hash, base64FileContent, accessToken);
Console.WriteLine(submissionResponse.Content);


Console.ReadKey();
return;

async Task<AuthResponse?> PerformAuth(string clientId, string clientSecret)
{

    var client = new RestClient("https://preprod-api.myinvois.hasil.gov.my");
    var request = new RestRequest("/connect/token", Method.Post);
    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
    request.AddParameter("client_id", clientId);
    request.AddParameter("client_secret", clientSecret);
    request.AddParameter("grant_type", "client_credentials");
    request.AddParameter("scope", "InvoicingAPI");
    var response = await client.ExecuteAsync(request);

    Console.WriteLine(response.Content);
    var authResponse1 = JsonSerializer.Deserialize<AuthResponse>(response.Content);
    return authResponse1;
}

async Task<RestResponse> SubmitInvoice(string s, string base64FileContent1, string accessToken1)
{
    var body = new
    {
        documents = new[]
        {
            new
            {
                format = "XML",
                documentHash = s,
                codeNumber = "INV12345",
                document = base64FileContent1
            }
        }
    };

    var client = new RestClient("https://preprod-api.myinvois.hasil.gov.my");
    var request = new RestRequest("/api/v1.0/documentsubmissions", Method.Post);
    request.AddHeader("Content-Type", "application/json");
    request.AddHeader("Authorization", $"Bearer {accessToken1}");

    request.AddStringBody(Newtonsoft.Json.JsonConvert.SerializeObject(body), DataFormat.Json);
    RestResponse restResponse = await client.ExecuteAsync(request);
    return restResponse;
}

string ComputeHash(string fileContent1)
{
    var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(fileContent1));
    var sb = new StringBuilder();
    foreach (var b in hashBytes)
    {
        sb.Append(b.ToString("x2")); // Convert each byte to a 2-digit hexadecimal string
    }
    return sb.ToString();
}