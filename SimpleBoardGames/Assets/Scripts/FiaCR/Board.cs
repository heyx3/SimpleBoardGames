using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;
using BoardGames;

namespace FiaCR
{
	public class Board : BoardGames.Board<Vector2i>
	{
		public static readonly BoardGames.Players Player_Humans = BoardGames.Players.One,
												  Player_TC = BoardGames.Players.Two;

		public enum Sizes
		{
			SixBySix = 6,
			SevenBySeven = 7,
			EightByEight = 8,
			NineByNine = 9,
		}

		#region Design constants

		public static readonly Dictionary<Sizes, uint> NHostsByBoardSize =
			new Dictionary<Sizes, uint>()
			{
				{ Sizes.SixBySix, 2 },
				{ Sizes.SevenBySeven, 3 },
				{ Sizes.EightByEight, 4 },
				{ Sizes.NineByNine, 5 },
			};
		public static readonly Dictionary<Sizes, uint> NJuliaMovesByBoardSize =
			new Dictionary<Sizes, uint>()
			{
				{ Sizes.SixBySix, 3 },
				{ Sizes.SevenBySeven, 4 },
				{ Sizes.EightByEight, 6 },
				{ Sizes.NineByNine, 9 },
			};
		public static readonly Dictionary<Sizes, uint> NBillyMovesByBoardSize =
			new Dictionary<Sizes, uint>()
			{
				{ Sizes.SixBySix, 3 },
				{ Sizes.SevenBySeven, 4 },
				{ Sizes.EightByEight, 6 },
				{ Sizes.NineByNine, 9 },
			};
		public static readonly Dictionary<Sizes, float> ChanceCurseMoveByBoardSize =
			new Dictionary<Sizes, float>()
			{
				{ Sizes.SixBySix, 0.85f },
				{ Sizes.SevenBySeven, 0.75f },
				{ Sizes.EightByEight, 0.65f },
				{ Sizes.NineByNine, 0.55f },
			};

		#endregion

		
		/// <summary>
		/// Raised when a new piece has been added to this board.
		/// </summary>
		public event System.Action<Board, Piece> OnPieceAdded;
		/// <summary>
		/// Raised when a piece has been removed from this board.
		/// </summary>
		public event System.Action<Board, Piece> OnPieceRemoved;
		/// <summary>
		/// Raised when a host has been added to this board.
		/// </summary>
		public event System.Action<Board, Vector2i, BoardGames.Players> OnHostCreated;
		/// <summary>
		/// Raised when a host has been removed from this board.
		/// </summary>
		public event System.Action<Board, Vector2i, BoardGames.Players> OnHostDestroyed;
		
		//TODO: Move "OnBoardDeserialized" to the base class's Deserialize() implementation.
		/// <summary>
		/// When this board deserializes a stream to read a new state,
		///     all current pieces are thrown away and new pieces are put on the board.
		/// </summary>
		public event System.Action<Board> OnBoardDeserialized;


		private Piece[,] pieces;
		private Dictionary<Vector2i, BoardGames.Players> hostsByPos =
			new Dictionary<Vector2i, BoardGames.Players>();
		private Sizes size;
		private int seed;


		public Sizes Size { get { return size; } }

		public IEnumerable<Vector2i> Hosts { get { return hostsByPos.Keys; } }


		public Board(Sizes _size, int _seed)
		{
			size = _size;
			seed = _seed;

			//Create the board.
			pieces = new Piece[(int)size, (int)size];
			for (int y = 0; y < (int)size; ++y)
				for (int x = 0; x < (int)size; ++x)
					pieces[x, y] = null;

			//Create some TC hosts.
			for (int i = 0; i < NHostsByBoardSize[size]; ++i)
			{
				Vector2i newPos = new Vector2i(NextInt((int)size), NextInt((int)size));
				while (hostsByPos.Any(kvp => kvp.Key.ManhattanDistance(newPos) < 2))
					newPos = new Vector2i(NextInt((int)size), NextInt((int)size));
				hostsByPos.Add(newPos, Player_TC);
			}

			//Add some TC pieces.
			foreach (var posAndType in hostsByPos)
			{
				AddPiece(new Piece(posAndType.Key, Player_TC, this));
			}
		}


