namespace DungeonAdventures.Src.Scenes
{
	/// <summary>
	/// Makes an actor stroll: it picks a spot between two columns, walks there at a stroll, stops a while
	/// (sometimes turning to face you), then picks another. Call <see cref="Update"/> once a frame.
	/// </summary>
	public sealed class Wanderer(Actor actor, int minX, int maxX, int seed)
	{
		private readonly Random _rng = new(seed);
		private int? _target;
		private int _rest = new Random(seed).Next(10, 60);
		private int _step;

		public Actor Actor => actor;

		/// <summary>True while it stands still (and can be talked to without walking off mid-sentence).</summary>
		public bool Resting => _target == null;

		/// <summary>Stops it where it is for a while and turns it to face `dir`.</summary>
		public void Hold(int dir, int frames)
		{
			_target = null;
			_rest = frames;
			actor.Frame = 0;
			actor.Dir = dir;
		}

		public void Update(int t)
		{
			if (_target == null)
			{
				if (--_rest > 0) return;
				_target = _rng.Next(minX, Math.Max(minX + 1, maxX));
				actor.Dir = _target >= actor.X ? Actor.Right : Actor.Left;
			}

			if (t % 2 == 0) return; // a stroll: half the hero's pace
			int to = _target.Value;
			actor.X += Math.Sign(to - actor.X);
			actor.Frame = 1 + _step++ / 2 % 8;
			if (actor.X != to) return;

			_target = null;
			actor.Frame = 0;
			_rest = _rng.Next(25, 110);
			if (_rng.Next(3) == 0) actor.Dir = Actor.Down; // stop and look about
		}
	}
}
