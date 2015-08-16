﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tik4net
{
    public interface ITikTrapSentence: ITikSentence
    {
        string CategoryCode { get; }

        string CategoryDescription { get; }

        string Message { get; }
    }
}