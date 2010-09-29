namespace NHg
{
	public interface IHgResult
	{
		bool Failed { get; }
		string StandardOutput { get; }
		string StandardError { get; }
	}
}