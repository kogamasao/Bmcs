namespace Bmcs.Function
{
    public interface IRateLimiter
    {
        /// <summary>
        /// 試行可否を判定し、可能な場合は試行回数を加算する
        /// ※メール送信等、成功しても回数を数えたい処理で使用する
        /// </summary>
        /// <param name="key">制限単位のキー</param>
        /// <param name="limitCount">期間内の上限回数</param>
        /// <param name="limitMinute">期間（分）</param>
        /// <returns>試行可能な場合true</returns>
        bool TryAttempt(string key, int limitCount, int limitMinute);

        /// <summary>
        /// 上限に達しているかを判定する（回数は加算しない）
        /// </summary>
        /// <param name="key">制限単位のキー</param>
        /// <param name="limitCount">期間内の上限回数</param>
        /// <param name="limitMinute">期間（分）</param>
        /// <returns>上限に達している場合true</returns>
        bool IsBlocked(string key, int limitCount, int limitMinute);

        /// <summary>
        /// 失敗回数を加算する
        /// ※認証の失敗時のみ数えることで、正常利用を妨げずに総当たりを防ぐ
        /// </summary>
        /// <param name="key">制限単位のキー</param>
        /// <param name="limitMinute">期間（分）</param>
        void AddFailure(string key, int limitMinute);

        /// <summary>
        /// 失敗回数をリセットする（認証成功時に使用）
        /// </summary>
        /// <param name="key">制限単位のキー</param>
        void Reset(string key);
    }
}
