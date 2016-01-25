using System;
using System.Collections.Generic;
using UnityEngine;


public class Fitchneil_Board : Singleton<Fitchneil_Board>
{
	public enum Spaces
	{
		King,
		Defender,
		Attacker,
		Empty,
	}
	public static bool IsSameTeam(Spaces a, Spaces b)
	{
		return ((a == Spaces.King || a == Spaces.Defender) && (b == Spaces.King || b == Spaces.Defender)) ||
			   (a == Spaces.Attacker && b == Spaces.Attacker);
	}
	public static bool IsEnemies(Spaces a, Spaces b)
	{
		return (a == Spaces.Attacker && (b == Spaces.King || b == Spaces.Defender)) ||
			   (b == Spaces.Attacker && (a == Spaces.King || a == Spaces.Defender));
	}


	public static readonly int BoardSize = 7;

	public Spaces[,] Board;
	private Spaces[,] tempBoard;

	private SpriteRenderer[,] boardPieces;


	/// <summary>
	/// Gets the piece at the given position, or "null" if no piece is at that position.
	/// </summary>
	public SpriteRenderer GetPiece(Vector2i pos)
	{
		return boardPieces[pos.x, pos.y];
	}
	/// <summary>
	/// Gets all spaces on the board occupied by the given type.
	/// </summary>
	public IEnumerable<Vector2i> GetPieces(Spaces type)
	{
		for (int x = 0; x < Board.GetLength(0); ++x)
			for (int y = 0; y < Board.GetLength(1); ++y)
				if (Board[x, y] == type)
					yield return new Vector2i(x, y);
	}

	public struct Move
	{
		public Vector2i Pos;
		public bool IsSpecial;
	}
	public List<Move> GetAllowedMoves(Vector2i piece)
	{
		if (GetPiece(piece) == null)
		{
			return new List<Move>();
		}
		else
		{
			List<Move> moves = new List<Move>();
			Spaces pType = Board[piece.x, piece.y];

			int x = piece.x + 1;
			while (x < BoardSize)
			{
				if (TryMove(pType, piece, new Vector2i(x, piece.y), moves))
					break;
				x += 1;
			}

			x = piece.x - 1;
			while (x >= 0)
			{
				if (TryMove(pType, piece, new Vector2i(x, piece.y), moves))
					break;
				x -= 1;
			}

			int y = piece.y + 1;
			while (y < BoardSize)
			{
				if (TryMove(pType, piece, new Vector2i(piece.x, y), moves))
					break;
				y += 1;
			}

			y = piece.y - 1;
			while (y >= 0)
			{
				if (TryMove(pType, piece, new Vector2i(piece.x, y), moves))
					break;
				y -= 1;
			}

			return moves;
		}
	}
	private bool TryMove(Spaces pType, Vector2i fromPos, Vector2i tryPos, List<Move> moves)
	{
		UnityEngine.Assertions.Assert.IsTrue(pType != Spaces.Empty, "Piece is type 'Empty'");

		//If something is already there, no moves are allowed and the movement is blocked.
		Spaces pType2 = Board[tryPos.x, tryPos.y];
		if (pType2 != Spaces.Empty)
		{
			return true;
		}

		//If this piece isn't a king and this is the throne square,
		//    no moves are allowed but the movement isn't blocked.
		if (pType != Spaces.King && tryPos.x == (BoardSize / 2) && tryPos.y == (BoardSize / 2))
		{
			return false;
		}


		//Otherwise, it's a valid move.

		Move mv = new Move();
		mv.Pos = tryPos;

		//The move is special if it's a winning move for the King piece or if it captures another piece.
		mv.IsSpecial = (pType == Spaces.King &&
						   (tryPos.x == 0 || tryPos.x == BoardSize - 1 ||
							tryPos.y == 0 || tryPos.y == BoardSize - 1)) ||
					   GetCapturesFromMove(fromPos, tryPos).Count > 0;
		moves.Add(mv);

		return false;
	}

