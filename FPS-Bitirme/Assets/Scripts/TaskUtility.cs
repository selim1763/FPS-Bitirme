using System;
using System.Threading.Tasks;
using UnityEngine;

namespace fps
{
	public static class TaskUtility
	{
		public static async Task WaitForSeconds(float sec)
		{
			float t = 0.0f;
			while (t < sec)
			{
				t += Time.deltaTime;
				await Task.Yield();
			}
		}

		public static async Task WaitUntil(Func<bool> predicate)
		{
			while (!predicate.Invoke())
			{
				await Task.Yield();
			}
		}

		public static async void Forget(this Task task)
		{
			await task;
		}
	}
	
}