using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.CustomAttributes
{
    public class CustomErrorMessage
    {
        public string PostMessage(string s, Boolean bError)
        {
            string sText = "";

            if (bError)
            {
                sText = "<div class='alert alert-danger fade in'>" +
                                "<span class='close' data-dismiss='alert'>×</span>" +
                                "<i class='fa fa-times fa-2x pull-left'></i>" +
                                "<p>" + s + "</p>" +
                                "</div>";
            }
            else
            {
                sText = "<div class='alert alert-success fade in'>" +
                                "<span class='close' data-dismiss='alert'>×</span>" +
                                "<i class='fa fa-check fa-2x pull-left'></i>" +
                                "<p>" + s + "</p>" +
                                "</div>";
            }

            if (s == "")
            {
                sText = "";
            }

            return sText;
        }
    }
}