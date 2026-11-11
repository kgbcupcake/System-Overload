namespace Maries_Ui
{
	/// <summary>Runs an action once when disposed (a second Dispose does nothing).</summary>
	internal sealed class Scope : IDisposable
	{
		private Action? _onDispose;
		public Scope(Action onDispose) => _onDispose = onDispose;
		public void Dispose()
		{
			_onDispose?.Invoke();
			_onDispose = null; // disposing twice must not undo someone else's Enable()
		}
	}
}
