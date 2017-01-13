﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUMCampusApp.classes.managers;
using TUMCampusApp.classes.userData;
using Windows.Data.Xml.Dom;

namespace TUMCampusApp.classes.tum
{
    class TUMOnlineRequest
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private string addition;
        private List<string> parameterArguments;
        private List<string> parameters;

        private static readonly string SERVICE_BASE_URL = "https://campus.tum.de/tumonline/wbservicesbasic.";

        #endregion
        //--------------------------------------------------------Construktor:----------------------------------------------------------------\\
        #region --Construktoren--
        public TUMOnlineRequest(TUMOnlineConst tumOC)
        {
            this.addition = tumOC.ToString();
            this.parameterArguments = new List<string>();
            this.parameters = new List<string>();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public async Task<string> doRequestAsync()
        {
            //Debug.WriteLine(buildUrl());
            return await NetUtils.downloadStringAsync(buildUrl());
        }

        public async Task<XmlDocument> doRequestDocumentAsync()
        {
            return await XmlDocument.LoadFromUriAsync(buildUrl());
        }

        public void addParameter(string param, string arg)
        {
            this.parameters.Add(param);
            this.parameterArguments.Add(arg);

        }

        public void addToken()
        {
            addParameter(Const.P_TOKEN, TumManager.getToken());
        }

        #endregion

        #region --Misc Methods (Private)--
        private Uri buildUrl()
        {
            string s = SERVICE_BASE_URL + addition;
            for(int i = 0; i < parameters.Count; i++)
            {
                if(i == 0)
                {
                    s += "?";
                }
                else
                {
                    s += "&";
                }
                s += parameters[i] + "=" + parameterArguments[i];
            }
            return new Uri(s);
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
