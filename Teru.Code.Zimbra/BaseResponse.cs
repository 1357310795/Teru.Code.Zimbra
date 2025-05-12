using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Teru.Code.Zimbra
{
    public class BaseResponse
    {
        /// <summary>
        /// The actual response will set its response type (xml, json) here
        /// </summary>
        public virtual string ResponseType { get; protected set; } = null;

        /// <summary>
        /// Clean up the response, so it can be used again
        /// </summary>
        public virtual void Clean()
        {
            // Implementation left to derived classes
        }

        /// <summary>
        /// Interpret the response object.
        /// 
        /// Creates the internal response object by converting the given text
        /// from the HTTP communication into a managed object
        /// </summary>
        /// <param name="responseText">The response text to parse</param>
        public virtual void SetResponse(string responseText)
        {
            // Implementation left to derived classes
        }

        /// <summary>
        /// Return the header of the response.
        /// </summary>
        /// <returns>The response header in the documented dictionary format</returns>
        public virtual JsonObject GetHeader()
        {
            // Implementation left to derived classes
            return null;
        }

        /// <summary>
        /// Return the body of the response.
        /// </summary>
        /// <returns>The response body in the documented dictionary format</returns>
        public virtual JsonObject GetBody()
        {
            // Implementation left to derived classes
            return null;
        }

        /// <summary>
        /// Returns whether we have a BatchResponse.
        /// </summary>
        /// <returns>Whether we have a BatchResponse</returns>
        public virtual bool IsBatch()
        {
            // Implementation left to derived classes
            return false;
        }

        /// <summary>
        /// Returns an informative dictionary about a batch response.
        /// 
        /// Returns a dictionary containing the following information:
        /// {
        ///     "hasFault": bool (whether the batch has at least one SoapFault)
        ///     "idToName": Dictionary (mapping a response id to the corresponding response name or Fault)
        ///     "nameToId": Dictionary (mapping response names (or Fault) to the corresponding response id(s))
        /// }
        /// 
        /// If the method is called with no batch response existing, returns null
        /// </summary>
        /// <returns>Informative dictionary or null</returns>
        public virtual JsonObject GetBatch()
        {
            // Implementation left to derived classes
            return null;
        }

        /// <summary>
        /// Returns the response with the given request_id.
        /// 
        /// Returns the specific response. If "requestId" isn't provided or 0,
        /// the first (or only one in a non-batch) response is returned.
        /// </summary>
        /// <param name="requestId">The request ID to get</param>
        /// <returns>The response</returns>
        public virtual JsonObject GetResponse(int? requestId = 0)
        {
            // Implementation left to derived classes
            return null;
        }

        /// <summary>
        /// Returns the fault error code of this response (overridden)
        /// 
        /// We provide this additional method because of zimbra bug
        /// https://bugzilla.zimbra.com/show_bug.cgi?id=95490
        /// 
        /// For batch responses, we return a dict of fault codes. The key is
        /// the request_id.
        /// </summary>
        /// <returns>Fault code string or dictionary</returns>
        public virtual Dictionary<int, string> GetFaultCode()
        {
            // Implementation left to derived classes
            return null;
        }

        /// <summary>
        /// Returns the fault error message of this response (overridden)
        /// 
        /// We provide this additional method because of zimbra bug
        /// https://bugzilla.zimbra.com/show_bug.cgi?id=95490
        /// 
        /// For batch responses, we return a dict of fault messages. The key is
        /// the request_id.
        /// </summary>
        /// <returns>Fault code error message or dictionary</returns>
        public virtual Dictionary<int, string> GetFaultMessage()
        {
            // Implementation left to derived classes
            return null;
        }

        /// <summary>
        /// Checks whether this response has at least one fault response
        /// (supports both batch and single responses)
        /// </summary>
        /// <returns>True if response contains faults</returns>
        public virtual bool IsFault()
        {
            if (IsBatch())
            {
                JsonObject info = GetBatch();
                return info["hasFault"].AsValue().GetValue<bool>();
            }
            else
            {
                JsonObject myResponse = GetResponse();
                if (myResponse.First().Key == "Fault")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add additional filters to the response dictionary
        /// 
        /// Currently the response dictionary is filtered like this:
        ///   * If a list only has one item, the list is replaced by that item
        ///   * Namespace-Keys (_jsns and xmlns) are removed
        /// </summary>
        /// <param name="responseDict">the pregenerated, but unfiltered response dict</param>
        /// <returns>The filtered dictionary</returns>
        protected virtual JsonObject FilterResponse(JsonObject responseDict)
        {
            JsonObject filteredDict = new JsonObject();

            foreach (var property in responseDict)
            {
                string key = property.Key;
                object value = property.Value;

                if (key == "_jsns" || key == "xmlns")
                {
                    continue;
                }

                if (value is JsonArray jsonArray)
                {
                    if (jsonArray.Count == 1)
                        filteredDict[key] = jsonArray[0].DeepClone();
                    else
                        filteredDict[key] = jsonArray.DeepClone();
                }
                else if (value is JsonObject jsonObj)
                {
                    if (jsonObj.Count == 1 && jsonObj.ContainsKey("_content"))
                        filteredDict[key] = jsonObj["_content"].DeepClone();
                    else
                        filteredDict[key] = jsonObj.DeepClone();
                }
                else if (value is JsonObject nestedJsonObj)
                {
                    JsonObject tmpDict = FilterResponse(nestedJsonObj);
                    filteredDict[key] = tmpDict;
                }
                else if (value is JsonValue jsonValue)
                {
                    filteredDict[key] = jsonValue.DeepClone();
                }
            }

            return filteredDict;
        }
    }
}
