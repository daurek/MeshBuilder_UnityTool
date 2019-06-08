#pragma once

#ifdef DLL_EXPORTS
#define DLL_API __declspec(dllexport) 
#else
#define DLL_API __declspec(dllimport) 
#endif

extern "C" 
{
	namespace meshBuilder 
	{
		/// Vector3f container that handles simple Vector operations
		struct Vector3
		{
			float x;
			float y;
			float z;

			/// Default constructor
			Vector3() : x(0), y(0), z(0) {};

			/// Creates vector with given coordinates
			Vector3(float _x, float _y, float _z) : x(_x), y(_y), z(_z) {};

			/// Compare vectors
			inline bool operator== (const Vector3& vector)
			{
				return x == vector.x && y == vector.y && z == vector.z;
			}

			/// Returns new added vector
			inline Vector3 operator+ (const Vector3& vector)
			{
				return Vector3(x + vector.x, y + vector.y, z + vector.z);
			}

			/// Adds given vector to the current vector
			inline Vector3& operator+= (const Vector3& vector)
			{
				x += vector.x;
				y += vector.y;
				z += vector.z;
				return *this;
			}

			/// Returns new substracted vector
			inline Vector3 operator- (const Vector3& vector)
			{
				return Vector3(x - vector.x, y - vector.y, z - vector.z);
			}

			/// Substracts given vector to the current vector
			inline Vector3& operator-= (const Vector3& vector)
			{
				x -= vector.x;
				y -= vector.y;
				z -= vector.z;
				return *this;
			}

			/// Returns new divided vector by given value
			inline Vector3 operator/ (const int value)
			{
				return Vector3(x / value, y / value, z / value);
			}

		};

		/// Int Array container that stores pointer to an int and it's size
		struct IntArray
		{
			int * array;
			int size;

			IntArray(int * _array, int _size)
			{
				array = _array;
				size = _size;
			}
		};

		/// Returns an array on vertex indices that are on the same position as the given vertex
		DLL_API IntArray	GetSameVertices(Vector3 vertices[], int length, int givenVertex);

		/// Creates a triangle on the given index with the given vertex indices
		DLL_API int *		CreateTriangle(int begin, int triangles[], int givenTriangle[]);

		/// Cross operation calculated through two given vectors and a right hand vector (direction)
		DLL_API Vector3		Cross(Vector3 firstVector, Vector3 secondVector, Vector3 rightHandVector);

		/// Normalizes the given vector
		DLL_API Vector3		Normalize(Vector3 givenVector);

		/// Calculates the middle point of the given vector array
		DLL_API Vector3		GetMiddlePoint(Vector3 vectors[], int size);
	}
}
