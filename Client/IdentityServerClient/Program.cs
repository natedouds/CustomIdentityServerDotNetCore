using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace IdentityServerClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DoStuff();
            Console.ReadLine();
        }

        private static async Task DoStuff()
        {
            //discover endpoints from metadata
            var disco = await GetEndpoints();

            //request token
            var tokenResponse = await RequestTokenClientCredential(disco);

            //call api with client credential
            CallApi(tokenResponse.AccessToken);

            //request token
            var tokenResponse2 = await RequestResourceOwnerPassword(disco);

            //call api with resource owner password
            CallApi(tokenResponse2.AccessToken);

        }

        private static async void CallApi(string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.SetBearerToken(accessToken);
                var response = await client.GetAsync("http://localhost:5001/identity");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(JArray.Parse(content));
                }
            }
        }

        private static async Task<DiscoveryResponse> GetEndpoints()
        {
            return await DiscoveryClient.GetAsync("http://localhost:5000");
        }

        private static async Task<TokenResponse> RequestResourceOwnerPassword(DiscoveryResponse disco)
        {
            var tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "api1");

            return ProcessResponse(tokenResponse);
        }

        private static async Task<TokenResponse> RequestTokenClientCredential(DiscoveryResponse disco)
        {
            var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            return ProcessResponse(tokenResponse);
        }

        private static TokenResponse ProcessResponse(TokenResponse tokenResponse)
        {
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
            }

            Console.WriteLine(tokenResponse.Json);
            return !tokenResponse.IsError ? tokenResponse : null;
        }
    }
}
