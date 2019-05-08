#pragma once

#ifdef DLL_EXPORTS
#define DLL_API __declspec(dllexport) 
#else
#define DLL_API __declspec(dllimport) 
#endif

extern "C" 
{
	namespace SampleCppDll 
	{
		class Utility;

		struct Vector3
		{
			float x;
			float y;
			float z;

			Vector3(float _x, float _y, float _z) : x(_x), y(_y), z(_z) {};
		};

		DLL_API Vector3 Add(Vector3 a, Vector3 b);
		DLL_API int Multiply(int a, int b);
	}
}
