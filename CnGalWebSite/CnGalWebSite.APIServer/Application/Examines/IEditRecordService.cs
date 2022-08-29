﻿using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Examines
{
    public interface IEditRecordService
    {
        Task<QueryData<ListUserMonitorEntryAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListUserMonitorEntryAloneModel searchModel, string userId);

        Task<QueryData<ListUserReviewEditRecordAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListUserReviewEditRecordAloneModel searchModel, string userId);

    }
}
