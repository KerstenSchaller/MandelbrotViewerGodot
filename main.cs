using Godot;
using System;

public partial class main : Node2D
{
	Rect2 zoomWindow;
	bool windowDrawActive;
	TextureRect textureRect;
	ShaderMaterial textureShaderMaterial;
	
	public override void _Ready()
	{
		textureRect = GetNode<TextureRect>("TextureRect");
		textureShaderMaterial = GetShaderMaterialFromTextureRect(textureRect);
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
		//mouse interaction for drawing a zoomWindow
			if (Input.IsActionJustPressed("left_mouse_click"))
			{
				GD.Print("Mousebutton pressed");
				zoomWindow.Position = textureRect.GetLocalMousePosition();
				windowDrawActive = true;

			}
			if (windowDrawActive)
			{
				zoomWindow.Size = textureRect.GetLocalMousePosition() - zoomWindow.Position;
			}
			if (Input.IsActionJustReleased("left_mouse_click"))
			{
				GD.Print("Mousebutton released");
				windowDrawActive = false;
				setShaderMaterialParameters(GetRectWithRatio(zoomWindow));
				//zoomWindow = new Rect2();
			}

			QueueRedraw();
	}

	// Function to get the ShaderMaterial from a TextureRect
	private ShaderMaterial GetShaderMaterialFromTextureRect(TextureRect textureRect)
	{
		// Check if the TextureRect has a material override
		if (textureRect.Material is ShaderMaterial)
		{
			// Cast the material override to ShaderMaterial and return it
			return textureRect.Material as ShaderMaterial;
		}

		// If no ShaderMaterial found, return null
		return null;
	}


	float lowerLimReal = -2;
	float upperLimReal = 1;
	float lowerLimImag = -1;
	float upperLimImag = 1;
	void setShaderMaterialParameters(Rect2 rectangle)
	{
		var start = rectangle.Position;
		var end = rectangle.Position + rectangle.Size;
		
		var textureSizePixels = textureRect.Size; //resolution of textureRect

		var tempLowerLim = mapPixelToComplexNumber(start.X,  start.Y,  lowerLimReal,  upperLimReal,  lowerLimImag,  upperLimImag,  textureSizePixels);
		var tempUpperLim = mapPixelToComplexNumber(end.X,  end.Y,  lowerLimReal,  upperLimReal,  lowerLimImag,  upperLimImag,  textureSizePixels);
		
		lowerLimReal = (float)tempLowerLim.Real;
		upperLimReal = (float)tempUpperLim.Real;
		lowerLimImag = (float)tempLowerLim.Imaginary;
		upperLimImag = (float)tempUpperLim.Imaginary;

		textureShaderMaterial.SetShaderParameter("lowerLimReal", lowerLimReal);
		textureShaderMaterial.SetShaderParameter("upperLimReal", upperLimReal);
		textureShaderMaterial.SetShaderParameter("lowerLimImag", lowerLimImag);
		textureShaderMaterial.SetShaderParameter("upperLimImag", upperLimImag);

		GD.Print("x: " + lowerLimReal + " -> " + upperLimReal + "y: " + lowerLimImag + " -> " + upperLimImag);
	}

	ComplexNumber mapPixelToComplexNumber(float xPixel, float yPixel, double startX, double endX, double startY, double endY, Vector2 resolution)
	{
		// length of area the current mandelbrot subset covers
		var lengthX = endX - startX;
		var lengthY = endY - startY;
		// width of one pixel
		var pixelWidth = lengthX/resolution.X;
		var pixelHeight = lengthY/resolution.Y;
		// complexNumber of pixel 0,0 is in the middle of the pixel
		ComplexNumber complexNumber = new ComplexNumber(startX + pixelWidth/2,startY + pixelHeight/2);
		// now move to the actual pixel
		complexNumber.add(new ComplexNumber((double)xPixel*pixelWidth,(double)yPixel*pixelHeight));
		return complexNumber;
	}



	public override void _Draw()
	{
		if(windowDrawActive)
		{
			DrawRect(zoomWindow,Colors.DarkRed,false,1);
			DrawRect(GetRectWithRatio(zoomWindow),Colors.Green,false,1);
		}


	}

	public static Rect2 GetRectWithRatio(Rect2 originalRect)
	{
		// Calculate the target width based on the original height and the desired ratio (3:2)
		float targetWidth = originalRect.Size.Y * (3.0f / 2.0f);
		Vector2 newPosition;
		Vector2 newSize;
		// Check if the calculated width fits within the original width
		if (targetWidth <= originalRect.Size.X)
		{
			// If it fits, create a new Rect2 with the calculated width
			float newWidth = targetWidth;
			float newHeight = originalRect.Size.Y;
			newSize = new Vector2(newWidth, newHeight);
			newPosition = originalRect.Position + (originalRect.Size - newSize) / 2.0f;

			// Ensure the modified rectangle stays inside the original rectangle
			newPosition.X = Mathf.Clamp(newPosition.X, originalRect.Position.X, originalRect.Position.X + originalRect.Size.X - newWidth);
			newPosition.Y = Mathf.Clamp(newPosition.Y, originalRect.Position.Y, originalRect.Position.Y + originalRect.Size.Y - newHeight);
		}
		else
		{
			// If the calculated width is larger than the original width,
			// calculate the target height based on the original width and the desired ratio
			float targetHeight = originalRect.Size.X * (2.0f / 3.0f);

			// Create a new Rect2 with the calculated height
			float newWidth = originalRect.Size.X;
			float newHeight = targetHeight;
			newSize = new Vector2(newWidth, newHeight);
			newPosition = originalRect.Position + (originalRect.Size - newSize) / 2.0f;

			// Ensure the modified rectangle stays inside the original rectangle
			newPosition.X = Mathf.Clamp(newPosition.X, originalRect.Position.X, originalRect.Position.X + originalRect.Size.X - newWidth);
			newPosition.Y = Mathf.Clamp(newPosition.Y, originalRect.Position.Y, originalRect.Position.Y + originalRect.Size.Y - newHeight);

		}
		// check if new rect is inverted and invert back, inverted means that position is one of the right corners with a negative size
		if(newSize.X < 0 || newSize.Y < 0)
		{
			newPosition = newPosition + newSize;
			newSize.X = Math.Abs(newSize.X);
			newSize.Y = Math.Abs(newSize.Y);		
		}
		return new Rect2(newPosition, newSize);
	}
}
