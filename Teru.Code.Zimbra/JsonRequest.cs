using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Teru.Code.Zimbra
{
    public class JsonRequest : BaseRequest
    {
        public override string RequestType => "json";

        private JsonObject request_dict;

        public JsonRequest() 
        {
            Init();
        }

        private void Init()
        {
            Dictionary<string, JsonNode?> dic = new Dictionary<string, JsonNode?>()
            {
                { "Header", new JsonObject(
                    new Dictionary<string, JsonNode?>()
                    {
                        { "context", new JsonObject(
                            new Dictionary<string, JsonNode?>()
                                {
                                    { "_jsns", "urn:zimbra" },
                                    { "format", new JsonObject(
                                            new Dictionary<string, JsonNode?>()
                                            {
                                                { "type", "js" }
                                            }
                                        )
                                    }
                                }
                            )
                        }
                    }
                    )
                },
                { "Body", new JsonObject() }
            };
            request_dict = new JsonObject(dic);
        }

        public override void Clean()
        {
            base.Clean();
            Init();
        }

        public override void SetContextParams(Dictionary<string, JsonNode?> parameters)
        {
            base.SetContextParams(parameters);

            foreach (var kvp in parameters)
            {
                request_dict["Header"]["context"].AsObject().Add(kvp.Key, kvp.Value);
            }
        }

        public void _CreateBatchNode(string onerror)
        {
            request_dict["Body"] = new JsonObject(new Dictionary<string, JsonNode?>()
            {
                { "BatchRequest", new JsonObject(
                    new Dictionary<string, JsonNode?>()
                        {
                            { "_jsns", "urn:zimbra" },
                            { "onerror", onerror }
                        }
                    ) 
                }
            });
        }

        public override int? AddRequest(string requestName, string requestDict, string @namespace)
        {
            base.AddRequest(requestName, requestDict, @namespace);
            JsonObject bodyNode = request_dict["Body"].AsObject();
            bodyNode.Add("_jsns", @namespace);
            var curRequestDict = JsonNode.Parse(requestDict);

            if (this.BatchRequest)
            {
                var requestId = this.BatchRequestId;
                curRequestDict.AsObject().Add("requestId", requestId);
                this.BatchRequestId += 1;
                if (bodyNode["BatchRequest"].AsObject().ContainsKey(requestName))
                {
                    var tmp = bodyNode["BatchRequest"][requestName].AsObject();
                    bodyNode["BatchRequest"][requestName] = new JsonArray(new JsonNode?[]
                    {
                        tmp,
                        curRequestDict
                    });
                }
                else
                {
                    bodyNode["BatchRequest"][requestName] = curRequestDict;
                }
                return requestId;
            }
            else
            {
                bodyNode.Add(requestName, JsonSerializer.Deserialize<JsonNode>(requestDict));
                return null;
            }
        }

        public override string GetRequest()
        {
            return JsonSerializer.Serialize(request_dict);
        }
    }
}
