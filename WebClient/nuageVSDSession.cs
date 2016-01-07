using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace nuageVSDClient
{
    class nuageVSDSession
    {
        private string username { get; set; }
        private string password { get; set; }
        private string enterprise { get; set; }
        private Uri baseUrl { get; set; }
        private NuageMe[] mes { get; set; }

        public nuageVSDSession(string username, string password, string enterprise, Uri baseUrl)
        {
            this.username = username;
            this.password = password;
            this.enterprise = enterprise;
            this.baseUrl = baseUrl;
 
        }

        public void start()
        {
            try
            {
                RunAsync().Wait();
            }
            catch (AggregateException e)
            {
                foreach (Exception ex in e.InnerExceptions)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

        }

        public async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                string authKey = nuageBase64.Base64Encode(username + ":" + password);

                client.BaseAddress = baseUrl;

                // Add the http header
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("X-Nuage-Organization", "csp");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Add("Authorization", "XREST " + authKey);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Config SSL Certificate Validation
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateServerCertificate);

                // List data response
                HttpResponseMessage reponse = await client.GetAsync("nuage/api/v3_2/me"); //Blocking call
                if (reponse.IsSuccessStatusCode)
                {
                    //byte[] result = await reponse.Content.ReadAsByteArrayAsync();
                    //Console.WriteLine("{0}", System.Text.Encoding.ASCII.GetString(result,0,result.Length));
                    this.mes = await reponse.Content.ReadAsAsync<NuageMe[]>();
                }

                foreach (var item in mes)
                {
                    Console.WriteLine("ID: {0}, APIKey :{1}", item.ID, item.APIKey);
                }

            }
        }

        //for testing purpose only, accept any dodgy certificate... 
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }
}
