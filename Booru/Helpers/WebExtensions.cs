using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Imouto.BooruParser.Helpers
{
    public class HttpException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; }

        public HttpException(string stringResponse, Exception exception, HttpStatusCode httpStatusCode) : base(stringResponse, exception)
        {
            HttpStatusCode = httpStatusCode;
        }
    }

    static class WebExtensions
    {
        

        public static async Task EnsureSuccessStatusCodeWithResponce(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception e)
                    {
                        throw new HttpException(stringResponse, e, response.StatusCode);
                    }
                }
                catch (WebException)
                {
                    throw;
                }
                catch (Exception)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
