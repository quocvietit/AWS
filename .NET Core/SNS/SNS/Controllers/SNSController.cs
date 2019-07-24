using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Amazon.SimpleNotificationService.Util;
using Newtonsoft.Json;
using RestSharp;
using Microsoft.Extensions.Logging;

namespace SNS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SNSController : ControllerBase
    {
        private readonly ILogger logger;

        public SNSController(ILogger<SNSController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public String CheckHealth()
        {
            this.logger.LogInformation("Call API: ");
            return "OK";
        }

        [HttpPost]
        public String SNSSubscriptionPost(String id = "")
        {
            this.logger.LogInformation("Call API: ");
            try
            {
                var jsonData = "";
                Stream req = Request.Body;
                req.Seek(0, System.IO.SeekOrigin.Begin);
                String json = new StreamReader(req).ReadToEnd();
                var sm = Message.ParseMessage(json);
                if (sm.Type.Equals("SubscriptionConfirmation")) //for confirmation
                {
                    this.logger.LogInformation("Received Confirm subscription request");
                    if (!string.IsNullOrEmpty(sm.SubscribeURL))
                    {
                        var uri = new Uri(sm.SubscribeURL);
                        this.logger.LogInformation("uri:" + uri.ToString());
                        var baseUrl = uri.GetLeftPart(System.UriPartial.Authority);
                        var resource = sm.SubscribeURL.Replace(baseUrl, "");
                        var response = new RestClient
                        {
                            BaseUrl = new Uri(baseUrl),
                        }.Execute(new RestRequest
                        {
                            Resource = resource,
                            Method = Method.GET,
                            RequestFormat = RestSharp.DataFormat.Xml
                        });
                    }
                }
                else // For processing of messages
                {
                    this.logger.LogInformation("Message received from SNS:" + sm.TopicArn);
                    dynamic message = JsonConvert.DeserializeObject(sm.MessageText);
                    this.logger.LogInformation($"EventTime : {message.detail.eventTime}");
                    this.logger.LogInformation($"EventName : {message.detail.eventName}");
                    this.logger.LogInformation($"RequestParams : {message.detail.requestParameters}");
                    this.logger.LogInformation($"ResponseParams : {message.detail.responseElements}");
                    this.logger.LogInformation($"RequestID : {message.detail.requestID}");
                }
                //do stuff
                return "Success";
            }
            catch (Exception ex)
            {
                this.logger.LogInformation("failed");
                return "";
            }
        }
    }
}
