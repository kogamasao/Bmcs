using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Bmcs.Function
{
    /// <summary>
    /// 試行回数制限
    /// アカウント復旧機能への総当たりを防ぐため、キー単位で一定時間内の試行回数を制限する。
    /// ※メモリ上で管理するため、アプリの再起動時は初期化される。
    /// 　複数インスタンスへスケールアウトする場合はインスタンス毎の制限となるため、
    /// 　Redis等の共有キャッシュへの移行が必要。
    /// </summary>
    public class RateLimiter : IRateLimiter
    {
        private readonly IMemoryCache MemoryCache;

        private readonly object LockObject = new object();

        public RateLimiter(IMemoryCache memoryCache)
        {
            MemoryCache = memoryCache;
        }

        public bool TryAttempt(string key, int limitCount, int limitMinute)
        {
            lock (LockObject)
            {
                var attemptDatetimeList = GetAttemptDatetimeList(key, limitMinute);

                if (attemptDatetimeList.Count >= limitCount)
                {
                    Save(key, attemptDatetimeList, limitMinute);

                    return false;
                }

                attemptDatetimeList.Add(DateTime.UtcNow);

                Save(key, attemptDatetimeList, limitMinute);

                return true;
            }
        }

        public bool IsBlocked(string key, int limitCount, int limitMinute)
        {
            lock (LockObject)
            {
                return GetAttemptDatetimeList(key, limitMinute).Count >= limitCount;
            }
        }

        public void AddFailure(string key, int limitMinute)
        {
            lock (LockObject)
            {
                var attemptDatetimeList = GetAttemptDatetimeList(key, limitMinute);

                attemptDatetimeList.Add(DateTime.UtcNow);

                Save(key, attemptDatetimeList, limitMinute);
            }
        }

        public void Reset(string key)
        {
            lock (LockObject)
            {
                MemoryCache.Remove(CreateCacheKey(key));
            }
        }

        /// <summary>
        /// 期間内の試行履歴を取得する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limitMinute"></param>
        /// <returns></returns>
        private List<DateTime> GetAttemptDatetimeList(string key, int limitMinute)
        {
            var attemptDatetimeList = MemoryCache.Get<List<DateTime>>(CreateCacheKey(key)) ?? new List<DateTime>();

            //期間外の試行履歴を除外
            return attemptDatetimeList.Where(r => r > DateTime.UtcNow.AddMinutes(-limitMinute)).ToList();
        }

        private void Save(string key, List<DateTime> attemptDatetimeList, int limitMinute)
        {
            MemoryCache.Set(CreateCacheKey(key), attemptDatetimeList, TimeSpan.FromMinutes(limitMinute));
        }

        /// <summary>
        /// キャッシュキーを生成する
        /// ※大文字小文字・前後の空白の違いで制限を回避されないよう正規化する
        /// 　（DBの照合順序は大文字小文字を区別せず、末尾の空白も無視されるため）
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string CreateCacheKey(string key)
        {
            return "RateLimit:" + (key ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
