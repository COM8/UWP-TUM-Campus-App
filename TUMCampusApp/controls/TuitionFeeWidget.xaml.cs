﻿using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TUMCampusAppAPI.Managers;
using TUMCampusAppAPI.TUMOnline;
using TUMCampusAppAPI.TUMOnline.Exceptions;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TUMCampusApp.Controls
{
    public sealed partial class TuitionFeeWidget : UserControl
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private DropShadowPanel dSP;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 22/01/2017 Created [Fabian Sauter]
        /// </history>
        public TuitionFeeWidget(DropShadowPanel dSP)
        {
            this.dSP = dSP;
            this.InitializeComponent();
            Task.Factory.StartNew(() => ShowTuitionFeesAsync());
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        /// <summary>
        /// Refreshes and shows the current tuition fee status. Or hides the widget if no outstanding fees are found.
        /// This method has to get called in a separate task.
        /// </summary>
        private async Task ShowTuitionFeesAsync()
        {
            try
            {
                await TuitionFeeManager.INSTANCE.downloadFeesAsync(false);
            }
            catch (BaseTUMOnlineException e)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    showFees(null);
                });
                return;
            }
            List<TUMTuitionFee> list = TuitionFeeManager.INSTANCE.getFees();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                showFees(list);
            });
        }

        /// <summary>
        /// Shows the given fees list on the screen or hides the widget if the list is empty.
        /// </summary>
        /// <param name="list">A list of tuition fees.</param>
        private void showFees(List<TUMTuitionFee> list)
        {
            tuitionFees_stckp.Children.Clear();

            if (list == null || list.Count <= 0 || list[0].money == null)
            {
                dSP.Visibility = Visibility.Collapsed;
            }
            else
            {
                foreach (var item in list)
                {
                    if (item != null && item.money != null)
                    {
                        tuitionFees_stckp.Children.Add(new TuitionFeeControl(item) {
                            Margin = new Thickness(0, 0, 0, 10)
                        });
                    }
                }
            }
            progressRing.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
