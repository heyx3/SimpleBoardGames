namespace BoardGames
{
	/// <summary>
	/// The different players/teams on the board.
	/// </summary>
	public enum Players
	{
		One = 0,
		Two = 1,
	}

	public static class PlayersExtensions
	{
		public static Players Switched(this Players p)
		{
			switch (p)
			{
				case Players.One: return Players.Two;
				case Players.Two: return Players.One;
				default: throw new System.NotImplementedException(p.ToString());
			}
		}
	}
}