	/// <summary>
	/// Gets all pieces that will be captured by moving the given piece to the given position.
	/// </summary>
	public List<Vector2i> GetCapturesFromMove(Vector2i currentPos, Vector2i nextPos)
	{
		Spaces pType = Board[currentPos.x, currentPos.y];

		//Create a copy of the board with the move completed.
		Array.Copy(Board, tempBoard, Board.Length);
		tempBoard[currentPos.x, currentPos.y] = Spaces.Empty;
		tempBoard[nextPos.x, nextPos.y] = pType;

		//See if any normal (i.e. non-king) enemies were captured.
		Spaces normalEnemy = (pType == Spaces.Attacker ? Spaces.Defender : Spaces.Attacker);
		List<Vector2i> caps = new List<Vector2i>();
		if (nextPos.x > 1 &&
			tempBoard[nextPos.x - 1, nextPos.y] == normalEnemy &&
			IsSameTeam(pType, tempBoard[nextPos.x - 2, nextPos.y]))
		{
			caps.Add(nextPos.LessX);
		}
		if (nextPos.x < BoardSize - 2 &&
			tempBoard[nextPos.x + 1, nextPos.y] == normalEnemy &&
			IsSameTeam(pType, tempBoard[nextPos.x + 2, nextPos.y]))
		{
			caps.Add(nextPos.MoreX);
		}
		if (nextPos.y > 1 &&
			tempBoard[nextPos.x, nextPos.y - 1] == normalEnemy &&
			IsSameTeam(pType, tempBoard[nextPos.x, nextPos.y - 2]))
		{
			caps.Add(nextPos.LessY);
		}
		if (nextPos.y < BoardSize - 2 &&
			tempBoard[nextPos.x, nextPos.y + 1] == normalEnemy &&
			IsSameTeam(pType, tempBoard[nextPos.x, nextPos.y + 2]))
		{
			caps.Add(nextPos.MoreY);
		}

		//See if the King was captured.
		if (pType == Spaces.Attacker)
		{
			//Only check if the king was right next to this piece.
			Vector2i kingPos = new Vector2i(-1, -1);

			if (nextPos.x > 0 && tempBoard[nextPos.x - 1, nextPos.y] == Spaces.King)
				kingPos = nextPos.LessX;
			if (nextPos.y > 0 && tempBoard[nextPos.x, nextPos.y - 1] == Spaces.King)
				kingPos = nextPos.LessY;
			if (nextPos.x < BoardSize - 1 && tempBoard[nextPos.x + 1, nextPos.y] == Spaces.King)
				kingPos = nextPos.MoreX;
			if (nextPos.y < BoardSize - 1 && tempBoard[nextPos.x, nextPos.y + 1] == Spaces.King)
				kingPos = nextPos.MoreY;

			if (kingPos != new Vector2i(-1, -1))
			{
				//The king must be surrounded by attackers/the throne to be captured.
				if (kingPos.x > 0 && IsAttackerOrThrone_temp(kingPos.LessX) &&
					kingPos.x < BoardSize - 1 && IsAttackerOrThrone_temp(kingPos.MoreX) &&
					kingPos.y > 0 && IsAttackerOrThrone_temp(kingPos.LessY) &&
					kingPos.y < BoardSize - 1 && IsAttackerOrThrone_temp(kingPos.MoreY))
				{
					caps.Add(kingPos);
				}
			}
		}

		return caps;
	}
	private bool IsAttackerOrThrone_temp(Vector2i pos)
	{
		return tempBoard[pos.x, pos.y] == Spaces.Attacker ||
			   (pos.x == (BoardSize / 2) && pos.y == (BoardSize / 2));
	}

