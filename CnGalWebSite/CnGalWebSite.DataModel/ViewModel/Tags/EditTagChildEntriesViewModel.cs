﻿using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class EditTagChildEntriesViewModel : BaseEditModel
    {
        public List<RelevancesModel> Entries { get; set; } = new List<RelevancesModel>();
    }
}
