#extension GL_OES_standard_derivatives : enable
#define pi 3.1415926535
#define n 3

precision lowp float;

uniform float time;
uniform vec2 mouse;
uniform vec2 resolution;

float dist3D(vec3 coordA, vec3 coordB) {
	float xDist = abs(coordA.x - coordB.x);
	float yDist = abs(coordA.y - coordB.y);
	float zDist = abs(coordA.z - coordB.z);

	float xyDist = sqrt(pow(xDist, 2.0) + pow(yDist, 2.0));

	return sqrt(pow(xyDist, 2.0) + pow(zDist, 2.0));
}

float avg (float numA, float numB) {
	return ((numA + numB) / 2.0);
}

float avg3 (float numA, float numB, float numC) {
	return ((numA + numB + numC) / 3.0);
}

float rnd (vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}


bool inPolygonXY(vec3 cor1, vec3 cor2, vec3 cor3, vec3 cor4, vec3 rayPos, float rot) {

	float m = (cor4.y - cor3.y) / (cor4.x - cor3.x);

	float b1 = cor1.y;

	float b2 = cor1.x;

	if (sin(rot + pi / 4.0) > 0.0) {

		bool belowY = rayPos.y <= m * rayPos.x + (cor4.y - m * cor4.x);
		bool aboveY = rayPos.y >= m * rayPos.x + (cor1.y - m * cor1.x);

		bool belowX = rayPos.x <= -1.0 * m * rayPos.y + (cor4.x + m * cor4.y);
		bool aboveX = rayPos.x >= -1.0 * m * rayPos.y + (cor1.x + m * cor1.y);
		return (aboveY && belowY && aboveX && belowX);
	}
	else {

		bool belowY = rayPos.y >= m * rayPos.x + (cor4.y - m * cor4.x);
		bool aboveY = rayPos.y <= m * rayPos.x + (cor1.y - m * cor1.x);

		bool belowX = rayPos.x >= -1.0 * m * rayPos.y + (cor4.x + m * cor4.y);
		bool aboveX = rayPos.x <= -1.0 * m * rayPos.y + (cor1.x + m * cor1.y);
		return (aboveY && belowY && aboveX && belowX);
	}

}

bool inPolygonXZ(vec3 cor1, vec3 cor2, vec3 cor3, vec3 cor4, vec3 rayPos, float rot) {

	float m = (cor4.z - cor3.z) / (cor4.x - cor3.x);

	float b1 = cor1.z;

	float b2 = cor1.x;

	if (sin(rot + pi / 4.0) > 0.0) {
		bool belowZ = rayPos.z <= m * rayPos.x + (cor4.z - m * cor4.x);
		bool aboveZ = rayPos.z >= m * rayPos.x + (cor1.z - m * cor1.x);

		bool belowX = rayPos.x <= -1.0 * m * rayPos.z + (cor4.x + m * cor4.z);
		bool aboveX = rayPos.x >= -1.0 * m * rayPos.z + (cor1.x + m * cor1.z);
		return (aboveZ && belowZ && aboveX && belowX);
	}
	
	else {
		bool belowZ = rayPos.z >= m * rayPos.x + (cor4.z - m * cor4.x);
		bool aboveZ = rayPos.z <= m * rayPos.x + (cor1.z - m * cor1.x);

		bool belowX = rayPos.x >= -1.0 * m * rayPos.z + (cor4.x + m * cor4.z);
		bool aboveX = rayPos.x <= -1.0 * m * rayPos.z + (cor1.x + m * cor1.z);
		return (aboveZ && belowZ && aboveX && belowX);
	}

}

