using System;
using UnityEngine;

namespace Cephei
{
	public class PhysicsRotator
	{
		[Serializable]
		public class Data
		{
			public OneAxisRotator.Data RightRotatorData;
			public OneAxisRotator.Data UpRotatorData;
			public OneAxisRotator.Data ForwardRotatorData;
		}

		public OneAxisRotator RightRotator { get; }
		public OneAxisRotator UpRotator { get; }
		public OneAxisRotator ForwardRotator { get; }

		public PhysicsRotator(Data data, Rigidbody rb)
		{
			RightRotator = new OneAxisRotator(SetAxis(data.RightRotatorData, Vector3.right), rb);
			UpRotator = new OneAxisRotator(SetAxis(data.UpRotatorData, Vector3.up), rb);
			ForwardRotator = new OneAxisRotator(SetAxis(data.ForwardRotatorData, Vector3.forward), rb);
		}

		public void Rotate(float fixedDelta, float rightInput = 0, float upInput = 0, float forwardInput = 0)
		{
			RightRotator.UpdateRotation(rightInput, fixedDelta);
			UpRotator.UpdateRotation(upInput, fixedDelta);
			ForwardRotator.UpdateRotation(forwardInput, fixedDelta);
		}

		public void SetData(Data newData)
		{
			SetDataToAxisRotator(newData.RightRotatorData, RightRotator, Vector3.right);
			SetDataToAxisRotator(newData.UpRotatorData, UpRotator, Vector3.up);
			SetDataToAxisRotator(newData.ForwardRotatorData, ForwardRotator, Vector3.forward);
		}

		private static void SetDataToAxisRotator(OneAxisRotator.Data rotatorData, OneAxisRotator rotator, Vector3 axis)
		{
			rotatorData.GetAxis = axis;
			rotator.SetData(rotatorData);
		}

		private static OneAxisRotator.Data SetAxis(OneAxisRotator.Data data, Vector3 axis)
		{
			data.GetAxis = axis;
			return data;
		}
	}
}