﻿using Shared.Classes;
using TumOnline.Classes.Managers;

namespace UI_Context.Classes.Templates.Controls
{
    public class TumIdTextBoxControlTemplate: AbstractDataTemplate
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private string _Text;
        public string Text
        {
            get => _Text;
            set => SetTextProperty(value);
        }

        private bool _IsValid;
        public bool IsValid
        {
            get => _IsValid;
            set => SetProperty(ref _IsValid, value);
        }

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        private void SetTextProperty(string value)
        {
            value = value.ToLowerInvariant();
            if (SetProperty(ref _Text, value, nameof(Text)))
            {
                IsValid = AccessManager.IsTumIdValid(value);
            }
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
