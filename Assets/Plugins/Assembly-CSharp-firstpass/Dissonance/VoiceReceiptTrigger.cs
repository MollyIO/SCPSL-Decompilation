using System;
using UnityEngine;

namespace Dissonance
{
	[HelpURL("https://placeholder-software.co.uk/dissonance/docs/Reference/Components/Voice-Receipt-Trigger/")]
	public class VoiceReceiptTrigger : BaseCommsTrigger
	{
		private RoomMembership? _membership;

		[SerializeField]
		private string _roomName;

		private bool _scriptDeactivated;

		[SerializeField]
		private bool _useTrigger;

		public string RoomName
		{
			get
			{
				return _roomName;
			}
			set
			{
				if (_roomName != value)
				{
					_roomName = value;
					LeaveRoom();
				}
			}
		}

		public override bool UseColliderTrigger
		{
			get
			{
				return _useTrigger;
			}
			set
			{
				_useTrigger = value;
			}
		}

		public override bool CanTrigger
		{
			get
			{
				if (base.Comms == null || !base.Comms.IsStarted)
				{
					return false;
				}
				if (_roomName == null)
				{
					return false;
				}
				if (_scriptDeactivated)
				{
					return false;
				}
				return true;
			}
		}

		[Obsolete("This is equivalent to enabling this component")]
		public void StartListening()
		{
			_scriptDeactivated = false;
		}

		[Obsolete("This is equivalent to disabling this component")]
		public void StopListening()
		{
			_scriptDeactivated = true;
		}

		protected override void Update()
		{
			base.Update();
			if (CheckVoiceComm())
			{
				if (CanTrigger && (!_useTrigger || base.IsColliderTriggered) && base.TokenActivationState)
				{
					JoinRoom();
				}
				else
				{
					LeaveRoom();
				}
			}
		}

		private void JoinRoom()
		{
			if (!_membership.HasValue)
			{
				_membership = base.Comms.Rooms.Join(RoomName);
			}
		}

		private void LeaveRoom()
		{
			if (_membership.HasValue)
			{
				base.Comms.Rooms.Leave(_membership.Value);
				_membership = null;
			}
		}

		protected override void OnDisable()
		{
			if (base.Comms != null)
			{
				LeaveRoom();
			}
			base.OnDisable();
		}
	}
}