		/// <summary>
		/// Uses the seed stored in this board to generate a new random integer.
		/// </summary>
		public int NextInt(int maxVal = int.MaxValue)
		{
			//I've been using this simple RNG for many projects now.
			//I first used it in Manbil: github.com/heyx3/Manbil.
			//It should always return a non-negative value, even with a negative seed.
			seed = (seed ^ 61) ^ (seed >> 16);
			seed += (seed << 3);
			seed ^= (seed >> 4);
			seed *= 0x27d4eb2d;
			seed ^= (seed >> 15);
			return seed % maxVal;
		}
		/// <summary>
		/// Uses the seed stored in this board to generate a new random float between 0 and 1.
		/// </summary>
		public float NextFloat()
		{
			//TODO: Double-check this gives valid results.
			return (float)(NextInt() / (double)int.MaxValue);
		}

		public Vector2i ToBoard(Vector3 world)
		{
			return new Vector2i(Mathf.Clamp((int)world.x, 0, (int)size),
								Mathf.Clamp((int)world.y, 0, (int)size));
		}
		public Vector3 ToWorld(Vector2i boardPos)
		{
			return new Vector3(boardPos.x + 0.5f, boardPos.y + 0.5f, 0.0f);
		}
		public bool IsInBounds(Vector2i boardPos)
		{
			return boardPos.x >= 0 && boardPos.x < (int)size &&
				   boardPos.y >= 0 && boardPos.y < (int)size;
		}

		public Piece GetPiece(Vector2i pos)
		{
			return pieces[pos.x, pos.y];
		}
		public override IEnumerable<Piece<Vector2i>> GetPieces()
		{
			for (int y = 0; y < (int)size; ++y)
				for (int x = 0; x < (int)size; ++x)
					if (pieces[x, y] != null)
						yield return pieces[x, y];
		}
		public void AddPiece(Piece p)
		{
			pieces[p.CurrentPos.Value.x, p.CurrentPos.Value.y] = p;
			p.CurrentPos.OnChanged += Callback_PieceMoved;
			p.Owner.OnChanged += Callback_PieceConverted;

			if (OnPieceAdded != null)
				OnPieceAdded(this, p);

			if (p.Owner.Value == Player_TC)
				CheckTCWin();
		}
		public void RemovePiece(Piece p)
		{
			pieces[p.CurrentPos.Value.x, p.CurrentPos.Value.y] = null;
			p.CurrentPos.OnChanged -= Callback_PieceMoved;
			p.Owner.OnChanged -= Callback_PieceConverted;

			if (OnPieceRemoved != null)
				OnPieceRemoved(this, p);
		}
		public void RemovePiece(Vector2i pos) { RemovePiece(GetPiece(pos)); }

		public BoardGames.Players? GetHost(Vector2i tilePos)
		{
			if (hostsByPos.ContainsKey(tilePos))
				return hostsByPos[tilePos];
			else
				return null;
		}
		public void AddHost(Vector2i tilePos, BoardGames.Players team)
		{
			hostsByPos.Add(tilePos, team);
			if (OnHostCreated != null)
				OnHostCreated(this, tilePos, team);
		}
		public void RemoveHost(Vector2i tilePos)
		{
			var team = hostsByPos[tilePos];
			hostsByPos.Remove(tilePos);

			if (OnHostDestroyed != null)
				OnHostDestroyed(this, tilePos, team);
		}

