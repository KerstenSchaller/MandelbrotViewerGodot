using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

public partial class ComplexNumber
{

	public double Real{get{return realPart;}}
	public double Imaginary{get{return imaginaryPart;}}

	double realPart;
	double imaginaryPart;

	public ComplexNumber()
	{
		imaginaryPart = 0;
		realPart = 0;
	}
	public ComplexNumber(double real, double imaginary)
	{
		imaginaryPart = imaginary;
		realPart = real;
	}

	public ComplexNumber(ComplexNumber c)
	{
		imaginaryPart = c.imaginaryPart;
		realPart = c.realPart;
	}

	public void add(ComplexNumber c)
	{
		realPart = realPart + c.realPart;
		imaginaryPart = imaginaryPart + c.imaginaryPart;
	}

	public void square()
	{
		multiply(this);
	}

	public void multiply(ComplexNumber c)
	{
		// (a+bi) * (c + di)
		// ac + adi + bci - bd
		var tempRealPart = (realPart*c.realPart) - (imaginaryPart*c.imaginaryPart);
		var tempImaginaryPart = (realPart*c.imaginaryPart) + (imaginaryPart*c.realPart); 
		realPart = tempRealPart;
		imaginaryPart = tempImaginaryPart;
	}

	public float length()
	{
		return (float)Math.Sqrt((realPart*realPart)+(imaginaryPart*imaginaryPart));
	}



}
//------------------------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------------------------
public partial class MandelBrotSet : Node2D
{


	
	Godot.Image _image; // mandelbrotset represented as image

	public Image image
	{
		get{return _image;}
	}

	// defines the area in the complex plane which is drawn
	float realLimitLow = -2;
	float realLimitHigh = 1;
	float imaginaryLimitLow = -1;
	float imaginaryLimitHigh = 1;

	int xResolution;
	int yResolution;

	// variables used for iterating the mandelbrotset
	const int limitIterations = 200;

	public void init(int _xResolution, int _yResolution)
	{
		xResolution = _xResolution;
		yResolution = _yResolution;
		_image = Image.Create(xResolution, yResolution, false, Image.Format.Rgb8);
	}

	public	void setComplexArea(Rect2 rectangle)
	{

		realLimitLow = (float)mapPixelToComplexNumber((int)rectangle.Position.X, (int)rectangle.Position.Y, realLimitLow, realLimitHigh, imaginaryLimitLow, imaginaryLimitHigh).Real;
		realLimitHigh = (float)mapPixelToComplexNumber((int)(rectangle.Position.X + rectangle.Size.X), (int)rectangle.Position.Y, realLimitLow, realLimitHigh, imaginaryLimitLow, imaginaryLimitHigh).Real;
		imaginaryLimitLow = (float)mapPixelToComplexNumber((int)rectangle.Position.X, (int)rectangle.Position.Y, realLimitLow, realLimitHigh, imaginaryLimitLow, imaginaryLimitHigh).Imaginary;
		imaginaryLimitHigh = (float)mapPixelToComplexNumber((int)rectangle.Position.X, (int)(rectangle.Position.Y + rectangle.Size.Y), realLimitLow, realLimitHigh, imaginaryLimitLow, imaginaryLimitHigh).Imaginary;

	}

	ComplexNumber mapPixelToComplexNumber(int xPixel, int yPixel, double startX, double endX, double startY, double endY)
	{
		// length of area the current mandelbrot subset covers
		var lengthX = endX - startX;
		var lengthY = endY - startY;
		// width of one pixel
		var pixelWidth = lengthX/xResolution;
		var pixelHeight = lengthY/yResolution;
		// complexNumber of pixel 0,0 is in the middle of the pixel
		ComplexNumber complexNumber = new ComplexNumber(startX + pixelWidth/2,startY + pixelHeight/2);
		// now move to the actual pixel
		complexNumber.add(new ComplexNumber((double)xPixel*pixelWidth,(double)yPixel*pixelHeight));
		return complexNumber;
	}


	public void updateMandelBrot()
	{
		calcMandelBrot( realLimitLow, realLimitHigh, imaginaryLimitLow, imaginaryLimitHigh);
	}
	
	void calcMandelBrot(double startX, double endX, double startY, double endY)
	{
		int maxMandelbrot = 0;

		for (int y = 0; y < yResolution; y++)
		{
			for (int x = 0; x < xResolution; x++)
			{
				calcMandelBrotPixel(x,y);
			}
		}
	}


	void calcMandelBrotPixel(int xPixel, int yPixel)
	{
		calcMandelBrotPixel(xPixel, yPixel, realLimitLow, realLimitHigh, imaginaryLimitLow, imaginaryLimitHigh);

	}

	void calcMandelBrotPixel(int xPixel, int yPixel, double startX, double endX, double startY, double endY)
	{
		var mandelBrotValue = isPixelInsideMandelbrot(xPixel, yPixel, startX, endX, startY, endY);
		//color pixel according to value
		if (mandelBrotValue == -1)
		{
			_image.SetPixel(xPixel, yPixel, Colors.Black);
		}
		else
		{
			float colorvalue = Mathf.Clamp(mandelBrotValue / 100f, 0, 1);

			_image.SetPixel(xPixel, yPixel, new Color(0, 0, colorvalue));
		}


	}


