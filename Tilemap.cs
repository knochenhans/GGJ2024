using Godot;
using System;

public partial class Tilemap : TileMap
{
	[Signal]
	public delegate void ClickedEventHandler(Vector2 position);

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
			{
				var position = GetLocalMousePosition();
				var cell = LocalToMap(ToLocal(position));
				var tileData = GetCellTileData(1, cell);

				var usedTiles = GetUsedCells(0);

				if (tileData == null && usedTiles.Contains(cell))
					EmitSignal(SignalName.Clicked, position);
			}
		}
	}
}