bool inCube (vec3 center, float size, float rot, vec3 rayPos) {
	vec3 c = center;
	//vec3 cor1 = vec3(-1.0 * size * sin(rot) + c.x, -1.0 * size * cos(rot) + c.y, c.z);
	//vec3 cor2 = vec3(size * cos(rot) + c.x, -1.0 * size * sin(rot) + c.y, c.z);
	//vec3 cor3 = vec3(-1.0 * size * cos(rot) + c.x, size * sin(rot) + c.y, c.z);
	//vec3 cor4 = vec3(size * sin(rot) + c.x, size * cos(rot) + c.y, c.z);
	vec3 cor1 = vec3(-1.0 * size * sin(rot) + c.x, c.y, (-1.0 * size * cos(rot) + c.z)) ;
	vec3 cor2 = vec3(size * cos(rot) + c.x,        c.y, (-1.0 * size * sin(rot) + c.z));
	vec3 cor3 = vec3(-1.0 * size * cos(rot) + c.x, c.y, (size * sin(rot) + c.z));
	vec3 cor4 = vec3(size * sin(rot) + c.x,        c.y, (size * cos(rot) + c.z));

	bool inZ = rayPos.z > c.z - (size / 2.0) * (resolution.y / resolution.x) && rayPos.z < c.z + (size / 2.0) * (resolution.y / resolution.x);
	bool inY = rayPos.y > c.y - size && rayPos.y < c.y + size / 2.0;

	if (inPolygonXZ(cor1, cor2, cor3, cor4, rayPos, rot) && inY) {
		return bool(true);
	}
	
	else {
		return bool(false);
	}
}

float inShade (vec3 point, vec4 lightSource, vec4 shape, float size) {

	const float rayStepSize = 0.05;
	const float steps = 3.0;
	bool inObj = false;
	vec3 rayPos = point;

	float dist = dist3D(lightSource.xyz, point);

	float xStep = (lightSource.x - point.x) / dist;
	float yStep = (lightSource.y - point.y) / dist;
	float zStep = (lightSource.z  * (resolution.x / resolution.y) - point.z) /dist ;

	for (float i = 0.1; i < steps; i += rayStepSize) {
		if (!inObj) {
			vec3 rayPos = vec3(point.x + (xStep * i), point.y + (yStep * i), point.z + (zStep * i));
			// the ray tavels based on the pixel

			if (inCube(shape.xyz, size, shape.w, rayPos) && shape.w != -1.0) {
				return lightSource.w * pow((i / dist), 2.0);
				inObj = true;
			}
			else if (dist3D(shape.xyz, rayPos) <= size && shape.w == -1.0) {
				return lightSource.w * pow((i / dist), 2.0);
				inObj = true;
			}

		}

	}
	return lightSource.w * (3.0 / dist);
}



///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////

//rotates on the center
vec4 newCube (vec3 center, float size, float rot, vec3 rayPos, vec3 col, float steps) {
	vec3 c = center;
	//vec3 cor1 = vec3(-1.0 * size * sin(rot) + c.x, -1.0 * size * cos(rot) + c.y, c.z);
	//vec3 cor2 = vec3(size * cos(rot) + c.x, -1.0 * size * sin(rot) + c.y, c.z);
	//vec3 cor3 = vec3(-1.0 * size * cos(rot) + c.x, size * sin(rot) + c.y, c.z);
	//vec3 cor4 = vec3(size * sin(rot) + c.x, size * cos(rot) + c.y, c.z);
	vec3 cor1 = vec3(-1.0 * size * sin(rot) + c.x, c.y, (-1.0 * size * cos(rot) + c.z)) ;
	vec3 cor2 = vec3(size * cos(rot) + c.x,        c.y, (-1.0 * size * sin(rot) + c.z));
	vec3 cor3 = vec3(-1.0 * size * cos(rot) + c.x, c.y, (size * sin(rot) + c.z));
	vec3 cor4 = vec3(size * sin(rot) + c.x,        c.y, (size * cos(rot) + c.z));

	bool inZ = rayPos.z > c.z - (size / 2.0) * (resolution.y / resolution.x) && rayPos.z < c.z + (size / 2.0) * (resolution.y / resolution.x);
	bool inY = rayPos.y > c.y - size && rayPos.y < c.y + size / 2.0;


	if (inPolygonXZ(cor1, cor2, cor3, cor4, rayPos, rot) && inY) {
		return vec4(col, 1.0);
	}
	
	else {
		return vec4(vec3(0.1), 1.0);
	}
}

