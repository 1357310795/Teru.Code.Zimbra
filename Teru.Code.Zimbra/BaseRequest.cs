using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Teru.Code.Zimbra.Exceptions;

namespace Teru.Code.Zimbra
{
    public class BaseRequest
    {
        /// <summary>
        /// Valid parameter name for Soap context
        /// </summary>
        public static readonly List<string> ValidContextParams = new List<string>
        {
            "authToken", "authTokenControl", "session",
            "account", "change", "targetServer", "userAgent",
            "via"
        };

        /// <summary>
        /// Are we doing batch requests?
        /// </summary>
        public bool BatchRequest { get; protected set; } = false;

        /// <summary>
        /// If so, keep the current request id
        /// </summary>
        public int? BatchRequestId { get; protected set; } = null;

        /// <summary>
        /// The specific request will set its request type (json, xml) here
        /// </summary>
        public virtual string RequestType { get; protected set; } = null;

        /// <summary>
        /// Clean up request, so the request object can be reused
        /// </summary>
        public virtual void Clean()
        {
            BatchRequest = false;
            BatchRequestId = null;
        }

        /// <summary>
        /// Set header context parameters. Refer to the top of <Zimbra
        /// Server-Root>/docs/soap.txt about specifics.
        /// 
        /// The <format>-Parameter cannot be changed, because it is set by the
        /// implementing class.
        /// 
        /// Should be called by implementing method to check for valid context
        /// params.
        /// </summary>
        /// <param name="params">A Dictionary containing context parameters.</param>
        /// <exception cref="RequestHeaderContextException">Thrown when invalid context parameter is provided</exception>
        public virtual void SetContextParams(Dictionary<string, JsonNode?> keyValues)
        {
            foreach (var kvp in keyValues)
            {
                if (!ValidContextParams.Contains(kvp.Key))
                {
                    throw new RequestHeaderContextException(
                        $"{kvp.Key} is not a valid context parameter."
                    );
                }
            }
        }

        /// <summary>
        /// Convenience function to inject the auth token into the header.
        /// </summary>
        /// <param name="token">Auth token</param>
        public virtual void SetAuthToken(string token)
        {
            SetContextParams(
                new Dictionary<string, JsonNode?>
                {
                    {
                        "authToken",
                        new JsonObject(
                            new Dictionary<string, JsonNode?>
                            {
                                { "_content", token }
                            }
                        )
                    }
                }
            );
        }

        /// <summary>
        /// Enables batch request gathering.
        /// 
        /// Do this first and then consecutively call "AddRequest" to add more
        /// requests.
        /// </summary>
        /// <param name="onerror">"continue" (default) if one request fails (and
        /// response with soap Faults for the request) or "stop" processing.</param>
        public virtual void EnableBatch(string onerror = "continue")
        {
            BatchRequest = true;
            BatchRequestId = 1;

            CreateBatchNode(onerror);
        }

        /// <summary>
        /// Prepare the request structure to support batch mode
        /// 
        /// The params are like in EnableBatch
        /// </summary>
        protected virtual void CreateBatchNode(string onerror)
        {
            // Implementation left to derived classes
        }

        /// <summary>
        /// Add a request.
        /// 
        /// This adds a request to the body or to the batchrequest-node if batch
        /// requesting is enabled. Has to update the BatchRequestId after
        /// adding a batch request!
        /// 
        /// Implementing classes should call this first for checks.
        /// </summary>
        /// <param name="requestName">The name of the request</param>
        /// <param name="requestDict">The request parameters as a serializable dictionary.</param>
        /// <param name="namespace">The XML namespace of the request.</param>
        /// <returns>The current request id (if batch processing) or null</returns>
        public virtual int? AddRequest(string requestName, string requestDict, string @namespace)
        {
            // Currently no checks
            // Implementation left to derived classes
            return null;
        }

        /// <summary>
        /// Return the request in the native form.
        /// </summary>
        /// <returns>The request content</returns>
        public virtual string GetRequest()
        {
            // Implementation left to derived classes
            return null;
        }

        public override string ToString()
        {
            return GetRequest();
        }
    }
}
