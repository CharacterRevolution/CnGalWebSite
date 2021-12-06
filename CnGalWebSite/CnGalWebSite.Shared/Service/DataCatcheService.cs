﻿using Microsoft.AspNetCore.Components;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public class DataCatcheService : IDataCacheService
    {
        /// <summary>
        /// 是否为APP模式
        /// </summary>
        public bool? IsApp { get; set; } = null;
        /// <summary>
        /// 刷新渲染框架方法
        /// </summary>
        public EventCallback RefreshApp { get; set; }
        /// <summary>
        /// 保存主题设置
        /// </summary>
        public EventCallback SavaTheme { get; set; }

        /// <summary>
        /// 身份验证成功后获得的标识 有效期一小时
        /// </summary>
        public string LoginKey { get; set; } = string.Empty;
        /// <summary>
        /// 第三方登入成功 服务端也验证成功后 返回唯一标识 有效期一小时
        /// </summary>
        public ThirdPartyLoginTempModel ThirdPartyLoginTempModel { get; set; } = null;
        /// <summary>
        /// 历史用户临时储存用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        /// <summary>
        /// 是否正在使用第三方登入
        /// </summary>
        public bool IsOnThirdPartyLogin { get; set; } = true;
        /// <summary>
        /// 用户二次身份验证方式
        /// </summary>
        public UserAuthenticationTypeModel UserAuthenticationTypeModel { get; set; } = new UserAuthenticationTypeModel();
        /// <summary>
        /// 主页数据缓存
        /// </summary>
        public HomeViewModel HomeViewModel { get; set; } = null;
        /// <summary>
        /// 主页渲染次数
        /// </summary>
        public int RenderTimes { get; set; } = 0;
        /// <summary>
        /// 主题设置
        /// </summary>
        public ThemeModel ThemeSetting { get; set; } = new ThemeModel();

        /// <summary>
        /// 词条主页数据缓存
        /// </summary>
        public IPageModelCatche<EntryIndexViewModel> EntryIndexPageCatche { get; set; }
        /// <summary>
        /// 文章主页数据缓存
        /// </summary>
        public IPageModelCatche<ArticleViewModel> ArticleIndexPageCatche { get; set; }
        /// <summary>
        /// 周边主页数据缓存
        /// </summary>
        public IPageModelCatche<PeripheryViewModel> PeripheryIndexPageCatche { get; set; }
        /// <summary>
        /// 标签主页数据缓存
        /// </summary>
        public IPageModelCatche<TagIndexViewModel> TagIndexPageCatche { get; set; }
        /// <summary>
        /// 主页动态数据缓存
        /// </summary>
        public IPageModelCatche<List<HomeNewsAloneViewModel>> HomePageNewsCatche { get; set; }
        /// <summary>
        /// 主页轮播图数据缓存
        /// </summary>
        public IPageModelCatche<List<DataModel.Model.Carousel>> HomePageCarouselsCatche { get; set; }

        /// <summary>
        /// 评论详情
        /// </summary>
        public CommentViewModel DetailComment { get; set; } = new CommentViewModel();
        /// <summary>
        /// 当前登入的用户的信息
        /// </summary>
        public UserInforViewModel UserInfor { get; set; } = new UserInforViewModel { Ranks = new List<RankViewModel>() };
        /// <summary>
        /// 主页缓存
        /// </summary>
        public List<KeyValuePair<List<CnGalWebSite.Shared.AppComponent.Normal.Cards.MainImageCardModel>, string>> HomeListCards { get; set; } = new List<KeyValuePair<List<AppComponent.Normal.Cards.MainImageCardModel>, string>>();

        private readonly HttpClient _httpClient;

        public DataCatcheService(HttpClient httpClient, IPageModelCatche<EntryIndexViewModel> entryIndexPageCatche, IPageModelCatche<ArticleViewModel> articleIndexPageCatche,
        IPageModelCatche<PeripheryViewModel> peripheryIndexPageCatche, IPageModelCatche<TagIndexViewModel> tagIndexPageCatche, IPageModelCatche<List<HomeNewsAloneViewModel>> homePageNewsCatche,
        IPageModelCatche<List<DataModel.Model.Carousel>> homePageCarouselsCatche)
        {
            _httpClient = httpClient;
            (EntryIndexPageCatche = entryIndexPageCatche).Init(ToolHelper.WebApiPath + "api/entries/GetEntryView/");
            (ArticleIndexPageCatche = articleIndexPageCatche).Init(ToolHelper.WebApiPath + "api/articles/GetArticleView/");
            (PeripheryIndexPageCatche = peripheryIndexPageCatche).Init(ToolHelper.WebApiPath + "api/peripheries/GetPeripheryView/");
            (TagIndexPageCatche = tagIndexPageCatche).Init(ToolHelper.WebApiPath + "api/tags/gettag/");
            HomePageNewsCatche = homePageNewsCatche;
            HomePageCarouselsCatche = homePageCarouselsCatche;
        }

        public async Task<List<CnGalWebSite.Shared.AppComponent.Normal.Cards.MainImageCardModel>> GetHomePageListCardMode(string apiUrl, string type, int maxCount, bool isRefresh)
        {
            //查找是否存在缓存
            if (isRefresh == false)
            {
                var model = HomeListCards.FirstOrDefault(s => s.Value == apiUrl);
                if (model.Key != null)
                {
                    return model.Key;
                }
            }
            else
            {
                HomeListCards.RemoveAll(s => s.Value == apiUrl);
            }

            //获取新数据
            var items = new List<CnGalWebSite.Shared.AppComponent.Normal.Cards.MainImageCardModel>();
            try
            {
                var model = await _httpClient.GetFromJsonAsync<List<EntryHomeAloneViewModel>>(ToolHelper.WebApiPath + apiUrl);
                //转换数据
                foreach (var item in model.Take(maxCount))
                {
                    items.Add(new AppComponent.Normal.Cards.MainImageCardModel
                    {
                        CommentCount = item.CommentCount,
                        Image = item.Image,
                        Name = item.DisPlayName,
                        ReadCount = item.ReadCount,
                        Url = string.IsNullOrWhiteSpace(type) ? item.DisPlayValue : (type + "/index/" + item.Id)
                    });
                }
                HomeListCards.Add(new KeyValuePair<List<CnGalWebSite.Shared.AppComponent.Normal.Cards.MainImageCardModel>, string>(items, apiUrl));
                return items;
            }
            catch (Exception)
            {
                return null;
            }
        }


    }
}
