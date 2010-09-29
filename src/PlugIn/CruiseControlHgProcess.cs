using System;
using System.Linq;

using NHg;

using ThoughtWorks.CruiseControl.Core.Util;

namespace CruiseControl.Mercurial
{
	public class CruiseControlHgProcess : IHgProcess
	{
		private readonly ProcessExecutor _processExecutor;

		public CruiseControlHgProcess(ProcessExecutor processExecutor)
		{
			if (processExecutor == null)
			{
				throw new ArgumentNullException("processExecutor");
			}

			_processExecutor = processExecutor;
		}

		#region IHgProcess Members

		public event EventHandler<OutputEventArgs> OutputReceived;

		public IHgResult Execute(HgArguments hgArguments)
		{
			if (hgArguments == null)
			{
				throw new ArgumentNullException("hgArguments");
			}
			_processExecutor.ProcessOutput += HandleProcessOutput;
			try
			{
				var processInfo = new ProcessInfo("hg", hgArguments.ToString());
				var result = _processExecutor.Execute(processInfo);
				return new CruiseControlHgResult(result);
			}
			finally
			{
				_processExecutor.ProcessOutput -= HandleProcessOutput;
			}
		}

		#endregion

		private void HandleProcessOutput(object sender, ProcessOutputEventArgs e)
		{
			OnOutputReceived(e.Data);
		}

		protected virtual void OnOutputReceived(string output)
		{
			if(OutputReceived != null)
			{
				OutputReceived(this, new OutputEventArgs(output));
			}
		}

		#region Nested type: CruiseControlHgResult

		private class CruiseControlHgResult : IHgResult
		{
			private readonly bool _failed;
			private readonly string _standardOutput;
			private readonly string _standardError;

			internal CruiseControlHgResult(ProcessResult processResult)
			{
				_failed = processResult.Failed;
				_standardOutput = processResult.StandardOutput.CloneStrong();
				_standardError = processResult.StandardError.CloneStrong();
			}

			#region IHgResult Members

			public bool Failed
			{
				get { return _failed; }
			}

			public string StandardOutput
			{
				get { return _standardOutput; }
			}

			public string StandardError
			{
				get { return _standardError; }
			}

			#endregion
		}

		#endregion
	}
}
