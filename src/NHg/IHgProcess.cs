using System;
using System.Linq;

namespace NHg
{
	public interface IHgProcess
	{
		event EventHandler<OutputEventArgs> OutputReceived;
		IHgResult Execute(HgArguments hgArguments);
	}
}
