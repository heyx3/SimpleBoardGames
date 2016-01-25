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
		return ((a == Spaces.King || a == Spaces.Defender) && (b == Spaces.King || a == Spaces.Defender)) ||
			   (a == Spaces.Attacker && b == Spaces.Attacker);
	}
	public static bool IsEnemies(Spaces a, Spaces b)
	{
		return (a == Spaces.Attacker && (b == Spaces.King || b == Spaces.Defender)) ||
			   (b == Spaces.Attacker && (a == Spaces.King || a == Spaces.Defender));
	}


	public static readonly int BoardSize = 7;

	public Spaces[,] Board;
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
				if (TryMove(pType, new Vector2i(x, piece.y), moves))
					break;
				x += 1;
			}

			x = piece.x - 1;
			while (x >= 0)
			{
				if (TryMove(pType, new Vector2i(x, piece.y), moves))
					break;
				x -= 1;
			}

			int y = piece.y + 1;
			while (y < BoardSize)
			{
				if (TryMove(pType, new Vector2i(piece.x, y), moves))
					break;
				y += 1;
			}

			y = piece.y - 1;
			while (y >= 0)
			{
				if (TryMove(pType, new Vector2i(piece.x, y), moves))
					break;
				y -= 1;
			}

			return moves;
		}
	}
	private bool TryMove(Spaces pType, Vector2i tryPos, List<Move> moves)
	{
		Spaces pType2 = Board[tryPos.x, tryPos.y];
		if (IsSameTeam(pType, pType2))
		{
			return true;
		}
		else if (IsEnemies(pType, pType2))
		{
			Move mv = new Move();
			mv.Pos = tryPos;
			mv.IsSpecial = true;
			moves.Add(mv);

			return true;
		}
		else if (pType == Spaces.King &&
				 (tryPos.x == 0 || tryPos.x == BoardSize - 1 ||
				  tryPos.y == 0 || tryPos.y == BoardSize - 1))
		{
			Move mv = new Move();
			mv.Pos = tryPos;
			mv.IsSpecial = true;
			moves.Add(mv);

			return false;
		}
		else
		{
			Move mv = new Move();
			mv.Pos = tryPos;
			mv.IsSpecial = false;
			moves.Add(mv);

			return false;
		}
	}

	public Transform MovePiece(Vector2i current, Vector2i next,
							   MoveToPosition.MoveFinishedDelegate onFinished = null)
	{
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

		return go.transform;
	}

	
	protected override void Awake()
	{
		base.Awake();


		Board = new Spaces[BoardSize, BoardSize];
		
		boardPieces = new SpriteRenderer[BoardSize, BoardSize];
		for (int x = 0; x < Board.GetLength(0); ++x)
		{
			for (int y = 0; y < Board.GetLength(1); ++y)
			{
				boardPieces[x, y] = null;
			}
		}
		

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

		switch (type)
		{
			case Spaces.King:
				boardPieces[pos.x, pos.y] = Utilities.CreateSprite(Fitchneil_Art.Instance.King,
																   "King Piece",
																   new Vector3(pos.x, pos.y, 0.0f));
				break;
			case Spaces.Defender:
				boardPieces[pos.x, pos.y] = Utilities.CreateSprite(Fitchneil_Art.Instance.Defender,
																   "Defender Piece",
																   new Vector3(pos.x, pos.y, 0.0f));
				break;
			case Spaces.Attacker:
				boardPieces[pos.x, pos.y] = Utilities.CreateSprite(Fitchneil_Art.Instance.Attacker,
																   "Attacker Piece",
																   new Vector3(pos.x, pos.y, 0.0f));
				break;

			case Spaces.Empty:
				boardPieces[pos.x, pos.y] = null;
				break;

			default: throw new NotImplementedException(type.ToString());
		}
	}
}