//Rotates on a corner
vec4 box (vec3 cor, float size, float rot, vec3 rayPos, vec3 col, float steps) {

	vec3 newCor2 = vec3(cor.x + (size * cos(rot)), cor.y + (size * sin(-1.0 * rot)), cor.z);
	vec3 newCor3 = vec3(cor.x + (size * sin(rot)), cor.y + (size * cos(rot)), cor.z);
	vec3 newCor4 = vec3(cor.x + (sqrt(2.0 * pow(size, 2.0)) * sin(rot + pi / 4.0)), cor.y + (sqrt(2.0 * pow(size, 2.0)) * cos(rot + pi / 4.0)), cor.z);

	bool inZ = rayPos.z > cor.z - size * (resolution.y / resolution.x) && rayPos.z < cor.z + size * (resolution.y / resolution.x);

	float edgeSize = 0.01;

	if (inPolygonXY(cor, newCor2, newCor3, newCor4, rayPos, rot) && inZ) {
		bool edgeX = (cor.x  < rayPos.x && rayPos.x < cor.x + edgeSize);
		bool edgeY = (cor.y < rayPos.y && rayPos.y < cor.y + edgeSize);
		bool edgeZ = (cor.z < rayPos.z && rayPos.z < cor.z + edgeSize);

		//edgeX && edgeY || edgeX && edgeZ || edgeZ && edgeY


		if (edgeX && edgeY || edgeX && edgeZ || edgeZ && edgeY) { // detects edge
			return vec4(vec3(1.0), 1.0);
		}
		return vec4(col, 1.0);
	}

	else {
			return vec4(vec3(0.1), 1.0);
	}
}


// same as cube except the size in the y direction is fixed
vec4 plane (vec3 center, float width, float height, float rot, vec3 rayPos, vec3 col, float steps) {
	vec3 c = center;
	vec3 cor1 = vec3(-1.0 * height * sin(rot) + c.x, c.y, (-1.0 * width * cos(rot) + c.z)) ;
	vec3 cor2 = vec3(height * cos(rot) + c.x,        c.y, (-1.0 * width * sin(rot) + c.z));
	vec3 cor3 = vec3(-1.0 * height * cos(rot) + c.x, c.y, (width * sin(rot) + c.z));
	vec3 cor4 = vec3(height * sin(rot) + c.x,        c.y, (width * cos(rot) + c.z));

	bool inY = rayPos.y > c.y - 0.005 && rayPos.y < c.y + 0.005;
	if (inPolygonXZ(cor1, cor2, cor3, cor4, rayPos, rot) && inY) {
		return vec4(col, 1.0);
	}
	
	else {
		return vec4(vec3(0.1), 1.0);
	}
}

vec4 sphere (vec3 center, float radius, vec3 rayPos, vec3 col) {

	bool inSphere = (dist3D(center, rayPos) <= radius); // if the ray is in the sphere

	if (inSphere) {
		return vec4(vec3(col), 1.0);
	}

	else {
		return vec4(vec3(0.1), 1.0);
	}
}


