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
}

public static class PlayersExtensions
{
	public static BoardGames.Players Switched(this BoardGames.Players p)
	{
		switch (p)
		{
			case BoardGames.Players.One: return BoardGames.Players.Two;
			case BoardGames.Players.Two: return BoardGames.Players.One;
			default: throw new System.NotImplementedException(p.ToString());
		}
	}
}