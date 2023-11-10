using System.Diagnostics;
using UnityEngine;

namespace Cephei
{
	public class SmartAcceleration : IUpdatable
	{
		public enum Stadia
		{
			Acceleration,
			Inertion
		}
		
		private enum WorkMode
		{
			Stadia,
			TargetValue
		}

		[System.Serializable]
		public class CurveData
		{
			public float TimeToMaxValue = 1;
			public AnimationCurve Curve;

			public CurveData(AnimationCurve curve)
			{
				Curve = curve;
			}
		}

		[System.Serializable]
		public class Data
		{
			public float Multiply = 1;
			public float AddedValue;
		
			public CurveData AccelerationCurve = new CurveData(AnimationCurve.Linear(0, 0, 1, 1));
			[Tooltip("To be move from up to down")]
			public CurveData InertionCurve = new CurveData(AnimationCurve.Linear(0, 1, 1, 0));

			[Header("View")] 
			public float ValueView;
			public float TView;
		}

		public float ClearValue { get; private set; }

		public float Value => ClearValue * _data.Multiply + _data.AddedValue;
		
		public float Time { get; private set; }

		private Data _data;

		private float _targetClearValue;
		
		private float _accuracy = 0.02f;
		private CurveExtensions.GetTimeByValueData _getTimeByValueData;
		
		private Stadia _currentStadia;
		private CurveData _currentCurve;
		private WorkMode _workMode;

		public SmartAcceleration(Data data)
		{
			_data = data;
			_getTimeByValueData = new CurveExtensions.GetTimeByValueData();
			
			ToStartPosition();
		}

		public SmartAcceleration(Data data, CurveExtensions.GetTimeByValueData getTimeByValueData, float accuracy)
		{
			_data = data;
			_getTimeByValueData = getTimeByValueData;
			_accuracy = accuracy;
			
			ToStartPosition();
		}

		public void UpdateWork(float delta)
		{
			if (_workMode == WorkMode.TargetValue && Diff(_targetClearValue, ClearValue) > _accuracy)
				MoveToTargetClearValue(delta);
			else if(_workMode == WorkMode.Stadia)
				UpdateTimeAndValue(delta);

			UpdateView();
		}

		private void MoveToTargetClearValue(float delta)
		{
			float oldClearValue = ClearValue;
			
			Stadia newStadia = ClearValue < _targetClearValue ? Stadia.Acceleration : Stadia.Inertion;
			SetStadia(newStadia, false);
			UpdateTimeAndValue(delta);

			bool isOverstepTarget = Diff(oldClearValue, _targetClearValue) < Diff(oldClearValue, ClearValue);
			if (isOverstepTarget)
			{
				ClearValue = _targetClearValue;
				Time = FindTimeByClearValue(_targetClearValue);
			}
		}


		public SmartAcceleration ToStartPosition() => SetPosition(Stadia.Acceleration, 0);

		public SmartAcceleration ToEndPosition() => SetPosition(Stadia.Inertion, 1);

		public void SetTargetClearValue(float value)
		{
			if (CheckValueOnEdge(value)) 
				return;

			_workMode = WorkMode.TargetValue;
			_targetClearValue = value;
		}

		public void SetStadia(Stadia newStadia, bool changeWorkMode = true)
		{
			if (changeWorkMode)
				_workMode = WorkMode.Stadia;
			
			if (newStadia == _currentStadia)
				return;

			_currentStadia = newStadia;

			SetCurveByStadia(newStadia);
			Time = FindTimeByClearValue(ClearValue);
		}

		public void SetData(Data newData)
		{
			float newClearValue = (Value - newData.AddedValue) / newData.Multiply;
			_data = newData;

			SetCurveByStadia(_currentStadia);
			Time = FindTimeByClearValue(newClearValue);
		}

		private void UpdateTimeAndValue(float delta)
		{
			UpdateTime(delta);
			UpdateValue(Time);
		}

		private void UpdateTime(float delta)
		{
			Time += delta / _currentCurve.TimeToMaxValue;
			Time = Mathf.Clamp(Time, 0, 1);
		}

		private void UpdateValue(float time) =>
			ClearValue = _currentCurve.Curve.Evaluate(time);

		private float FindTimeByClearValue(float value) =>
			_currentCurve.Curve.GetTimeByValue(value, _getTimeByValueData);

		private bool CheckValueOnEdge(float value)
		{
			bool valueEqualsMax = Mathf.Abs(value - 1) < Mathf.Epsilon;
			bool valueEqualsMin = Mathf.Abs(value) < Mathf.Epsilon;

			if (valueEqualsMax || valueEqualsMin)
			{
				Stadia newStadia = valueEqualsMax ? Stadia.Acceleration : Stadia.Inertion;
				SetStadia(newStadia);
				return true;
			}

			return false;
		}

		private SmartAcceleration SetPosition(Stadia stadia, int time)
		{
			_currentStadia = stadia;
			SetCurveByStadia(stadia);
			Time = time;
			UpdateValue(Time);

			return this;
		}

		private void SetCurveByStadia(Stadia stadia)
		{
			if (stadia == Stadia.Acceleration)
				_currentCurve = _data.AccelerationCurve;
			else if (stadia == Stadia.Inertion)
				_currentCurve = _data.InertionCurve;
		}

		private float Diff(float first, float second) => Mathf.Abs(first - second);

		private void UpdateView()
		{
			_data.ValueView = Value;
			_data.TView = Time;
		}
	}
}