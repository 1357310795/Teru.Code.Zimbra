using System.Text.Json;

namespace Teru.Code.Zimbra.Test
{
    public class UnitTest1
    {
        [Fact]
        public async void Test1()
        {
            ZimbraClient client = new ZimbraClient("https://mail.sjtu.edu.cn/service/soap");
            var usr_token = "XXX";
            JsonRequest request = client.GenRequest("json", usr_token) as JsonRequest;
            request.AddRequest("SearchRequest", """
                            {
                    "sortBy": "dateDesc",
                    "header": [
                        {
                        "n": "List-ID"
                        },
                        {
                        "n": "X-Zimbra-DL"
                        },
                        {
                        "n": "IN-REPLY-TO"
                        }
                    ],
                    "tz": {
                        "id": "Asia/Hong_Kong"
                    },
                    "locale": {
                        "_content": "zh_CN"
                    },
                    "offset": 0,
                    "limit": 100,
                    "query": "in:inbox",
                    "types": "message",
                    "recip": "0",
                    "needExp": 1
                }
                """,
                "urn:zimbraMail");
            var resp = await client.SendRequest(request);

            if (resp.IsFault())
            {
                Console.WriteLine(resp.GetFaultCode().First().Value);
                Console.WriteLine(resp.GetFaultMessage().First().Value);
            }
            else
            {
                Console.WriteLine(JsonSerializer.Serialize(resp.GetResponse()));
            }
        }

        [Fact]
        public async void Test2()
        {
            ZimbraClient client = new ZimbraClient("https://mail.sjtu.edu.cn/service/soap");
            var usr_token = "XXX";
            JsonRequest request = client.GenRequest("json", usr_token) as JsonRequest;
            request.AddRequest("SendMsgRequest", """
                  {
                    "_jsns": "urn:zimbraMail",
                    "m": {
                      "e": [
                        {
                          "t": "t",
                          "a": "test@qq.com",
                          "p": "test"
                        },
                        {
                          "t": "c",
                          "a": "test@mail.sjtu.edu.cn",
                          "p": "test"
                        },
                        {
                          "t": "f",
                          "a": "test@sjtu.edu.cn",
                          "p": "Teruteru"
                        }
                      ],
                      "su": {
                        "_content": "测试邮件主题"
                      },
                      "mp": [
                        {
                          "ct": "text/plain",
                          "content": {
                            "_content": "测试邮件内容"
                          }
                        }
                      ]
                    }
                  }
                """,
                "urn:zimbraMail");
            var resp = await client.SendRequest(request);

            if (resp.IsFault())
            {
                Console.WriteLine(resp.GetFaultCode().First().Value);
                Console.WriteLine(resp.GetFaultMessage().First().Value);
            }
            else
            {
                Console.WriteLine(JsonSerializer.Serialize(resp.GetResponse()));
            }
        }
    }
}