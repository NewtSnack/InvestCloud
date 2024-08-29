using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace InvestCloud
{
    internal class InvestCloud
    {                
        HttpClient client = new HttpClient();
        private int size;
        public int[,] Alpha;
        public int[,] Beta;
        private DateTime timeStart;
        private DateTime timeEnd;
        string hashedSolution { get; set; } = null;
        HttpStatusCode IsSuccess { get; set; }
        public InvestCloud(int size)
        {
            this.size = size;
            Alpha = new int[size, size];
            Beta = new int[size, size];
        }
        public async Task MultiplyMatrix()
        {
            timeStart = DateTime.Now;
            client.BaseAddress = new Uri("https://recruitment-test.investcloud.com/");
            string url = $"api/numbers/init/{size}";

            string result = await InitializeMatrix(url); //API GET to init the two arrays
            BuildMatrix(); //build two matrixes in memory O(n^2)
            MatrixMulitiplicationHash(Alpha, Beta); //0(n^3) and Md5 hash
            await POSTSolution(hashedSolution); //API POST to validate
            timeEnd = DateTime.Now;
            await Console.Out.WriteLineAsync($"{IsSuccess} Response from API with a runtime of {Math.Round((timeEnd - timeStart).TotalMinutes, 2)} minutes.");
            client.Dispose();
        }

        private async Task POSTSolution(string hashedSolution)
        {
            try
            {
                var content = new StringContent(hashedSolution, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/numbers/validate", content);
                response.EnsureSuccessStatusCode();
                IsSuccess = response.StatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<string> InitializeMatrix(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        public void BuildMatrix()
        {
            for (int i = 0; i < size; i++)
            {
                var MatrixLineAlpha = GetElement("A", "row", i).Result;
                var MatrixLineBeta = GetElement("B", "row", i).Result;
                if (i % 10 == 0)
                {
                    Console.WriteLine($" {Math.Round((decimal)i / size * 100, 2)} % complete");
                }
                for (int j = 0; j < size; j++)
                {
                    Alpha[i, j] = MatrixLineAlpha[j];
                    Beta[i, j] = MatrixLineBeta[j];
                }
            }
        }
        public async Task<int[]> GetElement(string dataset, string type, int idx)
        {
            string urlCall = $"api/numbers/{dataset}/{type}/{idx}";
            HttpResponseMessage response = await client.GetAsync(urlCall);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            MatrixRow matrixInfo = JsonConvert.DeserializeObject<MatrixRow>(jsonResponse); //deserialize and attach to Row object,                

            return matrixInfo.Value;

        }
        public void MatrixMulitiplicationHash(int[,] Alpha, int[,] Beta)
        {
            //function assumes Alpha and Beta are of same dimensions and the matrix is square of lengths 'size', created by the GET API request
            //incase the design requirements change in the future, throw error if the columns of Alpha do not equal rows of Beta
            if (Alpha.GetLength(1) != Beta.GetLength(0))
            {
                throw new Exception("Invalid Matrix sizes");
            }

            int[,] result = new int[size, size];
            StringBuilder resultMatrixSB = new StringBuilder();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        result[i, j] += Alpha[i, k] * Beta[k, j];
                    }
                    //concatenated string of the matrix' contents (left-toright,top - to - bottom)

                    resultMatrixSB.Append(result[i, j]);
                }              
            }
            //Md5 hashing
            using (MD5 md5 = MD5.Create())
            {
                byte[] input = Encoding.ASCII.GetBytes(resultMatrixSB.ToString());
                byte[] hashed = md5.ComputeHash(input);

                StringBuilder hashedBuilder = new StringBuilder();
                for (int i = 0; i < hashed.Length; i++)
                {
                    hashedBuilder.Append(hashed[i].ToString("X2"));
                }
                hashedSolution = hashedBuilder.ToString();
            }
        }    
    }
}
