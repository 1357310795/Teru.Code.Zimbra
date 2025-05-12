using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Teru.Code.Zimbra
{
    public class JsonResponse : BaseResponse
    {
        private JsonObject response_dict = null;

        public override string ResponseType => "json";

        public JsonResponse() 
        {

        }

        public override void Clean()
        {
            base.Clean();
            response_dict = null;
        }

        public override void SetResponse(string responseText)
        {
            response_dict = JsonNode.Parse(responseText).AsObject();
        }

        public override JsonObject GetBody()
        {
            return this.FilterResponse(response_dict["Body"].AsObject());
        }

        public override JsonObject GetHeader()
        {
            return this.FilterResponse(response_dict["Header"].AsObject());
        }

        public override bool IsBatch()
        {
            if (response_dict["Body"].AsObject().ContainsKey("BatchResponse"))
            {
                return true;
            }
            return false;
        }

        public override JsonObject GetBatch()
        {
            if (!IsBatch())
            {
                return null;
            }
            var ret_dict = new JsonObject(new Dictionary<string, JsonNode?>()
            {
                { "idToName", new JsonObject() },
                { "nameToId", new JsonObject() }
            });
            bool has_fault = false;
            foreach (var kvp in response_dict["Body"]["BatchResponse"].AsObject())
            {
                if (kvp.Key == "_jsns")
                {
                    continue;
                }
                if (kvp.Key == "Fault")
                {
                    has_fault = true;
                    continue;
                }
                var value = kvp.Value;
                if (value.GetValueKind() != JsonValueKind.Array)
                {
                    // This is a cornerstone
                    value = new JsonArray(new JsonNode[] { value });
                }
                foreach (var item in value.AsArray())
                {
                    var request_id = item["requestId"].GetValue<int>();
                    ret_dict["idToName"].AsObject().Add(request_id.ToString(), kvp.Key);
                    if (!ret_dict["nameToId"].AsObject().ContainsKey(kvp.Key))
                    {
                        ret_dict["nameToId"].AsObject().Add(kvp.Key, new JsonArray());
                    }
                    ret_dict["nameToId"][kvp.Key].AsArray().Add(request_id);
                }
            }
            ret_dict["hasFault"] = has_fault;
            return ret_dict;
        }

        public override JsonObject GetResponse(int? requestId = 0)
        {
            if (IsBatch())
            {
                foreach (var kvp in response_dict["Body"]["BatchResponse"].AsObject())
                {
                    if (kvp.Key == "_jsns")
                    {
                        continue; // pragma: no cover
                    }
                    var value = response_dict["Body"]["BatchResponse"][kvp.Key];
                    if (value.AsArray().First()["requestId"].GetValue<int>() == requestId)
                    {
                        return FilterResponse(new JsonObject(new Dictionary<string, JsonNode?>()
                            {
                                { kvp.Key, value }
                            }));
                    }
                }
                throw new Exception("Request ID not found in batch response");
            }
            else
            {
                var key = response_dict["Body"].AsObject().Select(x=>x.Key).First();
                return FilterResponse(new JsonObject(new Dictionary<string, JsonNode?>()
                {
                    { key, response_dict["Body"][key].DeepClone() }
                }));
            }
        }

        public override Dictionary<int, string> GetFaultCode()
        {
            if (IsBatch())
            {
                var ret_dict = new Dictionary<int, string>();
                foreach (var response in response_dict["Body"]["BatchResponse"]["Fault"].AsArray())
                {
                    var request_id = response["requestId"].GetValue<int>();
                    ret_dict[request_id] = response["Detail"]["Error"]["Code"].GetValue<string>();
                }
                return ret_dict;
            }
            else
            {
                return new Dictionary<int, string>()
                {
                    { 0, response_dict["Fault"]["Detail"]["Error"]["Code"].GetValue<string>() }
                };
            }
        }

        public override Dictionary<int, string> GetFaultMessage()
        {
            if (IsBatch())
            {
                var ret_dict = new Dictionary<int, string>();
                foreach (var response in response_dict["Body"]["BatchResponse"]["Fault"].AsArray())
                {
                    var request_id = response["requestId"].GetValue<int>();
                    ret_dict[request_id] = response["Reason"]["Text"].GetValue<string>();
                }
                return ret_dict;
            }
            else
            {
                return new Dictionary<int, string>()
                {
                    { 0, response_dict["Fault"]["Reason"]["Text"].GetValue<string>() }
                };
            }
        }
    }
}
