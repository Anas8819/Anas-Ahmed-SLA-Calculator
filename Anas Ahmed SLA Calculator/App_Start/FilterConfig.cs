﻿using System.Web;
using System.Web.Mvc;

namespace Anas_Ahmed_SLA_Calculator
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}