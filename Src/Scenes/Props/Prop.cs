namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// Something in a room. A room calls, in order: Plan (work out where it goes), Lights (baked into the
	/// walls), PaintBack (behind the actors), PaintFront (in front of them), then Animate every frame.
	/// Positions given as fractions are of the room's width, so a prop fits any terminal size.
	/// </summary>
	public abstract class Prop
	{
		public virtual void Plan(Room room) { }
		public virtual IEnumerable<(double X, double Y, double Power)> Lights(Room room) => Array.Empty<(double, double, double)>();
		public virtual void PaintBack(Room room, Canvas back) { }
		public virtual void PaintFront(Room room, Canvas front, bool[] seeThrough) { }
		public virtual void Animate(Room room, Canvas frame, int t) { }
		/// <summary>The ground's height at column x when this prop holds it (a stair), else null.</summary>
		public virtual int? GroundAt(Room room, int x) => null;
		/// <summary>Columns this prop takes, so wall decor keeps clear.</summary>
		public virtual IEnumerable<(int From, int To)> Spans(Room room) => Array.Empty<(int, int)>();
	}
}
