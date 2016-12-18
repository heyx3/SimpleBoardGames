namespace TronsTour
{
	public class Action_Move : BoardGames.Action<Vector2i>
	{
		public Vector2i StartPos { get; private set; }
		public Vector2i EndPos { get; private set; }

		public Piece ThePiece { get; private set; }


		public Action_Move(Vector2i pos, Piece isMoving)
			: base(isMoving.TheBoard)
		{
			ThePiece = isMoving;
			StartPos = ThePiece.CurrentPos;
			EndPos = pos;
		}


		public override void DoAction()
		{
			ThePiece.CurrentPos.Value = EndPos;
			base.DoAction();
		}
		public override void UndoAction()
		{
			ThePiece.CurrentPos.Value = StartPos;
			base.UndoAction();
		}
	}
}