		public override IEnumerable<BoardGames.Action<Vector2i>> GetActions(Players player)
		{
			if (player == Player_Humans)
			{
				for (Vector2i pos = new Vector2i(0, 0); pos.y < (int)size; ++pos.y)
					for (pos.x = 0; pos.x < (int)size; ++pos.x)
						if (GetPiece(pos) == null)
						{
							yield return new Action_PlaceFriendly(pos, this,
																  GetCapturesFrom(pos, Player_Humans, null),
																  GetBlockCompletionFrom(pos, Player_Humans, null));
						}
			}
		}
		public override IEnumerable<BoardGames.Action<Vector2i>> GetActions(Piece<Vector2i> piece)
		{
			if (piece.Owner.Value == Player_Humans)
			{
				//Breadth-first search up to two hops away.
				//Store a "hop" as the current position, and the number of hops left.
				Queue<KeyValuePair<Vector2i, int>> hopsToCheck = new Queue<KeyValuePair<Vector2i, int>>();
				HashSet<Vector2i> alreadyChecked = new HashSet<Vector2i>();
				hopsToCheck.Enqueue(new KeyValuePair<Vector2i, int>(piece.CurrentPos, 2));
				alreadyChecked.Add(piece.CurrentPos);
				while (hopsToCheck.Count > 0)
				{
					var hop = hopsToCheck.Dequeue();
					alreadyChecked.Add(hop.Key);

					if (GetPiece(hop.Key) == null)
						yield return new Action_Move((Piece)piece, hop.Key,
													 GetCapturesFrom(hop.Key, piece.Owner,
																	 piece.CurrentPos),
													 GetBlockCompletionFrom(hop.Key, piece.Owner,
																			piece.CurrentPos));

					//Make more hops.
					if (hop.Value > 0)
					{
						Vector2i nextPos = hop.Key.LessX;
						if (IsInBounds(nextPos) && GetPiece(nextPos) == null && !alreadyChecked.Contains(nextPos))
							hopsToCheck.Enqueue(new KeyValuePair<Vector2i, int>(nextPos, hop.Value - 1));
						nextPos = hop.Key.LessY;
						if (IsInBounds(nextPos) && GetPiece(nextPos) == null && !alreadyChecked.Contains(nextPos))
							hopsToCheck.Enqueue(new KeyValuePair<Vector2i, int>(nextPos, hop.Value - 1));
						nextPos = hop.Key.MoreX;
						if (IsInBounds(nextPos) && GetPiece(nextPos) == null && !alreadyChecked.Contains(nextPos))
							hopsToCheck.Enqueue(new KeyValuePair<Vector2i, int>(nextPos, hop.Value - 1));
						nextPos = hop.Key.MoreY;
						if (IsInBounds(nextPos) && GetPiece(nextPos) == null && !alreadyChecked.Contains(nextPos))
							hopsToCheck.Enqueue(new KeyValuePair<Vector2i, int>(nextPos, hop.Value - 1));
					}
				}
			}
			else
			{
				Vector2i newPos = piece.CurrentPos.Value.LessX;
				if (IsInBounds(newPos) && GetPiece(newPos) == null)
				{
					yield return new Action_Move((Piece)piece, newPos,
												 GetCapturesFrom(newPos, Player_TC, piece.CurrentPos),
												 GetBlockCompletionFrom(newPos, Player_TC,
																		piece.CurrentPos));
				}
				newPos = piece.CurrentPos.Value.LessY;
				if (IsInBounds(newPos) && GetPiece(newPos) == null)
				{
					yield return new Action_Move((Piece)piece, newPos,
												 GetCapturesFrom(newPos, Player_TC, piece.CurrentPos),
												 GetBlockCompletionFrom(newPos, Player_TC,
																		piece.CurrentPos));
				}
				newPos = piece.CurrentPos.Value.MoreX;
				if (IsInBounds(newPos) && GetPiece(newPos) == null)
				{
					yield return new Action_Move((Piece)piece, newPos,
												 GetCapturesFrom(newPos, Player_TC, piece.CurrentPos),
												 GetBlockCompletionFrom(newPos, Player_TC,
																		piece.CurrentPos));
				}
				newPos = piece.CurrentPos.Value.MoreY;
				if (IsInBounds(newPos) && GetPiece(newPos) == null)
				{
					yield return new Action_Move((Piece)piece, newPos,
												 GetCapturesFrom(newPos, Player_TC, piece.CurrentPos),
												 GetBlockCompletionFrom(newPos, Player_TC,
																		piece.CurrentPos));
				}
			}
		}

