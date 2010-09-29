using System;
using System.Linq;
using System.Text;

using ThoughtWorks.CruiseControl.Core.Util;

namespace CruiseControl.Mercurial.Fixtures
{
	public class TestProcessExecutor : ProcessExecutor
	{
		public override ProcessResult Execute(ProcessInfo processInfo)
		{
			if (processInfo == null)
			{
				throw new ArgumentNullException("processInfo");
			}

			var complete = false;

			var process = processInfo.CreateProcess();
			var outputBuilder = new StringBuilder();
			var errorBuilder = new StringBuilder();
			process.EnableRaisingEvents = true;
			process.Exited += (sender, args) => { complete = true; };
			process.Start();
			while (!complete)
			{
				outputBuilder.Append(process.StandardOutput.ReadToEnd());
				errorBuilder.Append(process.StandardError.ReadToEnd());
			}
			return new ProcessResult(outputBuilder.ToString(), errorBuilder.ToString(), process.ExitCode, false);
		}
	}
}
