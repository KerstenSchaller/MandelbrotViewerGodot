using Godot;
using System;

public partial class main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var textureRect = GetNode<TextureRect>("TextureRect");
		textureRect.Position = new Vector2();
		var visibleRect = GetViewport().GetVisibleRect();
		textureRect.SetSize(visibleRect.Size);

		GetTree().Root.SizeChanged += this.onSizeChange;

		 // Get the size of the visible area
		Vector2 screenSize = GetViewportRect().Size;

		// Define the desired width/height ratio
		float ratio = 3.0f / 2.0f;

		// Calculate the dimensions of the Rect2
		float rectWidth;
		float rectHeight;

		if (screenSize.X / screenSize.Y > ratio)
		{
			// Screen is wider, use height to calculate width
			rectHeight = screenSize.Y;
			rectWidth = rectHeight * ratio;
		}
		else
		{
			// Screen is taller, use width to calculate height
			rectWidth = screenSize.X;
			rectHeight = rectWidth / ratio;
		}

		// Create the Rect2 centered in the middle of the visible area
		Rect2 centeredRect = new Rect2(
			(screenSize.X - rectWidth) / 2.0f,
			(screenSize.Y - rectHeight) / 2.0f,
			rectWidth,
			rectHeight
		);

		// Print the result for demonstration (you can remove this in your actual project)
		GD.Print("Centered Rect2:", centeredRect);
	}

	void onSizeChange()
	{
		GD.Print("sizeChanged");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
