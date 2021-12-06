﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.ErrorCounts;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Messages;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Tables
{
    public class TableService : ITableService
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<SteamInfor, long> _steamInforRepository;
        private readonly IRepository<BasicInforTableModel, long> _basicInforTableModelRepository;
        private readonly IRepository<GroupInforTableModel, long> _groupInforTableModelRepository;
        private readonly IRepository<MakerInforTableModel, long> _makerInforTableModelRepository;
        private readonly IRepository<StaffInforTableModel, long> _staffInforTableModelRepository;
        private readonly IRepository<RoleInforTableModel, long> _roleInforTableModelRepository;
        private readonly IRepository<SteamInforTableModel, long> _steamInforTableModelRepository;


        public TableService( IRepository<BasicInforTableModel, long> basicInforTableModelRepository,IRepository<Entry, int> entryRepository,
        IRepository<GroupInforTableModel, long> groupInforTableModelRepository, IRepository<MakerInforTableModel, long> makerInforTableModelRepository,
        IRepository<StaffInforTableModel, long> staffInforTableModelRepository, IRepository<RoleInforTableModel, long> roleInforTableModelRepository,
        IRepository<SteamInforTableModel, long> steamInforTableModelRepository, IRepository<SteamInfor, long> steamInforRepository)
        {
            _entryRepository = entryRepository;
            _basicInforTableModelRepository = basicInforTableModelRepository;
            _groupInforTableModelRepository = groupInforTableModelRepository;
            _makerInforTableModelRepository = makerInforTableModelRepository;
            _staffInforTableModelRepository = staffInforTableModelRepository;
            _roleInforTableModelRepository = roleInforTableModelRepository;
            _steamInforTableModelRepository = steamInforTableModelRepository;
            _steamInforRepository = steamInforRepository;
        }

        public async Task UpdateBasicInforListAsync()
        {
            try
            {
                //通过Id获取词条

                var entries = await _entryRepository.GetAll().AsNoTracking()
                     .Include(s => s.Information).Include(s => s.Tags)
                     .Where(s => s.Type == EntryType.Game && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                     .Select(s => new
                     {
                         s.Id,
                         s.AnotherName,
                         s.DisplayName,
                         s.Name,
                         s.PubulishTime,
                         s.Information,
                         Tags = s.Tags.Select(s => s.Name).ToList()
                     })
                     .AsSingleQuery()
                     .ToListAsync();

                var Infors = new List<BasicInforTableModel>();
                //循环进行对应赋值
                foreach (var infor in entries)
                {
                    var tableModel = new BasicInforTableModel
                    {
                        RealId = infor.Id,
                        Name = infor.DisplayName ?? infor.Name,
                        IssueTime = infor.PubulishTime,
                        GameNickname = infor.AnotherName,
                        Tags = ""
                    };
                    //序列化标签
                    if (infor.Tags != null && infor.Tags.Count > 0)
                    {
                        for (var i = 0; i < infor.Tags.Count; i++)
                        {
                            tableModel.Tags += infor.Tags[i];

                            if (i != infor.Tags.Count - 1)
                            {
                                tableModel.Tags += "、";
                            }
                        }
                    }

                    //查找基本信息
                    foreach (var item in infor.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "原作":
                                    tableModel.Original = item.DisplayValue;
                                    break;
                                case "制作组":
                                    if (string.IsNullOrWhiteSpace(tableModel.ProductionGroup))
                                    {
                                        tableModel.ProductionGroup = item.DisplayValue;
                                    }
                                    break;
                                case "制作组名称":
                                    if (string.IsNullOrWhiteSpace(tableModel.ProductionGroup))
                                    {
                                        tableModel.ProductionGroup = item.DisplayValue;
                                    }
                                    break;
                                case "游戏平台":

                                    tableModel.GamePlatforms = item.DisplayValue;

                                    break;
                                case "引擎":
                                    tableModel.Engine = item.DisplayValue;
                                    break;
                                case "发行商":
                                    tableModel.Publisher = item.DisplayValue;
                                    break;
                                case "发行方式":
                                    tableModel.IssueMethod = item.DisplayValue;
                                    break;
                                case "官网":
                                    tableModel.OfficialWebsite = item.DisplayValue;
                                    break;
                                case "Steam平台Id":
                                    tableModel.SteamId = item.DisplayValue;
                                    break;
                                case "QQ群":
                                    tableModel.QQgroupGame = item.DisplayValue;
                                    break;
                            }
                        }
                    }
                    Infors.Add(tableModel);
                }

                //与数据中现有的项目对比
                //删除不存在的项目
                var currentIds = Infors.Select(s => s.RealId);

                await _basicInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
                //添加不存在的项目
                var oldIds = await _basicInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

                var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
                foreach (var item in newItems)
                {
                    await _basicInforTableModelRepository.InsertAsync(item);
                }
                //对已存在的项目进行更新
                var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
                var oldItems = await _basicInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
                foreach (var item in oldItems)
                {
                    var temp = currentItems.Find(s => s.RealId == item.RealId);
                    temp.Id = item.Id;
                    if (item.RealId != temp.RealId || item.Name != temp.Name || item.IssueTime != temp.IssueTime || item.Original != temp.Original
                        || item.ProductionGroup != temp.ProductionGroup || item.GamePlatforms != temp.GamePlatforms || item.Engine != temp.Engine
                        || item.Publisher != temp.Publisher || item.GameNickname != temp.GameNickname || item.Tags != temp.Tags || item.IssueMethod != temp.IssueMethod
                        || item.OfficialWebsite != temp.OfficialWebsite || item.SteamId != temp.SteamId || item.QQgroupGame != temp.QQgroupGame)
                    {
                        item.Name = temp.Name;
                        item.IssueTime = temp.IssueTime;
                        item.Original = temp.Original;
                        item.ProductionGroup = temp.ProductionGroup;
                        item.GamePlatforms = temp.GamePlatforms;
                        item.Engine = temp.Engine;
                        item.Publisher = temp.Publisher;
                        item.GameNickname = temp.GameNickname;
                        item.Tags = temp.Tags;
                        item.IssueMethod = temp.IssueMethod;
                        item.OfficialWebsite = temp.OfficialWebsite;
                        item.SteamId = temp.SteamId;
                        item.QQgroupGame = temp.QQgroupGame;
                        await _basicInforTableModelRepository.UpdateAsync(item);
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        public async Task UpdateGroupInforListAsync()
        {
            //通过Id获取词条 
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Information)
                .Where(s => s.Type == EntryType.ProductionGroup && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.IsHidden,
                    s.Name,
                    s.DisplayName,
                    s.AnotherName,
                    s.Id,
                    s.Information
                })
                .ToListAsync();
            var Infors = new List<GroupInforTableModel>();
            //循环进行对应赋值
            foreach (var infor in entries)
            {
                var tableModel = new GroupInforTableModel
                {
                    RealId = infor.Id,
                    Name = infor.DisplayName ?? infor.Name,
                    AnotherNameGroup = infor.AnotherName
                };
                foreach (var item in infor.Information)
                {
                    if (item.Modifier == "基本信息")
                    {
                        switch (item.DisplayName)
                        {
                            case "QQ群":
                                tableModel.QQgroupGroup = item.DisplayValue;
                                break;
                        }
                    }
                }
                Infors.Add(tableModel);
            }

            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = Infors.Select(s => s.RealId);

            await _groupInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
            //添加不存在的项目
            var oldIds = await _groupInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

            var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
            foreach (var item in newItems)
            {
                await _groupInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
            var oldItems = await _groupInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.RealId == item.RealId);
                temp.Id = item.Id;
                if (item.RealId != temp.RealId || item.Name != temp.Name || item.QQgroupGroup != temp.QQgroupGroup || item.AnotherNameGroup != temp.AnotherNameGroup)
                {
                    item.Name = temp.Name;
                    item.AnotherNameGroup = temp.AnotherNameGroup;
                    item.QQgroupGroup = temp.QQgroupGroup;

                    await _groupInforTableModelRepository.UpdateAsync(item);
                }
            }


        }

        public async Task UpdateStaffInforListAsync()
        {
            //通过Id获取词条 
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Information).ThenInclude(s => s.Additional)
                .Where(s => s.Type == EntryType.Game && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.Information,
                    s.Name,
                    s.DisplayName
                })
                .ToListAsync();
            var Infors = new List<StaffInforTableModel>();
            //循环进行对应赋值
            foreach (var inEntry in entries)
            {

                foreach (var item in inEntry.Information)
                {
                    if (item.Modifier == "STAFF")
                    {
                        var tableModel = new StaffInforTableModel
                        {
                            RealId = item.Id,
                            GameName = inEntry.DisplayName ?? inEntry.Name
                        };
                        foreach (var infor in item.Additional)
                        {
                            switch (infor.DisplayName)
                            {
                                case "职位（官方称呼）":
                                    tableModel.PositionOfficial = infor.DisplayValue;
                                    break;
                                case "昵称（官方称呼）":
                                    tableModel.NicknameOfficial = infor.DisplayValue;
                                    break;
                                case "职位（通用）":
                                    tableModel.PositionGeneral = (PositionGeneralType)Enum.Parse(typeof(PositionGeneralType), infor.DisplayValue);
                                    break;
                                case "角色":
                                    tableModel.Role = infor.DisplayValue;
                                    break;
                                case "子项目":
                                    tableModel.Subcategory = infor.DisplayValue;
                                    break;
                                case "隶属组织":
                                    tableModel.SubordinateOrganization = infor.DisplayValue;
                                    break;

                            }

                        }
                        Infors.Add(tableModel);
                    }
                }

            }
            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = Infors.Select(s => s.RealId);

            await _staffInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
            //添加不存在的项目
            var oldIds = await _staffInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

            var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
            foreach (var item in newItems)
            {
                await _staffInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
            var oldItems = await _staffInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.RealId == item.RealId);
                temp.Id = item.Id;
                if (item.RealId != temp.RealId || item.GameName != temp.GameName || item.Subcategory != temp.Subcategory || item.PositionOfficial != temp.PositionOfficial
                    || item.NicknameOfficial != temp.NicknameOfficial || item.PositionGeneral != temp.PositionGeneral || item.Role != temp.Role || item.SubordinateOrganization != temp.SubordinateOrganization)
                {
                    item.GameName = temp.GameName;
                    item.Subcategory = temp.Subcategory;
                    item.PositionOfficial = temp.PositionOfficial;
                    item.NicknameOfficial = temp.NicknameOfficial;
                    item.PositionGeneral = temp.PositionGeneral;
                    item.Role = temp.Role;
                    item.SubordinateOrganization = temp.SubordinateOrganization;

                    await _staffInforTableModelRepository.UpdateAsync(item);
                }
            }



        }

        public async Task UpdateMakerInforListAsync()
        {
            //通过Id获取词条 
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Information)
                .Where(s => s.Type == EntryType.Staff && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.DisplayName,
                    s.AnotherName,
                    s.Information
                })
                .ToListAsync();
            var Infors = new List<MakerInforTableModel>();
            //循环进行对应赋值
            foreach (var infor in entries)
            {
                var tableModel = new MakerInforTableModel
                {
                    RealId = infor.Id,
                    Name = infor.DisplayName ?? infor.Name,
                    AnotherName = infor.AnotherName

                };
                foreach (var item in infor.Information)
                {
                    if (item.Modifier == "基本信息")
                    {
                        switch (item.DisplayName)
                        {
                            case "昵称（官方称呼）":
                                tableModel.Nickname = item.DisplayValue;
                                break;
                        }
                    }
                    else if (item.Modifier == "相关网站")
                    {
                        switch (item.DisplayName)
                        {
                            case "Bilibili":
                                tableModel.Bilibili = item.DisplayValue;
                                break;
                            case "B站":
                                tableModel.Bilibili = item.DisplayValue;
                                break;
                            case "微博":
                                tableModel.MicroBlog = item.DisplayValue;
                                break;
                            case "博客":
                                tableModel.Blog = item.DisplayValue;
                                break;

                            case "Lofter":
                                tableModel.Lofter = item.DisplayValue;
                                break;
                            case "其他活动网站":
                                tableModel.Other = item.DisplayValue;
                                break;
                        }
                    }
                }
                Infors.Add(tableModel);
            }

            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = Infors.Select(s => s.RealId);

            await _makerInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
            //添加不存在的项目
            var oldIds = await _makerInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

            var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
            foreach (var item in newItems)
            {
                await _makerInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
            var oldItems = await _makerInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.RealId == item.RealId);
                temp.Id = item.Id;
                if (item.RealId != temp.RealId || item.Name != temp.Name || item.AnotherName != temp.AnotherName || item.Nickname != temp.Nickname
                    || item.Bilibili != temp.Bilibili || item.MicroBlog != temp.MicroBlog || item.Blog != temp.Blog || item.Lofter != temp.Lofter
                     || item.Other != temp.Other)
                {
                    item.Name = temp.Name;
                    item.AnotherName = temp.AnotherName;
                    item.Nickname = temp.Nickname;
                    item.MicroBlog = temp.MicroBlog;
                    item.Bilibili = temp.Bilibili;
                    item.Blog = temp.Blog;
                    item.Lofter = temp.Lofter;
                    item.Other = temp.Other;

                    await _makerInforTableModelRepository.UpdateAsync(item);
                }
            }
        }

        public async Task UpdateRoleInforListAsync()
        {
            //通过Id获取词条 
            var entries = await _entryRepository.GetAll().AsNoTracking()
                 .Include(s => s.Information)
                 .Include(s => s.Relevances)
                 .Where(s => s.Type == EntryType.Role && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                 .Select(s => new
                 {
                     s.Id,
                     s.Name,
                     s.DisplayName,
                     s.AnotherName,
                     s.Information,
                     s.Relevances
                 })
                 .ToListAsync();
            var Infors = new List<RoleInforTableModel>();
            //循环进行对应赋值
            foreach (var infor in entries)
            {
                var tableModel = new RoleInforTableModel
                {
                    RealId = infor.Id,
                    Name = infor.DisplayName ?? infor.Name,
                    AnotherNameRole = infor.AnotherName
                };
                foreach (var item in infor.Information)
                {
                    if (item.Modifier == "基本信息")
                    {
                        switch (item.DisplayName)
                        {
                            case "声优":
                                tableModel.CV = item.DisplayValue;
                                break;
                            case "性别":
                                tableModel.Gender = (GenderType)Enum.Parse(typeof(GenderType), item.DisplayValue);
                                break;
                            case "身材数据":
                                tableModel.FigureData = item.DisplayValue;
                                break;
                            case "身材(主观)":
                                tableModel.FigureSubjective = item.DisplayValue;
                                break;
                            case "生日":
                                try
                                {
                                    tableModel.Birthday = DateTime.ParseExact(item.DisplayValue, "M月d日", null);
                                }
                                catch
                                {

                                    tableModel.Birthday = null;
                                }
                                break;
                            case "发色":
                                tableModel.Haircolor = item.DisplayValue;
                                break;
                            case "瞳色":
                                tableModel.Pupilcolor = item.DisplayValue;
                                break;
                            case "服饰":
                                tableModel.ClothesAccessories = item.DisplayValue;
                                break;
                            case "性格":
                                tableModel.Character = item.DisplayValue;
                                break;
                            case "角色身份":
                                tableModel.RoleIdentity = item.DisplayValue;
                                break;
                            case "身高":
                                tableModel.RoleHeight = item.DisplayValue;
                                break;
                            case "血型":
                                tableModel.BloodType = item.DisplayValue;
                                break;
                            case "年龄":
                                tableModel.RoleAge = item.DisplayValue;
                                break;
                            case "兴趣":
                                tableModel.RoleTaste = item.DisplayValue;
                                break;
                        }
                    }


                }

                foreach (var item in infor.Relevances)
                {
                    if (item.Modifier == "游戏" && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                    {
                        if (string.IsNullOrWhiteSpace(tableModel.GameName))
                        {
                            tableModel.GameName = item.DisplayName;
                        }
                        else
                        {
                            tableModel.GameName += ("、" + item.DisplayName);
                        }
                    }
                }
                Infors.Add(tableModel);
            }

            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = Infors.Select(s => s.RealId);

            await _roleInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
            //添加不存在的项目
            var oldIds = await _roleInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

            var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
            foreach (var item in newItems)
            {
                await _roleInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
            var oldItems = await _roleInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.RealId == item.RealId);
                temp.Id = item.Id;

                if (item.RealId != temp.RealId || item.Name != temp.Name || item.CV != temp.CV || item.AnotherNameRole != temp.AnotherNameRole
                    || item.Gender != temp.Gender || item.FigureData != temp.FigureData || item.FigureSubjective != temp.FigureSubjective || item.Birthday != temp.Birthday
                    || item.Haircolor != temp.Haircolor || item.Pupilcolor != temp.Pupilcolor || item.ClothesAccessories != temp.ClothesAccessories || item.Character != temp.Character
                              || item.RoleIdentity != temp.RoleIdentity || item.BloodType != temp.BloodType || item.RoleHeight != temp.RoleHeight || item.RoleTaste != temp.RoleTaste
                     || item.RoleAge != temp.RoleAge || item.GameName != temp.GameName)
                {
                    item.Name = temp.Name;
                    item.CV = temp.CV;
                    item.AnotherNameRole = temp.AnotherNameRole;
                    item.Gender = temp.Gender;
                    item.FigureData = temp.FigureData;
                    item.FigureSubjective = temp.FigureSubjective;
                    item.Birthday = temp.Birthday;
                    item.Haircolor = temp.Haircolor;
                    item.ClothesAccessories = temp.ClothesAccessories;
                    item.Pupilcolor = temp.Pupilcolor;
                    item.Character = temp.Character;
                    item.RoleIdentity = temp.RoleIdentity;
                    item.BloodType = temp.BloodType;
                    item.RoleHeight = temp.RoleHeight;
                    item.RoleTaste = temp.RoleTaste;
                    item.RoleAge = temp.RoleAge;
                    item.GameName = temp.GameName;

                    await _roleInforTableModelRepository.UpdateAsync(item);
                }
            }

        }

        public async Task UpdateSteamInforListAsync()
        {
            //通过Id获取
            var Infors = new List<SteamInforTableModel>();
            //循环添加
            var steamList = await _steamInforRepository.GetAll().AsNoTracking().Include(s => s.Entry).ToListAsync();
            foreach (var item in steamList)
            {
                var steamInfor = item;
                if (steamInfor != null && steamInfor.Entry != null)
                {
                    var steam = new SteamInforTableModel
                    {
                        SteamId = steamInfor.SteamId,
                        OriginalPrice = steamInfor.OriginalPrice,
                        PriceNow = steamInfor.PriceNow,
                        CutNow = steamInfor.CutNow,
                        PriceLowest = steamInfor.PriceLowest,
                        CutLowest = steamInfor.CutLowest,
                        LowestTime = steamInfor.LowestTime,
                    };
                    if (item.Entry != null)
                    {
                        steam.EntryId = item.Entry.Id;
                        steam.Name = item.Entry.Name;
                    }
                    Infors.Add(steam);
                }
            }
            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = Infors.Select(s => s.EntryId);

            await _steamInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.EntryId) == false);
            //添加不存在的项目
            var oldIds = await _steamInforTableModelRepository.GetAll().Select(s => s.EntryId).ToListAsync();

            var newItems = Infors.Where(s => oldIds.Contains(s.EntryId) == false);
            foreach (var item in newItems)
            {
                await _steamInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = Infors.Where(s => oldIds.Contains(s.EntryId)).ToList();
            var oldItems = await _steamInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.EntryId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.EntryId == item.EntryId);
                temp.Id = item.Id;

                if (item.Name != temp.Name || item.SteamId != temp.SteamId || item.OriginalPrice != temp.OriginalPrice
                    || item.PriceNow != temp.PriceNow || item.CutNow != temp.CutNow || item.PriceLowest != temp.PriceLowest || item.CutLowest != temp.CutLowest
                    || item.LowestTime != temp.LowestTime)
                {
                    item.Name = temp.Name;
                    item.SteamId = temp.SteamId;
                    item.OriginalPrice = temp.OriginalPrice;
                    item.PriceNow = temp.PriceNow;
                    item.CutNow = temp.CutNow;
                    item.PriceLowest = temp.PriceLowest;
                    item.CutLowest = temp.CutLowest;
                    item.LowestTime = temp.LowestTime;

                    await _steamInforTableModelRepository.UpdateAsync(item);
                }
            }
        }

        public async Task UpdateAllInforListAsync()
        {
            await UpdateBasicInforListAsync();
            await UpdateGroupInforListAsync();
            await UpdateMakerInforListAsync();
            await UpdateRoleInforListAsync();
            await UpdateStaffInforListAsync();
            await UpdateSteamInforListAsync();
        }
    }
}
