using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Matrix.Multiply.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Matrix.Multiply.Services
{
    public class LanguageService
    {
        private readonly ILogger logger;
        private HttpClient client;
        int[][] arrayA;
        int[][] arrayB;
        int[][] result;
        int size = 0;
        const int batchSize = 45;
        private string _md5Hash;

        public LanguageService(HttpClient _client, ILogger _logger, int _size)
        {
            client = _client;
            logger = _logger;
            size = _size;
            arrayA = new int[size][];
            arrayB = new int[size][];
            result = new int[size][];


        }

        public string process()
        {
            if (!initialize())
            {
                return "Initialize Failed";
            }
            var masterWatch = System.Diagnostics.Stopwatch.StartNew();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int batchStart = 0;
            List<ArrayRow> arrayRows = new List<ArrayRow>();
            do
            {
                int batchEnd = Math.Min(batchSize, size - batchStart);
                arrayRows.AddRange(Task.WhenAll(Enumerable.Range(batchStart, batchEnd).Select(DownloadDataAsyncA)).Result.ToList());
                arrayRows.AddRange(Task.WhenAll(Enumerable.Range(batchStart, batchEnd).Select(DownloadDataAsyncB)).Result.ToList());
                batchStart += batchSize;
                if (batchStart > size)
                {
                    batchStart = size;
                }

            } while (batchStart < size);

            foreach(var row in arrayRows)
            { 
             switch(row.arrayType)
                {
                    case ArrayTypes.A:
                        arrayA[row.rowIndex] = row.row;
                        break;
                    case ArrayTypes.B:
                        arrayB[row.rowIndex] = row.row;
                        break;
                }
            }

            watch.Stop();
            logger.LogInformation("Fetched all data in {time}.", watch.Elapsed.TotalSeconds);

            watch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, size, rowIndex =>
            {
                result[rowIndex] = new int[size];
                for (int colIndex = 0; colIndex < size; colIndex++)
                {
                    int val = 0;
                    for (int i = 0; i < size; i++)
                    {
                        val += arrayA[rowIndex][i] * arrayB[i][colIndex];
                    }
                    result[rowIndex][colIndex] = val;

                }
            });
            watch.Stop();
            logger.LogInformation("Cross Multiplied in {time}. Concatenate string", watch.Elapsed.TotalSeconds);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                sb.Append(String.Join("", result[i]));
            }
            String concatenatedString = sb.ToString();
            String hash = MD5Hash(concatenatedString);
            logger.LogInformation("Concatenate string. Return MD5 {md5}", hash);

            String valid = validate(hash);
            masterWatch.Stop();
            return "Hash '" + hash + "' for " + size + " elements returned " + valid + " in "+ masterWatch.Elapsed.TotalSeconds + " seconds ";
        }

        private async Task<ArrayRow> DownloadDataAsyncA(int rowIndex)
        {
            var response = await client.GetAsync("/api/numbers/A/row/" + rowIndex).ConfigureAwait(false);
            ServiceRetrievesResponse serviceRetrievesResponse = JsonConvert.DeserializeObject<ServiceRetrievesResponse>(await response.Content.ReadAsStringAsync());
            return new ArrayRow(serviceRetrievesResponse.Value, ArrayTypes.A, rowIndex);
        }

        private async Task<ArrayRow> DownloadDataAsyncB(int rowIndex)
        {
            var response = await client.GetAsync("/api/numbers/B/row/" + rowIndex).ConfigureAwait(false);
            ServiceRetrievesResponse serviceRetrievesResponse = JsonConvert.DeserializeObject<ServiceRetrievesResponse>(await response.Content.ReadAsStringAsync());
            return new ArrayRow(serviceRetrievesResponse.Value, ArrayTypes.B, rowIndex);
        }

        private bool initialize()
        {
            var response = client.GetAsync("/api/numbers/init/" + size).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsAsync<ServiceInitializeResponse>().Result.Success;

        }

        private string validate(string hash)
        {
            var body = new StringContent(hash, Encoding.UTF8, "application/json");
            var response = client.PostAsync("/api/numbers/validate", body).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsAsync<ServiceInitializeResponse>().Result.Value;

        }

        private string MD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
