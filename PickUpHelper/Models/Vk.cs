using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Catel.Windows.Interactivity;
using StringExtensions;
using xNet;
using FormUrlEncodedContent = System.Net.Http.FormUrlEncodedContent;
using HttpStatusCode = System.Net.HttpStatusCode;
using StreamContent = System.Net.Http.StreamContent;

namespace PickUpHelper.Models
{
    public class Vk
    {
        private const string ClientId = "5627888";
        private const string RedirectId = "https://oauth.vk.com/blank.html";
        private const string SecretKey = "bxhLTbrE2Adjw8ZsQpXT";
        public string Token;
        private AntiCaptcha _ac;
        private readonly HttpClient _client;
        public List<Victim> Victims;
        private readonly string _name;
        private readonly string _pass;
        public string[] VictimsList;
        private string _captchaImg;
        private int _notValid;
        private Dictionary<string, string> _parameters;

        public Vk(string name, string pass)
        {
            _name = name;
            _pass = pass;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
            Victims = new List<Victim>();
        }
        public async Task VkLogin()
        {
            var responce = await
                _client.GetAsync(
                    $"https://oauth.vk.com/authorize?client_id={ClientId}&redirect_uri={RedirectId}&scope=notify,friends,messages,offline&response_type=token&v=5.53");
            var content = await responce.Content.ReadAsStringAsync();
            var parameters = new Dictionary<string, string>();
            parameters["ip_h"] = content.Between("ip_h\" value=\"", "\"", StringComparison.InvariantCulture);
            parameters["lg_h"] = content.Between("lg_h\" value=\"", "\"", StringComparison.InvariantCulture);
            parameters["_origin"] = content.Between("_origin\" value=\"", "\"", StringComparison.InvariantCulture);
            parameters["to"] = content.Between("to\" value=\"", "\"", StringComparison.InvariantCulture);
            parameters["expire"] = "0";
            parameters["email"] = _name;
            parameters["pass"] = _pass;
            var result =
                await
                    _client.PostAsync($"https://login.vk.com/?act=login&soft=1", new FormUrlEncodedContent(parameters));
            var content2 = await result.Content.ReadAsStringAsync();
            var parameters2 = new Dictionary<string, string>
            {
                ["act"] = content2.Between("act=", "&", StringComparison.InvariantCulture),
                ["client_id"] = ClientId,
                ["settings"] = content2.Between("settings=", "&", StringComparison.InvariantCulture),
                ["redirect_uri"] = RedirectId,
                ["response_type"] = "token",
                ["group_ids"] = "",
                ["direct_hash"] = content2.Between("direct_hash=", "&", StringComparison.InvariantCulture),
                ["token_type"] = "0",
                ["v"] = "5.63",
                ["state"] = "",
                ["display"] = "page",
                ["display"] = "page",
                ["ip_h"] = content2.Between("ip_h=", "&", StringComparison.InvariantCulture),
                ["hash"] = content2.Between("&hash=", "&", StringComparison.InvariantCulture),
                ["https"] = "1"
            };
            var result2 =
                await
                    _client.PostAsync(
                        $"https://login.vk.com/",
                        new FormUrlEncodedContent(parameters2));
            var content3 = await result2.Content.ReadAsStringAsync();
            Token = result2.RequestMessage.RequestUri.ToString()
                .Between("token=", "&", StringComparison.InvariantCulture);
        }
        public bool ReadyCheck()
        {
            return !String.IsNullOrEmpty(Token);
        }
        public async Task CreateList(string count, string town, string sex, string ageFrom, string ageTo, string online)
        {
            var parameters = new Dictionary<string, string>
            {
                ["access_token"] = Token,
                ["sort"] = "1",
                ["country"] = "1",
                ["status"] = "6",
                ["has_photo"] = "1",
                ["count"] = count,
                ["hometown"] = town,
                ["sex"] = sex,
                ["hometown"] = town,
                ["age_from"] = ageFrom,
                ["age_to"] = ageTo,
                ["online"] = online
            };
            var responce =
                await
                    _client.PostAsync($"https://api.vk.com/method/users.search.xml",
                        new FormUrlEncodedContent(parameters));
            var content = await responce.Content.ReadAsStringAsync();
            VictimsList = content.Substrings("<user>", "</user>");
            foreach (var v in VictimsList)
            {
                var id = v.Substring("<uid>", "</uid>");
                var name = v.Substring("<first_name>", "</first_name>");
                var lastName = v.Substring("<last_name>", "</last_name>");
                Victims.Add(new Victim(name, lastName, id));
            }
        }
        public async Task SendMsg(string massage)
        {
            foreach (var ids in Victims)
            {
                var validreq = await _client.GetAsync("https://vk.com/id" + ids.Uid);
                var isValid = await validreq.Content.ReadAsStringAsync();
                if (isValid.Contains("Написать сообщение"))
                {
                    _parameters = new Dictionary<string, string>()
                    {
                        ["access_token"] = Token,
                        ["user_id"] = ids.Uid,
                        ["message"] = massage
                    };
                    var responce =
                        await
                            _client.PostAsync($"https://api.vk.com/method/messages.send",
                                new FormUrlEncodedContent(_parameters));
                    var content = await responce.Content.ReadAsStringAsync();

                    if (content.Contains("Captcha needed"))
                    {
                        var captchaSid = content.Substring("captcha_sid\":\"", "\"").Replace("\\", "").Trim();
                        _captchaImg = content.Substring("captcha_img\":\"", "\"").Replace("\\", "").Trim();
                        var bytereq = await _client.GetByteArrayAsync(_captchaImg);
                        var bas64body = Convert.ToBase64String(bytereq);
                        AntiCaptcha ac = new AntiCaptcha(captchaSid, bas64body);
                        await ac.SendCaptcha();
                        _parameters["captcha_sid"] = captchaSid;
                        _parameters["captcha_key"] = ac.Captcha;
                        responce = await
                            _client.PostAsync($"https://api.vk.com/method/messages.send",
                                new FormUrlEncodedContent(_parameters));
                        var content2 = await responce.Content.ReadAsStringAsync();
                    }               
                }
                else _notValid++;
            }
        }
    }
}


