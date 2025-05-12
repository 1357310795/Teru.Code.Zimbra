using System.Net;
using System.Text;
using Teru.Code.Zimbra.Exceptions;

namespace Teru.Code.Zimbra
{
    public class ZimbraClient
    {
        /// <summary>
        /// URL to the zimbra soap interface
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Timeout of the request
        /// </summary>
        public int? Timeout { get; set; }

        private readonly HttpClient _client;

        public ZimbraClient(string url, int? timeout = null)
        {
            this.Url = url;
            this.Timeout = timeout;
            _client = new HttpClient();
            if (timeout.HasValue)
            {
                _client.Timeout = TimeSpan.FromMilliseconds(timeout.Value);
            }
        }

        /// <summary>
        /// Convenience method to quickly generate a token
        /// </summary>
        /// <param name="requestType">Type of request (defaults to json)</param>
        /// <param name="token">Authentication token</param>
        /// <param name="setBatch">Also set this request to batch mode?</param>
        /// <param name="batchOnerror">Onerror-parameter for batch mode</param>
        /// <returns>The request</returns>
        public object GenRequest(string requestType = "json", string token = null, bool setBatch = false, string batchOnerror = null)
        {

            BaseRequest localRequest;

            if (requestType == "json")
            {
                localRequest = new JsonRequest();
            }
            else if (requestType == "xml")
            {
                throw new NotImplementedException();
                //localRequest = new XmlRequest();
            }
            else
            {
                throw new UnknownRequestTypeException();
            }

            if (token != null)
            {
                localRequest.SetAuthToken(token);
            }

            if (setBatch)
            {
                localRequest.EnableBatch(batchOnerror);
            }

            return localRequest;
        }

        public async Task<BaseResponse> SendRequest(BaseRequest request, BaseResponse response = null)
        {
            /// <summary>
            /// Send the request.
            /// </summary>
            /// <param name="request">The request to send</param>
            /// <param name="response">A prebuilt response object</param>
            /// <returns>The response</returns>

            BaseResponse localResponse = null;

            if (response == null)
            {
                if (request.RequestType == "json")
                {
                    localResponse = new JsonResponse();
                }
                else if (request.RequestType == "xml")
                {
                    throw new NotImplementedException();
                    //localResponse = new XmlResponse();
                }
                else
                {
                    throw new UnknownRequestTypeException();
                }
            }

            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post, this.Url);

                req.Content = new StringContent(request.GetRequest());
                req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var resp = await _client.SendAsync(req);

                var serverResponse = await resp.Content.ReadAsStringAsync();

                if (response == null)
                {
                    localResponse.SetResponse(serverResponse);
                }
                else
                {
                    response.SetResponse(serverResponse);
                }
            }
            catch (HttpRequestException e)
            {
                throw;
            }

            if (response == null)
            {
                return localResponse;
            }
            else
            {
                return response;
            }
        }
    }
}
