using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FiaCR.UnityLogic
{
	public class FCR_PieceDispatcher : Singleton<FCR_PieceDispatcher>
	{
		public GameObject PiecePrefab, PieceCaptureEffectsPrefab;

		public AnimationCurve PieceMovementCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
		public float PieceMovementTime = 0.5f;

		private Dictionary<Piece, FCR_Piece> pieceToGameObj = new Dictionary<Piece, FCR_Piece>();

		/// <summary>
		/// Once pieces in this list get an FCR_Piece object created for them,
		///     they will start out tired.
		/// </summary>
		public HashSet<Piece> PiecesToStartTiredOut = new HashSet<Piece>();

		public Board TheBoard { get; private set; }
		public IEnumerable<FCR_Piece> Pieces { get { return pieceToGameObj.Values; } }
		public FCR_Piece GetPiece(Piece gamePiece) { return pieceToGameObj[gamePiece]; }


		private void Start()
		{
			TheBoard = (Board)GameMode.FCR_Game_Offline.Instance.TheBoard;

			TheBoard.OnPieceAdded += Callback_PieceAdded;
			TheBoard.OnPieceRemoved += Callback_PieceDestroyed;
			TheBoard.OnBoardDeserialized += Callback_BoardReset;

			Callback_BoardReset(TheBoard);
		}

		private void Callback_PieceAdded(Board theBoard, Piece piece)
		{
			GameObject obj = Instantiate(PiecePrefab);
			Transform tr = obj.transform;
			FCR_Piece renderer = obj.GetComponent<FCR_Piece>();

			tr.position = TheBoard.ToWorld(piece.CurrentPos);
			renderer.ToTrack = piece;

			pieceToGameObj.Add(piece, renderer);
		}
		private void Callback_PieceDestroyed(Board theBoard, Piece removed)
		{
			//Spawn the effects for when a piece is captures.
			Vector3 pos = TheBoard.ToWorld(removed.CurrentPos.Value);
			Instantiate(PieceCaptureEffectsPrefab).transform.position = pos;

			//Destroy the GameObject for the piece.
			Destroy(pieceToGameObj[removed].gameObject);
			pieceToGameObj.Remove(removed);
		}
		private void Callback_BoardReset(Board theBoard)
		{
			foreach (FCR_Piece obj in pieceToGameObj.Values)
				Destroy(obj);
			pieceToGameObj.Clear();

			foreach (Piece piece in TheBoard.GetPieces())
				Callback_PieceAdded(TheBoard, piece);
		}
	}
}
