using System;
using UnityEngine;

namespace Cephei
{
	public static class CurveExtensions
	{
		public static AnimationCurve DefaultCurve = GetDefaultCurve();

		private static AnimationCurve GetDefaultCurve()
		{
			Keyframe firstKey = new Keyframe(0, 0, 0, 0, 0, 0);
			Keyframe secondKey = new Keyframe(1, 1, 0, 0, 0, 0);
			
			return new AnimationCurve(firstKey, secondKey);
		}

		[Serializable]
		public class GetTimeByValueData
		{
			public float AccuracyDrop = 0.005f;
			public float Frequency = 100;
		}

		/// <summary>
		/// Give time on curve by input value. Work only with curve where lenght equals one.
		/// </summary>
		/// <returns></returns>
		public static float GetTimeByValue(this AnimationCurve curve, float value, GetTimeByValueData data = default)
		{
			if (data == null)
				data = new GetTimeByValueData();
			
			float step = 1 / data.Frequency;
			float bestResult = Mathf.Infinity;
			float bestTime = default;
			
			for (int i = 0; i < data.Frequency; i++)
			{
				float currentTime = step * i;
				float currentValue = curve.Evaluate(currentTime);
				float differenceToValue = Difference(currentValue, value);
				
				if (differenceToValue < data.AccuracyDrop)
					return currentTime;
				
				if (differenceToValue < Difference(bestResult, value))
				{
					bestResult = currentValue;
					bestTime = currentTime;
				}
			}

			return bestTime;
			
			float Difference(float firstValue, float secondValue)
			{
				return Mathf.Abs(firstValue - secondValue);
			}
		}
	}
}