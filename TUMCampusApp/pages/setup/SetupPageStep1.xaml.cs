﻿using System;
using System.Text.RegularExpressions;
using TUMCampusAppAPI.Managers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TUMCampusAppAPI;
using TUMCampusApp.Classes;
using Data_Manager;
using System.Threading.Tasks;

namespace TUMCampusApp.Pages.Setup
{
    public sealed partial class SetupPageStep1 : Page
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 24/12/2016  Created [Fabian Sauter]
        /// </history>
        public SetupPageStep1()
        {
            this.InitializeComponent();
            string uId = Settings.getSettingString(SettingsConsts.USER_ID);
            studentID_tbx.Text = uId == null ? "" : uId;
            populateFacultiesComboBox();
            faculty_cbox.SelectedIndex = Settings.getSettingInt(SettingsConsts.FACULTY_INDEX);
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
        /// Checks whether the current student id is valid.
        /// </summary>
        private bool isIdValid()
        {
            Regex reg = new Regex("[a-z]{2}[0-9]{2}[a-z]{3}");
            return reg.Match(studentID_tbx.Text.ToLower()).Success;
        }

        /// <summary>
        /// Adds all faculties to the faculty_cbox.
        /// </summary>
        private void populateFacultiesComboBox()
        {
            foreach (Faculties f in Enum.GetValues(typeof(Faculties)))
            {
                faculty_cbox.Items.Add(new ComboBoxItem()
                {
                    Content = UiUtils.GetLocalizedString(f.ToString() + "_Text"),

                });
            }
        }

        private void enableNextButton()
        {
            next_btn.IsEnabled = true;
            next_prgr.Visibility = Visibility.Collapsed;
        }

        private void disableNextButton()
        {
            next_btn.IsEnabled = false;
            next_prgr.Visibility = Visibility.Visible;
        }

        private async Task showMessageDialogAsync(string title, string msg)
        {
            MessageDialog message = new MessageDialog(msg)
            {
                Title = title
            };
            await message.ShowAsync();
        }

        private async Task showErrorMessageDialogAsync(string msg)
        {
            await showMessageDialogAsync(UiUtils.GetLocalizedString("Error_Text"), msg);
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void skip_btn_Click(object sender, RoutedEventArgs e)
        {
            Settings.setSetting(SettingsConsts.TUM_ONLINE_ENABLED, false);
            Settings.setSetting(SettingsConsts.HIDE_WIZARD_ON_STARTUP, true);
            if (Window.Current.Content is Frame f)
            {
                f.Navigate(typeof(MainPage2));
            }
        }

        private async void next_btn_ClickAsync(object sender, RoutedEventArgs e)
        {
            disableNextButton();
            if (!isIdValid())
            {
                await showErrorMessageDialogAsync(UiUtils.GetLocalizedString("InvalidId_Text"));
                enableNextButton();
            }
            else if (faculty_cbox.SelectedIndex < 0)
            {
                await showErrorMessageDialogAsync(UiUtils.GetLocalizedString("SelectFaculty_Text"));
                enableNextButton();
            }
            else
            {
                if (tumOnlineToken_tbx.Visibility == Visibility.Collapsed)
                {
                    string studentId = studentID_tbx.Text.ToLower();
                    int facultyIndex = faculty_cbox.SelectedIndex;
                    Task t = Task.Run(async () =>
                    {
                        string result = null;
                        try
                        {
                            result = await TumManager.INSTANCE.reqestNewTokenAsync(studentId);
                        }
                        catch (Exception ex)
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                await showErrorMessageDialogAsync(UiUtils.GetLocalizedString("RequestTokenError_Text") + ex.Message);
                                enableNextButton();
                            });
                            return;
                        }

                        if (result == null)
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => await showErrorMessageDialogAsync(UiUtils.GetLocalizedString("RequestNewTokenError_Text")));
                        }
                        else
                        {
                            Settings.setSetting(SettingsConsts.FACULTY_INDEX, facultyIndex);
                            Settings.setSetting(SettingsConsts.USER_ID, studentId);
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                if (Window.Current.Content is Frame f)
                                {
                                    f.Navigate(typeof(SetupPageStep2));
                                }
                            });
                        }
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => enableNextButton());
                    });
                }
                else
                {
                    string token = tumOnlineToken_tbx.Text.ToUpper();
                    if (!TumManager.INSTANCE.isTokenValid(token))
                    {
                        await showErrorMessageDialogAsync(UiUtils.GetLocalizedString("InvalidToken_Text"));
                        enableNextButton();
                    }
                    else
                    {
                        Settings.setSetting(SettingsConsts.FACULTY_INDEX, faculty_cbox.SelectedIndex);
                        Settings.setSetting(SettingsConsts.USER_ID, studentID_tbx.Text.ToLower());
                        TumManager.INSTANCE.saveToken(token);
                        if (Window.Current.Content is Frame f)
                        {
                            f.Navigate(typeof(SetupPageStep2));
                        }
                    }
                }
            }
        }

        private void useExistingToken_btn_Click(object sender, RoutedEventArgs e)
        {
            if (tumOnlineToken_tbx.Visibility == Visibility.Collapsed)
            {
                tumOnlineToken_tbx.Visibility = Visibility.Visible;
                useExistingToken_btn.Content = UiUtils.GetLocalizedString("SetupPage1DontUseExistingToken");
            }
            else
            {
                tumOnlineToken_tbx.Visibility = Visibility.Collapsed;
                useExistingToken_btn.Content = UiUtils.GetLocalizedString("SetupPage1UseExistingToken");
            }
        }

        #endregion
    }
}
