using isRock.LineBot;
using isRock.LineBot.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LinebotConversationExample.Controllers
{
    public class CICTestController : ApiController
    {
        string[] sArray;
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
        public void sendconfirmsg (string UserID,string ChannelAccessToken)
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
        [HttpPost]
        public IHttpActionResult POST()
        {
            const string ChannelAccessToken = "S3o7moA62IOh6lIIdD5ECoK4zCKGa9D6C9vR0wzkmX2KaQDtj6Zcp7Q+ijNHS1WV2S5fx2a7a+oFKhIehTvm2dDqMD2qLYRQgbN4/6yq+inaRzj7eU0glrmQy0G2PErxMS5Erm9wQifEaQ/xD55QoQdB04t89/1O/w1cDnyilFU=";
            var responseMsg = "";
            bool confirm = false;
            string[] profile = new string[6];

            try
            {
                //定義資訊蒐集者
                isRock.LineBot.Conversation.InformationCollector<LeaveRequestV2> CIC =
                    new isRock.LineBot.Conversation.InformationCollector<LeaveRequestV2>(ChannelAccessToken);
                CIC.OnMessageTypeCheck += (s, e) => {
                    switch (e.CurrentPropertyName)
                    {
                        case "名子":
                            if (e.ReceievedMessage.Length<1)
                            {
                                e.isMismatch = true;
                                e.ResponseMessage = "請輸入姓名，不可少於一個字元";
                            }
                            profile[0] = e.ReceievedMessage;
                            break;
                        case "年齡":
                            if (e.ReceievedMessage.All(char.IsDigit))
                            {
                                 Console.WriteLine(e.ReceievedMessage);
                                 profile[1] = e.ReceievedMessage;
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
                                profile[2] = e.ReceievedMessage;
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
                                profile[3] = e.ReceievedMessage;
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
                                profile[4] = e.ReceievedMessage;
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
                            profile[5] = e.ReceievedMessage;
                            break;
                        default:
                            break;
                    }

                };

                //取得 http Post RawData(should be JSO
                string postData = Request.Content.ReadAsStringAsync().Result;
                //剖析JSON
                var ReceivedMessage = isRock.LineBot.Utility.Parsing(postData);
                if (ReceivedMessage.events[0].type == "follow")
                {
                    isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                    var userInfo = bot.GetUserInfo(ReceivedMessage.events[0].source.userId);
                    var wellcome = $"{userInfo.displayName} 你好! 歡迎使用舒果溢\n本聊天機器人與各功能由陳亭妤、許沛涵、黃子豪、張柏榮共同開發\n要不我們先設定個人資料呢?";
                    isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage(wellcome);
                    m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"好的", "#設定個人資料"));
                    m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"不要", "#不設定"));
                    bot.ReplyMessage(ReceivedMessage.events[0].replyToken, m);
                }
                if (ReceivedMessage.events[0].type == "message")
                {
                    if (ReceivedMessage.events[0].message.text == "#不設定")
                    {
                        isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                        var wellcome = "不設定的話無法使用此聊天機器人喔!";
                        isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage(wellcome);
                        m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"好的", "#設定個人資料"));
                        m.quickReply.items.Add(new isRock.LineBot.QuickReplyMessageAction($"不要", "#不設定"));
                        bot.ReplyMessage(ReceivedMessage.events[0].replyToken, m);
                    }
                    else if(ReceivedMessage.events[0].message.text == "#沒有問題")
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
                    else if(ReceivedMessage.events[0].message.text == "#個人資料")
                    {
                        isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                        string res = $"您的個人資料如下 \n 姓名:  '黃子豪' \n 年齡: '21' \n生日:  '2000-07-20' \n身高: 177.0 \n體重: 88.0 \n活動程度: '中度活動' ";
                        //建立一個TextMessage物件
                        isRock.LineBot.TextMessage m =
                         new isRock.LineBot.TextMessage(res);
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
                                string token = ReceivedMessage.events[0].replyToken;
                                responseMsg += result.ResponseMessageCandidate;
                                responseMsg += $"以下是您的資料\n";
                                responseMsg += Newtonsoft.Json.JsonConvert.SerializeObject(result.ConversationState.ConversationEntity);
                                sArray = Newtonsoft.Json.JsonConvert.SerializeObject(result.ConversationState.ConversationEntity).Split(new char[4] { '{', '}', ',' ,':'});//分別以!還有~y作為分隔符號
                                confirm = true;
                                string res = $"您的個人資料如下 \n姓名: {sArray[2]} \n年齡: {sArray[4]} \n生日: {sArray[6]} \n身高: {sArray[8]} \n體重: {sArray[10]} \n活動程度: {sArray[12]} ";
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
                    else
                    {
                        isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);
                        string Msg = $"來看看我們推薦你今日的食譜，或是將你的照片進行辨識吧!";
                        bot.ReplyMessage(ReceivedMessage.events[0].replyToken, Msg);
                    }

                }
                    //回覆API OK
                    return Ok();
            }
            catch (Exception ex)
            {
                //如果你要偵錯的話
                isRock.LineBot.Utility.PushMessage("Ua3d3e1675bca2f5e468a6c80bf49f332", ex.Message, ChannelAccessToken);
                return Ok();
                
            }
        }
    }

    /// <summary>
    /// 用來表達一個對話
    /// </summary>
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
