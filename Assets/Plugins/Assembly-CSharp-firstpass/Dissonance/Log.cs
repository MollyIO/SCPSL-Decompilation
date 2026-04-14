using System;
using System.Diagnostics;

namespace Dissonance
{
	public class Log
	{
		private readonly string _traceFormat;

		private readonly string _debugFormat;

		private readonly string _basicFormat;

		private readonly int _category;

		public bool IsTrace
		{
			get
			{
				return ShouldLog(LogLevel.Trace);
			}
		}

		public bool IsDebug
		{
			get
			{
				return ShouldLog(LogLevel.Debug);
			}
		}

		public bool IsInfo
		{
			get
			{
				return ShouldLog(LogLevel.Info);
			}
		}

		public bool IsWarn
		{
			get
			{
				return ShouldLog(LogLevel.Warn);
			}
		}

		public bool IsError
		{
			get
			{
				return ShouldLog(LogLevel.Error);
			}
		}

		internal Log(int category, string name)
		{
			_category = category;
			_basicFormat = string.Concat("[Dissonance:", (LogCategory)category, "] ({0:HH:mm:ss.fff}) ", name, ": {1}");
			_debugFormat = "DEBUG " + _basicFormat;
			_traceFormat = "TRACE " + _basicFormat;
		}

		[DebuggerHidden]
		private bool ShouldLog(LogLevel level)
		{
			return level >= Logs.GetLogLevel(_category);
		}

		[DebuggerHidden]
		private void WriteLog(LogLevel level, string message)
		{
			if (ShouldLog(level))
			{
				string format;
				switch (level)
				{
				case LogLevel.Trace:
					format = _traceFormat;
					break;
				case LogLevel.Debug:
					format = _debugFormat;
					break;
				case LogLevel.Info:
				case LogLevel.Warn:
				case LogLevel.Error:
					format = _basicFormat;
					break;
				default:
					throw new ArgumentOutOfRangeException("level", level, null);
				}
				Logs.SendLogMessage(string.Format(format, DateTime.UtcNow, message), level);
			}
		}

		[DebuggerHidden]
		private void WriteLogFormat<TA>(LogLevel level, string format, [CanBeNull] TA p0)
		{
			if (ShouldLog(level))
			{
				WriteLog(level, string.Format(format, p0));
			}
		}

