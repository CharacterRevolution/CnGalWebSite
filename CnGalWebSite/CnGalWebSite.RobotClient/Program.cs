﻿using CnGalWebSite.Helper.Helper;
using CnGalWebSite.RobotClient;
using MeowMiraiLib.Msg.Type;
using CnGalWebSite.DataModel.Model;
using System.Linq.Dynamic.Core.Tokenizer;
using Masuda.Net;
using Masuda.Net.HelpMessage;

var httpClient = new HttpClient();
var cacheX = new CacheX();


OutputHelper.Repeat();
OutputHelper.WriteCenter("CnGal资料站 看板娘 v3.3.22", 1.8);
OutputHelper.Repeat();


Console.WriteLine("->读取配置文件");
var settingX = new SettingX();
settingX.Init();

Console.WriteLine("->读取关注的QQ群");
var groupX = new GroupX(settingX.BasicSetting, httpClient);
groupX.Init();

Console.WriteLine("->读取自动回复");
var replyX = new ReplyX(settingX.BasicSetting, httpClient, settingX.MessageArgs);
replyX.Init();


Console.WriteLine("->读取表情");
var faceX = new FaceX(settingX.BasicSetting, httpClient, settingX.MessageArgs);
replyX.Init();

Console.WriteLine("->读取事件");
var eventX = new EventX(settingX.BasicSetting, httpClient, settingX.MessageArgs);
eventX.Init();

Console.WriteLine("->读取敏感词列表");
var sensitiveWordX = new SensitiveWordX(settingX.BasicSetting);
await sensitiveWordX.InitAsync();

Console.WriteLine("->初始化消息处理模块");
var messageX = new MessageX(settingX.BasicSetting, httpClient, replyX.Replies, cacheX.ReplyCache, settingX.MessageArgs, faceX.Faces, sensitiveWordX);

var clientX = new ClientX(settingX.BasicSetting, messageX, groupX, settingX.MessageArgs);
    try
    {
        Console.WriteLine("->初始化 SDK 客户端");
        clientX.Init();
    }
    catch (Exception ex)
    {
        OutputHelper.PressError(ex, "初始化 SDK 客户端失败", "配置文件错误或 依赖项 未正确启动");
    }
var miraiTask = Task.Run(async () =>
{



    var c = clientX.MiraiClient;

    _ = c.ConnectAsync();

    //定时任务计时器
    System.Timers.Timer t = new(1000 * 60); //每一分钟查看一次
    t.Start(); //启动计时器
    t.Elapsed += async (s, e) =>
    {
        var message = eventX.GetCurrentTimeEvent();
        if (string.IsNullOrWhiteSpace(message) == false)
        {
            var result = await messageX.ProcMessageAsync(RobotReplyRange.Group,message, "", null, 0, null);

            if (result != null)
            {
                foreach (var item in groupX.Groups)
                {
                    result.SendTo = item.GroupId;
                    clientX.SendMessage(result);
                }
            }
        }
    };

    //随机任务计时器
    System.Timers.Timer t2 = new(1000 * 60); //每一分钟查看一次
    t2.Start(); //启动计时器
    t2.Elapsed += async (s, e) =>
    {
        var message = eventX.GetProbabilityEvents();
        if (string.IsNullOrWhiteSpace(message) == false)
        {
            var result = await messageX.ProcMessageAsync(RobotReplyRange.Group, message, "", null, 0, null);

            if (result != null)
            {
                foreach (var item in groupX.Groups)
                {
                    result.SendTo = item.GroupId;
                    clientX.SendMessage(result);
                }
            }
        }
    };

    c.OnFriendMessageReceive += async (s, e) =>
    {
        try
        {
            await clientX.ReplyFromFriendAsync(s, e);
        }
        catch (Exception ex)
        {
            OutputHelper.PressError(ex);
        }

    };
    //c.OnTempMessageReceive+= async (s, e) =>
    //{
    //    try
    //    {
    //        await clientX.ReplyFromTempAsync(s, e, CnGalWebSite.DataModel.Model.RobotReplyRange.Friend);
    //    }
    //    catch (Exception ex)
    //    {
    //        OutputHelper.PressError(ex);
    //    }

    //};

    c.OnGroupMessageReceive += async (s, e) =>
    {
        try
        {
            await clientX.ReplyFromGroupAsync(s, e);
        }
        catch (Exception ex)
        {
            OutputHelper.PressError(ex);
        }

    };

    while (true)
    {
        await Task.Delay(10000);
    }

});
var masudaTask = Task.Run(async () =>
{

    while (true)
    {
        await Task.Delay(10000);
    }

});

//定时刷新数据定时器
if (settingX.BasicSetting.IntervalTime > 0)
{
    var refreshTimer = new System.Threading.Timer(new System.Threading.TimerCallback(Refresh), null, 0, 1000 * settingX.BasicSetting.IntervalTime);
    async void Refresh(object o)
    {
        await groupX.RefreshAsync();
        await replyX.RefreshAsync();
        await eventX.RefreshAsync();
        await faceX.RefreshAsync();
}
}
// 现以加入默认配置 私域 简洁初始化
MasudaBot MasudaBot2
    = new(102013323, "0vd9SZUDMMFyVpyv", "SpFFlqNMSxpeh4I0OWtOgp9gor6cZK4I", BotType.Private);

// 普通消息事件注册(需要私域)
MasudaBot2.MessageAction += async (bot, msg, type) =>
{
    // 回复文本
    await bot.ReplyMessageAsync(msg, "muda");
    // 组合消息
    await bot.ReplyMessageAsync(msg, new PlainMessage("muda"), new AtMessage(msg.Author.Id));
    //
};

while (true)
{
    switch (System.Console.ReadLine()) // 控制台操作
    {
        //.....
    }

}
