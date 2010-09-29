using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NHg
{
	public class HgProcess : IHgProcess
	{
		private const string _defaultExecutableName = "hg";
		private readonly string _executableName;

		public HgProcess() : this(_defaultExecutableName) {}

		public HgProcess(string executableName)
		{
			if (string.IsNullOrEmpty(executableName))
			{
				throw new ArgumentOutOfRangeException("executableName", executableName, "ExecutableName cannot be null or empty.");
			}
			_executableName = executableName;
		}

		protected virtual void OnOutputReceived(string output)
		{
			if (OutputReceived != null)
			{
				OutputReceived(this, new OutputEventArgs(output));
			}
		}

		#region IHgProcess Members

		public event EventHandler<OutputEventArgs> OutputReceived;

		public IHgResult Execute(HgArguments hgArguments)
		{
			var startInfo = new ProcessStartInfo(_executableName, hgArguments.ToString())
			                {
			                	CreateNoWindow = true,
			                	ErrorDialog = false,
			                	RedirectStandardError = true,
			                	RedirectStandardOutput = true,
			                	UseShellExecute = false,
			                };
			using (var process = new Process { StartInfo = startInfo, })
			{
				var errorBuilder = new StringBuilder();
				var outBuilder = new StringBuilder();

				process.ErrorDataReceived += (sender, e) => errorBuilder.AppendLine(e.Data);
				process.OutputDataReceived += (sender, e) => outBuilder.AppendLine(e.Data);

				process.Start();
				process.BeginErrorReadLine();
				process.BeginOutputReadLine();

				process.WaitForExit();

				return new HgResult(process.ExitCode != 0, outBuilder.ToString().Trim(), errorBuilder.ToString().Trim());
			}
		}

		#endregion
	}
}
