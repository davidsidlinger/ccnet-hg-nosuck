using System;
using System.Globalization;
using System.Linq;

using log4net;

namespace NHg
{
	public static class Extensions
	{
		public static T Tap<T>(this T tapped, Action<T> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			action(tapped);
			return tapped;
		}

		public static T TapIf<T>(this T tapped, Func<bool> condition, Action<T> action)
		{
			if (condition == null)
			{
				throw new ArgumentNullException("condition");
			}
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			if (condition())
			{
				action(tapped);
			}
			return tapped;
		}

		public static void DebugCurrent(this ILog log, string format, params object[] args)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentOutOfRangeException("format", format, "Format cannot be null or empty.");
			}
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			log.DebugFormat(CultureInfo.CurrentCulture, format, args);
		}

		public static void InfoCurrent(this ILog log, string format, params object[] args)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentOutOfRangeException("format", format, "Format cannot be null or empty.");
			}
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			log.InfoFormat(CultureInfo.CurrentCulture, format, args);
		}

		public static void WarnCurrent(this ILog log, string format, params object[] args)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentOutOfRangeException("format", format, "Format cannot be null or empty.");
			}
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			log.WarnFormat(CultureInfo.CurrentCulture, format, args);
		}

		public static void ErrorCurrent(this ILog log, string format, params object[] args)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentOutOfRangeException("format", format, "Format cannot be null or empty.");
			}
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			log.ErrorFormat(CultureInfo.CurrentCulture, format, args);
		}

		public static void FatalCurrent(this ILog log, string format, params object[] args)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentOutOfRangeException("format", format, "Format cannot be null or empty.");
			}
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			log.FatalFormat(CultureInfo.CurrentCulture, format, args);
		}

		public static T CloneStrong<T>(this T obj) where T : ICloneable
		{
			return (T)obj.Clone();
		}
	}
}
