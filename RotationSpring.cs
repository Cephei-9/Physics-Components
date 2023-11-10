using UnityEngine;

namespace Cephei
{
	public class RotationSpring : IFixedUpdatable
	{
		[System.Serializable]
		public enum WorkMode
		{
			AroundAxis,
			Cross
		}

		[System.Serializable]
		public class AxisData
		{
			public float Force = 5;
			public float Damp = 1;
			public AnimationCurve ForceByAngle = AnimationCurve.Linear(0, 0, 1, 1);
		}

		public struct AxisCash
		{
			public Vector3 TargetAxis;
			public float TargetNormalsDot;

			public void SetValues(Vector3 targetAxis, float targetNormalsDot)
			{
				TargetAxis = targetAxis;
				TargetNormalsDot = targetNormalsDot;
			}
		}

		[System.Serializable]
		public class Data
		{
			public AxisData RightAxisData;
			public AxisData UpAxisData;
			public AxisData ForwardAxisData;
			[Space] public WorkMode WorkMode;
		}

		public Quaternion TargetRotation { get; private set; }

		private Data _data;
		private Rigidbody _rb;

		private AxisCash _forwardCash;
		private AxisCash _rightCash;
		private AxisCash _upCash;

		private Vector3 TargetForward => TargetRotation * Vector3.forward;
		private Vector3 TargetRight => TargetRotation * Vector3.right;
		private Vector3 TargetUp => TargetRotation * Vector3.up;

		private Transform Transform => _rb.transform;

		public RotationSpring(Data data, Rigidbody rb)
		{
			_data = data;
			_rb = rb;

			Reset();
		}

		public void UpdateWork(float fixedDelta)
		{
			AddForwardResistance();
			AddRightResistance();
			AddUpResistance();
		}

		public void SetTargetRotation(Quaternion targetRotation) => TargetRotation = targetRotation;

		public void SetData(Data newData) => _data = newData;

		public void Reset()
		{
			InitForward();
			InitRight();
			InitUp();
		}

		private void InitForward() => InitAxis(Transform.right, Transform.up, TargetUp, ref _forwardCash);

		private void InitRight() => InitAxis(Transform.forward, Transform.up, TargetUp, ref _rightCash);

		private void InitUp() => InitAxis(Transform.forward, Transform.right, TargetRight, ref _upCash);

		private void InitAxis(Vector3 realAxis, Vector3 realNormal, Vector3 normal, ref AxisCash cash)
		{
			cash.TargetAxis = GetProjectAxis(realAxis, normal, realNormal);
			cash.TargetNormalsDot = GetNormalsDot(realNormal, normal);
		}

		private void AddForwardResistance() =>
			AddResistance(Transform.right, TargetUp, Transform.up, Transform.forward, ref _forwardCash,
				_data.ForwardAxisData);

		private void AddRightResistance() =>
			AddResistance(Transform.forward, TargetUp, Transform.up, Transform.right, ref _rightCash,
				_data.RightAxisData);

		private void AddUpResistance() =>
			AddResistance(Transform.forward, TargetRight, Transform.right, Transform.up, ref _upCash, _data.UpAxisData);

		private void AddResistance(Vector3 realAxis, Vector3 normal, Vector3 realNormal, Vector3 rotationAxis,
			ref AxisCash cash, AxisData axisData)
		{
			if (axisData.Force < Mathf.Epsilon)
				return;

			UpdateTargetAxis(realAxis, normal, realNormal, ref cash);

			float force = CalculateForce(realAxis, cash.TargetAxis, axisData);
			Vector3 axis = GetRotationAxis(realAxis, cash.TargetAxis, rotationAxis);

			AddForceAndDamp(force, axis, axisData);
		}

		private Vector3 GetRotationAxis(Vector3 realAxis, Vector3 targetAxis, Vector3 rotationAxis)
		{
			Vector3 cross = Vector3.Cross(realAxis, targetAxis).normalized;
			if (_data.WorkMode == WorkMode.Cross)
				return cross;


			float sign = Mathf.Sign(Vector3.Dot(cross, rotationAxis));
			return rotationAxis * sign;
		}

		private void UpdateTargetAxis(Vector3 realAxis, Vector3 normal, Vector3 realNormal, ref AxisCash cash)
		{
			Vector3 projectAxis = GetProjectAxis(realAxis, normal, realNormal, out bool projectIsZero);

			if (projectIsZero)
			{
				cash.TargetAxis = projectAxis;
				return;
			}

			Vector3 projectTargetAxis = Vector3.Project(cash.TargetAxis, projectAxis).normalized;
			float currentNormalsDot = GetNormalsDot(realNormal, normal);

			bool realAndTargetAxisIsOpposite = Vector3.Dot(projectAxis, projectTargetAxis) < 0;
			if (realAndTargetAxisIsOpposite)
			{
				bool normalsDotIsEquals = Mathf.Abs(currentNormalsDot - cash.TargetNormalsDot) < Mathf.Epsilon;
				//Если вектора противоположны, то это может произойти по двум обстаятельствам. Либо тело повернулось по
				//нормали более чем на 90 градусов. Либо тело повернулось по целевой оси и перешла 90 градусов. Это можно
				//проверить тем равны ли дот продукты в предыдущем и текущем физическом апдейте.
				//Если они равны, то значит мы вращались по нормали, а если разнятся, то значит мы вращаемся по целевой оси.
				//И от этого зависит какую ось выбрать, и какой таргет дот нам нужно будет указать

				if (normalsDotIsEquals)
					cash.SetValues(projectAxis, currentNormalsDot);
				else
					cash.TargetAxis = projectTargetAxis;

				return;
			}

			cash.SetValues(projectTargetAxis, currentNormalsDot);
		}

		private float CalculateForce(Vector3 realAxis, Vector3 targetAxis, AxisData axisData)
		{
			float angle = Vector3.Angle(realAxis, targetAxis);
			return axisData.ForceByAngle.Evaluate(angle / 180) * 180 * axisData.Force;
		}

		private void AddForceAndDamp(float force, Vector3 axis, AxisData axisData)
		{
			Vector3 projectVelocity = Vector3.Project(_rb.angularVelocity, axis);
			
			
			//axis = Vector3.forward * Vector3.Dot(axis, Transform.forward);

			_rb.AddTorque(axis * force, ForceMode.Acceleration);
			_rb.AddTorque(projectVelocity * (-1 * axisData.Damp), ForceMode.Acceleration);
		}

		private Vector3 GetProjectAxis(Vector3 realAxis, Vector3 normal, Vector3 realNormal) =>
			GetProjectAxis(realAxis, normal, realNormal, out _);

		private Vector3 GetProjectAxis(Vector3 realAxis, Vector3 normal, Vector3 realNormal, out bool projectIsZero)
		{
			Vector3 project = Vector3.ProjectOnPlane(realAxis, normal).normalized;

			projectIsZero = project == Vector3.zero;
			if (projectIsZero)
				project = -1 * Vector3.Dot(realAxis, normal) * realNormal;

			return project;
		}

		private float GetNormalsDot(Vector3 realNormal, Vector3 normal) =>
			Mathf.Sign(Vector3.Dot(realNormal, normal));
	}
}