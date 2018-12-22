#r "Microsoft.Azure.EventGrid"
#r "Newtonsoft.Json"

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.EventGrid.Models;

static string eventGridUrl = "https://[Event Grid Topic Name].[Region].eventgrid.azure.net/api/events?api-version=2018-01-01";
static string sasKey = "[Event Grid Topic Sas Key]";
public static async Task Run(JObject eventGridEvent, ILogger log)
{
    var data = eventGridEvent["data"];
    var httpRequest = data["httpRequest"];
    var url = httpRequest["url"]?.ToString() ?? "";
    var subject = eventGridEvent["subject"]?.ToString() ?? "";
    var hasMicrosoftAuthorization = subject.EndsWith("Microsoft.Authorization");
    var hasUrl = url.Contains("[this funciton hostname]/providers/Microsoft.Authorization/CheckAccess");
    var isSucceeded = (data["status"]?.ToString() ?? "") == "Succeeded";
    // 自分自身のサービス認証を通知しないためにfunction のアクセスをバイパスさせる
    if (hasMicrosoftAuthorization && hasUrl && isSucceeded) return;

    var hc = new HttpClient();
    eventGridEvent["topic"] = null;
    var json = $"[{eventGridEvent.ToString(Formatting.None)}]";
    hc.DefaultRequestHeaders.Add("aeg-sas-key", sasKey);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    var res = await hc.PostAsync(eventGridUrl, content);
    log.LogInformation(res.ToString());
}