using System;
using UnityEngine;

namespace Cephei
{
	public class PhysicsLerp : IFixedUpdatable
	{
		[Serializable]
		public class Data
		{
			public float MoveFactor = 5;
			public float RotateFactor = 5;
			[Space]
			public float KeepDistance = 2;
			public float KeepForce = 15;
			[Space]
			public float Damp = 10;

			public Data(float moveFactor, float keepDistance, float keepForce, float damp)
			{
				MoveFactor = moveFactor;
				KeepDistance = keepDistance;
				KeepForce = keepForce;
				Damp = damp;
			}
		}

		public Transform Target;

		private Data _data;
		private Rigidbody _rb;
		
		private AxisFriction _friction;

		public PhysicsLerp(Data data, Rigidbody rb, Transform target)
		{
			_data = data;
			_rb = rb;
			Target = target;

			CreateFriction(data, rb);
		}

		private void CreateFriction(Data data, Rigidbody rb)
		{
			AxisFriction.Data frictionData = new AxisFriction.Data(data.Damp, data.Damp, data.Damp, Space.World);
			_friction = new AxisFriction(frictionData, rb);
		}

		public void UpdateWork(float fixedDelta)
		{
			Move();
			KeepDistance();
			
			_friction.UpdateWork(fixedDelta);
		}

		private void KeepDistance()
		{
			Vector3 toTarget = Target.position - _rb.position;
			float magnitude = toTarget.magnitude;

			if (magnitude > _data.KeepDistance)
			{
				float deltaDistance = magnitude - _data.KeepDistance;
				float sqrDelta = deltaDistance * deltaDistance;
				
				_rb.AddForce(toTarget.normalized * (sqrDelta * _data.KeepForce), ForceMode.Acceleration);
			}
		}

		public void SetData(Data lerpData)
		{
			_data = lerpData;
		}

		public void UpdateData(float speedFactor, float rotateFactor)
		{
			_data.MoveFactor = speedFactor;
			_data.RotateFactor = rotateFactor;
		}

		private void Move()
		{
			Vector3 toTarget = Target.position - _rb.position;
			float sqrMagintude = toTarget.sqrMagnitude;
			
			_rb.AddForce(toTarget.normalized * (sqrMagintude * _data.MoveFactor), ForceMode.Acceleration);
		}

		public void UpdateRotateFactor(float rotateFactor) => _data.RotateFactor = rotateFactor;

		public void UpdateData(float speedFactor, float keepDistance, float keepForce, float damp)
		{
			_data.MoveFactor = speedFactor;
			_data.KeepDistance = keepDistance;
			_data.KeepForce = keepForce;
			_data.Damp = damp;
		}
	}
}