		[DebuggerHidden]
		private void WriteLogFormat<TA, TB>(LogLevel level, string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
		{
			if (ShouldLog(level))
			{
				WriteLog(level, string.Format(format, p0, p1));
			}
		}

		[DebuggerHidden]
		private void WriteLogFormat<TA, TB, TC>(LogLevel level, string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2)
		{
			if (ShouldLog(level))
			{
				WriteLog(level, string.Format(format, p0, p1, p2));
			}
		}

		[DebuggerHidden]
		private void WriteLogFormat<TA, TB, TC, TD>(LogLevel level, string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3)
		{
			if (ShouldLog(level))
			{
				WriteLog(level, string.Format(format, p0, p1, p2, p3));
			}
		}

		[DebuggerHidden]
		private void WriteLogFormat<TA, TB, TC, TD, TE>(LogLevel level, string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4)
		{
			if (ShouldLog(level))
			{
				WriteLog(level, string.Format(format, p0, p1, p2, p3, p4));
			}
		}

		[DebuggerHidden]
		private void WriteLogFormat<TA, TB, TC, TD, TE, TF>(LogLevel level, string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4, [CanBeNull] TF p5)
		{
			if (ShouldLog(level))
			{
				WriteLog(level, string.Format(format, p0, p1, p2, p3, p4, p5));
			}
		}

		[DebuggerHidden]
		private void WriteLogFormat<TA, TB, TC, TD, TE, TF, TG>(LogLevel level, string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4, [CanBeNull] TF p5, [CanBeNull] TG p6)
		{
			if (ShouldLog(level))
			{
				WriteLog(level, string.Format(format, p0, p1, p2, p3, p4, p5, p6));
			}
		}

		[DebuggerHidden]
		private void WriteLogFormat<TA, TB, TC, TD, TE, TF, TG, TH>(LogLevel level, string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4, [CanBeNull] TF p5, [CanBeNull] TG p6, [CanBeNull] TH p7)
		{
			if (ShouldLog(level))
			{
				WriteLog(level, string.Format(format, p0, p1, p2, p3, p4, p5, p6, p7));
			}
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Trace(string message)
		{
			WriteLog(LogLevel.Trace, message);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Trace<TA>(string format, [CanBeNull] TA p0)
		{
			WriteLogFormat(LogLevel.Trace, format, p0);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Trace<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
		{
			WriteLogFormat(LogLevel.Trace, format, p0, p1);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Debug(string message)
		{
			WriteLog(LogLevel.Debug, message);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Debug<TA>(string format, [CanBeNull] TA p0)
		{
			WriteLogFormat(LogLevel.Debug, format, p0);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Debug<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
		{
			WriteLogFormat(LogLevel.Debug, format, p0, p1);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Debug<TA, TB, TC>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2)
		{
			WriteLogFormat(LogLevel.Debug, format, p0, p1, p2);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Debug<TA, TB, TC, TD>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3)
		{
			WriteLogFormat(LogLevel.Debug, format, p0, p1, p2, p3);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Debug<TA, TB, TC, TD, TE>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4)
		{
			WriteLogFormat(LogLevel.Debug, format, p0, p1, p2, p3, p4);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Debug<TA, TB, TC, TD, TE, TF>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4, [CanBeNull] TF p5)
		{
			WriteLogFormat(LogLevel.Debug, format, p0, p1, p2, p3, p4, p5);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Debug<TA, TB, TC, TD, TE, TF, TG>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4, [CanBeNull] TF p5, [CanBeNull] TG p6)
		{
			WriteLogFormat(LogLevel.Debug, format, p0, p1, p2, p3, p4, p5, p6);
		}

		[DebuggerHidden]
		[Conditional("DEBUG")]
		public void Debug<TA, TB, TC, TD, TE, TF, TG, TH>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4, [CanBeNull] TF p5, [CanBeNull] TG p6, [CanBeNull] TH p7)
		{
			WriteLogFormat(LogLevel.Debug, format, p0, p1, p2, p3, p4, p5, p6, p7);
		}

		[DebuggerHidden]
		public void Info(string message)
		{
			WriteLog(LogLevel.Info, message);
		}

		[DebuggerHidden]
		public void Info<TA>(string format, [CanBeNull] TA p0)
		{
			WriteLogFormat(LogLevel.Info, format, p0);
		}

		[DebuggerHidden]
		public void Info<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
		{
			WriteLogFormat(LogLevel.Info, format, p0, p1);
		}

		[DebuggerHidden]
		public void Info<TA, TB, TC>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2)
		{
			WriteLogFormat(LogLevel.Info, format, p0, p1, p2);
		}

		[DebuggerHidden]
		public void Info<TA, TB, TC, TD>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3)
		{
			WriteLogFormat(LogLevel.Info, format, p0, p1, p2, p3);
		}

		[DebuggerHidden]
		public void Info<TA, TB, TC, TD, TE>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4)
		{
			WriteLogFormat(LogLevel.Info, format, p0, p1, p2, p3, p4);
		}

		[DebuggerHidden]
		public void Warn(string message)
		{
			WriteLog(LogLevel.Warn, message);
		}

		[DebuggerHidden]
		public void Warn<TA>(string format, [CanBeNull] TA p0)
		{
			WriteLogFormat(LogLevel.Warn, format, p0);
		}

		[DebuggerHidden]
		public void Warn<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
		{
			WriteLogFormat(LogLevel.Warn, format, p0, p1);
		}

		[DebuggerHidden]
		public void Warn<TA, TB, TC>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2)
		{
			WriteLogFormat(LogLevel.Warn, format, p0, p1, p2);
		}

		[DebuggerHidden]
		public void Warn<TA, TB, TC, TD>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3)
		{
			WriteLogFormat(LogLevel.Warn, format, p0, p1, p2, p3);
		}

		[DebuggerHidden]
		public void Warn<TA, TB, TC, TD, TE>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2, [CanBeNull] TD p3, [CanBeNull] TE p4)
		{
			WriteLogFormat(LogLevel.Warn, format, p0, p1, p2, p3, p4);
		}

