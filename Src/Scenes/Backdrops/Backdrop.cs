namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// What shows behind a room where the room is clear: an outdoor sky with far hills. Painted on the screen's
	/// view every frame, given the camera so it can scroll slower than the room (parallax).
	/// </summary>
	public abstract class Backdrop
	{
		/// <param name="view">The screen-sized canvas to fill.</param>
		/// <param name="cameraX">The room column at the screen's left edge.</param>
		/// <param name="roomWidth">The room's full width.</param>
		/// <param name="t">The frame number.</param>
		public abstract void Paint(Canvas view, int cameraX, int roomWidth, int t);
	}
}
