// Header
#include "MeshBuilder.h"
// System
#include <vector>
#include <math.h>

namespace meshBuilder 
{
	IntArray	GetSameVertices(Vector3 vertices[], int length, int givenVertex)
	{
		// Create temporal container
		std::vector<int> duplicateVertexIndices;

		// Loop through vertices 
		for (size_t i = 0; i < length; i++)
			// Until we find vertices with the same position
			if (vertices[i] == vertices[givenVertex])
				// Add them to the vector list 
				duplicateVertexIndices.push_back(i);

		// Return list data and size
		return IntArray(duplicateVertexIndices.data(), duplicateVertexIndices.size());
	}

	int *		CreateTriangle(int begin, int triangles[], int givenTriangle[])
	{
		// Loop vertex indices of triangle
		for (int i = 0; i < 3; i++)
			// Fill triangles array
			triangles[begin + i] = givenTriangle[i];
		// Return modified array
		return triangles;
	}

	Vector3		Cross(Vector3 firstVector, Vector3 secondVector, Vector3 rightHandVector)
	{
		// Substract right hand vector from both vectors
		firstVector -= rightHandVector;
		secondVector -= rightHandVector;

		// Return resulting vector from cross operation
		return Vector3(
			firstVector.y * secondVector.z - firstVector.z * secondVector.y,
		  -(firstVector.x * secondVector.z - firstVector.z * secondVector.x),
			firstVector.x * secondVector.y - firstVector.y * secondVector.x
		);
	}

	Vector3		Normalize(Vector3 givenVector)
	{
		// Calculate vector length
		float length = sqrt(givenVector.x * givenVector.x + givenVector.y * givenVector.y + givenVector.z * givenVector.z);
		// Return normalized vector
		return Vector3(givenVector.x / length, givenVector.y / length, givenVector.z / length);
	}

	Vector3		GetMiddlePoint(Vector3 vectors[], int size)
	{
		Vector3 middlePoint;
		// Loop through given vector array
		for (size_t i = 0; i < size; i++)
			// Add every vector to point
			middlePoint += vectors[i];

		// Divide by size returning middle point
		return middlePoint / size;
	}
}
