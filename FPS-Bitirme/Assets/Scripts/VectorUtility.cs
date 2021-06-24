using UnityEngine;

namespace DefaultNamespace
{
	public static class VectorUtility
	{
		
		public static Vector2 RandomPointInsideCircle(float radius)
		{
			float a = Random.Range(0.0f, 1.0f) * 2 * Mathf.PI;
			float r = radius * Mathf.Sqrt(Random.Range(0.0f, 1.0f));

			float x = r * Mathf.Cos(a);
			float y = r * Mathf.Sin(a);
			
			return new Vector2(x, y);
		}

	}
}