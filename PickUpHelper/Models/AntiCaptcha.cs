using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Json.NET;
using System.Net.Http.Formatting;
using System.Threading;
using Anticaptcha;
using StringExtensions;
using Newtonsoft.Json.Linq;

namespace PickUpHelper.Models
{
    class AntiCaptcha
    {
        private const string Host = "https://api.anti-captcha.com/createTask";
        private const string HostAnsw = "https://api.anti-captcha.com/getTaskResult";
        private const string ClientKey = "f73c7486c64d17700fce7f5eedea5ce5";
        private string _captchaSid;
        private readonly string _captchaImg;
        private readonly HttpClient _client;
        private readonly JObject _jobs;
        private readonly JObject _jobsAnsw;
        private string _contentAnsw;
        public string Captcha;

        public AntiCaptcha(string captchaSid, string base64body)
        {
            _captchaSid = captchaSid;
            _client = new HttpClient();
            _jobs = new JObject();
            _jobs["clientKey"] = ClientKey;
            _jobs["task"] = new JObject();
            _jobs["task"]["type"] = "ImageToTextTask";
            _jobs["task"]["body"] = base64body;
            _jobs["task"]["phrase"] = false;
            _jobs["task"]["case"] = false;
            _jobs["task"]["math"] = 0;
            _jobsAnsw = new JObject();
        }
        public async Task SendCaptcha()
        {
            var responce =
                await
                    _client.PostAsJsonAsync(Host, _jobs);
            var content = await responce.Content.ReadAsStringAsync();
            var taskId = content.Between("taskId\":", "}", StringComparison.InvariantCulture);
            _jobsAnsw["clientKey"] = ClientKey;
            _jobsAnsw["taskId"] = taskId;
            do
            {
                Thread.Sleep(1000);
                var responceAnsw = await _client.PostAsJsonAsync(HostAnsw, _jobsAnsw);
                _contentAnsw = await responceAnsw.Content.ReadAsStringAsync();
            } while (!_contentAnsw.Contains("ready"));
            Captcha = _contentAnsw.Between("text\":\"", "\"", StringComparison.InvariantCulture);
        }
    }
}
