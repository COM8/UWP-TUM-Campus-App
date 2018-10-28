﻿using Data_Manager;
using Logging;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TUMCampusAppAPI.DBTables;
using Windows.Data.Json;

namespace TUMCampusAppAPI.Managers
{
    public class NewsDBManager : AbstractTumDBManager
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public static NewsDBManager INSTANCE;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 06/01/2017 Created [Fabian Sauter]
        /// </history>
        public NewsDBManager()
        {

        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        /// <summary>
        /// Returns the most current news id from the db.
        /// </summary>
        private string getLastNewsId()
        {
            string lastId = "";
            List<NewsTable> list = dB.Query<NewsTable>(true, "SELECT id FROM " + DBTableConsts.NEWS_TABLE + " ORDER BY id DESC LIMIT 1;");
            if (list != null && list.Count > 0)
            {
                lastId += list[0].id;
            }
            return lastId;
        }

        /// <summary>
        /// Returns all news sources from the db.
        /// </summary>
        /// <returns>A list of NewsSourceTable elements.</returns>
        public List<NewsSourceTable> getAllNewsSourcesFormDb()
        {
            return dB.Query<NewsSourceTable>(true, "SELECT * FROM " + DBTableConsts.NEWS_SOURCE_TABLE + ";");
        }

        public List<NewsTable> getNewsWithImage()
        {
            return dB.Query<NewsTable>(true, "SELECT * FROM " + DBTableConsts.NEWS_TABLE + " WHERE imageUrl IS NOT NULL AND imageUrl != '';");
        }

        /// <summary>
        /// Returns all news from the db in descending order by date.
        /// Also only returns these, where the NewsSourceTable is not disabled.
        /// </summary>
        /// <returns>A list of NewsTable elements.</returns>
        public List<NewsTable> getAllNewsFormDb()
        {
            return dB.Query<NewsTable>(true, "SELECT n.* FROM " + DBTableConsts.NEWS_TABLE + " n JOIN " + DBTableConsts.NEWS_SOURCE_TABLE + " s ON n.src = s.src WHERE s.enabled = ? ORDER BY date DESC;", true);
        }

        /// <summary>
        /// Returns a list of news, their date is greater or equal to yesterdays date.
        /// Also the first tumMovie news, that is equal or bigger than todays date gets added to the list.
        /// Will not add read news.
        /// </summary>
        /// <returns>Returns a list of NewsTable elements, max 11 entries.</returns>
        public List<NewsTable> getNewsForHomePage()
        {
            List<NewsTable> news = getAllNewsFormDb();
            List<NewsTable> result = new List<NewsTable>();
            DateTime tumMovieDate = DateTime.MaxValue;
            int tumMovieIndex = -1;
            DateTime yesterday = DateTime.Now.AddDays(-1);

            // First run get next tumMovie:
            for (int i = 0; i < news.Count; i++)
            {
                if (news[i].read)
                {
                    continue;
                }

                if (news[i].date.CompareTo(yesterday.Date) < 0)
                {
                    break;
                }

                if (news[i].src.Equals("2"))
                {
                    if (news[i].date.Date.CompareTo(DateTime.Now.Date) >= 0 && news[i].date.CompareTo(tumMovieDate) < 0)
                    {
                        tumMovieIndex = i;
                        tumMovieDate = news[i].date;
                    }
                }
            }
            if (tumMovieIndex >= 0)
            {
                result.Insert(0, news[tumMovieIndex]);
            }

            // Second run add current news from yesterday and today:
            for (int i = 0; i < news.Count; i++)
            {
                if (news[i].read || news[i].src.Equals("2"))
                {
                    continue;
                }

                if (result.Count >= 10 || news[i].date.Date.CompareTo(yesterday.Date) < 0)
                {
                    break;
                }

                if (news[i].date.Date.CompareTo(yesterday.Date) >= 0)
                {
                    result.Add(news[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the NewsSourceTable for the given id.
        /// </summary>
        /// <param name="src">The src of the NewsSourceTable.</param>
        /// <returns>Returns the NewsSourceTable for the given id.</returns>
        public NewsSourceTable getNewsSource(string src)
        {
            List<NewsSourceTable> sources = dB.Query<NewsSourceTable>(true, "SELECT * FROM " + DBTableConsts.NEWS_SOURCE_TABLE + " WHERE src LIKE ?;", src);
            return sources.Count > 0 ? sources[0] : null;
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public void updateNewsRead(string id, bool read)
        {
            dB.Execute("UPDATE " + DBTableConsts.NEWS_TABLE + " SET read = ? WHERE id = ?;", read, id);
        }

        /// <summary>
        /// Tries to download news if it is necessary and caches them into the local db.
        /// </summary>
        /// <param name="force">Forces to redownload all news.</param>
        /// <returns>Returns the syncing task or null if did not sync.</returns>
        public Task downloadNews(bool force)
        {
            if (!force && Settings.getSettingBoolean(SettingsConsts.ONLY_USE_WIFI_FOR_UPDATING) && !DeviceInfo.isConnectedToWifi())
            {
                return null;
            }

            waitForSyncToFinish();
            REFRESHING_TASK_SEMA.Wait();
            refreshingTask = Task.Run(async () =>
            {
                if ((force || SyncDBManager.INSTANCE.needSync(DBTableConsts.NEWS_TABLE, Consts.VALIDITY_THREE_HOURS).NEEDS_SYNC) && DeviceInfo.isConnectedToInternet())
                {
                    if (force)
                    {
                        clearCachedNewsImages();
                    }
                    try
                    {
                        Uri url;
                        if (force)
                        {
                            url = new Uri(Consts.NEWS_URL);
                        }
                        else
                        {
                            url = new Uri(Consts.NEWS_URL + getLastNewsId());
                        }
                        JsonArray jsonArray = await NetUtils.downloadJsonArrayAsync(url);
                        if (jsonArray == null)
                        {
                            return;
                        }

                        cleanupDb();
                        List<NewsTable> news = new List<NewsTable>();
                        foreach (JsonValue val in jsonArray)
                        {
                            try
                            {

                                NewsTable n = new NewsTable(val.GetObject());
                                news.Add(n);
                                if (!string.IsNullOrEmpty(n.imageUrl))
                                {
                                    Task t = CacheDBManager.INSTANCE.cacheImageAsync(new Uri(n.imageUrl));
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error("Caught an exception during parsing news!", e);
                            }
                        }
                        foreach (NewsTable n in news)
                        {
                            dB.InsertOrReplace(n);
                        }
                        SyncDBManager.INSTANCE.update(new SyncTable(DBTableConsts.NEWS_TABLE));
                    }
                    catch (Exception e)
                    {
                        SyncDBManager.INSTANCE.update(new SyncTable(DBTableConsts.NEWS_TABLE, SyncResult.STATUS_ERROR_UNKNOWN, e.ToString()));
                    }
                }
            });
            REFRESHING_TASK_SEMA.Release();

            return refreshingTask;
        }

        /// <summary>
        /// Tries to download news sources if it is necessary and caches them into the local db.
        /// </summary>
        /// <param name="force">Forces to redownload all news sources.</param>
        /// <returns>Returns the syncing task or null if did not sync.</returns>
        public Task downloadNewsSources(bool force)
        {
            if (!force && Settings.getSettingBoolean(SettingsConsts.ONLY_USE_WIFI_FOR_UPDATING) && !DeviceInfo.isConnectedToWifi())
            {
                return null;
            }

            waitForSyncToFinish();
            REFRESHING_TASK_SEMA.Wait();
            refreshingTask = Task.Run(async () =>
            {
                if ((force || SyncDBManager.INSTANCE.needSync(DBTableConsts.NEWS_SOURCE_TABLE, Consts.VALIDITY_ONE_MONTH).NEEDS_SYNC) && DeviceInfo.isConnectedToInternet())
                {
                    try
                    {
                        JsonArray jsonArray = await NetUtils.downloadJsonArrayAsync(new Uri(Consts.NEWS_SOURCES_URL));
                        if (jsonArray == null)
                        {
                            return;
                        }

                        List<NewsSourceTable> list = new List<NewsSourceTable>();
                        foreach (JsonValue val in jsonArray)
                        {
                            try
                            {
                                list.Add(new NewsSourceTable(val.GetObject()));
                            }
                            catch (Exception e)
                            {
                                Logger.Error("Caught an exception during parsing news sources!", e);
                            }
                        }
                        replaceNewsSources(list);
                        SyncDBManager.INSTANCE.update(new SyncTable(DBTableConsts.NEWS_SOURCE_TABLE));
                    }
                    catch (Exception e)
                    {
                        SyncDBManager.INSTANCE.update(new SyncTable(DBTableConsts.NEWS_SOURCE_TABLE, SyncResult.STATUS_ERROR_UNKNOWN, e.ToString()));
                    }
                }
            });
            REFRESHING_TASK_SEMA.Release();

            return refreshingTask;
        }

        /// <summary>
        /// Updates the disabled status for a specific news source.
        /// </summary>
        /// <param name="id">The id of that specific news source.</param>
        /// <param name="enabled">Whether to enable it.</param>
        public void updateNewsSourceStatus(int id, bool enabled)
        {
            dB.Execute("UPDATE " + DBTableConsts.NEWS_SOURCE_TABLE + " SET enabled = ? WHERE id = ?;", enabled, id);
        }

        #endregion

        #region --Misc Methods (Private)--
        /// <summary>
        /// Removes all old items (older than 3 months) from the db.
        /// </summary>
        private void cleanupDb()
        {
            List<NewsTable> all = getAllNewsFormDb();
            DateTime date = DateTime.Now.AddMonths(-3);
            foreach (var item in all)
            {
                if (item.date.CompareTo(date) < 0)
                {
                    dB.Execute("DELETE FROM " + DBTableConsts.NEWS_TABLE + " WHERE id = ?;", item.id);
                }
            }
        }

        /// <summary>
        /// Deletes all NewsSourceTable entries from the NewsSourceTable table and replaces them with the new ones.
        /// It keeps whether the NewsSourceTable was enabled.
        /// </summary>
        /// <param name="list">A list of NewsSourceTable objects.</param>
        private void replaceNewsSources(List<NewsSourceTable> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                NewsSourceTable old = getNewsSource(list[i].src);
                if (old != null)
                {
                    list[i].enabled = old.enabled;
                }
            }
            dB.DeleteAll<NewsSourceTable>();
            dB.InsertAll(list);
        }

        /// <summary>
        /// Starts a new task and clears all cached news images.
        /// </summary>
        private void clearCachedNewsImages()
        {
            Task.Run(async () =>
            {
                List<Uri> uris = new List<Uri>();
                foreach (NewsTable n in getNewsWithImage())
                {
                    uris.Add(new Uri(n.imageUrl));
                }
                await ImageCache.Instance.RemoveAsync(uris);
            });
        }

        #endregion

        #region --Misc Methods (Protected)--
        protected override void dropTables()
        {
            dB.DropTable<NewsTable>();
            dB.DropTable<NewsSourceTable>();
        }

        protected override void createTables()
        {
            dB.CreateTable<NewsTable>();
            dB.CreateTable<NewsSourceTable>();
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}