void main(void) {

	float rxry = resolution.x / resolution.y;
	vec2 st = gl_FragCoord.xy / resolution.xy;
	st.x *= rxry;

	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	vec3 col;

	vec4 camPos = vec4(vec3(0.0, 1.0, -6.0), 0.0);

	vec4 light = vec4(vec3(-1.0, 2.0, 1.5), 1.5);
	vec4 light2 = vec4(vec3(1.0, 0.5, -1.0), 0.5);
	vec4 light3 = vec4(vec3(1.0, 1.3, 1.0), 2.0);

	vec4 boxC1 = vec4(vec3(0.0, 0.5, 0.0), 0.0);
	vec4 boxC2 = vec4(vec3(1.5, 0.0 ,-2.0), 0.0);
	//vec4 boxCorner1 = vec4(vec3(0.0, 0.5, 3.0), 0.3); //x, y, z, rotation

	vec4 planeC1 = vec4(vec3(0.0, -0.5, 0.0), pi/ 4.0);

	vec4 sphereCenter = vec4(vec3(-1.7, 0.0, -1.7), -1.0);

	// different colours
	vec3 red = vec3(0.92, 0.25, 0.2);
	vec3 green = vec3(44.0 / 255.0, 209.0 / 255.0, 58.0 / 255.0);
	vec3 blue = vec3(57.0 / 255.0, 123.0 / 255.0, 230.0 / 255.0);
	vec3 yellow = vec3(1.0, 1.0, 0.0);
	vec3 white = vec3(1.0);

	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	const float rayStepSize = 0.15;
	const float steps = 70.0;
	float shadeMult;

	bool inObj = false;
	vec3 rayPos;
	for (float i = 0.0; i < steps; i += rayStepSize) {
		if (!inObj) {
			rayPos = vec3(camPos.x + ((st.x - (rxry / 2.0)) * rayStepSize * i), camPos.y + ((st.y - 0.5) * rayStepSize * i), camPos.z + (rayStepSize * i));
			// the ray tavels based on the pixel

			//vec4 box1 = box(boxCorner1.xyz, 1.3, boxCorner1.w, rayPos, blue, i);
			vec4 box1 = newCube(boxC1.xyz, 1.0, boxC1.w, rayPos, blue, i);
			col = max(box1.xyz * (box1.w), col);

			vec4 box2 = newCube(boxC2.xyz, 0.5, boxC2.w, rayPos, yellow, i);
			col = max(box2.xyz * (box2.w), col);

			vec4 plane1 = plane(planeC1.xyz, 50.0, 200.0, planeC1.w, rayPos, red, i);
			col = max(plane1.xyz * plane1.w, col);

			vec4 sphere1 = sphere(sphereCenter.xyz, 0.5, rayPos, green);
			col = max(sphere1.xyz * sphere1.w, col);

			vec4 sphere2 = sphere(light.xyz, 0.1, rayPos, white * vec3(light.w));
			col = max(sphere2.xyz * sphere2.w, col);

			vec4 sphere3 = sphere(light2.xyz, 0.1, rayPos, white * vec3(light2.w));
			col = max(sphere3.xyz * sphere3.w, col);

			vec4 sphere4 = sphere(light3.xyz, 0.1, rayPos, white * vec3(light3.w));
			col = max(sphere4.xyz * sphere4.w, col);


			if (col != vec3(0.1)) { //makes the ray stop after it his an object
			inObj = true;

			}

		}

	}


	//shadeMult = (min(inShade(rayPos, light, boxC1, 1.0), inShade(rayPos, light, sphereCenter, 0.5)) + min(inShade(rayPos, light2, boxC1, 1.0), inShade(rayPos, light2, sphereCenter, 0.5))) / 2.0;
	shadeMult = avg3(avg3(inShade(rayPos, light, boxC1, 1.0), inShade(rayPos, light, sphereCenter, 0.5), inShade(rayPos, light, boxC2, 0.5)),
	avg3(inShade(rayPos, light2, boxC1, 1.0), inShade(rayPos, light2, sphereCenter, 0.5), inShade(rayPos, light2, boxC2, 0.5)),
				 avg3(inShade(rayPos, light3, boxC1, 1.0), inShade(rayPos, light3, sphereCenter, 0.5), inShade(rayPos, light3, boxC2, 0.5)));
	gl_FragColor = vec4(col * vec3(shadeMult), 1.0);

}
