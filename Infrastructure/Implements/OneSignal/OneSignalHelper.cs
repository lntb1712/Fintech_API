using Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Implements.OneSignal
{
    public class OneSignalHelper
    {
        private const string ONESIGNAL_APP_ID = "one-signal-appid";// GetEnvironmentVariable("ONESIGNAL_APP_ID");
        private const string ONESIGNAL_API_KEY = "one-signal-id";

        private static async Task<bool> CreateNotification(string appId, string authKey, List<string> pushTokens, string noticeTitle, string noticeContent, object payloadData, string imageURL)
        {
            try
            {
                var requestBody = new object();
                if (pushTokens?.Count > 0)
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                    var clientHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; },
                    };
                    var client = new HttpClient(clientHandler)
                    {
                        Timeout = TimeSpan.FromSeconds(10)
                    };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", $"Key {authKey}");

                    //included_segments = new string[] { "Active Users" }, //All Active Users
                    //include_player_ids = pushTokens.Distinct().ToArray(),
                    requestBody = (new
                    {
                        app_id = appId,
                        headings = new { en = noticeTitle, vi = noticeTitle },
                        contents = new { en = noticeContent, vi = noticeContent },
                        include_subscription_ids = pushTokens.Distinct().ToArray(),
                        data = payloadData,
                        ios_badgeType = "SetTo",
                        ios_badgeCount = 1,
                        ios_attachments = Utils.IsImageURL(imageURL) ? new { id1 = imageURL } : null,
                        big_picture = Utils.IsImageURL(imageURL) ? imageURL : "",
                        priority = 10,
                        ttl = 24 * 60 * 60,//seconds
                    });

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    // Handle the response
                    using var response = await client.PostAsync($"https://api.onesignal.com/notifications", jsonContent);
                    // Read the response content
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        // Failed, check the response for errors
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        var invalidPlayerIds = errorResponse.errors?.invalid_player_ids;

                        if (invalidPlayerIds != null && invalidPlayerIds.Count > 0)
                        {
                            // Filter out invalid IDs and retry the notification
                            var validPushTokens = pushTokens.Where(token => !invalidPlayerIds.Contains(token)).Distinct().ToArray();

                            if (validPushTokens.Length > 0)
                            {
                                // Retry the notification with only valid subscription IDs
                                requestBody = new
                                {
                                    app_id = appId,
                                    headings = new { en = noticeTitle, vi = noticeTitle },
                                    contents = new { en = noticeContent, vi = noticeContent },
                                    include_subscription_ids = validPushTokens,
                                    data = payloadData,
                                    ios_badgeType = "SetTo",
                                    ios_badgeCount = 1,
                                    ios_attachments = Utils.IsImageURL(imageURL) ? new { id1 = imageURL } : null,
                                    big_picture = Utils.IsImageURL(imageURL) ? imageURL : "",
                                    priority = 10,
                                    ttl = 24 * 60 * 60, // seconds
                                };

                                jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                                // Retry the request with valid tokens
                                using var retryResponse = await client.PostAsync("https://api.onesignal.com/notifications", jsonContent);
                                var retryResponseContent = await retryResponse.Content.ReadAsStringAsync();

                                if (retryResponse.IsSuccessStatusCode)
                                {
                                    Console.WriteLine("Notification retried and sent successfully.");
                                }
                                else
                                {
                                    Console.WriteLine("Retry failed. Response: " + retryResponseContent);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No valid subscription IDs to retry.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unexpected error occurred: " + responseContent);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error from push notfication: " + ex.Message);
            }
        }

        public static async Task<bool> SendNotificationToUsers(List<string> listTokens, string title, string content, object payloadData, string imageUrl)
        {
            try
            {
                return await CreateNotification(ONESIGNAL_APP_ID, ONESIGNAL_API_KEY, listTokens, title, content, payloadData, imageUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error from push notfication: " + ex.Message);
                return true;
            }
        }
    }
}