		[DebuggerHidden]
		public void Error(string message)
		{
			WriteLog(LogLevel.Error, message);
		}

		[DebuggerHidden]
		public void Error<TA>(string format, [CanBeNull] TA p0)
		{
			WriteLogFormat(LogLevel.Error, format, p0);
		}

		[DebuggerHidden]
		public void Error<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
		{
			WriteLogFormat(LogLevel.Error, format, p0, p1);
		}

		[DebuggerHidden]
		public void Error<TA, TB, TC>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2)
		{
			WriteLogFormat(LogLevel.Error, format, p0, p1, p2);
		}

		[DebuggerHidden]
		[NotNull]
		public DissonanceException CreateUserErrorException(string problem, string likelyCause, string documentationLink, string guid)
		{
			return new DissonanceException(UserErrorMessage(problem, likelyCause, documentationLink, guid));
		}

		[DebuggerHidden]
		[NotNull]
		public string UserErrorMessage(string problem, string likelyCause, string documentationLink, string guid)
		{
			string arg = string.Format("Voice Error: {0}! This is likely caused by \"{1}\". Error ID: {3}", problem, likelyCause, documentationLink, guid);
			return string.Format(_basicFormat, DateTime.UtcNow, arg);
		}

		[DebuggerHidden]
		[NotNull]
		public string PossibleBugMessage(string problem, string guid)
		{
			return string.Format("Voice Error: {0}! Error ID: {1}", problem, guid);
		}

		[DebuggerHidden]
		[NotNull]
		public DissonanceException CreatePossibleBugException(string problem, string guid)
		{
			return new DissonanceException(PossibleBugMessage(problem, guid));
		}

		[DebuggerHidden]
		[NotNull]
		public Exception CreatePossibleBugException<T>([NotNull] Func<string, T> factory, string problem, string guid) where T : Exception
		{
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			return factory(PossibleBugMessage(problem, guid));
		}

		[ContractAnnotation("assertion:true => false; assertion:false => true")]
		public bool AssertAndLogWarn(bool assertion, string msg)
		{
			if (!assertion)
			{
				Warn(msg);
			}
			return !assertion;
		}

		[ContractAnnotation("assertion:true => false; assertion:false => true")]
		public bool AssertAndLogWarn<TA>(bool assertion, string format, TA arg0)
		{
			if (!assertion)
			{
				Warn(format, arg0);
			}
			return !assertion;
		}

		[ContractAnnotation("assertion:true => false; assertion:false => true")]
		public bool AssertAndLogError(bool assertion, string guid, string msg)
		{
			if (!assertion)
			{
				Error(PossibleBugMessage(msg, guid));
			}
			return !assertion;
		}

		[ContractAnnotation("assertion:true => false; assertion:false => true")]
		public bool AssertAndLogError<TA>(bool assertion, string guid, string format, TA arg0)
		{
			if (!assertion)
			{
				Error(PossibleBugMessage(string.Format(format, arg0), guid));
			}
			return !assertion;
		}

		[ContractAnnotation("assertion:false => halt")]
		public void AssertAndThrowPossibleBug(bool assertion, string guid, string msg)
		{
			if (!assertion)
			{
				throw CreatePossibleBugException(msg, guid);
			}
		}

		[ContractAnnotation("assertion:false => halt")]
		public void AssertAndThrowPossibleBug<TA>(bool assertion, string guid, string format, TA arg0)
		{
			if (!assertion)
			{
				throw CreatePossibleBugException(string.Format(format, arg0), guid);
			}
		}

		[ContractAnnotation("assertion:false => halt")]
		public void AssertAndThrowPossibleBug<TA, TB>(bool assertion, string guid, string format, TA arg0, TB arg1)
		{
			if (!assertion)
			{
				throw CreatePossibleBugException(string.Format(format, arg0, arg1), guid);
			}
		}

		[ContractAnnotation("assertion:false => halt")]
		public void AssertAndThrowPossibleBug<TA, TB, TC>(bool assertion, string guid, string format, TA arg0, TB arg1, TC arg2)
		{
			if (!assertion)
			{
				throw CreatePossibleBugException(string.Format(format, arg0, arg1, arg2), guid);
			}
		}
	}
}
