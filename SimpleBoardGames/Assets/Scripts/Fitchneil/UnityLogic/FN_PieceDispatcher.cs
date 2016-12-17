using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Fitchneil.UnityLogic
{
	public class FN_PieceDispatcher : Singleton<FN_PieceDispatcher>
	{
		public Board TheBoard { get; private set; }
		
		public GameObject PiecePrefab;

		private Dictionary<Piece, FN_Piece> pieceToGameObj = new Dictionary<Piece, FN_Piece>();


		public void ChangeBoard(Board newBoard)
		{
			if (TheBoard != null)
			{
				TheBoard.OnPieceCaptured += Callback_PieceDestroyed;
				TheBoard.OnBoardDeserialized += Callback_BoardReset;
			}
		}

		private void Callback_PieceDestroyed(Board theBoard, Piece captured, Piece capturer)
		{
			Destroy(pieceToGameObj[captured]);
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
			FN_Piece renderer = obj.GetComponent<FN_Piece>();

			renderer.ToTrack = piece;
			return renderer;
		}
	}
}