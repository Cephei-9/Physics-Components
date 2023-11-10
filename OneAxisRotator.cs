using System;
using UnityEngine;

namespace Cephei
{
	public class OneAxisRotator
	{
		/*
		- На сколько большой должна быть форс
		    - Чем больше форс тем меньше длительность реверса
		    - Чем больше форс тем точнее будет следовать за графиком реальная скорость при больших изгибах
		    - Но если этот компонент комбинировать с другими компонентами(например с пружиной) то тогда чем сильнее форс тем сильнее должна быть и пружина
		    
		- Нужно ли прокидывать отдельную дату для определителя значения кривой
		    - Если нужна большая точность и речь идет об очень больших скоростях то можно ее и увеличить
		    - Если точность не важна, то ее можно и уменьшить. В дефолтном состоянии точтость определения 0.005. Тоесть это пол процента, и это довольно большая точность.
		    - Но при изменении этой даты нужно задуматься об изменении acceuracy у ускорения. Это значение при котором он считает что текущее значение и таргет равны и прекращает движение.
		    - Чем меньше точность определение на графике, тем больше должен быть этот парраметр чтобы не случалось дефектов и наоборот. В дефолтной версии он в 4 раза больше чем точность графика, но с этим можно играться
		 */
		
		[Serializable]
		public enum WorkMode
		{
			ByForce,
			Absolute
		}
		
		[System.Serializable]
		public class Data
		{
			public SmartAcceleration.Data AcceleratioinData;
			[Space]
			public float Force = 3;
			public Space Space;
			public WorkMode WorkMode;
			
			public virtual Vector3 GetAxis { get; set; }
		}
		
		[System.Serializable]
		public class AxisData : Data
		{
			public Vector3 Axis;
			public override Vector3 GetAxis => Axis;
		}

		private Data _data;

		private Rigidbody _rb;
		private readonly Transform _transform;
		private SmartAcceleration _acceleration;

		private float _lastInput;

		public OneAxisRotator(Data data, Rigidbody rb)
		{
			_data = data;
			_rb = rb;
			_transform = _rb.transform;

			_acceleration = new SmartAcceleration(data.AcceleratioinData).ToEndPosition();
		}

		public void UpdateRotation(float input, float fixedDelta)
		{
			UpdateAcceleration(input, fixedDelta);
			
			float signInput = Mathf.Sign(input);
			Rotate(signInput);
		}

		public void SetData(Data newData)
		{
			_data = newData;
			_acceleration.SetData(newData.AcceleratioinData);
		}

		private void UpdateAcceleration(float input, float delta)
		{
			if (Mathf.Approximately(Mathf.Sign(input), Mathf.Sign(_lastInput)) == false)
				_acceleration.ToEndPosition();

			_acceleration.SetTargetClearValue(Mathf.Abs(input));
			_acceleration.UpdateWork(delta);

			_lastInput = input;
		}

		private void Rotate(float inputSign)
		{
			if (_acceleration.Value < Mathf.Epsilon)
				return;

			Vector3 axis = GetAxis();
			Vector3 projectVelocity = Vector3.Project(_rb.angularVelocity, axis);
			
			float velocitySign = Mathf.Sign(Vector3.Dot(projectVelocity, axis));
			float magnitude = projectVelocity.magnitude;
			
			bool isNotEqualsSigns = Mathf.Approximately(inputSign, velocitySign) == false;
			bool isReverseMovement = isNotEqualsSigns && magnitude > Mathf.Epsilon;

			if (_data.WorkMode == WorkMode.ByForce && (isReverseMovement || magnitude < _acceleration.Value))
				AddTorqueByForce(inputSign, axis);
			else if(_data.WorkMode == WorkMode.Absolute)
				AddAbsoluteTorque(inputSign, axis, magnitude, isReverseMovement);
			
		}

		private Vector3 GetAxis()
		{
			if(_data.Space == Space.Self)
				return TransformAxis();

			return _data.GetAxis;
		}

		private Vector3 TransformAxis()
		{
			return _transform.forward * _data.GetAxis.z + _transform.right * _data.GetAxis.x +
			       _transform.up * _data.GetAxis.y;
		}

		private void AddAbsoluteTorque(float sign, Vector3 axis, float magnitude, bool isReverse)
		{
			int reverseFactor = isReverse ? -1 : 1;
			float diff = _acceleration.Value - magnitude * reverseFactor;
			
			_rb.AddTorque(axis * (diff * sign), ForceMode.VelocityChange);
		}

		private void AddTorqueByForce(float sign, Vector3 axis)
		{
			float force = sign * _data.Force * _acceleration.ClearValue;
			_rb.AddTorque(axis * force, ForceMode.Acceleration);
		}
	}
}