﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Coments
{
    public class EditVoteCanCommentModel
    {
        public long[] Ids { get; set; } = Array.Empty<long>();

        public bool CanComment { get; set; }
    }
}