		public override void Serialize(BinaryWriter stream)
		{
			stream.Write((byte)size);
			stream.Write((Int32)seed);

			//Write the type of piece in each tile.
			for (int y = 0; y < (int)size; ++y)
			{
				for (int x = 0; x < (int)size; ++x)
				{
					Piece piece = GetPiece(new Vector2i(x, y));
					if (piece == null)
						stream.Write((byte)0);
					else if (piece.Owner.Value == Player_Humans)
						stream.Write((byte)1);
					else
						stream.Write((byte)2);
				}
			}

			//Write the hosts as a list.
			UnityEngine.Assertions.Assert.IsTrue(hostsByPos.Count <= byte.MaxValue);
			stream.Write((byte)hostsByPos.Count);
			foreach (var posAndType in hostsByPos)
			{
				UnityEngine.Assertions.Assert.IsTrue(posAndType.Key.x <= byte.MaxValue);
				UnityEngine.Assertions.Assert.IsTrue(posAndType.Key.y <= byte.MaxValue);
				
				stream.Write((byte)posAndType.Key.x);
				stream.Write((byte)posAndType.Key.y);
				stream.Write((byte)posAndType.Value);
			}
		}
		public override void Deserialize(BinaryReader stream)
		{
			size = (Sizes)stream.ReadByte();
			seed = stream.ReadInt32();

			if (pieces.GetLength(0) != (int)size)
				pieces = new Piece[(int)size, (int)size];
			hostsByPos.Clear();

			//Read the type of piece in each tile.
			for (int y = 0; y < (int)size; ++y)
			{
				for (int x = 0; x < (int)size; ++x)
				{
					byte b = stream.ReadByte();
					switch (b)
					{
						case 0: pieces[x, y] = null; break;
						case 1:
							AddPiece(new Piece(new Vector2i(x, y), Player_Humans, this));
							break;
						case 2:
							AddPiece(new Piece(new Vector2i(x, y), Player_TC, this));
							break;

						default:
							Debug.LogError("Unknown piece type " + b.ToString());
							pieces[x, y] = null;
							break;
					}
				}
			}

			//Read the hosts.
			int nHosts = (int)stream.ReadByte();
			for (int i = 0; i < nHosts; ++i)
			{
				hostsByPos.Add(new Vector2i((int)stream.ReadByte(), (int)stream.ReadByte()),
							   (Players)stream.ReadByte());
			}

			if (OnBoardDeserialized != null)
				OnBoardDeserialized(this);
		}

		private void Callback_PieceMoved(BoardGames.Piece<Vector2i> piece,
										 Vector2i oldPos, Vector2i newPos)
		{
			//If moving off a host tile, create a new piece there.
			if (hostsByPos.ContainsKey(oldPos))
			{
				pieces[oldPos.x, oldPos.y] = new Piece(oldPos, hostsByPos[oldPos], this);
				AddPiece(pieces[oldPos.x, oldPos.y]);
			}
			else
			{
				pieces[oldPos.x, oldPos.y] = null;
			}
			
			pieces[newPos.x, newPos.y] = (Piece)piece;

			//If this was a cursed piece, see if The Curse just won.
			if (piece.Owner.Value == Player_TC)
				CheckTCWin();
		}
		private void Callback_PieceConverted(BoardGames.Piece<Vector2i> piece,
											 BoardGames.Players oldOwner, BoardGames.Players newOwner)
		{
			//If there are no more cursed pieces, and all TC hosts are occupied by friendly pieces,
			//    then the humans won.
			if (newOwner == Player_Humans)
			{
				if (GetPieces(p => p.Owner.Value == Player_TC).Count() == 0)
				{
					var tcHosts = hostsByPos.Where(kvp => kvp.Value == Player_TC);
					if (tcHosts.All(kvp =>
									{
										var pc = GetPiece(kvp.Key);
										return pc != null && pc.Owner.Value == Player_Humans;
									}))
					{
						FinishedGame(Player_Humans);
					}
				}
			}
			else
			{
				CheckTCWin();
			}
		}
		
