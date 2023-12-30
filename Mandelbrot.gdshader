shader_type canvas_item;

uniform float scale : hint_range(0.1,1) = 1;

uniform float lowerLimReal = -2.0;
uniform float upperLimReal = 1.0;
uniform float lowerLimImag = -1.0;
uniform float upperLimImag = 1.0;


vec2 multiplyComplex(vec2 c1, vec2 c2)
{
	// (a+bi) * (c + di)
	// ac + adi + bci - bd
	// r: ac - bd
	// i: ad + bc 
	vec2 retval;
	retval.x = (c1.x*c2.x) - (c1.y*c2.y);
	retval.y = (c1.x*c2.y) + (c1.y*c2.x); 
	return retval;
}

vec2 squareComplex(vec2 c)
{
	return multiplyComplex(c,c);
}

vec2 mapUVToComplex(vec2 uv)
{
	vec2 retval;
	retval.x = lowerLimReal + uv.x*(upperLimReal - lowerLimReal);
	retval.y = lowerLimImag + uv.y*(upperLimImag - lowerLimImag);
	return retval;
}

float complexLength(vec2 c)
{
	return sqrt((c.x*c.x) + (c.y*c.y));
}


float isMandelbrot(vec2 c)
{
	float maxIterations = 200.0;
	vec2 targetComplexNumber = vec2(0,0); 
	if(complexLength(c) > 2.0)// to large already
	{
		return 0.0;
	}
	for(float i=0.0; i < maxIterations;i=i+1.0)
	{
		targetComplexNumber = targetComplexNumber + c;
		targetComplexNumber = squareComplex(targetComplexNumber);
		if(complexLength(targetComplexNumber) > 2.0)
		{
			return i;
		}
	}
	return -1.0;

}

void fragment() 
{
	//sample texture
	// UV is a vec2, values between 0 and 1, it represents the current pixel
	//vec4 pixelcolor = texture(TEXTURE, UV)
	// acces r g b channels like: pixelcolor.r 
	// COLOR =, COLOR.r = ...are used to set the newPixelColor

	vec2 complexNumber = mapUVToComplex(UV);
	float result = isMandelbrot(complexNumber);
	if(result == -1.0)
	{
		// is inside isMandelbrot
		COLOR = vec4(0,0,0,1);

	}
	else
	{
		// is not inside isMandelbrot
		if(result > 100.0)
		{
			COLOR = vec4(0,0,1.0,1.0);
		}
		else
		{
			COLOR = vec4(0,0,result/100.0,1.0);
		}
		
	}
	

}
