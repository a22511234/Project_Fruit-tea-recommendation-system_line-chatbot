# 菓然有料-情緒辨識之水果茶飲推薦系統

**110年東吳大學資訊管理學系畢業專題製作**

開發一套以水果代替人工果糖的手做水果茶飲推薦系統，根據**人臉情緒辨識**客製每日推薦水果、利用水果營養分析辨識讓使用者了解水果的營養價值，並透過圖像化的營養記錄功能，鼓勵使用者多多攝取水果並養成長久的習慣。
此部分的程式碼為情緒辨識之水果茶飲推薦系統其中Line聊天機器人的部分


## 情緒辨識之水果茶飲推薦系統-Line聊天機器人

#### Line聊天機器人之系統特色

1. 情緒分析水果推薦
	當使用者使用臉部情緒辨識功能，透過Azure Face API即時分析使用者當下的情緒反應，系統會針對不同情緒分析結果給予對應的水果品項推薦，並將其結果存入系統。
2. 適地性服務：
	使用者能藉由定位功能找到商店，進而購買所需的水果。推薦的商店將會參考商家與使用者的距離、商家資訊、使用者評論以進行綜合評分。

#### Line聊天機器人之使用之技術
1.	Visual Studio：作為Line Chat Bot 的整合式開發工具。
2.	Azure Cosmos Data Base：作為後端資料存取及寫入的資料庫，儲存個人資料。 
3.	Azure Cognitive Services-Face：用於臉部辨識，分析使用者情緒強度，了解使用者身心狀況，並作為水果推薦的考量。
4.	Google Maps Platform & Google Place Platform：使用此工具推薦使用者附近地點的資訊，並進行定位服務、給予使用者附近店家以購買水果。

### 流程圖 Flowchart

[![](https://i.imgur.com/lgBUhf2.png)](https://i.imgur.com/lgBUhf2.png)


## 參考資源
LineBotSDK：https://www.nuget.org/packages/LineBotSDK
<br/>線上課程：https://www.udemy.com/line-bot/
<br/>電子書：https://www.pubu.com.tw/ebook/103305

### End
