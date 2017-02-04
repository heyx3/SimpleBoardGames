using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Fitchneil.UnityLogic
{
	public class FN_PieceDispatcher : Singleton<FN_PieceDispatcher>
	{
		public Sprite Sprite_Defender, Sprite_Attacker, Sprite_King;
		public GameObject PiecePrefab, PieceCaptureEffectsPrefab;

		public AnimationCurve PieceMovementCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
		public float PieceMovementTime = 0.5f;


		public Board TheBoard { get; private set; }

		private Dictionary<Piece, FN_Piece> pieceToGameObj = new Dictionary<Piece, FN_Piece>();


		private void Start()
		{
			TheBoard = (Board)BoardGames.UnityLogic.GameMode.GameMode<Vector2i>.Instance.TheBoard;
			TheBoard.OnPieceCaptured += Callback_PieceDestroyed;
			TheBoard.OnBoardDeserialized += Callback_BoardReset;

			Callback_BoardReset(TheBoard);
		}

		private void Callback_PieceDestroyed(Board theBoard, Piece captured, Piece capturer)
		{
			//Spawn the effects for when a piece is captured.
			Vector3 pos = Board.ToWorld(captured.CurrentPos.Value);
			Instantiate(PieceCaptureEffectsPrefab).transform.position = pos;

			//Destroy the GameObject for the piece.
			Destroy(pieceToGameObj[captured].gameObject);
			pieceToGameObj.Remove(captured);
		}
		private void Callback_BoardReset(Board theBoard)
		{
			foreach (FN_Piece obj in pieceToGameObj.Values)
				Destroy(obj);
			pieceToGameObj.Clear();

			foreach (Piece p in theBoard.GetPieces())
				pieceToGameObj.Add(p, MakePiece(p));
		}

		private FN_Piece MakePiece(Piece piece)
		{
			GameObject obj = Instantiate(PiecePrefab);
			Transform tr = obj.transform;
			FN_Piece renderer = obj.GetComponent<FN_Piece>();

			tr.position = Board.ToWorld(piece.CurrentPos);
			renderer.ToTrack = piece;

			return renderer;
		}
	}
}