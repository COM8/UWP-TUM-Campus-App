﻿using System;
using Canteens.Classes.Manager;
using Shared.Classes;
using Shared.Classes.Collections;
using Storage.Classes;
using Storage.Classes.Models.Canteens;

namespace UI_Context.Classes.Templates.Pages.Content
{
    public class CanteensPageTemplate: AbstractDataTemplate
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly CustomObservableCollection<Canteen> CANTEENS = new CustomObservableCollection<Canteen>(true);
        public readonly CustomObservableCollection<Dish> DISHES = new CustomObservableCollection<Dish>(true);

        private Canteen _SelectedCanteen;
        public Canteen SelectedCanteen
        {
            get => _SelectedCanteen;
            set => SetSelectedCanteen(value);
        }
        private bool _IsLoadingCanteens;
        public bool IsLoadingCanteens
        {
            get => _IsLoadingCanteens;
            set => SetProperty(ref _IsLoadingCanteens, value);
        }
        private bool _IsLoadingDishes;
        public bool IsLoadingDishes
        {
            get => _IsLoadingDishes;
            set => SetProperty(ref _IsLoadingDishes, value);
        }
        private bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => SetProperty(ref _IsLoading, value);
        }
        private DateTime _DishDate;
        public DateTime DishDate
        {
            get => _DishDate;
            set => SetProperty(ref _DishDate, value);
        }

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public CanteensPageTemplate()
        {
            DishDate = DateTime.MaxValue;
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public void SetSelectedCanteen(Canteen value)
        {
            if (SetProperty(ref _SelectedCanteen, value, nameof(SelectedCanteen)))
            {
                if (!(value is null))
                {
                    Storage.Classes.Settings.SetSetting(SettingsConsts.LAST_SELECTED_CANTEEN_ID, value.Id);
                }
                DISHES.Clear();
                if (value is null)
                {
                    DishDate = DishManager.INSTANCE.GetNextDate(value.Id, DateTime.Now.AddDays(-1));
                }
                else
                {
                    DishDate = DateTime.MaxValue;
                }
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