		private void CheckTCWin()
		{
			//If there are no empty spaces to place new pieces,
			//    and any existing friendly pieces have no movement options,
			//    then the humans lost.

			for (int y = 0; y < (int)size; ++y)
			{
				for (int x = 0; x < (int)size; ++x)
				{
					if (pieces[x, y] == null)
						return;

					//Check if the piece is friendly and has any moves.
					if (pieces[x, y].Owner.Value == Player_Humans &&
						GetActions(pieces[x, y]).Count() != 0)
					{
						return;
					}
				}
			}

			//There are no moves available for the humans.
			FinishedGame(Player_TC);
		}

		/// <summary>
		/// Gets all captures done by placing/moving a piece to a position.
		/// </summary>
		/// <param name="piecePos">The new position of the piece.</param>
		/// <param name="team">The piece's owner.</param>
		/// <param name="previousPos">
		/// If the action is a movement, this is the previous position of the piece.
		/// Otherwise, this is null.
		/// </param>
		private HashSet<Piece> GetCapturesFrom(Vector2i piecePos, BoardGames.Players team, Vector2i? previousPos)
		{
			HashSet<Piece> caps = new HashSet<Piece>();
			BoardGames.Players enemyTeam = team.Switched();
			
			//Check all orthogonal directions away from the piece to find a line of enemies.
			for (int i = 0; i < 4; ++i)
			{
				Vector2i dir = new Vector2i();
				switch (i)
				{
					case 0: dir = new Vector2i(-1, 0); break;
					case 1: dir = new Vector2i(1, 0); break;
					case 2: dir = new Vector2i(0, -1); break;
					case 3: dir = new Vector2i(0, 1); break;
				}

				//If there is a line of one or more enemies, followed by a friendly piece,
				//    then this is a capture.
				Vector2i lineStart = piecePos + dir;
				if (IsPieceAt(lineStart, enemyTeam))
				{
					Vector2i lineEnd = GetLineEnd(lineStart, dir);
					if (IsPieceAt(lineEnd + dir, team) &&
						(!previousPos.HasValue || previousPos.Value != lineEnd + dir))
					{
						for (Vector2i enemyPos = lineStart; enemyPos != (lineEnd + dir); enemyPos += dir)
							caps.Add(GetPiece(enemyPos));
					}
				}
			}

			return caps;
		}
		private Vector2i GetLineEnd(Vector2i lineStart, Vector2i lineDir)
		{
			BoardGames.Players team = GetPiece(lineStart).Owner;
			Vector2i linePos = lineStart;
			while (IsPieceAt(linePos + lineDir, team))
				linePos += lineDir;

			return linePos;
		}
		private bool IsPieceAt(Vector2i pos, BoardGames.Players team)
		{
			return IsInBounds(pos) && GetPiece(pos) != null && GetPiece(pos).Owner.Value == team;
		}

		/// <summary>
		/// Gets any 3x3 block of ally pieces made by placing/moving a piece to a position.
		/// </summary>
		/// <param name="previousPos">
		/// If the action is a movement, this is the previous position of the piece.
		/// Otherwise, this is null.
		/// </param>
		private Vector2i? GetBlockCompletionFrom(Vector2i piecePos, BoardGames.Players team,
												 Vector2i? previousPos)
		{
			//Try every single possible 3x3 square the piece can be a part of.
			for (int yMin = piecePos.y - 2; yMin <= piecePos.y; ++yMin)
				for (int xMin = piecePos.x - 2; xMin <= piecePos.x; ++xMin)
					if (IsBlockOf(team, piecePos, new Vector2i(xMin, yMin), previousPos))
						return new Vector2i(xMin, yMin);
			return null;
		}
		private bool IsBlockOf(BoardGames.Players team, Vector2i exception, Vector2i min,
							   Vector2i? failException)
		{
			for (int y = 0; y < 3; ++y)
				for (int x = 0; x < 3; ++x)
				{
					Vector2i pos = min + new Vector2i(x, y);

					if (failException.HasValue && pos == failException.Value)
						return false;
					else if (pos == exception)
						continue;
					
					if (!IsInBounds(pos) || GetPiece(pos) == null || GetPiece(pos).Owner.Value != team)
						return false;
				}

			return true;
		}
	}
}