	int isPixelInsideMandelbrot(int xPixel, int yPixel, double startX, double endX, double startY, double endY)
	{
		ComplexNumber c = mapPixelToComplexNumber( xPixel, yPixel, startX, endX, startY, endY);
		ComplexNumber complexResult = new ComplexNumber();
		if(c.length() > 2)return 255;
		for(int i=0;i<limitIterations;i++)
		{
			if(complexResult.length() == float.NaN || float.IsInfinity(complexResult.length()))return i; // return if already to big
			complexResult.square();
			complexResult.add(c);
			if(complexResult.length() > 2 ) // check wether point is far away
			{
				return i;
			}
		}
		return -1;
	}

}

//------------------------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------------------------

public partial class Game : Node2D
{

	MandelBrotSet mandelBrotSet = new MandelBrotSet();
	Rect2 zoomWindow = new Rect2();

	public override void _Ready()
	{
		mandelBrotSet.init( 900, 600);
		this.AddChild(mandelBrotSet);
		resetOptions();
		mandelBrotSet.updateMandelBrot();
		setImageToTextureRect(mandelBrotSet.image);
	}

	public void setImageToTextureRect(Image image)
	{
		var textureRect = GetNode<TextureRect>("TextureRect");
		var windowSize = GetTree().Root.ContentScaleSize;// size of window
		image.Resize(windowSize.X, windowSize.Y, Image.Interpolation.Nearest);
		ImageTexture tex = ImageTexture.CreateFromImage(image);
		textureRect.Texture = null;
		textureRect.Texture = tex;
	}

	//chatgptcode
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

	bool windowDrawActive = false;
	public override void _PhysicsProcess(double delta)
	{
		// Get mouse position
		Vector2 mousePosition = GetGlobalMousePosition();

		//check if mouse is inside panelcontainer
		var panelNodeRect = GetNode<Panel>("Panel").GetGlobalRect();
		var min_x = panelNodeRect.Position.X;
		var min_y = panelNodeRect.Position.Y;
		var max_x = min_x + panelNodeRect.Size.X;
		var max_y = min_y + panelNodeRect.Size.Y;

		if (!((mousePosition.X > min_x) && (mousePosition.X < max_x) && (mousePosition.Y > min_y) && (mousePosition.Y < max_y)))
		{
			//mouse interaction for drawing a zoomWindow
			if (Input.IsActionJustPressed("left_mouse_click"))
			{
				GD.Print("Mousebutton pressed");
				zoomWindow.Position = GetGlobalMousePosition();
				windowDrawActive = true;

			}
			if (windowDrawActive)
			{
				zoomWindow.Size = GetGlobalMousePosition() - zoomWindow.Position;
			}
			if (Input.IsActionJustReleased("left_mouse_click"))
			{
				GD.Print("Mousebutton released");
				windowDrawActive = false;
				mandelBrotSet.setComplexArea(GetRectWithRatio(zoomWindow));
				mandelBrotSet.updateMandelBrot();
				setImageToTextureRect(mandelBrotSet.image);
				zoomWindow = new Rect2();
			}

		}

	}

	void resetOptions()
	{
		var resolutionLabelX = GetNode<Label>("Panel/GridContainer/resolutionLabelX");
		var yResolutionEdit = GetNode<LineEdit>("Panel/GridContainer/yResolutionEdit");

		yResolutionEdit.Text = "600";
		resolutionLabelX.Text = "900";

		mandelBrotSet.init(900,600);

	}

	private void _on_apply_button_pressed()
	{
		var resolutionLabelX = GetNode<Label>("Panel/GridContainer/resolutionLabelX");
		var yResolutionEdit = GetNode<LineEdit>("Panel/GridContainer/yResolutionEdit");

		//resolution
		/*
		yResolution = Int32.Parse(yResolutionEdit.Text);
		xResolution = (int)(3f*yResolution/2f);
		resolutionLabelX.Text = xResolution.ToString();
		image = Image.Create(xResolution, yResolution, false, Image.Format.Rgb8);

		updateMandelbrot();
		*/
		

	}

	private void _on_reset_button_pressed()
	{
		resetOptions();
	}

	private void _on_zoom_step_slider_value_changed(double value)
	{
		var zoomValueLabel = GetNode<Label>("Panel/GridContainer/zoomValueLabel");
		zoomValueLabel.Text = value.ToString();
	}

	int counter = 0;
	public override void _Process(double delta)
	{

		this.QueueRedraw();
	}


	
	public override void _Draw()
	{

		if(windowDrawActive)
		{
			DrawRect(zoomWindow,Colors.DarkRed,false,1);
			DrawRect(GetRectWithRatio(zoomWindow),Colors.Green,false,1);
		}


	}


}











