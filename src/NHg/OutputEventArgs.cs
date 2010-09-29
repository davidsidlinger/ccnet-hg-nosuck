using System;

namespace NHg
{
	[Serializable]
	public class OutputEventArgs : EventArgs
	{
		private readonly string _output;

		public OutputEventArgs(string output)
		{
			_output = output;
		}

		public string Output
		{
			get { return _output; }
		}
	}
}