﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TUMCampusApp.Pages;
using TUMCampusAppAPI.DBTables;
using TUMCampusAppAPI.Managers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TUMCampusApp.Controls
{
    public sealed partial class NewsWidget : UserControl
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private WidgetControl widgetControl;
        private HomePage homePage;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 17/06/2017 Created [Fabian Sauter]
        /// </history>
        public NewsWidget(WidgetControl widgetControl, HomePage homePage)
        {
            this.widgetControl = widgetControl;
            this.homePage = homePage;
            this.InitializeComponent();
            Task.Factory.StartNew(() => showNews());
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void showNews()
        {
            Task t1 = NewsManager.INSTANCE.downloadNewsSourcesAsync(false);
            Task t2 = NewsManager.INSTANCE.downloadNewsAsync(false);
            Task.WaitAll(t1, t2);

            List<NewsTable> news = NewsManager.INSTANCE.getNewsForHomePage();
            if (news == null || news.Count <= 0)
            {
                return;
            }

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (NewsTable item in news)
                {
                    homePage.addWidget(new WidgetControl()
                    {
                        WidgetContent = new NewsControl() { News = item },
                        Visibility = Visibility.Visible
                    });
                }
            }).AsTask();
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
