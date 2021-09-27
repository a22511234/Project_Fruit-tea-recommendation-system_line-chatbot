using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using isRock.LineBot.Conversation;
using Newtonsoft.Json;
using FireSharp.Config;
using FireSharp.Response;
using FireSharp.Interfaces;

namespace LineBotFaceRecognition.Controllers
{
    public class LineBotWebHookController : isRock.LineBot.LineWebHookControllerBase
    {


        [Route("api/LineFaceRec")]
        [HttpPost]
        public IHttpActionResult POST()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //取得Web.config中的 app settings
            var token = "S3o7moA62IOh6lIIdD5ECoK4zCKGa9D6C9vR0wzkmX2KaQDtj6Zcp7Q+ijNHS1WV2S5fx2a7a+oFKhIehTvm2dDqMD2qLYRQgbN4/6yq+inaRzj7eU0glrmQy0G2PErxMS5Erm9wQifEaQ/xD55QoQdB04t89/1O/w1cDnyilFU=";
            const string AdminUserId = "Ua3d3e1675bca2f5e468a6c80bf49f332";
            var responseMsg = "";
            bool confirm = false;
            isRock.LineBot.Event LineEvent = null;
            IFirebaseConfig fcon = new FirebaseConfig()
            {
                AuthSecret = "bW4Tp1NHyMLMkWo1loJcqf9gVzKgzmYw6LHF1Eu3",
                BasePath = "https://dailyjuicy-40e64-default-rtdb.firebaseio.com/"
            };
            try
            {
                //設定資料庫
                IFirebaseClient client = new FireSharp.FirebaseClient(fcon);
                //設定ChannelAccessToken(或抓取Web.Config)
                this.ChannelAccessToken = token;
                //取得Line Event(本例是範例，因此只取第一個)
                LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                //定義資訊蒐集者
                isRock.LineBot.Conversation.InformationCollector<LeaveRequestV2> CIC = new isRock.LineBot.Conversation.InformationCollector<LeaveRequestV2>(ChannelAccessToken);
                // 底下是個人資料輸入確認資訊
                CIC.OnMessageTypeCheck += (s, e) =>
                {
                    switch (e.CurrentPropertyName)
                    {
                        case "名子":
                            if (e.ReceievedMessage.Length < 1)
                            {
                                e.isMismatch = true;
                                e.ResponseMessage = "請輸入姓名，不可少於一個字元";
                            }
                            userdatabaseupdate(e.CurrentPropertyName, e.ReceievedMessage);
                            break;
                        case "年齡":
                            if (e.ReceievedMessage.All(char.IsDigit))
                            {
                                Console.WriteLine(e.ReceievedMessage);
                                userdatabaseupdate(e.CurrentPropertyName, e.ReceievedMessage);
                            }
                            else
                            {
                                e.isMismatch = true;
                                e.ResponseMessage = "年齡介於0~100，請不要亂輸入";
                            }
                            break;
                        case "生日日期":
                            if (IsDate(e.ReceievedMessage))
                            {
                                Console.WriteLine(e.ReceievedMessage);
                                userdatabaseupdate(e.CurrentPropertyName, e.ReceievedMessage);
                            }
                            else
                            {
                                e.isMismatch = true;
                                e.ResponseMessage = "請輸入正確生日，請不要亂輸入，例:2000-07-20";
                            }
                            break;
                        case "身高":
                            if (e.ReceievedMessage.All(char.IsDigit))
                            {
                                Console.WriteLine(e.ReceievedMessage);
                                userdatabaseupdate(e.CurrentPropertyName, e.ReceievedMessage);
                            }
                            else
                            {
                                e.isMismatch = true;
                                e.ResponseMessage = "身高介於120~200，請不要亂輸入";
                            }
                            break;
                        case "體重":
                            if (e.ReceievedMessage.All(char.IsDigit))
                            {
                                Console.WriteLine(e.ReceievedMessage);
                                userdatabaseupdate(e.CurrentPropertyName, e.ReceievedMessage);
                            }
                            else
                            {
                                e.isMismatch = true;
                                e.ResponseMessage = "體重介於40~120，請不要亂輸入";
                            }
                            break;
                        case "活動":
                            if (e.ReceievedMessage != "低度活動" && e.ReceievedMessage != "中度活動" && e.ReceievedMessage != "高度活動")
                            {
                                e.isMismatch = true;
                                e.ResponseMessage = "你只能輸入低度活動, 中度活動, 高度活動其中之一，不要自己亂填寫哦!";
                            }
                            userdatabaseupdate(e.CurrentPropertyName, e.ReceievedMessage);
                            break;
                        default:
                            break;
                    }

                };
                //配合Line verify 
                if (LineEvent.replyToken == "00000000000000000000000000000000") return Ok();
                if (this.ReceivedMessage.events[0].type == "follow")
                {
                    isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                    var userInfo = bot.GetUserInfo(ReceivedMessage.events[0].source.userId);
                    var wellcome = $"{userInfo.displayName} 你好! 歡迎使用菓然有料\n本聊天機器人與系統將幫助你邁向健康生活\n要不我們先設定個人資料呢?";
                    isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage(wellcome);
                    m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"好的", "#設定個人資料"));
                    m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"不要", "#不設定"));
                    bot.ReplyMessage(ReceivedMessage.events[0].replyToken, m);
                    user user = new user()
                    {
                        UserID = ReceivedMessage.events[0].source.userId,
                        Name = "",
                        Age = "",
                        Birthday = "",
                        Height = "",
                        Weight = "",
                        Active = ""
                    };
                    var setter = client.Set("UserList/" + ReceivedMessage.events[0].source.userId, user);
                }
                //回覆訊息
                if (LineEvent.type == "message")
                {
                    if (LineEvent.message.type == "image") //收到圖片
                    {
                        //辨識與繪製圖片
                        string userID = ReceivedMessage.events[0].source.userId;
                        string waiting = "處理照片中...";
                        this.ReplyMessage(ReceivedMessage.events[0].replyToken, waiting);
                        string Messagess = ProcessImageAsync(LineEvent, token);
                        string a = Messagess;
                        string complete = $"照片分析完畢.\n依照分析結果您的心情為 {a}，因此我們推薦你西瓜、蘋果、香蕉，點選下面圖片連結，觀看為您推薦的水果茶品吧!";
                        this.PushMessage(userID, complete);
                        flex(userID, ChannelAccessToken);
                    }
                    else if (LineEvent.message.type == "location")
                    {
                        string userid = LineEvent.source.userId;
                        string lng = "" + LineEvent.message.longitude;
                        string lat = "" + LineEvent.message.latitude;
                        google_map_api(userid, lat, lng, token);

                    }
                    else if (LineEvent.message.type == "text")
                    {
                        if (LineEvent.message.text == "#不設定")
                        {
                            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                            var wellcome = "不設定的話無法使用此聊天機器人喔!";
                            isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage(wellcome);
                            m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"好的", "#設定個人資料"));
                            m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"不要", "#不設定"));
                            bot.ReplyMessage(ReceivedMessage.events[0].replyToken, m);
                        }
                        else if (ReceivedMessage.events[0].message.text == "#沒有問題")
                        {
                            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);

                            string Msg = $"快來拍一張來照片進行情緒辨識，或是將你的水果照片進行辨識吧!";
                            //建立一個TextMessage物件
                            isRock.LineBot.TextMessage m =
                             new isRock.LineBot.TextMessage(Msg);
                            m.quickReply.items.Add(
                                new isRock.LineBot.QuickReplyCameraAction(
                                "Show Camera", new Uri("https://image.pngaaa.com/830/1261830-middle.png")));
                            m.quickReply.items.Add(
                                new isRock.LineBot.QuickReplyCamerarollAction(
                                "Show Cameraroll", new Uri("https://www.pinclipart.com/picdir/big/164-1647836_album-collection-list-music-playlist-songs-icon-gallery.png")));
                            bot.ReplyMessage(ReceivedMessage.events[0].replyToken, m);
                        }
                        else if (ReceivedMessage.events[0].message.text == "#個人資料")
                        {
                            var result = client.Get("UserList/" + ReceivedMessage.events[0].source.userId);
                            user user = result.ResultAs<user>();
                            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                            string res = $"您的個人資料如下 \n 姓名: {user.Name} \n 年齡: {user.Age} \n生日: {user.Birthday} \n身高: {user.Height} \n體重: {user.Weight} \n活動程度: {user.Active} ";
                            //建立一個TextMessage物件
                            isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage(res);
                            m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction("重新設定個人資料", "#重新個人資料"));
                            bot.ReplyMessage(ReceivedMessage.events[0].replyToken, m);
                        }
                        else if (ReceivedMessage.events[0].message.text == "#適地性服務")
                        {
                            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                            string Msg = $"傳入你的「位置資訊」讓我們幫你找附近的超市";
                            isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage(Msg);
                            m.quickReply.items.Add(new isRock.LineBot.QuickReplyLocationAction("位置資訊", new Uri("https://i.imgur.com/YA0JixQ.png")));
                            bot.ReplyMessage(ReceivedMessage.events[0].replyToken, m);
                        }
                        else if (ReceivedMessage.events[0].message.text == "#情緒分析")
                        {
                            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                            string Msg = $"傳入你的「自拍照」讓我們幫你分析你的現在心情";
                            //建立一個TextMessage物件
                            isRock.LineBot.TextMessage m =new isRock.LineBot.TextMessage(Msg);
                            m.quickReply.items.Add(new isRock.LineBot.QuickReplyCameraAction("Show Camera", new Uri("https://image.pngaaa.com/830/1261830-middle.png")));
                            m.quickReply.items.Add( new isRock.LineBot.QuickReplyCamerarollAction("Show Cameraroll", new Uri("https://www.pinclipart.com/picdir/big/164-1647836_album-collection-list-music-playlist-songs-icon-gallery.png")));
                            bot.ReplyMessage(ReceivedMessage.events[0].replyToken, m);
                        }
                        else if (ReceivedMessage.events[0].message.text == "#設定個人資料" || ReceivedMessage.events[0].message.text == "#重新個人資料" || confirm == false)
                        {
                            confirm = false;
                            //定義接收CIC結果的類別
                            ProcessResult<LeaveRequestV2> result;
                            if (ReceivedMessage.events[0].message.text == "#設定個人資料" || ReceivedMessage.events[0].message.text == "#重新個人資料")
                            {
                                //把訊息丟給CIC 
                                result = CIC.Process(ReceivedMessage.events[0], true);
                                responseMsg = "開始個人資料設定程序\n";
                            }
                            else
                            {
                                //把訊息丟給CIC 
                                result = CIC.Process(ReceivedMessage.events[0]);
                            }
                            //處理 CIC回覆的結果
                            switch (result.ProcessResultStatus)
                            {
                                case ProcessResultStatus.Processed:
                                    if (result.ResponseButtonsTemplateCandidate != null)
                                    {
                                        //如果有template Message，直接回覆，否則放到後面一起回覆
                                        isRock.LineBot.Utility.ReplyTemplateMessage(
                                            ReceivedMessage.events[0].replyToken,
                                            result.ResponseButtonsTemplateCandidate,
                                            ChannelAccessToken);
                                        return Ok();
                                    }
                                    //取得候選訊息發送
                                    responseMsg += result.ResponseMessageCandidate;
                                    isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, responseMsg, ChannelAccessToken);
                                    break;
                                case ProcessResultStatus.Done:
                                    responseMsg += result.ResponseMessageCandidate;
                                    responseMsg += $"以下是您的資料\n";
                                    responseMsg += Newtonsoft.Json.JsonConvert.SerializeObject(result.ConversationState.ConversationEntity);
                                    var results = client.Get("UserList/" + ReceivedMessage.events[0].source.userId);
                                    user user = results.ResultAs<user>();
                                    confirm = true;
                                    string res = $"您的個人資料如下 \n 姓名: {user.Name} \n 年齡: {user.Age} \n生日: {user.Birthday} \n身高: {user.Height} \n體重: {user.Weight} \n活動程度: {user.Active} ";
                                    isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, res, ChannelAccessToken);
                                    sendconfirmsg(ReceivedMessage.events[0].source.userId, ChannelAccessToken);
                                    break;
                                case ProcessResultStatus.Pass:
                                    responseMsg = $"來看看我們推薦你今日的食譜，或是將你的照片進行辨識吧! \n如果想要設定個人資料，請跟我說 : 『#設定個人資料』";
                                    isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, responseMsg, ChannelAccessToken);
                                    break;
                                case ProcessResultStatus.Exception:
                                    //取得候選訊息發送
                                    responseMsg += result.ResponseMessageCandidate;
                                    isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, responseMsg, ChannelAccessToken);
                                    break;
                                case ProcessResultStatus.Break:
                                    //取得候選訊息發送
                                    responseMsg += result.ResponseMessageCandidate;
                                    isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, responseMsg, ChannelAccessToken);
                                    break;
                                case ProcessResultStatus.InputDataFitError:
                                    responseMsg += "資料型態不合";
                                    responseMsg += result.ResponseMessageCandidate;
                                    isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, responseMsg, ChannelAccessToken);
                                    break;
                                default:
                                    //取得候選訊息發送
                                    responseMsg += result.ResponseMessageCandidate;
                                    isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, responseMsg, ChannelAccessToken);
                                    break;
                            }
                            //回覆用戶訊息
                        }
                    }
                    else
                    {
                        this.ReplyMessage(LineEvent.replyToken, "這是展示人臉辨識的LINE Bot，請拍一張有人的照片給我唷...");
                    }

                }
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //如果發生錯誤，傳訊息給Admin
                this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }

        /// <summary>
        /// 處理照片
        /// </summary>
        /// <param name="LineEvent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private string ProcessImageAsync(isRock.LineBot.Event LineEvent, string token)
        {
            //取得照片從LineEvent取得用戶上傳的圖檔bytes
            var byteArray = isRock.LineBot.Utility.GetUserUploadedContent(LineEvent.message.id, token);
            //取得圖片檔案FileStream, 分別作為繪圖與分析用
            Stream MemStream1 = new MemoryStream(byteArray);
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(MemStream1);
            string ImgurURL = "";
            using (MemoryStream m = new MemoryStream())
            {
                bmp.Save(m, System.Drawing.Imaging.ImageFormat.Png);
                ImgurURL = UploadImage2Imgur(m.ToArray());
            }
            string dect = TryFunction(ImgurURL);
            string FaceData = formatFaceApi(dect);//結果是要放入資料庫的


            return FaceData;
        }

        //Upload Image to Imgur
        private string UploadImage2Imgur(byte[] bytes)
        {
            var Imgur_CLIENT_ID = "50140947a620cb3";
            var Imgur_CLIENT_SECRET = "579e3c98ced5660c5628fb426436ae474b3a928d";

            //建立 ImgurClient準備上傳圖片
            var client = new ApiClient(Imgur_CLIENT_ID, Imgur_CLIENT_SECRET);
            var httpClient = new HttpClient();

            var endpoint = new ImageEndpoint(client, httpClient);
            IImage image;
            //上傳Imgur
            image = endpoint.UploadImageAsync(new MemoryStream(bytes)).GetAwaiter().GetResult();

            return image.Link;
        }
        static string TryFunction(string URL)
        {
            string host = "https://react-native-face-test.cognitiveservices.azure.com/face/v1.0/detect?returnFaceAttributes=age,emotion";
            string subscriptionKey = "541a8e95880049fabb04e5944019974d";
            var body = new { url = URL };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(host);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = client.SendAsync(request).Result;
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                return jsonResponse;
            }
        }
        static string formatFaceApi(string FaceData)
        {
            string result = "";
            string[] FaceDataArray = FaceData.Split(new char[7] { '[', '{', '"', ':', ',', '}', ']' });
            FaceDataArray = FaceDataArray.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            foreach (var item in FaceDataArray)
                result += item + "\n";
            string[] emotion = { "憤怒", "藐視", "厭惡", "恐懼", "開心", "普通", "悲傷", "驚訝" };
            double[] emotion_value = new double[emotion.Length];
            int index = 0;
            for (int i = 16; i < 31; i += 2)
            {
                emotion_value[index] = Convert.ToDouble(FaceDataArray[i]);
                index++;
            }
            double maxValue = emotion_value.Max();
            int maxIndex = emotion_value.ToList().IndexOf(maxValue);
            return emotion[maxIndex];
            /* 用來處理當出現兩個人臉時
            var searchData = Array.FindAll(sArray, (v) => { return v.StartsWith("faceId"); });
           if (searchData.Length == 1)
           {
        Console.WriteLine("只有一個face ID");
           }
           */
        }
        static void flex(string userid, string token)
        { //提供三個水果選項的，之後要補水果名稱與相對應知圖片網址
            var flex = @"
[
{
""type"":""flex"",
    ""altText"": ""this is flex message"",
    ""contents"": 
{
  ""type"": ""bubble"",
  ""body"": {
                ""type"": ""box"",
    ""layout"": ""vertical"",
    ""contents"": [
      {
                    ""type"": ""image"",
        ""url"": ""https://i.imgur.com/kAVKapB.png"",
        ""size"": ""full"",
        ""aspectMode"": ""cover"",
        ""aspectRatio"": ""1:1"",
        ""gravity"": ""center"",
        ""action"": {
                        ""type"": ""uri"",
          ""label"": ""action"",
          ""uri"": ""http://linecorp.com/""
        }
                },
      {
                    ""type"": ""box"",
        ""layout"": ""vertical"",
        ""contents"": [],
        ""position"": ""absolute"",
        ""background"": {
                        ""type"": ""linearGradient"",
          ""angle"": ""0deg"",
          ""endColor"": ""#00000000"",
          ""startColor"": ""#00000099""
        },
        ""width"": ""100%"",
        ""height"": ""40%"",
        ""offsetBottom"": ""0px"",
        ""offsetStart"": ""0px"",
        ""offsetEnd"": ""0px""
      },
      {
                    ""type"": ""box"",
        ""layout"": ""horizontal"",
        ""contents"": [
          {
                        ""type"": ""box"",
            ""layout"": ""vertical"",
            ""contents"": [
              {
                            ""type"": ""box"",
                ""layout"": ""horizontal"",
                ""contents"": [
                  {
                                ""type"": ""text"",
                    ""text"": ""菓然有料"",
                    ""size"": ""xl"",
                    ""color"": ""#ffffff"",
                    ""style"": ""normal"",
                    ""position"": ""relative"",
                    ""align"": ""start"",
                    ""weight"": ""bold""
                  }
                ]
              }
            ],
            ""spacing"": ""xs""
          }
        ],
        ""position"": ""absolute"",
        ""offsetBottom"": ""0px"",
        ""offsetStart"": ""0px"",
        ""offsetEnd"": ""0px"",
        ""paddingAll"": ""20px""
      }
    ],
    ""paddingAll"": ""0px""
  }
        }
}
]";
            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(token);
            bot.PushMessageWithJSON(userid, flex);
        }
        public bool IsDate(string strDate)
        {
            try
            {
                DateTime.Parse(strDate);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void sendconfirmsg(string UserID, string ChannelAccessToken)
        {
            var ConfirmMsg = new isRock.LineBot.ConfirmTemplate();
            ConfirmMsg.altText = "請在手機上觀看";
            ConfirmMsg.text = "請確認以上資料有無錯誤";
            var actions = new List<isRock.LineBot.TemplateActionBase>();
            actions.Add(new isRock.LineBot.MessageAction() { label = "沒問題", text = "#沒有問題" });
            actions.Add(new isRock.LineBot.MessageAction() { label = "有問題", text = "#重新個人資料" });
            ConfirmMsg.actions = actions;
            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
            bot.PushMessage(UserID, ConfirmMsg);
        }
        static void google_map_api(string userid, string lat, string lng, string token)
        {
            var ChannelAccessToken = token;
            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
            string location0_name, location0_lat, location0_lng, location0_placeID, location1_name, location1_lat, location1_lng, location1_placeID, location2_name, location2_lat, location2_lng, location2_placeID;
            string[] PlaceID = new string[3];
            string[] addr = new string[3];
            string[] tel = { "無", "無", "無" };
            string sURL = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={lat},{lng}&radius=1250&type=supermarket&keyword=%E9%A4%90%E5%BB%B3&key=AIzaSyBlHt4hrRVMmnCvuPV-fPNiiYxbCoHwL90";
            using (var client = new WebClient())
            using (var stream = client.OpenRead(sURL))
            using (var reader = new StreamReader(stream))
            using (var json = new JsonTextReader(reader))
            {
                var jObject = Newtonsoft.Json.Linq.JObject.Load(json);
                location0_name = (string)jObject["results"][0]["name"];
                location0_lat = (string)jObject["results"][0]["geometry"]["location"]["lat"];
                location0_lng = (string)jObject["results"][0]["geometry"]["location"]["lng"];
                location0_placeID = (string)jObject["results"][0]["place_id"];
                PlaceID[0] = location0_placeID;
                location1_name = (string)jObject["results"][1]["name"];
                location1_lat = (string)jObject["results"][1]["geometry"]["location"]["lat"];
                location1_lng = (string)jObject["results"][1]["geometry"]["location"]["lng"];
                location1_placeID = (string)jObject["results"][1]["place_id"];
                PlaceID[1] = location1_placeID;
                location2_name = (string)jObject["results"][2]["name"];
                location2_lat = (string)jObject["results"][2]["geometry"]["location"]["lat"];
                location2_lng = (string)jObject["results"][2]["geometry"]["location"]["lng"];
                location2_placeID = (string)jObject["results"][2]["place_id"];
                PlaceID[2] = location2_placeID;
            }
            int index = 0;
            while (index < 3)
            {
                string sURL2 = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={PlaceID[index]}&language=zh-TW&key=AIzaSyBlHt4hrRVMmnCvuPV-fPNiiYxbCoHwL90";
                using (var client = new WebClient())
                using (var stream = client.OpenRead(sURL2))
                using (var reader = new StreamReader(stream))
                using (var json = new JsonTextReader(reader))
                {
                    var jObject = Newtonsoft.Json.Linq.JObject.Load(json);
                    addr[index] = "店家地址 : " + (string)jObject["result"]["formatted_address"];
                    tel[index] = "店家電話 : " + (string)jObject["result"]["formatted_phone_number"];
                }
                index++;
            }
            var flex = @"
                        [{""type"": ""template"",
                          ""altText"": ""this is a carousel template"",
                          ""template"": {
                                        ""type"": ""carousel"",
                            ""columns"": [
                              {
                                            ""title"": ""標題1"",
                                ""text"": ""地址資訊1 \r\n 電話資訊1"",
                                ""actions"": [
                                  {
                                                ""type"": ""uri"",
                                    ""label"": ""跳轉Google Map"",
                                    ""uri"": ""uri1""
                                  }
                                ],
                                ""imageBackgroundColor"": ""#F7F7F7""
                              },
                              {
                                            ""title"": ""標題2"",
                                ""text"": ""地址資訊2\r\n電話資訊2"",
                                ""actions"": [
                                  {
                                                ""type"": ""uri"",
                                    ""label"": ""跳轉Google Map"",
                                    ""uri"": ""uri2""
                                  }
                                ],
                                ""imageBackgroundColor"": ""#F7F7F7""
                              },
                              {
                                            ""title"": ""標題3"",
                                ""text"": ""地址資訊3 \r\n 電話資訊3"",
                                ""actions"": [
                                  {
                                                ""type"": ""uri"",
                                    ""label"": ""跳轉Google Map"",
                                    ""uri"": ""uri3""
                                  }
                                ],
                                ""imageBackgroundColor"": ""#FFFFFF""
                              }
                            ]
                          }
                                }
                        ] ";

            string uri1 = $"https://www.google.com/maps/search/?api=1&query={location0_lat},{location0_lng}&query_place_id={location0_placeID}";
            string uri2 = $"https://www.google.com/maps/search/?api=1&query={location1_lat},{location1_lng}&query_place_id={location1_placeID}";
            string uri3 = $"https://www.google.com/maps/search/?api=1&query={location2_lat},{location2_lng}&query_place_id={location2_placeID}";
            flex = flex.Replace("uri1", uri1);
            flex = flex.Replace("uri2", uri2);
            flex = flex.Replace("uri3", uri3);
            flex = flex.Replace("標題1", location0_name);
            flex = flex.Replace("標題2", location1_name);
            flex = flex.Replace("標題3", location2_name);
            flex = flex.Replace("地址資訊1", addr[0]);
            flex = flex.Replace("地址資訊2", addr[1]);
            flex = flex.Replace("地址資訊3", addr[2]);
            flex = flex.Replace("電話資訊1", tel[0]);
            flex = flex.Replace("電話資訊2", tel[1]);
            flex = flex.Replace("電話資訊3", tel[2]);
            bot.PushMessage(userid, "以下是離你最近的三個超市");
            bot.PushMessageWithJSON(userid, flex);

        }
        public void userdatabaseupdate(string item, string ans)
        {
            IFirebaseConfig fcon = new FirebaseConfig()
            {
                AuthSecret = "bW4Tp1NHyMLMkWo1loJcqf9gVzKgzmYw6LHF1Eu3",
                BasePath = "https://dailyjuicy-40e64-default-rtdb.firebaseio.com/"
            };
            IFirebaseClient client = new FireSharp.FirebaseClient(fcon);

            var results = client.Get("UserList/" + ReceivedMessage.events[0].source.userId);
            user users = results.ResultAs<user>();
            switch (item)
            {
                case "名子":
                    user name = new user()
                    {
                        UserID = users.UserID,
                        Name = ans,
                        Age = "",
                        Birthday = "",
                        Height = "",
                        Weight = "",
                        Active = ""
                    };
                    var setter_name = client.Update("UserList/" + users.UserID, name);
                    break;
                case "年齡":
                    user age = new user()
                    {
                        UserID = users.UserID,
                        Name = users.Name,
                        Age = ans,
                        Birthday = "",
                        Height = "",
                        Weight = "",
                        Active = ""
                    };
                    var setter_age = client.Update("UserList/" + users.UserID, age);
                    break;
                case "生日日期":
                    user birth = new user()
                    {
                        UserID = users.UserID,
                        Name = users.Name,
                        Age = users.Age,
                        Birthday = ans,
                        Height = "",
                        Weight = "",
                        Active = ""
                    };
                    var setter_birth = client.Update("UserList/" + users.UserID, birth);
                    break;
                case "身高":
                    user height = new user()
                    {
                        UserID = users.UserID,
                        Name = users.Name,
                        Age = users.Age,
                        Birthday = users.Birthday,
                        Height = ans,
                        Weight = "",
                        Active = ""
                    };
                    var setter_height = client.Update("UserList/" + users.UserID, height);
                    break;
                case "體重":
                    user weight = new user()
                    {
                        UserID = users.UserID,
                        Name = users.Name,
                        Age = users.Age,
                        Birthday = users.Birthday,
                        Height = users.Height,
                        Weight = ans,
                        Active = ""
                    };
                    var setter_weight = client.Update("UserList/" + users.UserID, weight);
                    break;
                case "活動":
                    user active = new user()
                    {
                        UserID = users.UserID,
                        Name = users.Name,
                        Age = users.Age,
                        Birthday = users.Birthday,
                        Height = users.Height,
                        Weight = users.Weight,
                        Active = ans
                    };
                    var setter_active = client.Update("UserList/" + users.UserID, active);
                    break;
            }
        }

        public class LeaveRequestV2 : ConversationEntity
        {

            [Question("請問您的名字是?")]
            [Order(1)]
            public string 名子 { get; set; }

            [Question("請問您的年齡是?")]
            [Order(2)]
            public string 年齡 { get; set; }


            [Question("請問您的生日日期是? 例:2000-07-20")]
            [Order(3)]
            public string 生日日期 { get; set; }

            [Question("請問您的身高(CM)是?")]
            [Order(4)]
            public float 身高 { get; set; }

            [Question("請問您的體重(Kg)是?")]
            [Order(5)]
            public float 體重 { get; set; }
            [ButtonsTemplateQuestion("詢問", "請問您的活動量是?", "https://i.imgur.com/RU8e4rc.png", "低度活動", "中度活動", "高度活動")]
            [Question("請問您你的活動量多少呢?")]
            [Order(6)]
            public string 活動 { get; set; }
        }
        public class user
        {
            public string UserID { get; set; }
            public string Name { get; set; }
            public string Age { get; set; }
            public string Birthday { get; set; }
            public string Height { get; set; }
            public string Weight { get; set; }
            public string Active { get; set; }

        }
    }
}

