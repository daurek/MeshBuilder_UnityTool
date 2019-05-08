#include "SampleCppDll.h"

namespace SampleCppDll {

	class Utility 
	{
	};

	Vector3 Add(Vector3 a, Vector3 b)
	{
		return Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
	}
	
	int Multiply(int a, int b)
	{
		return a * b;
	}

}
