using System;
using UnityEngine;

namespace Cephei
{
	public class AxisFriction : IFixedUpdatable
	{
		[Serializable]
		public class Data
		{
			public float ForwardFriction = 1;
			public float UpFriction = 1;
			public float RightFriction = 1;
			[Space] 
			public Space Space;

			public Data()
			{
			}

			public Data(float forwardFriction, float upFriction, float rightFriction, Space space)
			{
				ForwardFriction = forwardFriction;
				UpFriction = upFriction;
				RightFriction = rightFriction;
				
				Space = space;
			}
		}

		private Vector3 Forward => _data.Space == Space.Self ? _rb.transform.forward : Vector3.forward;
		private Vector3 Right => _data.Space == Space.Self ? _rb.transform.right : Vector3.right;
		private Vector3 Up => _data.Space == Space.Self ? _rb.transform.up : Vector3.up;
		
		private Rigidbody _rb;
		private Data _data;

		public AxisFriction(Data data, Rigidbody rb)
		{
			_rb = rb;
			_data = data;
		}

		public void UpdateWork(float fixedDelta)
		{
			AddFrictionToAxis(Forward, _data.ForwardFriction);
			AddFrictionToAxis(Up, _data.UpFriction);
			AddFrictionToAxis(Right, _data.RightFriction);
		}

		public void SetData(Data data) => _data = data;

		private void AddFrictionToAxis(Vector3 normal, float friction)
		{
			if (friction < Mathf.Epsilon)
				return;
			
			Vector3 projectVelocity = Vector3.Project(_rb.velocity, normal);
			_rb.AddForce(-projectVelocity * friction, ForceMode.Acceleration);
		}
	}
}