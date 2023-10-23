using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolator<T>
{
    private Tuple<T, T> _interpValues;
    
    private T _defaultVal;
    
    private T _targetVal;
    
    private float _interpTime;

    private float _interpTimer;

    private bool _interpolating;
    
    private bool _inTargetState;
    
    private bool _interpolatingTowardsTargetState;

    private Func<T, T, float, T> _interpFunc;

    private Action _onToggledOn;
    
    private Action _onToggledOff;

    private Action _onReachedTargetState;

    private Action _onReachedDefaultState;

    public T DefaultVal { set => _defaultVal = value; }
    
    public T TargetVal { set => _targetVal = value; }

    public bool Interpolating => _interpolating;
    
    public Interpolator(T defaultVal, T targetVal, float interpTime,
        Func<T, T, float, T> interpFunc,
        Action onToggledOn = null, Action onToggledOff = null,
        Action onReachedTarget = null, Action onReachedDefault = null)
    {
        _defaultVal = defaultVal;
        _targetVal = targetVal;

        _interpTime = interpTime;

        _interpFunc = interpFunc;

        _onToggledOn += onToggledOn;
        _onToggledOff += onToggledOff;

        _onReachedTargetState += onReachedTarget;
        _onReachedDefaultState += onReachedDefault;
    }

    public T Update()
    {
        _interpTimer += Time.deltaTime;

        float t = _interpTimer / _interpTime;
        
        T value = _interpFunc.Invoke(_interpValues.Item1, _interpValues.Item2, t);

        if (t >= 1f)
        {
            value = _interpValues.Item2;

            _interpolating = false;

            if (_interpolatingTowardsTargetState)
            {
                _inTargetState = true;

                _onReachedTargetState?.Invoke();
            }
            else
            {
                _inTargetState = false;

                _onReachedDefaultState?.Invoke();
            }
        }

        return value;
    }

    public void Toggle(bool enable)
    {
        bool interpToTarget = enable && !_inTargetState && !_interpolatingTowardsTargetState;

        bool interpToDefault = !enable && _inTargetState && _interpolatingTowardsTargetState;

        if (interpToTarget || interpToDefault)
        {
            _interpTimer = _interpolating ? _interpTime - _interpTimer : 0;

            if (interpToTarget)
            {
                _interpolatingTowardsTargetState = true;

                _interpValues = new Tuple<T, T>(_defaultVal, _targetVal);

                _onToggledOn?.Invoke();
            }
            else
            {
                _interpolatingTowardsTargetState = false;

                _interpValues = new Tuple<T, T>(_targetVal, _defaultVal);

                _onToggledOff?.Invoke();
            }

            _interpolating = true;
        }
    }

    public void Reset()
    {
        _interpolating = false;
        
        _inTargetState = false;
        _interpolatingTowardsTargetState = false;

        _interpTimer = 0;
    }
}
