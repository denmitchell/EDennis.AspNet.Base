using System;
using System.Threading;

namespace EDennis.NetStandard.Base {
    /// <summary>
    /// Class to product a "Comb Guid," which combines a timestamp and 
    /// a random set of bytes into a Guid that preserves sort order.  
    /// Note that this implementation should perform better for inserts
    /// than either NEWID() or NEWSEQUENTIALID() in SQL Server.
    /// 
    /// adapted from https://github.com/richardtallent/RT.Comb
    /// also see https://www.nuget.org/packages/RT.Comb/
    /// also see https://jim.blacksweb.com/2017/01/23/comb-guid-what-is-it-and-why-should-i-use-it/
    /// also see https://www.informit.com/articles/article.aspx?p=25862
    /// </summary>
    public static class CombGuid {

        private const int EMBED_AT_INDEX = 10;
        private const long INCREMENT_TICKS = 1 * DateTimeExtensions.TICKS_PER_MILLISECOND;

        //used to prevent collision of consecutive timestamps
        private static long lastValue = DateTime.UtcNow.Ticks;

        /// <summary>
        /// Creates a new Comb Guid
        /// </summary>
        /// <returns>Comb Guid, which consists of a timestamp and a random portion</returns>
        public static Guid Create() => Create(Guid.NewGuid(), GetTimestamp());


        /// <summary>
        /// Creates a new Comb Guid through simple combination of a
        /// regular Guid argument and a DateTime argument
        /// </summary>
        /// <param name="regularGuid">Typically produced via Guid.NewGuid()</param>
        /// <param name="timestamp">A representation of Now (approximately)</param>
        /// <returns></returns>
        public static Guid Create(Guid regularGuid, DateTime timestamp) {
            //Console.WriteLine(timestamp.ToString("HH:mm:ss.ffffff"));
            var guidBytes = regularGuid.ToByteArray();
            var timeBytes = timestamp.ToBytes();
            Array.Copy(timeBytes, 0, guidBytes, EMBED_AT_INDEX, DateTimeExtensions.DATE_BYTES);
            return new Guid(guidBytes);
        }

        /// <summary>
        /// Gets the timestamp portion of a Comb guid.
        /// </summary>
        /// <param name="comb"></param>
        /// <returns></returns>
        public static DateTime GetTimestamp(Guid comb) {
            var gbytes = comb.ToByteArray();
            var dbytes = new byte[DateTimeExtensions.DATE_BYTES];
            Array.Copy(gbytes, EMBED_AT_INDEX, dbytes, 0, DateTimeExtensions.DATE_BYTES);
            return dbytes.ToDateTime();
        }

        private static readonly object lockObj = new object();
        /// <summary>
        /// Gets the next timestamp, ensuring that no two timestamps are the same
        /// </summary>
        /// <returns></returns>
        private static DateTime GetTimestamp() {

            if (Monitor.TryEnter(lockObj, 1)) {
                try {
                    var now = DateTime.UtcNow.Ticks;
                    var elapsed = now - lastValue;
                    if (elapsed < INCREMENT_TICKS)
                        now = Interlocked.Add(ref lastValue, INCREMENT_TICKS);
                    else
                        now = Interlocked.Add(ref lastValue, elapsed);
                    return new DateTime(now);
                } finally {
                    Monitor.Exit(lockObj);
                }
            } else {
                return new DateTime(Interlocked.Add(ref lastValue, INCREMENT_TICKS));
            }
        }


    }

    /// <summary>
    /// Provides extension methods for converting a DateTime to/from a byte array
    /// (from https://github.com/richardtallent/RT.Comb)
    /// </summary>
    internal static class DateTimeExtensions {
        public const int DATE_BYTES = 6;
        public const long TICKS_PER_MILLISECOND = 10000;
        private static readonly DateTime MIN_DATE = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Transform datetime argument into byte array
        /// </summary>
        /// <param name="dt">datetime value</param>
        /// <returns>byte array representation of datetime argument</returns>
        public static byte[] ToBytes(this DateTime dt) {
            var ms = ToUnixTimeMilliseconds(dt);
            var msBytes = BitConverter.GetBytes(ms);
            if (BitConverter.IsLittleEndian) Array.Reverse(msBytes);
            var result = new byte[DATE_BYTES];
            var index = msBytes.GetUpperBound(0) + 1 - DATE_BYTES;
            Array.Copy(msBytes, index, result, 0, DATE_BYTES);
            return result;
        }


        /// <summary>
        /// Try to extract and return datetime portion of combGuid.
        /// </summary>
        /// <param name="combGuid">Comb Guid</param>
        /// <returns>datetime portion of Comb Guid argument</returns>
        public static DateTime ToDateTime(this byte[] combGuid) {
            var msBytes = new byte[8];
            var index = 8 - DATE_BYTES;
            try {
                Array.Copy(combGuid, 0, msBytes, index, DATE_BYTES);
                if (BitConverter.IsLittleEndian) Array.Reverse(msBytes);
                var ms = BitConverter.ToInt64(msBytes, 0);
                return FromUnixTimeMilliseconds(ms);
            } catch (Exception ex) {
                throw new ArgumentException($"Cannot convert {string.Join(" ", msBytes)} to DateTime", ex);
            }
        }


        private static long ToUnixTimeMilliseconds(DateTime dt)
            => (dt.Ticks - MIN_DATE.Ticks) / TICKS_PER_MILLISECOND;

        private static DateTime FromUnixTimeMilliseconds(long ms)
            => MIN_DATE.AddTicks(ms * TICKS_PER_MILLISECOND);
    }

}