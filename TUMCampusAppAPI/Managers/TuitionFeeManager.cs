﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TUMCampusAppAPI.DBTables;
using TUMCampusAppAPI.TUMOnline;
using Windows.Data.Xml.Dom;

namespace TUMCampusAppAPI.Managers
{
    public class TuitionFeeManager : AbstractManager
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public static TuitionFeeManager INSTANCE;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 05/01/2017 Created [Fabian Sauter]
        /// </history>
        public TuitionFeeManager()
        {
            dB.CreateTable<TUMTuitionFeeTable>();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        /// <summary>
        /// Downloads your personal tuition fees.
        /// </summary>
        /// <returns>Returns the downloaded tuition fees in form of a XmlDocument.</returns>
        private async Task<XmlDocument> getFeeStatusAsync()
        {
            TUMOnlineRequest req = new TUMOnlineRequest(TUMOnlineConst.TUITION_FEE_STATUS);
            req.addToken();
            return await req.doRequestDocumentAsync();
        }

        /// <summary>
        /// Searches in the local db for all tuition fees and returns them.
        /// </summary>
        /// <returns>Returns all found tuition fees.</returns>
        public List<TUMTuitionFeeTable> getFees()
        {
            return dB.Query<TUMTuitionFeeTable>("SELECT * FROM TUMTuitionFeeTable WHERE money NOT LIKE 0");
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public async override Task InitManagerAsync()
        {
            //await downloadFeesAsync(false);
        }

        /// <summary>
        /// Tries to download your tuition fees if it is necessary and caches them into the local db.
        /// </summary>
        /// <param name="force">Forces to redownload all tuition fees.</param>
        /// <returns>Returns an async Task.</returns>
        public async Task downloadFeesAsync(bool force)
        {
            if(!force && Util.getSettingBoolean(Const.ONLY_USE_WIFI_FOR_UPDATING) && !DeviceInfo.isConnectedToWifi())
            {
                return;
            }
            if ((force || SyncManager.INSTANCE.needSync(this, CacheManager.VALIDITY_ONE_DAY).NEEDS_SYNC) && DeviceInfo.isConnectedToInternet())
            {
                XmlDocument doc = await getFeeStatusAsync();
                dB.DropTable<TUMTuitionFeeTable>();
                dB.CreateTable<TUMTuitionFeeTable>();
                foreach (var element in doc.SelectNodes("/rowset/row"))
                {
                    dB.Insert(new TUMTuitionFeeTable(element));
                }
                SyncManager.INSTANCE.replaceIntoDb(new SyncTable(this));
            }
            return;
        }

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
