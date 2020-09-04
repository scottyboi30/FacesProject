﻿using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FacesApiTest
{
    class Program
    {
        static async Task Main(string[] args)
        {

            const string imagePath = @"oscars-2017.jpg";
            const string urlAddress = "http://localhost:6000/api/faces";
            var imgUtil = new ImageUtility();
            var bytes = imgUtil.ConvertToBytes(imagePath);
            List<byte[]> faceList = null;
            var byteContent = new ByteArrayContent(bytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            using (var httpClient = new HttpClient())
            {
                using var response = await httpClient.PostAsync(urlAddress, byteContent);
                var apiResponse = await response.Content.ReadAsStringAsync();
                faceList = JsonConvert.DeserializeObject<List<byte[]>>(apiResponse);
            }

            if (faceList != null && faceList.Count > 0)
            {
                for (var i = 0; i < faceList.Count; i++)
                {
                    imgUtil.FromBytesToImage(faceList[i], "face" + i);

                }
            }
        }
    }
}