	public Transform MovePiece(Vector2i current, Vector2i next,
							   MoveToPosition.MoveFinishedDelegate onFinished = null)
	{
		List<Vector2i> captures = GetCapturesFromMove(current, next);


		SpriteRenderer sprR = GetPiece(current);
		UnityEngine.Assertions.Assert.IsNotNull(sprR);

		GameObject go = GetPiece(current).gameObject;

		MoveToPosition mtp = go.AddComponent<MoveToPosition>();
		mtp.EndPos = new Vector3(next.x + 0.5f, next.y + 0.5f, 0.0f);
		mtp.TotalTime = Fitchneil_Constants.Instance.MovePieceTime;
		mtp.MovementCurve = Fitchneil_Constants.Instance.MovePieceCurve;
		if (onFinished != null)
			mtp.OnFinishedMove += onFinished;
		
		Board[next.x, next.y] = Board[current.x, current.y];
		Board[current.x, current.y] = Spaces.Empty;

		//Destroy all captured pieces.
		foreach (Vector2i v in captures)
		{
			//Spawn some effects.
			GameObject dpGO = Instantiate(Fitchneil_Art.Instance.DestroyedPieceEffectPrefab);
			dpGO.transform.position = new Vector3(v.x + 0.5f, v.y + 0.5f, 0.0f);

			//Destroy the piece.
			Board[v.x, v.y] = Spaces.Empty;
			SpriteSelector.Instance.Objects.Remove(boardPieces[v.x, v.y]);
			Destroy(boardPieces[v.x, v.y]);
			boardPieces[v.x, v.y] = null;
		}

		boardPieces[next.x, next.y] = boardPieces[current.x, current.y];
		boardPieces[current.x, current.y] = null;

		return go.transform;
	}

	
	protected override void Awake()
	{
		base.Awake();

		Board = new Spaces[BoardSize, BoardSize];
		tempBoard = new Spaces[BoardSize, BoardSize];
		
		boardPieces = new SpriteRenderer[BoardSize, BoardSize];
		for (int x = 0; x < Board.GetLength(0); ++x)
		{
			for (int y = 0; y < Board.GetLength(1); ++y)
			{
				Board[x, y] = Spaces.Empty;
				boardPieces[x, y] = null;
			}
		}
	}
	void Start()
	{
		//Set up specific pieces.
		int centerPos = (BoardSize / 2);

		SetPiece(new Vector2i(centerPos, centerPos), Spaces.King);

		for (int i = 1; i <= 2; ++i)
		{
			SetPiece(new Vector2i(centerPos - i, centerPos), Spaces.Defender);
			SetPiece(new Vector2i(centerPos + i, centerPos), Spaces.Defender);
			SetPiece(new Vector2i(centerPos, centerPos - i), Spaces.Defender);
			SetPiece(new Vector2i(centerPos, centerPos + i), Spaces.Defender);
		}

		for (int i = -1; i <= 1; ++i)
		{
			SetPiece(new Vector2i(0, centerPos + i), Spaces.Attacker);
			SetPiece(new Vector2i(BoardSize - 1, centerPos + i), Spaces.Attacker);
			SetPiece(new Vector2i(centerPos + i, 0), Spaces.Attacker);
			SetPiece(new Vector2i(centerPos + i, BoardSize - 1), Spaces.Attacker);
		}
		SetPiece(new Vector2i(0, 0), Spaces.Attacker);
		SetPiece(new Vector2i(0, BoardSize - 1), Spaces.Attacker);
		SetPiece(new Vector2i(BoardSize - 1, 0), Spaces.Attacker);
		SetPiece(new Vector2i(BoardSize - 1, BoardSize - 1), Spaces.Attacker);
	}
	private void SetPiece(Vector2i pos, Spaces type)
	{
		Board[pos.x, pos.y] = type;

		Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0.0f);

		switch (type)
		{
			case Spaces.King:
				boardPieces[pos.x, pos.y] = Utilities.CreateSprite(Fitchneil_Art.Instance.King,
																   "King Piece", worldPos);
				break;
			case Spaces.Defender:
				boardPieces[pos.x, pos.y] = Utilities.CreateSprite(Fitchneil_Art.Instance.Defender,
																   "Defender Piece", worldPos);
				break;
			case Spaces.Attacker:
				boardPieces[pos.x, pos.y] = Utilities.CreateSprite(Fitchneil_Art.Instance.Attacker,
																   "Attacker Piece", worldPos);
				break;

			case Spaces.Empty:
				boardPieces[pos.x, pos.y] = null;
				break;

			default: throw new NotImplementedException(type.ToString());
		}
	}
}