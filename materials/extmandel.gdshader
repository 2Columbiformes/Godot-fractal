shader_type canvas_item;

uniform int   smoothing = 0;
uniform float red_frequency=1.6615;
uniform float green_frequency=1.246;
uniform float blue_frequency=0.831;
uniform float red_phase=6.3;
uniform float green_phase=6.3;
uniform float blue_phase=6.3;
uniform highp float scale = 0.5;
uniform highp vec2  position = vec2(0.75, 0.0);
uniform float aspect_ratio = 2.0;
uniform float power = 2.0;
uniform float iterations = 50.0;
uniform float escape = 1.0;


/****** Math functions ******/

highp vec2 c_rectopol(highp vec2 c) { 	// Convert from rectangular to polar
	highp float radius = length(c);
	highp float theta = atan(c.y, c.x);
	return vec2(radius, theta);
}

highp vec2 c_poltorec(highp vec2 c) { 	// Convert from rectangular to Polar
	highp float radius = abs(c.x);
	highp float theta = c.y;
	highp float a = radius * cos(theta);
	highp float b = radius * sin(theta);
	return vec2(a, b);
}


highp vec2 c_pow(highp vec2 base, highp float ex) { // Calculate base ^ exponent
	highp vec2  b = c_rectopol(base);
	return c_poltorec( vec2(pow(b.x, ex), b.y*ex) );
}


/****** Color functions ******/
vec4 getColor(float i) {
		/* Sin/Cos creates a smooth wave between 1 and -1, offset from each other.
		*  You can have 4 evenly distributed offsets from sin, cos, -sin, -cos
		*  (sin(x) +1) / 2 -> changes the wave to go between 0 and 1 with the same frequency, +2../4 will go from 0.5 and 1.0
		*  Calculate full cycles with: sin(cycles*6.5*i)   or   (sin(cycles*5.4*i)+1.0)/2.0   or   (cos(cycles*3.8*i)+1.0)/2.0
		*/
		float ival = 2.*sqrt(i);
       	return vec4( (sin(red_frequency*ival + red_phase) +1.0)/2.0,
	       			 (sin(green_frequency*ival + green_phase) +1.0)/2.0,
       				 (sin(blue_frequency*ival + blue_phase) + 1.0)/2.0,
					 1.);
}


/****** Fragement Main ******/
// Mandelbrot is z^2+C

void fragment() {
	float i = 0.0;

	highp vec2 c;
	c.x = aspect_ratio * (UV.x - 0.5) / scale - position.x;
	c.y = (UV.y - 0.5) / scale + position.y;

	highp vec2 z = c;

	if(abs(power-2.0) < 0.001)
		while( i<iterations && length(z) <= escape ) {
			z = vec2(z.x*z.x - z.y*z.y, power*z.x*z.y) + c;		//20fps
			i++;
		}
	else
		while( i<iterations && length(z) <= escape ) {
			z = c_pow(z, power) + c;							//6-7fps
			i++;
		}


	vec4 color;

	if (i >= iterations)
		color = vec4(0.0, 0.0, 0.0, 1.0);
	else {
		if(smoothing==1) i -= log(log(length(z))) / log(abs(power));
		color = getColor(i/4.);
	}

	COLOR = color;
}

