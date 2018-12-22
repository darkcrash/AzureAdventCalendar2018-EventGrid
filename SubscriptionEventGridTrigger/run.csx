#r "Microsoft.Azure.EventGrid"
#r "Newtonsoft.Json"

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.EventGrid.Models;

// Slack incoming-Webhook URL
static string webhookUrl = "https://hooks.slack.com/services/******/*********/*******************";

public static void Run(JObject eventGridEvent, ILogger log)
{
    log.LogInformation(eventGridEvent.ToString(Formatting.Indented));

    var hc = new HttpClient();
    var data = eventGridEvent["data"];
    var authorization = data["authorization"];
    var claims = data["claims"];

    var colorText = (data["status"]?.ToString() == "Succeeded" ? "good" : "");
    var fieldsObject = new object[] {
                    new {
                        title = "id",
                        value = eventGridEvent["id"],
                        @short = true
                    },
                    new {
                        title = "Event Time",
                        value = DateTime.Parse(eventGridEvent["eventTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss"),
                        @short = true
                    },
                    new {
                        title = "Status",
                        value = data["status"],
                        @short = true
                    },
                    new {
                        title = "Data Version",
                        value = eventGridEvent["dataVersion"],
                        @short = true
                    },
                    new {
                        title = "Metadata Version",
                        value = eventGridEvent["metadataVersion"],
                        @short = true
                    },
                    new {
                        title = "Subject",
                        value = eventGridEvent["subject"],
                        @short = false
                    },
                    new {
                        title = "Tenant Id",
                        value = data["tenantId"],
                        @short = true
                    },
                    new {
                        title = "Resource Provider",
                        value = data["resourceProvider"],
                        @short = true
                    },
                    new {
                        title = "Topic",
                        value = eventGridEvent["topic"],
                        @short = false
                    }
                };
    var attachmentsObject = new object[] {
            new {
                title = data["operationName"],
                color = colorText,
                fields = fieldsObject
            },
            new {
                title = "data",
                text = data.ToString()
            }
        };
    var json = JsonConvert.SerializeObject(new
    {
        text = $"*{eventGridEvent["eventType"]}*",
        icon_emoji = ":ghost:",
        username = "Azure Subscription",
        attachments = attachmentsObject
    });

    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    var res = hc.PostAsync(webhookUrl, content).Result;
}
