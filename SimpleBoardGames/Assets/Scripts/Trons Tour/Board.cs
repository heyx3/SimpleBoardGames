using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace TronsTour
{
	public class Board : BoardGames.Board<Vector2i>
	{
		/// <summary>
		/// Pieces move like a Knight in chess (2 steps along one axis, 1 step along another axis).
		/// </summary>
		private static readonly Vector2i[] possibleMoves = new Vector2i[]
		{
			new Vector2i(2, 1), new Vector2i(1, 2),
			new Vector2i(2, -1), new Vector2i(-1, 2),
			new Vector2i(-2, 1), new Vector2i(1, -2),
			new Vector2i(-2, -1), new Vector2i(-1, -2),
		};


		/// <summary>
		/// Raised when a tile becomes visited or un-visited.
		/// The last argument is whether or not the tile was visited.
		/// </summary>
		public event System.Action<Board, Vector2i, bool> OnTileVisitedChanged;
		public event System.Action<Board> OnBoardDeserialized;

		public int Width { get { return visitedSpaces.GetLength(0); } }
		public int Height { get { return visitedSpaces.GetLength(1); } }

		private Piece piece1, piece2;
		private bool[,] visitedSpaces;


		public Board(int width, int height)
		{
			//Create the board.
			visitedSpaces = new bool[width, height];
			for (int y = 0; y < Height; ++y)
				for (int x = 0; x < Width; ++x)
					visitedSpaces[x, y] = false;

			//Create the pieces.
			piece1 = new Piece(new Vector2i((Width - 1) / 2, 0), BoardGames.Players.One, this);
			piece2 = new Piece(new Vector2i(Width / 2, Height - 1), BoardGames.Players.Two, this);

			//Set up the pieces.
			foreach (var piece in GetPieces())
			{
				visitedSpaces[piece.CurrentPos.Value.x, piece.CurrentPos.Value.y] = true;

				piece.CurrentPos.OnChanged += (_piece, oldPos, newPos) =>
				{
					visitedSpaces[newPos.x, newPos.y] = true;

					if (OnTileVisitedChanged != null)
						OnTileVisitedChanged(this, newPos, true);
				};
			}

			//When a move is undone, undo the change to the "visitedSpaces" array.
			OnUndoAction += (thisBoard, action) =>
			{
				Action_Move move = (Action_Move)action;

				visitedSpaces[move.EndPos.x, move.EndPos.y] = false;

				if (OnTileVisitedChanged != null)
					OnTileVisitedChanged(this, move.EndPos, false);
			};
			
			piece1.CurrentPos.OnChanged += Callback_PieceMoved;
			piece2.CurrentPos.OnChanged += Callback_PieceMoved;
		}


		public bool IsInBounds(Vector2i boardPos)
		{
			return boardPos.x >= 0 && boardPos.x < Width &&
				   boardPos.y >= 0 && boardPos.y < Height;
		}
		public bool WasVisited(Vector2i boardPos) { return visitedSpaces[boardPos.x, boardPos.y]; }

		public Piece GetPiece(BoardGames.Players player)
		{
			switch (player)
			{
				case BoardGames.Players.One: return piece1;
				case BoardGames.Players.Two: return piece2;
				default: throw new NotImplementedException(player.ToString());
			}
		}
		public Piece GetPiece(Vector2i space)
		{
			if (space == piece1.CurrentPos.Value)
				return piece1;
			else if (space == piece2.CurrentPos.Value)
				return piece2;
			else
				return null;
		}

		public override IEnumerable<BoardGames.Piece<Vector2i>> GetPieces()
		{
			yield return piece1;
			yield return piece2;
		}
		public override IEnumerable<BoardGames.Action<Vector2i>> GetActions(BoardGames.Piece<Vector2i> piece)
		{
			//Get all possible spaces to move to and filter out the illegal ones.
			foreach (Vector2i v in possibleMoves)
			{
				Action_Move move = new Action_Move(piece.CurrentPos.Value + v, (Piece)piece);
				if (IsInBounds(move.EndPos) && !visitedSpaces[move.EndPos.x, move.EndPos.y])
					yield return move;
			}
		}

		public override void Serialize(BinaryWriter stream)
		{
			stream.Write(piece1.CurrentPos.Value.x);
			stream.Write(piece1.CurrentPos.Value.y);
			stream.Write(piece2.CurrentPos.Value.x);
			stream.Write(piece2.CurrentPos.Value.y);
			
			stream.Write(Width);
			stream.Write(Height);
			for (int y = 0; y < Height; ++y)
				for (int x = 0; x < Width; ++x)
					stream.Write(visitedSpaces[x, y]);
		}
		public override void Deserialize(BinaryReader stream)
		{
			piece1.CurrentPos.Value = new Vector2i(stream.ReadInt32(), stream.ReadInt32());
			piece2.CurrentPos.Value = new Vector2i(stream.ReadInt32(), stream.ReadInt32());
			
			visitedSpaces = new bool[stream.ReadInt32(), stream.ReadInt32()];
			for (int y = 0; y < Height; ++y)
				for (int x = 0; x < Width; ++x)
					visitedSpaces[x, y] = stream.ReadBoolean();

			if (OnBoardDeserialized != null)
				OnBoardDeserialized(this);
		}

		private void Callback_PieceMoved(BoardGames.Piece<Vector2i> thePiece,
										 Vector2i oldPos, Vector2i newPos)
		{
			//If the other piece has no moves left, this piece just won.
			if (GetActions(GetPiece(thePiece.Owner.Value.Switched())).Count() == 0)
				FinishedGame(thePiece.Owner.Value);
		}
	}
}