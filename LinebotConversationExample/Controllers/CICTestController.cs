using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using isRock.LineBot.Conversation;
using Newtonsoft.Json;

namespace LineBotFaceRecognition.Controllers
{
    public class LineBotWebHookController : isRock.LineBot.LineWebHookControllerBase
    {

        [Route("api/LineBot")]
        [HttpPost]
        public IHttpActionResult POST()
        {
            //取得Web.config中的 app settings
            var token = "S3o7moA62IOh6lIIdD5ECoK4zCKGa9D6C9vR0wzkmX2KaQDtj6Zcp7Q+ijNHS1WV2S5fx2a7a+oFKhIehTvm2dDqMD2qLYRQgbN4/6yq+inaRzj7eU0glrmQy0G2PErxMS5Erm9wQifEaQ/xD55QoQdB04t89/1O/w1cDnyilFU=";
            const string AdminUserId = "Ua3d3e1675bca2f5e468a6c80bf49f332";
            var responseMsg = "";
            bool confirm = false;
            isRock.LineBot.Event LineEvent = null;
            try
            {
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

                            break;
                        case "年齡":
                            if (e.ReceievedMessage.All(char.IsDigit))
                            {
                                Console.WriteLine(e.ReceievedMessage);

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
                    var wellcome = $"{userInfo.displayName} 你好! 歡迎使用菓然有料\n本聊天機器人與各功能由陳亭妤、許沛涵、黃子豪、張柏榮共同開發\n要不我們先設定個人資料呢?";
                    isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage(wellcome);
                    m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"好的", "#設定個人資料"));
                    m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"不要", "#不設定"));
                    bot.ReplyMessage(ReceivedMessage.events[0].replyToken, m);
                }
                //回覆訊息
                if (LineEvent.type == "message")
                {
                    if (LineEvent.message.type == "image") //收到圖片
                    {
                        //辨識與繪製圖片
                        string userID = ReceivedMessage.events[0].source.userId;
                        string waiting = "處理照片中...";
                        string a = " 開心 ";
                        string complete = $"照片分析完畢.\n依照分析結果您的心情為{a}，因此我們推薦你以下三樣水果，點選其中一個進去，觀看相對應的食譜吧!";

                        isRock.LineBot.TextMessage TextMsg = new isRock.LineBot.TextMessage(waiting);
                        isRock.LineBot.TextMessage TextMsg2 = new isRock.LineBot.TextMessage(complete);
                        var Messages = new List<isRock.LineBot.MessageBase>();
                        Messages.Add(TextMsg);
                        Messages.Add(TextMsg2);
                        this.ReplyMessage(ReceivedMessage.events[0].replyToken, Messages);
                        flex(userID);
                        var Messagess = ProcessImageAsync(LineEvent, token);
                        this.PushMessage(userID, Messagess);
                    }
                    else if (LineEvent.message.type == "location")
                    {
                        this.ReplyMessage(ReceivedMessage.events[0].replyToken, "功能尚未解鎖");
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
                            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                            string res = $"您的個人資料如下 \n 姓名:  '黃子豪' \n 年齡: '21' \n生日:  '2000-07-20' \n身高: 177.0 \n體重: 88.0 \n活動程度: '中度活動' ";
                            //建立一個TextMessage物件
                            isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage(res);
                            m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction("重新設定個人資料", "#重新個人資料"));
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
                                    string temp = "等資料庫建立";
                                    confirm = true;
                                    string res = $"您的個人資料如下 \n姓名: {temp} \n年齡: {temp} \n生日: {temp} \n身高: {temp} \n體重: {temp} \n活動程度: {temp} ";
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
        private List<isRock.LineBot.MessageBase> ProcessImageAsync(isRock.LineBot.Event LineEvent, string token)
        {
            string Msg = "";
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
            Msg = $"{ImgurURL}";
            string dect = TryFunction(ImgurURL);
            string FaceData = formatFaceApi(dect);//結果是要放入資料庫的
            Msg = Msg + "\n" + FaceData;
            //建立文字訊息
            isRock.LineBot.TextMessage TextMsg = new isRock.LineBot.TextMessage(Msg);
            //建立圖形訊息(用上傳後的網址)
            isRock.LineBot.ImageMessage imageMsg = new isRock.LineBot.ImageMessage(new Uri(ImgurURL), new Uri(ImgurURL));
            //建立集合
            var Messages = new List<isRock.LineBot.MessageBase>();
            Messages.Add(TextMsg);
            Messages.Add(imageMsg);

            //一次把集合中的多則訊息回覆給用戶
            return Messages;
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
            return result;
            /* 用來處理當出現兩個人臉時
            var searchData = Array.FindAll(sArray, (v) => { return v.StartsWith("faceId"); });
           if (searchData.Length == 1)
           {
        Console.WriteLine("只有一個face ID");
           }
           */
        }
        static void flex(string userid)
        { //提供三個水果選項的，之後要補水果名稱與相對應知圖片網址
            var flex = @"
[
{
  ""type"": ""template"",
  ""altText"": ""this is an image carousel template"",
  ""template"": {
                ""type"": ""image_carousel"",
    ""columns"": [
      {
                    ""imageUrl"": ""https://i.imgur.com/8EwEXYY.png"",
        ""action"": {
                        ""type"": ""uri"",
          ""label"": ""香蕉"",
          ""uri"": ""https://i.imgur.com/8EwEXYY.png""
        }
                },
      {
                    ""imageUrl"": ""https://i.imgur.com/6C96oJv.png"",
        ""action"": {
                        ""type"": ""uri"",
          ""label"": ""蘋果"",
          ""uri"": ""https://i.imgur.com/6C96oJv.png""
        }
                },
      {
                    ""imageUrl"": ""https://i.imgur.com/FidEH2v.png"",
        ""action"": {
                        ""type"": ""uri"",
          ""label"": ""奇異果"",
          ""uri"": ""https://i.imgur.com/FidEH2v.png""
        }
                }
    ]
  }
        
}
    
]
";
            isRock.LineBot.Bot bot = new isRock.LineBot.Bot("S3o7moA62IOh6lIIdD5ECoK4zCKGa9D6C9vR0wzkmX2KaQDtj6Zcp7Q+ijNHS1WV2S5fx2a7a+oFKhIehTvm2dDqMD2qLYRQgbN4/6yq+inaRzj7eU0glrmQy0G2PErxMS5Erm9wQifEaQ/xD55QoQdB04t89/1O/w1cDnyilFU=");
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


    }
    public class LeaveRequestV2 : ConversationEntity
    {

        [Question("請問您的名子是?")]
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
        [ButtonsTemplateQuestion("詢問", "請問您的活動量是?", "https://arock.blob.core.windows.net/blogdata201706/22-124357-ad3c87d6-b9cc-488a-8150-1c2fe642d237.png", "低度活動", "中度活動", "高度活動")]
        [Question("請問您你的活動量多少呢?")]
        [Order(6)]
        public string 活動 { get; set; }
    }
}

