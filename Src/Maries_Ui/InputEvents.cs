namespace Maries_Ui
{
	public enum MouseAction { LeftClick, RightClick, MiddleClick, WheelUp, WheelDown }

	/// <summary>A mouse press at a 0-based screen column (X) and row (Y).</summary>
	public readonly record struct MouseEvent(MouseAction Action, int X, int Y);

	/// <summary>One piece of input: either a key or a mouse event.</summary>
	public readonly record struct InputEvent(ConsoleKeyInfo? Key, MouseEvent? Mouse);
}
