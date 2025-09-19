shader_type canvas_item;

uniform bool smoothing = true;
uniform float red_frequency=1.6615;
uniform float green_frequency=1.246;
uniform float blue_frequency=0.831;
uniform float red_phase=6.3;
uniform float green_phase=6.3;
uniform float blue_phase=6.3;
uniform highp float scale = 0.5;
uniform highp vec2  position = vec2(0.0, 0.0);
uniform highp vec2 seed = vec2(-0.794084, 0.136444);
uniform float aspect_ratio = 2.0;
uniform float power = 2.0;
uniform float iterations = 50.0;
uniform float escape = 4.0;
uniform sampler2D Tex;
uniform bool paused = false;


/****** Math functions ******/

highp vec2 c_pol(highp vec2 c) { 	// Convert from rectangular to polar
	highp float radius = length(c);
	highp float theta = atan(c.y, c.x);
	return vec2(radius, theta);
}

highp vec2 c_rec(highp vec2 c) { 	// Convert from rectangular to Polar
	highp float radius = abs(c.x);
	highp float theta = c.y;
	highp float a = radius * cos(theta);
	highp float b = radius * sin(theta);
	return vec2(a, b);
}


highp vec2 c_pow(highp vec2 base, highp float ex) { // Calculate base ^ exponent
	highp vec2  b = c_pol(base);
	return c_rec( vec2(pow(b.x, ex), b.y*ex) );
}


/****** Color functions ******/

vec4 alpha_blend (vec4 top, vec4 bot) {
	return vec4( top.r * top.a + bot.r * bot.a * (1. - top.a),
				 top.g * top.a + bot.g * bot.a * (1. - top.a),
				 top.b * top.a + bot.b * bot.a * (1. - top.a),
				 1.0);
}


vec4 getColor(float i) {
	/* Sin/Cos creates a smooth wave between 1 and -1, offset from each other.
	*  You can have 4 evenly distributed offsets from sin, cos, -sin, -cos
	*  (sin(x) +1) / 2 -> changes the wave to go between 0 and 1 with the same frequency, +2../4 will go from 0.5 and 1.0
	*  Calculate full cycles with: sin(cycles*6.5*i)   or   (sin(cycles*5.4*i)+1.0)/2.0   or   (cos(cycles*3.8*i)+1.0)/2.0
	*/
   	float ival = 2.*log(i+1.);
   	return vec4( (sin(red_frequency*ival + red_phase) +1.0)/2.0,
       			 (sin(green_frequency*ival + green_phase) +1.0)/2.0,
   				 (sin(blue_frequency*ival + blue_phase) + 1.0)/2.0,
				 1.);
}


/****** Fragement Main ******/

void fragment() {
	vec4 color;
	if (paused) { color = texture(Tex, UV); }
	else {
		float i = 0.0;
		highp vec2 c = seed;
		highp vec2 z;
		z.x = aspect_ratio * (UV.x - 0.5) / scale - position.x;
		z.y = (UV.y - 0.5) / scale + position.y;
		if (abs(power-2.0) < 0.001)
			while( i<iterations && length(z) <= escape ) {
				z = vec2(z.x*z.x - z.y*z.y, power*z.x*z.y) + c;		//20fps
				i++;
			}
		else
			while( i<iterations && length(z) <= escape ) {
				z = c_pow(z, power) + c;							//6-7fps
				i++;
			}
		if (i >= iterations)
			color = vec4(0.0, 0.0, 0.0, 1.0);
		else {
			if(smoothing) i -= log(log(length(z))) / log(abs(power));
			color = getColor(i/4.);
		}
	}
	COLOR = color;
}

