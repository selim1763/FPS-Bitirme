using UnityEngine;

namespace fps.Enemy
{
	public static class GizmoUtility
	{
		public static void DrawGizmoDisk(Transform transform, float radius, Color color, float gizmoThickness = 0.1f)
		{
			Matrix4x4 oldMatrix = Gizmos.matrix;
			Gizmos.color = color;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, gizmoThickness, 1));
			Gizmos.DrawSphere(Vector3.zero, radius);
			Gizmos.matrix = oldMatrix;
		}
	}
}