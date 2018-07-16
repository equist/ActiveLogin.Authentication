﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ActiveLogin.Authentication.BankId.Api.Errors;
using ActiveLogin.Authentication.Common.Serialization;

namespace ActiveLogin.Authentication.BankId.Api
{
    internal static class BankIdHttpClientExtensions
    {
        public static async Task<TResult> PostAsync<TRequest, TResult>(this HttpClient httpClient, string url, TRequest request, IJsonSerializer jsonSerializer)
        {
            var httpResponseMessage = await GetHttpResponseAsync(url, request, httpClient.PostAsync, jsonSerializer);
            return jsonSerializer.Deserialize<TResult>(await httpResponseMessage.Content.ReadAsStreamAsync());
        }

        private static async Task<HttpResponseMessage> GetHttpResponseAsync<TRequest>(string url, TRequest request, Func<string, HttpContent, Task<HttpResponseMessage>> httpRequest, IJsonSerializer jsonSerializer)
        {
            var requestJson = jsonSerializer.Serialize(request);
            var requestContent = GetJsonStringContent(requestJson);

            var httpResponseMessage = await httpRequest(CleanUrl(url), requestContent);
            await BankIdApiErrorHandler.EnsureSuccessAsync(httpResponseMessage, jsonSerializer);
            return httpResponseMessage;
        }

        private static StringContent GetJsonStringContent(string requestJson)
        {
            var requestContent = new StringContent(requestJson, Encoding.Default, "application/json");
            requestContent.Headers.ContentType.CharSet = string.Empty;
            return requestContent;
        }

        private static string CleanUrl(string url)
        {
            return url.TrimStart('/');
        }
    }
}