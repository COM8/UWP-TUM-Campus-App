﻿using System;
using System.Threading.Tasks;
using Shared.Classes;
using UI_Context.Classes;
using UI_Context.Classes.Context.Pages;
using UI_Context.Classes.Templates.Pages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UI.Pages
{
    public sealed partial class SetupPage: Page
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly SetupPageContext VIEW_MODEL = new SetupPageContext();

        /// <summary>
        /// Where should we navigate the frame to once we finished?
        /// </summary>
        private Type doneTargetPage = null;
        private VisualState curViewState;
        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public SetupPage()
        {
            InitializeComponent();
            VIEW_MODEL.MODEL.PropertyChanged += MODEL_PropertyChanged;
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void UpdateViewState(VisualState newState)
        {
            if (newState == curViewState)
            {
                return;
            }

            if (newState == State_2)
            {
                VIEW_MODEL.StopAutoActivationCheck();
            }
            VisualStateManager.GoToState(this, newState.Name, true);
            curViewState = newState;
            if (newState == State_2)
            {
                VIEW_MODEL.StartAutoActivationCheck();
            }
        }

        private void NavigateAway()
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                return;
            }

            if (doneTargetPage is null)
            {
                UiUtils.NavigateToPage(typeof(MainPage));
            }
            else
            {
                UiUtils.NavigateToPage(doneTargetPage);
            }

            // Make sure we remove the last entry from the back stack to prevent navigation back to this page:
            UiUtils.RemoveLastBackStackEntry();
        }

        private async Task RequestTokenAsync()
        {
            if (await VIEW_MODEL.RequestNewTokenAsync())
            {
                UpdateViewState(State_2);
            }
            else
            {
                info_ian.Show(Localisation.GetLocalizedString("SetupPage.RequestTokenFailed.Text"));
            }
        }

        private async Task CheckIfTokenIsActivatedAsync()
        {
            if (await VIEW_MODEL.CheckIfTokenIsActivatedAsync())
            {
                OnTokenActivated();
            }
            else
            {
                info_ian.Show(Localisation.GetLocalizedString("SetupPage.TokenActivationFailed.Text"));
            }
        }

        private void OnTokenActivated()
        {
            VIEW_MODEL.StoreIdAndToken();
            UpdateViewState(State_3);
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Type type)
            {
                doneTargetPage = type;
            }
            titleBar.OnPageNavigatedTo();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            titleBar.OnPageNavigatedFrom();
        }

        private async void WhatIsTumOnline_link_Click(object sender, RoutedEventArgs e)
        {
            await VIEW_MODEL.OnWhatIsTumOnlineAsync().ConfAwaitFalse();
        }

        private async void next1_ipbtn_Click(Controls.IconProgressButtonControl sender, RoutedEventArgs args)
        {
            await RequestTokenAsync().ConfAwaitFalse();
        }

        private void cancel1_ibtn_Click(Controls.IconButtonControl sender, RoutedEventArgs args)
        {
            NavigateAway();
        }

        private async void TumIdTextBoxControl_EnterKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            await RequestTokenAsync().ConfAwaitFalse();
        }

        private async void mail_link_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await VIEW_MODEL.OpenDefaultMailAppAsync().ConfAwaitFalse();
        }

        private async void tumOnline_link_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await VIEW_MODEL.OnWhatIsTumOnlineAsync().ConfAwaitFalse();
        }

        private void cancel2_ibtn_Click(Controls.IconButtonControl sender, RoutedEventArgs args)
        {
            NavigateAway();
        }

        private void back2_ibtn_Click(Controls.IconButtonControl sender, RoutedEventArgs args)
        {
            UpdateViewState(State_1);
        }

        private async void check2_ipbtn_Click(Controls.IconProgressButtonControl sender, RoutedEventArgs args)
        {
            await CheckIfTokenIsActivatedAsync().ConfAwaitFalse();
        }

        private void done3_ibtn_Click(Controls.IconButtonControl sender, RoutedEventArgs args)
        {
            NavigateAway();
        }

        private void MODEL_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SetupPageTemplate.IsTokenActivated) when VIEW_MODEL.MODEL.IsTokenActivated:
                    OnTokenActivated();
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
