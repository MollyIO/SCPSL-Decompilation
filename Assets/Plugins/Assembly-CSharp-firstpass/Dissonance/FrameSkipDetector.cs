using System;

namespace Dissonance
{
	internal struct FrameSkipDetector
	{
		private readonly float _maxFrameTime;

		private readonly float _minimumBreakerDuration;

		private readonly float _maxBreakerDuration;

		private readonly float _breakerResetPerSecond;

		private float _breakerCloseTimer;

		private float _currentBreakerDuration;

		private bool _breakerClosed;

		internal bool IsBreakerClosed
		{
			get
			{
				return _breakerClosed;
			}
		}

		public FrameSkipDetector(TimeSpan maxFrameTime, TimeSpan minimumBreakerDuration, TimeSpan maxBreakerDuration, TimeSpan breakerResetPerSecond)
		{
			_maxFrameTime = (float)maxFrameTime.TotalSeconds;
			_minimumBreakerDuration = (float)minimumBreakerDuration.TotalSeconds;
			_maxBreakerDuration = (float)maxBreakerDuration.TotalSeconds;
			_breakerResetPerSecond = (float)breakerResetPerSecond.TotalSeconds;
			_breakerClosed = true;
			_breakerCloseTimer = 0f;
			_currentBreakerDuration = _minimumBreakerDuration;
		}

		public bool IsFrameSkip(float deltaTime)
		{
			bool flag = deltaTime > _maxFrameTime;
			bool result = flag && _breakerClosed;
			UpdateBreaker(flag, deltaTime);
			return result;
		}

		private void UpdateBreaker(bool skip, float dt)
		{
			if (skip)
			{
				_breakerClosed = false;
				_currentBreakerDuration = Math.Min(_currentBreakerDuration * 2f, _maxBreakerDuration);
			}
			else
			{
				_currentBreakerDuration = Math.Max(_currentBreakerDuration - _breakerResetPerSecond * dt, _minimumBreakerDuration);
			}
			_breakerCloseTimer += dt;
			if (_breakerCloseTimer >= _currentBreakerDuration)
			{
				_breakerCloseTimer = 0f;
				_breakerClosed = true;
			}
		}
	}
}
