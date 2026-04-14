using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MEC
{
	public class Timing : MonoBehaviour
	{
		private struct ProcessIndex : IEquatable<ProcessIndex>
		{
			public Segment seg;

			public int i;

			public bool Equals(ProcessIndex other)
			{
				return seg == other.seg && i == other.i;
			}

			public override bool Equals(object other)
			{
				if (other is ProcessIndex)
				{
					return Equals((ProcessIndex)other);
				}
				return false;
			}

			public static bool operator ==(ProcessIndex a, ProcessIndex b)
			{
				return a.seg == b.seg && a.i == b.i;
			}

			public static bool operator !=(ProcessIndex a, ProcessIndex b)
			{
				return a.seg != b.seg || a.i != b.i;
			}

			public override int GetHashCode()
			{
				return (int)(seg - 4) * 306783378 + i;
			}
		}

		[Tooltip("How quickly the SlowUpdate segment ticks.")]
		public float TimeBetweenSlowUpdateCalls = 1f / 7f;

		[Tooltip("How much data should be sent to the profiler window when it's open.")]
		public DebugInfoType ProfilerDebugAmount;

		[Tooltip("When using manual timeframe, should it run automatically after the update loop or only when TriggerManualTimframeUpdate is called.")]
		public bool AutoTriggerManualTimeframe = true;

		[Tooltip("A count of the number of Update coroutines that are currently running.")]
		[Space(12f)]
		public int UpdateCoroutines;

		[Tooltip("A count of the number of FixedUpdate coroutines that are currently running.")]
		public int FixedUpdateCoroutines;

		[Tooltip("A count of the number of LateUpdate coroutines that are currently running.")]
		public int LateUpdateCoroutines;

		[Tooltip("A count of the number of SlowUpdate coroutines that are currently running.")]
		public int SlowUpdateCoroutines;

		[Tooltip("A count of the number of RealtimeUpdate coroutines that are currently running.")]
		public int RealtimeUpdateCoroutines;

		[Tooltip("A count of the number of EditorUpdate coroutines that are currently running.")]
		public int EditorUpdateCoroutines;

		[Tooltip("A count of the number of EditorSlowUpdate coroutines that are currently running.")]
		public int EditorSlowUpdateCoroutines;

		[Tooltip("A count of the number of EndOfFrame coroutines that are currently running.")]
		public int EndOfFrameCoroutines;

		[Tooltip("A count of the number of ManualTimeframe coroutines that are currently running.")]
		public int ManualTimeframeCoroutines;

		[NonSerialized]
		public float localTime;

		[NonSerialized]
		public float deltaTime;

		public Func<float, float> SetManualTimeframeTime;

		public static Func<IEnumerator<float>, CoroutineHandle, IEnumerator<float>> ReplacementFunction;

		public const float WaitForOneFrame = 0f;

		private static object _tmpRef;

		private static int _tmpInt;

		private static bool _tmpBool;

		private static Segment _tmpSegment;

		private static CoroutineHandle _tmpHandle;

		private int _currentUpdateFrame;

		private int _currentLateUpdateFrame;

		private int _currentFixedUpdateFrame;

		private int _currentSlowUpdateFrame;

		private int _currentRealtimeUpdateFrame;

		private int _currentEndOfFrameFrame;

		private int _nextUpdateProcessSlot;

		private int _nextLateUpdateProcessSlot;

		private int _nextFixedUpdateProcessSlot;

		private int _nextSlowUpdateProcessSlot;

		private int _nextRealtimeUpdateProcessSlot;

		private int _nextEditorUpdateProcessSlot;

		private int _nextEditorSlowUpdateProcessSlot;

		private int _nextEndOfFrameProcessSlot;

		private int _nextManualTimeframeProcessSlot;

		private int _lastUpdateProcessSlot;

		private int _lastLateUpdateProcessSlot;

		private int _lastFixedUpdateProcessSlot;

		private int _lastSlowUpdateProcessSlot;

		private int _lastRealtimeUpdateProcessSlot;

		private int _lastEndOfFrameProcessSlot;

		private int _lastManualTimeframeProcessSlot;

		private float _lastUpdateTime;

		private float _lastLateUpdateTime;

		private float _lastFixedUpdateTime;

		private float _lastSlowUpdateTime;

		private float _lastRealtimeUpdateTime;

		private float _lastEndOfFrameTime;

		private float _lastManualTimeframeTime;

		private float _lastSlowUpdateDeltaTime;

		private float _lastEditorUpdateDeltaTime;

		private float _lastEditorSlowUpdateDeltaTime;

		private float _lastManualTimeframeDeltaTime;

		private ushort _framesSinceUpdate;

		private ushort _expansions = 1;

		private byte _instanceID;

		private bool _EOFPumpRan;

		private static readonly Dictionary<CoroutineHandle, HashSet<CoroutineHandle>> Links = new Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>();

		private static readonly WaitForEndOfFrame EofWaitObject = new WaitForEndOfFrame();

		private readonly Dictionary<CoroutineHandle, HashSet<CoroutineHandle>> _waitingTriggers = new Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>();

		private readonly HashSet<CoroutineHandle> _allWaiting = new HashSet<CoroutineHandle>();

		private readonly Dictionary<CoroutineHandle, ProcessIndex> _handleToIndex = new Dictionary<CoroutineHandle, ProcessIndex>();

		private readonly Dictionary<ProcessIndex, CoroutineHandle> _indexToHandle = new Dictionary<ProcessIndex, CoroutineHandle>();

		private readonly Dictionary<CoroutineHandle, string> _processTags = new Dictionary<CoroutineHandle, string>();

		private readonly Dictionary<string, HashSet<CoroutineHandle>> _taggedProcesses = new Dictionary<string, HashSet<CoroutineHandle>>();

		private readonly Dictionary<CoroutineHandle, int> _processLayers = new Dictionary<CoroutineHandle, int>();

		private readonly Dictionary<int, HashSet<CoroutineHandle>> _layeredProcesses = new Dictionary<int, HashSet<CoroutineHandle>>();

		private IEnumerator<float>[] UpdateProcesses = new IEnumerator<float>[256];

		private IEnumerator<float>[] LateUpdateProcesses = new IEnumerator<float>[8];

		private IEnumerator<float>[] FixedUpdateProcesses = new IEnumerator<float>[64];

		private IEnumerator<float>[] SlowUpdateProcesses = new IEnumerator<float>[64];

		private IEnumerator<float>[] RealtimeUpdateProcesses = new IEnumerator<float>[8];

		private IEnumerator<float>[] EditorUpdateProcesses = new IEnumerator<float>[8];

		private IEnumerator<float>[] EditorSlowUpdateProcesses = new IEnumerator<float>[8];

		private IEnumerator<float>[] EndOfFrameProcesses = new IEnumerator<float>[8];

		private IEnumerator<float>[] ManualTimeframeProcesses = new IEnumerator<float>[8];

		private bool[] UpdatePaused = new bool[256];

		private bool[] LateUpdatePaused = new bool[8];

		private bool[] FixedUpdatePaused = new bool[64];

		private bool[] SlowUpdatePaused = new bool[64];

		private bool[] RealtimeUpdatePaused = new bool[8];

		private bool[] EditorUpdatePaused = new bool[8];

		private bool[] EditorSlowUpdatePaused = new bool[8];

		private bool[] EndOfFramePaused = new bool[8];

		private bool[] ManualTimeframePaused = new bool[8];

		private CoroutineHandle _eofWatcherHandle;

		private const ushort FramesUntilMaintenance = 64;

		private const int ProcessArrayChunkSize = 64;

		private const int InitialBufferSizeLarge = 256;

		private const int InitialBufferSizeMedium = 64;

		private const int InitialBufferSizeSmall = 8;

		private static readonly Dictionary<byte, Timing> ActiveInstances = new Dictionary<byte, Timing>();

		private static Timing _instance;

		public static float LocalTime
		{
			get
			{
				return Instance.localTime;
			}
		}

		public static float DeltaTime
		{
			get
			{
				return Instance.deltaTime;
			}
		}

		public static Thread MainThread { get; private set; }

		public static Timing Instance
		{
			get
			{
				if (_instance == null || !_instance.gameObject)
				{
					GameObject gameObject = GameObject.Find("Timing Controller");
					if (gameObject == null)
					{
						GameObject gameObject2 = new GameObject();
						gameObject2.name = "Timing Controller";
						gameObject = gameObject2;
						UnityEngine.Object.DontDestroyOnLoad(gameObject);
						_instance = gameObject.AddComponent<Timing>();
					}
					else
					{
						_instance = gameObject.GetComponent<Timing>() ?? gameObject.AddComponent<Timing>();
					}
				}
				return _instance;
			}
			set
			{
				_instance = value;
			}
		}

		public static event Action OnPreExecute;

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
			}
			else
			{
				deltaTime = _instance.deltaTime;
			}
			_instanceID = 1;
			while (ActiveInstances.ContainsKey(_instanceID))
			{
				_instanceID++;
			}
			if (_instanceID == 16)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				throw new OverflowException("You are only allowed 15 instances of MEC at one time.");
			}
			ActiveInstances.Add(_instanceID, this);
			if (MainThread == null)
			{
				MainThread = Thread.CurrentThread;
			}
		}

		private void OnDestroy()
		{
			if (_instance == this)
			{
				_instance = null;
			}
			ActiveInstances.Remove(_instanceID);
		}

		private void OnEnable()
		{
			if (_nextEditorUpdateProcessSlot > 0 || _nextEditorSlowUpdateProcessSlot > 0)
			{
				OnEditorStart();
			}
			if (_nextEndOfFrameProcessSlot > 0)
			{
				RunCoroutineSingletonOnInstance(_EOFPumpWatcher(), "MEC_EOFPumpWatcher", SingletonBehavior.Abort);
			}
		}

		private void Update()
		{
			if (Timing.OnPreExecute != null)
			{
				Timing.OnPreExecute();
			}
			if (_lastSlowUpdateTime + TimeBetweenSlowUpdateCalls < Time.realtimeSinceStartup && _nextSlowUpdateProcessSlot > 0)
			{
				ProcessIndex key = new ProcessIndex
				{
					seg = Segment.SlowUpdate
				};
				if (UpdateTimeValues(key.seg))
				{
					_lastSlowUpdateProcessSlot = _nextSlowUpdateProcessSlot;
				}
				key.i = 0;
				while (key.i < _lastSlowUpdateProcessSlot)
				{
					if (!SlowUpdatePaused[key.i] && SlowUpdateProcesses[key.i] != null && !(localTime < SlowUpdateProcesses[key.i].Current))
					{
						if (ProfilerDebugAmount == DebugInfoType.None || _indexToHandle.ContainsKey(key))
						{
						}
						if (!SlowUpdateProcesses[key.i].MoveNext())
						{
							if (_indexToHandle.ContainsKey(key))
							{
								KillCoroutinesOnInstance(_indexToHandle[key]);
							}
						}
						else if (SlowUpdateProcesses[key.i] != null && float.IsNaN(SlowUpdateProcesses[key.i].Current))
						{
							if (ReplacementFunction == null)
							{
								if (_indexToHandle.ContainsKey(key))
								{
									KillCoroutinesOnInstance(_indexToHandle[key]);
								}
							}
							else
							{
								SlowUpdateProcesses[key.i] = ReplacementFunction(SlowUpdateProcesses[key.i], _indexToHandle[key]);
								ReplacementFunction = null;
								key.i--;
							}
						}
						if (ProfilerDebugAmount == DebugInfoType.None)
						{
						}
					}
					key.i++;
				}
			}
			if (_nextRealtimeUpdateProcessSlot > 0)
			{
				ProcessIndex key2 = new ProcessIndex
				{
					seg = Segment.RealtimeUpdate
				};
				if (UpdateTimeValues(key2.seg))
				{
					_lastRealtimeUpdateProcessSlot = _nextRealtimeUpdateProcessSlot;
				}
				key2.i = 0;
				while (key2.i < _lastRealtimeUpdateProcessSlot)
				{
					if (!RealtimeUpdatePaused[key2.i] && RealtimeUpdateProcesses[key2.i] != null && !(localTime < RealtimeUpdateProcesses[key2.i].Current))
					{
						if (ProfilerDebugAmount == DebugInfoType.None || _indexToHandle.ContainsKey(key2))
						{
						}
						if (!RealtimeUpdateProcesses[key2.i].MoveNext())
						{
							if (_indexToHandle.ContainsKey(key2))
							{
								KillCoroutinesOnInstance(_indexToHandle[key2]);
							}
						}
						else if (RealtimeUpdateProcesses[key2.i] != null && float.IsNaN(RealtimeUpdateProcesses[key2.i].Current))
						{
							if (ReplacementFunction != null)
							{
								RealtimeUpdateProcesses[key2.i] = ReplacementFunction(RealtimeUpdateProcesses[key2.i], _indexToHandle[key2]);
								ReplacementFunction = null;
							}
							key2.i--;
						}
						if (ProfilerDebugAmount == DebugInfoType.None)
						{
						}
					}
					key2.i++;
				}
			}
			if (_nextUpdateProcessSlot > 0)
			{
				ProcessIndex key3 = new ProcessIndex
				{
					seg = Segment.Update
				};
				if (UpdateTimeValues(key3.seg))
				{
					_lastUpdateProcessSlot = _nextUpdateProcessSlot;
				}
				key3.i = 0;
				while (key3.i < _lastUpdateProcessSlot)
				{
					if (!UpdatePaused[key3.i] && UpdateProcesses[key3.i] != null && !(localTime < UpdateProcesses[key3.i].Current))
					{
						if (ProfilerDebugAmount == DebugInfoType.None || _indexToHandle.ContainsKey(key3))
						{
						}
						if (!UpdateProcesses[key3.i].MoveNext())
						{
							if (_indexToHandle.ContainsKey(key3))
							{
								KillCoroutinesOnInstance(_indexToHandle[key3]);
							}
						}
						else if (UpdateProcesses[key3.i] != null && float.IsNaN(UpdateProcesses[key3.i].Current))
						{
							if (ReplacementFunction != null)
							{
								UpdateProcesses[key3.i] = ReplacementFunction(UpdateProcesses[key3.i], _indexToHandle[key3]);
								ReplacementFunction = null;
							}
							key3.i--;
						}
						if (ProfilerDebugAmount == DebugInfoType.None)
						{
						}
					}
					key3.i++;
				}
			}
			if (AutoTriggerManualTimeframe)
			{
				TriggerManualTimeframeUpdate();
			}
			else if (++_framesSinceUpdate > 64)
			{
				_framesSinceUpdate = 0;
				if (ProfilerDebugAmount != DebugInfoType.None)
				{
				}
				RemoveUnused();
				if (ProfilerDebugAmount == DebugInfoType.None)
				{
				}
			}
		}

		private void FixedUpdate()
		{
			if (Timing.OnPreExecute != null)
			{
				Timing.OnPreExecute();
			}
			if (_nextFixedUpdateProcessSlot <= 0)
			{
				return;
			}
			ProcessIndex key = new ProcessIndex
			{
				seg = Segment.FixedUpdate
			};
			if (UpdateTimeValues(key.seg))
			{
				_lastFixedUpdateProcessSlot = _nextFixedUpdateProcessSlot;
			}
			key.i = 0;
			while (key.i < _lastFixedUpdateProcessSlot)
			{
				if (!FixedUpdatePaused[key.i] && FixedUpdateProcesses[key.i] != null && !(localTime < FixedUpdateProcesses[key.i].Current))
				{
					if (ProfilerDebugAmount == DebugInfoType.None || _indexToHandle.ContainsKey(key))
					{
					}
					if (!FixedUpdateProcesses[key.i].MoveNext())
					{
						if (_indexToHandle.ContainsKey(key))
						{
							KillCoroutinesOnInstance(_indexToHandle[key]);
						}
					}
					else if (FixedUpdateProcesses[key.i] != null && float.IsNaN(FixedUpdateProcesses[key.i].Current))
					{
						if (ReplacementFunction != null)
						{
							FixedUpdateProcesses[key.i] = ReplacementFunction(FixedUpdateProcesses[key.i], _indexToHandle[key]);
							ReplacementFunction = null;
						}
						key.i--;
					}
					if (ProfilerDebugAmount == DebugInfoType.None)
					{
					}
				}
				key.i++;
			}
		}

		private void LateUpdate()
		{
			if (Timing.OnPreExecute != null)
			{
				Timing.OnPreExecute();
			}
			if (_nextLateUpdateProcessSlot <= 0)
			{
				return;
			}
			ProcessIndex key = new ProcessIndex
			{
				seg = Segment.LateUpdate
			};
			if (UpdateTimeValues(key.seg))
			{
				_lastLateUpdateProcessSlot = _nextLateUpdateProcessSlot;
			}
			key.i = 0;
			while (key.i < _lastLateUpdateProcessSlot)
			{
				if (!LateUpdatePaused[key.i] && LateUpdateProcesses[key.i] != null && !(localTime < LateUpdateProcesses[key.i].Current))
				{
					if (ProfilerDebugAmount == DebugInfoType.None || _indexToHandle.ContainsKey(key))
					{
					}
					if (!LateUpdateProcesses[key.i].MoveNext())
					{
						if (_indexToHandle.ContainsKey(key))
						{
							KillCoroutinesOnInstance(_indexToHandle[key]);
						}
					}
					else if (LateUpdateProcesses[key.i] != null && float.IsNaN(LateUpdateProcesses[key.i].Current))
					{
						if (ReplacementFunction != null)
						{
							LateUpdateProcesses[key.i] = ReplacementFunction(LateUpdateProcesses[key.i], _indexToHandle[key]);
							ReplacementFunction = null;
						}
						key.i--;
					}
					if (ProfilerDebugAmount == DebugInfoType.None)
					{
					}
				}
				key.i++;
			}
		}

		public void TriggerManualTimeframeUpdate()
		{
			if (Timing.OnPreExecute != null)
			{
				Timing.OnPreExecute();
			}
			if (_nextManualTimeframeProcessSlot > 0)
			{
				ProcessIndex key = new ProcessIndex
				{
					seg = Segment.ManualTimeframe
				};
				if (UpdateTimeValues(key.seg))
				{
					_lastManualTimeframeProcessSlot = _nextManualTimeframeProcessSlot;
				}
				key.i = 0;
				while (key.i < _lastManualTimeframeProcessSlot)
				{
					if (!ManualTimeframePaused[key.i] && ManualTimeframeProcesses[key.i] != null && !(localTime < ManualTimeframeProcesses[key.i].Current))
					{
						if (ProfilerDebugAmount == DebugInfoType.None || _indexToHandle.ContainsKey(key))
						{
						}
						if (!ManualTimeframeProcesses[key.i].MoveNext())
						{
							if (_indexToHandle.ContainsKey(key))
							{
								KillCoroutinesOnInstance(_indexToHandle[key]);
							}
						}
						else if (ManualTimeframeProcesses[key.i] != null && float.IsNaN(ManualTimeframeProcesses[key.i].Current))
						{
							if (ReplacementFunction != null)
							{
								ManualTimeframeProcesses[key.i] = ReplacementFunction(ManualTimeframeProcesses[key.i], _indexToHandle[key]);
								ReplacementFunction = null;
							}
							key.i--;
						}
						if (ProfilerDebugAmount == DebugInfoType.None)
						{
						}
					}
					key.i++;
				}
			}
			if (++_framesSinceUpdate > 64)
			{
				_framesSinceUpdate = 0;
				if (ProfilerDebugAmount != DebugInfoType.None)
				{
				}
				RemoveUnused();
				if (ProfilerDebugAmount == DebugInfoType.None)
				{
				}
			}
		}

		private bool OnEditorStart()
		{
			return false;
		}

		private IEnumerator<float> _EOFPumpWatcher()
		{
			while (_nextEndOfFrameProcessSlot > 0)
			{
				if (!_EOFPumpRan)
				{
					base.StartCoroutine(_EOFPump());
				}
				_EOFPumpRan = false;
				yield return 0f;
			}
			_EOFPumpRan = false;
		}

		private IEnumerator _EOFPump()
		{
			while (_nextEndOfFrameProcessSlot > 0)
			{
				yield return EofWaitObject;
				if (Timing.OnPreExecute != null)
				{
					Timing.OnPreExecute();
				}
				ProcessIndex coindex = new ProcessIndex
				{
					seg = Segment.EndOfFrame
				};
				_EOFPumpRan = true;
				if (UpdateTimeValues(coindex.seg))
				{
					_lastEndOfFrameProcessSlot = _nextEndOfFrameProcessSlot;
				}
				coindex.i = 0;
				while (coindex.i < _lastEndOfFrameProcessSlot)
				{
					if (!EndOfFramePaused[coindex.i] && EndOfFrameProcesses[coindex.i] != null && !(localTime < EndOfFrameProcesses[coindex.i].Current))
					{
						if (ProfilerDebugAmount == DebugInfoType.None || _indexToHandle.ContainsKey(coindex))
						{
						}
						if (!EndOfFrameProcesses[coindex.i].MoveNext())
						{
							if (_indexToHandle.ContainsKey(coindex))
							{
								KillCoroutinesOnInstance(_indexToHandle[coindex]);
							}
						}
						else if (EndOfFrameProcesses[coindex.i] != null && float.IsNaN(EndOfFrameProcesses[coindex.i].Current))
						{
							if (ReplacementFunction != null)
							{
								EndOfFrameProcesses[coindex.i] = ReplacementFunction(EndOfFrameProcesses[coindex.i], _indexToHandle[coindex]);
								ReplacementFunction = null;
							}
							coindex.i--;
						}
						if (ProfilerDebugAmount == DebugInfoType.None)
						{
						}
					}
					coindex.i++;
				}
			}
		}

		private void RemoveUnused()
		{
			Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>.Enumerator enumerator = _waitingTriggers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value.Count == 0)
				{
					_waitingTriggers.Remove(enumerator.Current.Key);
					enumerator = _waitingTriggers.GetEnumerator();
				}
				else if (_handleToIndex.ContainsKey(enumerator.Current.Key) && CoindexIsNull(_handleToIndex[enumerator.Current.Key]))
				{
					CloseWaitingProcess(enumerator.Current.Key);
					enumerator = _waitingTriggers.GetEnumerator();
				}
			}
			ProcessIndex key = default(ProcessIndex);
			ProcessIndex processIndex = default(ProcessIndex);
			key.seg = (processIndex.seg = Segment.Update);
			key.i = (processIndex.i = 0);
			while (key.i < _nextUpdateProcessSlot)
			{
				if (UpdateProcesses[key.i] != null)
				{
					if (key.i != processIndex.i)
					{
						UpdateProcesses[processIndex.i] = UpdateProcesses[key.i];
						UpdatePaused[processIndex.i] = UpdatePaused[key.i];
						if (_indexToHandle.ContainsKey(processIndex))
						{
							RemoveGraffiti(_indexToHandle[processIndex]);
							_handleToIndex.Remove(_indexToHandle[processIndex]);
							_indexToHandle.Remove(processIndex);
						}
						_handleToIndex[_indexToHandle[key]] = processIndex;
						_indexToHandle.Add(processIndex, _indexToHandle[key]);
						_indexToHandle.Remove(key);
					}
					processIndex.i++;
				}
				key.i++;
			}
			key.i = processIndex.i;
			while (key.i < _nextUpdateProcessSlot)
			{
				UpdateProcesses[key.i] = null;
				UpdatePaused[key.i] = false;
				if (_indexToHandle.ContainsKey(key))
				{
					RemoveGraffiti(_indexToHandle[key]);
					_handleToIndex.Remove(_indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				key.i++;
			}
			UpdateCoroutines = (_nextUpdateProcessSlot = processIndex.i);
			key.seg = (processIndex.seg = Segment.FixedUpdate);
			key.i = (processIndex.i = 0);
			while (key.i < _nextFixedUpdateProcessSlot)
			{
				if (FixedUpdateProcesses[key.i] != null)
				{
					if (key.i != processIndex.i)
					{
						FixedUpdateProcesses[processIndex.i] = FixedUpdateProcesses[key.i];
						FixedUpdatePaused[processIndex.i] = FixedUpdatePaused[key.i];
						if (_indexToHandle.ContainsKey(processIndex))
						{
							RemoveGraffiti(_indexToHandle[processIndex]);
							_handleToIndex.Remove(_indexToHandle[processIndex]);
							_indexToHandle.Remove(processIndex);
						}
						_handleToIndex[_indexToHandle[key]] = processIndex;
						_indexToHandle.Add(processIndex, _indexToHandle[key]);
						_indexToHandle.Remove(key);
					}
					processIndex.i++;
				}
				key.i++;
			}
			key.i = processIndex.i;
			while (key.i < _nextFixedUpdateProcessSlot)
			{
				FixedUpdateProcesses[key.i] = null;
				FixedUpdatePaused[key.i] = false;
				if (_indexToHandle.ContainsKey(key))
				{
					RemoveGraffiti(_indexToHandle[key]);
					_handleToIndex.Remove(_indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				key.i++;
			}
			FixedUpdateCoroutines = (_nextFixedUpdateProcessSlot = processIndex.i);
			key.seg = (processIndex.seg = Segment.LateUpdate);
			key.i = (processIndex.i = 0);
			while (key.i < _nextLateUpdateProcessSlot)
			{
				if (LateUpdateProcesses[key.i] != null)
				{
					if (key.i != processIndex.i)
					{
						LateUpdateProcesses[processIndex.i] = LateUpdateProcesses[key.i];
						LateUpdatePaused[processIndex.i] = LateUpdatePaused[key.i];
						if (_indexToHandle.ContainsKey(processIndex))
						{
							RemoveGraffiti(_indexToHandle[processIndex]);
							_handleToIndex.Remove(_indexToHandle[processIndex]);
							_indexToHandle.Remove(processIndex);
						}
						_handleToIndex[_indexToHandle[key]] = processIndex;
						_indexToHandle.Add(processIndex, _indexToHandle[key]);
						_indexToHandle.Remove(key);
					}
					processIndex.i++;
				}
				key.i++;
			}
			key.i = processIndex.i;
			while (key.i < _nextLateUpdateProcessSlot)
			{
				LateUpdateProcesses[key.i] = null;
				LateUpdatePaused[key.i] = false;
				if (_indexToHandle.ContainsKey(key))
				{
					RemoveGraffiti(_indexToHandle[key]);
					_handleToIndex.Remove(_indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				key.i++;
			}
			LateUpdateCoroutines = (_nextLateUpdateProcessSlot = processIndex.i);
			key.seg = (processIndex.seg = Segment.SlowUpdate);
			key.i = (processIndex.i = 0);
			while (key.i < _nextSlowUpdateProcessSlot)
			{
				if (SlowUpdateProcesses[key.i] != null)
				{
					if (key.i != processIndex.i)
					{
						SlowUpdateProcesses[processIndex.i] = SlowUpdateProcesses[key.i];
						SlowUpdatePaused[processIndex.i] = SlowUpdatePaused[key.i];
						if (_indexToHandle.ContainsKey(processIndex))
						{
							RemoveGraffiti(_indexToHandle[processIndex]);
							_handleToIndex.Remove(_indexToHandle[processIndex]);
							_indexToHandle.Remove(processIndex);
						}
						_handleToIndex[_indexToHandle[key]] = processIndex;
						_indexToHandle.Add(processIndex, _indexToHandle[key]);
						_indexToHandle.Remove(key);
					}
					processIndex.i++;
				}
				key.i++;
			}
			key.i = processIndex.i;
			while (key.i < _nextSlowUpdateProcessSlot)
			{
				SlowUpdateProcesses[key.i] = null;
				SlowUpdatePaused[key.i] = false;
				if (_indexToHandle.ContainsKey(key))
				{
					RemoveGraffiti(_indexToHandle[key]);
					_handleToIndex.Remove(_indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				key.i++;
			}
			SlowUpdateCoroutines = (_nextSlowUpdateProcessSlot = processIndex.i);
			key.seg = (processIndex.seg = Segment.RealtimeUpdate);
			key.i = (processIndex.i = 0);
			while (key.i < _nextRealtimeUpdateProcessSlot)
			{
				if (RealtimeUpdateProcesses[key.i] != null)
				{
					if (key.i != processIndex.i)
					{
						RealtimeUpdateProcesses[processIndex.i] = RealtimeUpdateProcesses[key.i];
						RealtimeUpdatePaused[processIndex.i] = RealtimeUpdatePaused[key.i];
						if (_indexToHandle.ContainsKey(processIndex))
						{
							RemoveGraffiti(_indexToHandle[processIndex]);
							_handleToIndex.Remove(_indexToHandle[processIndex]);
							_indexToHandle.Remove(processIndex);
						}
						_handleToIndex[_indexToHandle[key]] = processIndex;
						_indexToHandle.Add(processIndex, _indexToHandle[key]);
						_indexToHandle.Remove(key);
					}
					processIndex.i++;
				}
				key.i++;
			}
			key.i = processIndex.i;
			while (key.i < _nextRealtimeUpdateProcessSlot)
			{
				RealtimeUpdateProcesses[key.i] = null;
				RealtimeUpdatePaused[key.i] = false;
				if (_indexToHandle.ContainsKey(key))
				{
					RemoveGraffiti(_indexToHandle[key]);
					_handleToIndex.Remove(_indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				key.i++;
			}
			RealtimeUpdateCoroutines = (_nextRealtimeUpdateProcessSlot = processIndex.i);
			key.seg = (processIndex.seg = Segment.EndOfFrame);
			key.i = (processIndex.i = 0);
			while (key.i < _nextEndOfFrameProcessSlot)
			{
				if (EndOfFrameProcesses[key.i] != null)
				{
					if (key.i != processIndex.i)
					{
						EndOfFrameProcesses[processIndex.i] = EndOfFrameProcesses[key.i];
						EndOfFramePaused[processIndex.i] = EndOfFramePaused[key.i];
						if (_indexToHandle.ContainsKey(processIndex))
						{
							RemoveGraffiti(_indexToHandle[processIndex]);
							_handleToIndex.Remove(_indexToHandle[processIndex]);
							_indexToHandle.Remove(processIndex);
						}
						_handleToIndex[_indexToHandle[key]] = processIndex;
						_indexToHandle.Add(processIndex, _indexToHandle[key]);
						_indexToHandle.Remove(key);
					}
					processIndex.i++;
				}
				key.i++;
			}
			key.i = processIndex.i;
			while (key.i < _nextEndOfFrameProcessSlot)
			{
				EndOfFrameProcesses[key.i] = null;
				EndOfFramePaused[key.i] = false;
				if (_indexToHandle.ContainsKey(key))
				{
					RemoveGraffiti(_indexToHandle[key]);
					_handleToIndex.Remove(_indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				key.i++;
			}
			EndOfFrameCoroutines = (_nextEndOfFrameProcessSlot = processIndex.i);
			key.seg = (processIndex.seg = Segment.ManualTimeframe);
			key.i = (processIndex.i = 0);
			while (key.i < _nextManualTimeframeProcessSlot)
			{
				if (ManualTimeframeProcesses[key.i] != null)
				{
					if (key.i != processIndex.i)
					{
						ManualTimeframeProcesses[processIndex.i] = ManualTimeframeProcesses[key.i];
						ManualTimeframePaused[processIndex.i] = ManualTimeframePaused[key.i];
						if (_indexToHandle.ContainsKey(processIndex))
						{
							RemoveGraffiti(_indexToHandle[processIndex]);
							_handleToIndex.Remove(_indexToHandle[processIndex]);
							_indexToHandle.Remove(processIndex);
						}
						_handleToIndex[_indexToHandle[key]] = processIndex;
						_indexToHandle.Add(processIndex, _indexToHandle[key]);
						_indexToHandle.Remove(key);
					}
					processIndex.i++;
				}
				key.i++;
			}
			key.i = processIndex.i;
			while (key.i < _nextManualTimeframeProcessSlot)
			{
				ManualTimeframeProcesses[key.i] = null;
				ManualTimeframePaused[key.i] = false;
				if (_indexToHandle.ContainsKey(key))
				{
					RemoveGraffiti(_indexToHandle[key]);
					_handleToIndex.Remove(_indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				key.i++;
			}
			ManualTimeframeCoroutines = (_nextManualTimeframeProcessSlot = processIndex.i);
		}

		private void EditorRemoveUnused()
		{
			Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>.Enumerator enumerator = _waitingTriggers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (_handleToIndex.ContainsKey(enumerator.Current.Key) && CoindexIsNull(_handleToIndex[enumerator.Current.Key]))
				{
					CloseWaitingProcess(enumerator.Current.Key);
					enumerator = _waitingTriggers.GetEnumerator();
				}
			}
			ProcessIndex key = default(ProcessIndex);
			ProcessIndex processIndex = default(ProcessIndex);
			key.seg = (processIndex.seg = Segment.EditorUpdate);
			key.i = (processIndex.i = 0);
			while (key.i < _nextEditorUpdateProcessSlot)
			{
				if (EditorUpdateProcesses[key.i] != null)
				{
					if (key.i != processIndex.i)
					{
						EditorUpdateProcesses[processIndex.i] = EditorUpdateProcesses[key.i];
						EditorUpdatePaused[processIndex.i] = EditorUpdatePaused[key.i];
						if (_indexToHandle.ContainsKey(processIndex))
						{
							RemoveGraffiti(_indexToHandle[processIndex]);
							_handleToIndex.Remove(_indexToHandle[processIndex]);
							_indexToHandle.Remove(processIndex);
						}
						_handleToIndex[_indexToHandle[key]] = processIndex;
						_indexToHandle.Add(processIndex, _indexToHandle[key]);
						_indexToHandle.Remove(key);
					}
					processIndex.i++;
				}
				key.i++;
			}
			key.i = processIndex.i;
			while (key.i < _nextEditorUpdateProcessSlot)
			{
				EditorUpdateProcesses[key.i] = null;
				EditorUpdatePaused[key.i] = false;
				if (_indexToHandle.ContainsKey(key))
				{
					RemoveGraffiti(_indexToHandle[key]);
					_handleToIndex.Remove(_indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				key.i++;
			}
			EditorUpdateCoroutines = (_nextEditorUpdateProcessSlot = processIndex.i);
			key.seg = (processIndex.seg = Segment.EditorSlowUpdate);
			key.i = (processIndex.i = 0);
			while (key.i < _nextEditorSlowUpdateProcessSlot)
			{
				if (EditorSlowUpdateProcesses[key.i] != null)
				{
					if (key.i != processIndex.i)
					{
						EditorSlowUpdateProcesses[processIndex.i] = EditorSlowUpdateProcesses[key.i];
						EditorUpdatePaused[processIndex.i] = EditorUpdatePaused[key.i];
						if (_indexToHandle.ContainsKey(processIndex))
						{
							RemoveGraffiti(_indexToHandle[processIndex]);
							_handleToIndex.Remove(_indexToHandle[processIndex]);
							_indexToHandle.Remove(processIndex);
						}
						_handleToIndex[_indexToHandle[key]] = processIndex;
						_indexToHandle.Add(processIndex, _indexToHandle[key]);
						_indexToHandle.Remove(key);
					}
					processIndex.i++;
				}
				key.i++;
			}
			key.i = processIndex.i;
			while (key.i < _nextEditorSlowUpdateProcessSlot)
			{
				EditorSlowUpdateProcesses[key.i] = null;
				EditorSlowUpdatePaused[key.i] = false;
				if (_indexToHandle.ContainsKey(key))
				{
					RemoveGraffiti(_indexToHandle[key]);
					_handleToIndex.Remove(_indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				key.i++;
			}
			EditorSlowUpdateCoroutines = (_nextEditorSlowUpdateProcessSlot = processIndex.i);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, Segment.FixedUpdate, null, null, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, GameObject gameObj)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, Segment.FixedUpdate, (!(gameObj == null)) ? new int?(gameObj.GetInstanceID()) : ((int?)null), null, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, int layer)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, Segment.FixedUpdate, layer, null, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, string tag)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, Segment.FixedUpdate, null, tag, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, GameObject gameObj, string tag)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, Segment.FixedUpdate, (!(gameObj == null)) ? new int?(gameObj.GetInstanceID()) : ((int?)null), tag, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, int layer, string tag)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, Segment.FixedUpdate, layer, tag, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, segment, null, null, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, GameObject gameObj)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, segment, (!(gameObj == null)) ? new int?(gameObj.GetInstanceID()) : ((int?)null), null, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, int layer)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, segment, layer, null, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, string tag)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, segment, null, tag, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, segment, (!(gameObj == null)) ? new int?(gameObj.GetInstanceID()) : ((int?)null), tag, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, int layer, string tag)
		{
			return (coroutine != null) ? Instance.RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(Instance._instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, Segment.Update, null, null, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, GameObject gameObj)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, Segment.Update, (!(gameObj == null)) ? new int?(gameObj.GetInstanceID()) : ((int?)null), null, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, int layer)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, Segment.Update, layer, null, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, string tag)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, Segment.Update, null, tag, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, GameObject gameObj, string tag)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, Segment.Update, (!(gameObj == null)) ? new int?(gameObj.GetInstanceID()) : ((int?)null), tag, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, int layer, string tag)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, Segment.Update, layer, tag, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, segment, null, null, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, GameObject gameObj)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, segment, (!(gameObj == null)) ? new int?(gameObj.GetInstanceID()) : ((int?)null), null, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, int layer)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, segment, layer, null, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, string tag)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, segment, null, tag, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, segment, (!(gameObj == null)) ? new int?(gameObj.GetInstanceID()) : ((int?)null), tag, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, int layer, string tag)
		{
			return (coroutine != null) ? RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(_instanceID), true) : default(CoroutineHandle);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, CoroutineHandle handle, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutines(handle);
			}
			else if (IsRunning(handle))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
					return handle;
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = Instance.RunCoroutineInternal(coroutine, Segment.Update, null, null, new CoroutineHandle(Instance._instanceID), false);
					WaitForOtherHandles(coroutineHandle, handle, false);
					return coroutineHandle;
				}
				}
			}
			return Instance.RunCoroutineInternal(coroutine, Segment.Update, null, null, new CoroutineHandle(Instance._instanceID), true);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, GameObject gameObj, SingletonBehavior behaviorOnCollision)
		{
			return (!(gameObj == null)) ? RunCoroutineSingleton(coroutine, gameObj.GetInstanceID(), behaviorOnCollision) : RunCoroutine(coroutine);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, int layer, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutines(layer);
			}
			else if (Instance._layeredProcesses.ContainsKey(layer))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
				{
					HashSet<CoroutineHandle>.Enumerator enumerator = Instance._layeredProcesses[layer].GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (IsRunning(enumerator.Current))
						{
							return enumerator.Current;
						}
					}
					break;
				}
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, null, new CoroutineHandle(Instance._instanceID), false);
					WaitForOtherHandles(coroutineHandle, _instance._layeredProcesses[layer], false);
					return coroutineHandle;
				}
				}
			}
			return Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, null, new CoroutineHandle(Instance._instanceID), true);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, string tag, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutines(tag);
			}
			else if (Instance._taggedProcesses.ContainsKey(tag))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
				{
					HashSet<CoroutineHandle>.Enumerator enumerator = Instance._taggedProcesses[tag].GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (IsRunning(enumerator.Current))
						{
							return enumerator.Current;
						}
					}
					break;
				}
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = Instance.RunCoroutineInternal(coroutine, Segment.Update, null, tag, new CoroutineHandle(Instance._instanceID), false);
					WaitForOtherHandles(coroutineHandle, _instance._taggedProcesses[tag], false);
					return coroutineHandle;
				}
				}
			}
			return Instance.RunCoroutineInternal(coroutine, Segment.Update, null, tag, new CoroutineHandle(Instance._instanceID), true);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, GameObject gameObj, string tag, SingletonBehavior behaviorOnCollision)
		{
			return (!(gameObj == null)) ? RunCoroutineSingleton(coroutine, gameObj.GetInstanceID(), tag, behaviorOnCollision) : RunCoroutineSingleton(coroutine, tag, behaviorOnCollision);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, int layer, string tag, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutines(layer, tag);
				return Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, tag, new CoroutineHandle(Instance._instanceID), true);
			}
			if (!Instance._taggedProcesses.ContainsKey(tag) || !Instance._layeredProcesses.ContainsKey(layer))
			{
				return Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, tag, new CoroutineHandle(Instance._instanceID), true);
			}
			if (behaviorOnCollision == SingletonBehavior.Abort)
			{
				HashSet<CoroutineHandle>.Enumerator enumerator = Instance._taggedProcesses[tag].GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (_instance._processLayers.ContainsKey(enumerator.Current) && _instance._processLayers[enumerator.Current] == layer)
					{
						return enumerator.Current;
					}
				}
			}
			if (behaviorOnCollision == SingletonBehavior.Wait)
			{
				List<CoroutineHandle> list = new List<CoroutineHandle>();
				HashSet<CoroutineHandle>.Enumerator enumerator2 = Instance._taggedProcesses[tag].GetEnumerator();
				while (enumerator2.MoveNext())
				{
					if (Instance._processLayers.ContainsKey(enumerator2.Current) && Instance._processLayers[enumerator2.Current] == layer)
					{
						list.Add(enumerator2.Current);
					}
				}
				if (list.Count > 0)
				{
					CoroutineHandle coroutineHandle = _instance.RunCoroutineInternal(coroutine, Segment.Update, layer, tag, new CoroutineHandle(_instance._instanceID), false);
					WaitForOtherHandles(coroutineHandle, list, false);
					return coroutineHandle;
				}
			}
			return Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, tag, new CoroutineHandle(Instance._instanceID), true);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, CoroutineHandle handle, Segment segment, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutines(handle);
			}
			else if (IsRunning(handle))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
					return handle;
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = Instance.RunCoroutineInternal(coroutine, segment, null, null, new CoroutineHandle(Instance._instanceID), false);
					WaitForOtherHandles(coroutineHandle, handle, false);
					return coroutineHandle;
				}
				}
			}
			return Instance.RunCoroutineInternal(coroutine, segment, null, null, new CoroutineHandle(Instance._instanceID), true);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, SingletonBehavior behaviorOnCollision)
		{
			return (!(gameObj == null)) ? RunCoroutineSingleton(coroutine, segment, gameObj.GetInstanceID(), behaviorOnCollision) : RunCoroutine(coroutine, segment);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, int layer, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutines(layer);
			}
			else if (Instance._layeredProcesses.ContainsKey(layer))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
				{
					HashSet<CoroutineHandle>.Enumerator enumerator = Instance._layeredProcesses[layer].GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (IsRunning(enumerator.Current))
						{
							return enumerator.Current;
						}
					}
					break;
				}
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = Instance.RunCoroutineInternal(coroutine, segment, layer, null, new CoroutineHandle(Instance._instanceID), false);
					WaitForOtherHandles(coroutineHandle, _instance._layeredProcesses[layer], false);
					return coroutineHandle;
				}
				}
			}
			return Instance.RunCoroutineInternal(coroutine, segment, layer, null, new CoroutineHandle(Instance._instanceID), true);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, string tag, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutines(tag);
			}
			else if (Instance._taggedProcesses.ContainsKey(tag))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
				{
					HashSet<CoroutineHandle>.Enumerator enumerator = Instance._taggedProcesses[tag].GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (IsRunning(enumerator.Current))
						{
							return enumerator.Current;
						}
					}
					break;
				}
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = Instance.RunCoroutineInternal(coroutine, segment, null, tag, new CoroutineHandle(Instance._instanceID), false);
					WaitForOtherHandles(coroutineHandle, _instance._taggedProcesses[tag], false);
					return coroutineHandle;
				}
				}
			}
			return Instance.RunCoroutineInternal(coroutine, segment, null, tag, new CoroutineHandle(Instance._instanceID), true);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag, SingletonBehavior behaviorOnCollision)
		{
			return (!(gameObj == null)) ? RunCoroutineSingleton(coroutine, segment, gameObj.GetInstanceID(), tag, behaviorOnCollision) : RunCoroutineSingleton(coroutine, segment, tag, behaviorOnCollision);
		}

		public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, int layer, string tag, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutines(layer, tag);
				return Instance.RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(Instance._instanceID), true);
			}
			if (!Instance._taggedProcesses.ContainsKey(tag) || !Instance._layeredProcesses.ContainsKey(layer))
			{
				return Instance.RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(Instance._instanceID), true);
			}
			switch (behaviorOnCollision)
			{
			case SingletonBehavior.Abort:
			{
				HashSet<CoroutineHandle>.Enumerator enumerator2 = Instance._taggedProcesses[tag].GetEnumerator();
				while (enumerator2.MoveNext())
				{
					if (_instance._processLayers.ContainsKey(enumerator2.Current) && _instance._processLayers[enumerator2.Current] == layer)
					{
						return enumerator2.Current;
					}
				}
				break;
			}
			case SingletonBehavior.Wait:
			{
				List<CoroutineHandle> list = new List<CoroutineHandle>();
				HashSet<CoroutineHandle>.Enumerator enumerator = Instance._taggedProcesses[tag].GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (_instance._processLayers.ContainsKey(enumerator.Current) && _instance._processLayers[enumerator.Current] == layer)
					{
						list.Add(enumerator.Current);
					}
				}
				if (list.Count > 0)
				{
					CoroutineHandle coroutineHandle = _instance.RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(_instance._instanceID), false);
					WaitForOtherHandles(coroutineHandle, list, false);
					return coroutineHandle;
				}
				break;
			}
			}
			return Instance.RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(Instance._instanceID), true);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, CoroutineHandle handle, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutinesOnInstance(handle);
			}
			else if (_handleToIndex.ContainsKey(handle) && !CoindexIsNull(_handleToIndex[handle]))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
					return handle;
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = RunCoroutineInternal(coroutine, Segment.Update, null, null, new CoroutineHandle(_instanceID), false);
					WaitForOtherHandles(coroutineHandle, handle, false);
					return coroutineHandle;
				}
				}
			}
			return RunCoroutineInternal(coroutine, Segment.Update, null, null, new CoroutineHandle(_instanceID), true);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, GameObject gameObj, SingletonBehavior behaviorOnCollision)
		{
			return (!(gameObj == null)) ? RunCoroutineSingletonOnInstance(coroutine, gameObj.GetInstanceID(), behaviorOnCollision) : RunCoroutineOnInstance(coroutine);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, int layer, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutinesOnInstance(layer);
			}
			else if (_layeredProcesses.ContainsKey(layer))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
				{
					HashSet<CoroutineHandle>.Enumerator enumerator = _layeredProcesses[layer].GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (IsRunning(enumerator.Current))
						{
							return enumerator.Current;
						}
					}
					break;
				}
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = RunCoroutineInternal(coroutine, Segment.Update, layer, null, new CoroutineHandle(_instanceID), false);
					WaitForOtherHandles(coroutineHandle, _layeredProcesses[layer], false);
					return coroutineHandle;
				}
				}
			}
			return RunCoroutineInternal(coroutine, Segment.Update, layer, null, new CoroutineHandle(_instanceID), true);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, string tag, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutinesOnInstance(tag);
			}
			else if (_taggedProcesses.ContainsKey(tag))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
				{
					HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (IsRunning(enumerator.Current))
						{
							return enumerator.Current;
						}
					}
					break;
				}
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = RunCoroutineInternal(coroutine, Segment.Update, null, tag, new CoroutineHandle(_instanceID), false);
					WaitForOtherHandles(coroutineHandle, _taggedProcesses[tag], false);
					return coroutineHandle;
				}
				}
			}
			return RunCoroutineInternal(coroutine, Segment.Update, null, tag, new CoroutineHandle(_instanceID), true);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, GameObject gameObj, string tag, SingletonBehavior behaviorOnCollision)
		{
			return (!(gameObj == null)) ? RunCoroutineSingletonOnInstance(coroutine, gameObj.GetInstanceID(), tag, behaviorOnCollision) : RunCoroutineSingletonOnInstance(coroutine, tag, behaviorOnCollision);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, int layer, string tag, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutinesOnInstance(layer, tag);
				return RunCoroutineInternal(coroutine, Segment.Update, layer, tag, new CoroutineHandle(_instanceID), true);
			}
			if (!_taggedProcesses.ContainsKey(tag) || !_layeredProcesses.ContainsKey(layer))
			{
				return RunCoroutineInternal(coroutine, Segment.Update, layer, tag, new CoroutineHandle(_instanceID), true);
			}
			if (behaviorOnCollision == SingletonBehavior.Abort)
			{
				HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (_processLayers.ContainsKey(enumerator.Current) && _processLayers[enumerator.Current] == layer)
					{
						return enumerator.Current;
					}
				}
			}
			if (behaviorOnCollision == SingletonBehavior.Wait)
			{
				List<CoroutineHandle> list = new List<CoroutineHandle>();
				HashSet<CoroutineHandle>.Enumerator enumerator2 = _taggedProcesses[tag].GetEnumerator();
				while (enumerator2.MoveNext())
				{
					if (_processLayers.ContainsKey(enumerator2.Current) && _processLayers[enumerator2.Current] == layer)
					{
						list.Add(enumerator2.Current);
					}
				}
				if (list.Count > 0)
				{
					CoroutineHandle coroutineHandle = RunCoroutineInternal(coroutine, Segment.Update, layer, tag, new CoroutineHandle(_instanceID), false);
					WaitForOtherHandles(coroutineHandle, list, false);
					return coroutineHandle;
				}
			}
			return RunCoroutineInternal(coroutine, Segment.Update, layer, tag, new CoroutineHandle(_instanceID), true);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, SingletonBehavior behaviorOnCollision)
		{
			return (!(gameObj == null)) ? RunCoroutineSingletonOnInstance(coroutine, segment, gameObj.GetInstanceID(), behaviorOnCollision) : RunCoroutineOnInstance(coroutine, segment);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, int layer, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutinesOnInstance(layer);
			}
			else if (_layeredProcesses.ContainsKey(layer))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
				{
					HashSet<CoroutineHandle>.Enumerator enumerator = _layeredProcesses[layer].GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (IsRunning(enumerator.Current))
						{
							return enumerator.Current;
						}
					}
					break;
				}
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = RunCoroutineInternal(coroutine, segment, layer, null, new CoroutineHandle(_instanceID), false);
					WaitForOtherHandles(coroutineHandle, _layeredProcesses[layer], false);
					return coroutineHandle;
				}
				}
			}
			return RunCoroutineInternal(coroutine, segment, layer, null, new CoroutineHandle(_instanceID), true);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, string tag, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutinesOnInstance(tag);
			}
			else if (_taggedProcesses.ContainsKey(tag))
			{
				switch (behaviorOnCollision)
				{
				case SingletonBehavior.Abort:
				{
					HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (IsRunning(enumerator.Current))
						{
							return enumerator.Current;
						}
					}
					break;
				}
				case SingletonBehavior.Wait:
				{
					CoroutineHandle coroutineHandle = RunCoroutineInternal(coroutine, segment, null, tag, new CoroutineHandle(_instanceID), false);
					WaitForOtherHandles(coroutineHandle, _taggedProcesses[tag], false);
					return coroutineHandle;
				}
				}
			}
			return RunCoroutineInternal(coroutine, segment, null, tag, new CoroutineHandle(_instanceID), true);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag, SingletonBehavior behaviorOnCollision)
		{
			return (!(gameObj == null)) ? RunCoroutineSingletonOnInstance(coroutine, segment, gameObj.GetInstanceID(), tag, behaviorOnCollision) : RunCoroutineSingletonOnInstance(coroutine, segment, tag, behaviorOnCollision);
		}

		public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, int layer, string tag, SingletonBehavior behaviorOnCollision)
		{
			if (coroutine == null)
			{
				return default(CoroutineHandle);
			}
			if (behaviorOnCollision == SingletonBehavior.Overwrite)
			{
				KillCoroutinesOnInstance(layer, tag);
				return RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(_instanceID), true);
			}
			if (!_taggedProcesses.ContainsKey(tag) || !_layeredProcesses.ContainsKey(layer))
			{
				return RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(_instanceID), true);
			}
			switch (behaviorOnCollision)
			{
			case SingletonBehavior.Abort:
			{
				HashSet<CoroutineHandle>.Enumerator enumerator2 = _taggedProcesses[tag].GetEnumerator();
				while (enumerator2.MoveNext())
				{
					if (_processLayers.ContainsKey(enumerator2.Current) && _processLayers[enumerator2.Current] == layer)
					{
						return enumerator2.Current;
					}
				}
				break;
			}
			case SingletonBehavior.Wait:
			{
				List<CoroutineHandle> list = new List<CoroutineHandle>();
				HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (_processLayers.ContainsKey(enumerator.Current) && _processLayers[enumerator.Current] == layer)
					{
						list.Add(enumerator.Current);
					}
				}
				if (list.Count > 0)
				{
					CoroutineHandle coroutineHandle = RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(_instanceID), false);
					WaitForOtherHandles(coroutineHandle, list, false);
					return coroutineHandle;
				}
				break;
			}
			}
			return RunCoroutineInternal(coroutine, segment, layer, tag, new CoroutineHandle(_instanceID), true);
		}

		private CoroutineHandle RunCoroutineInternal(IEnumerator<float> coroutine, Segment segment, int? layer, string tag, CoroutineHandle handle, bool prewarm)
		{
			ProcessIndex processIndex = new ProcessIndex
			{
				seg = segment
			};
			if (_handleToIndex.ContainsKey(handle))
			{
				_indexToHandle.Remove(_handleToIndex[handle]);
				_handleToIndex.Remove(handle);
			}
			float num = localTime;
			float num2 = deltaTime;
			switch (segment)
			{
			case Segment.Update:
				if (_nextUpdateProcessSlot >= UpdateProcesses.Length)
				{
					IEnumerator<float>[] updateProcesses = UpdateProcesses;
					bool[] updatePaused = UpdatePaused;
					UpdateProcesses = new IEnumerator<float>[UpdateProcesses.Length + 64 * _expansions++];
					UpdatePaused = new bool[UpdateProcesses.Length];
					for (int j = 0; j < updateProcesses.Length; j++)
					{
						UpdateProcesses[j] = updateProcesses[j];
						UpdatePaused[j] = updatePaused[j];
					}
				}
				if (UpdateTimeValues(processIndex.seg))
				{
					_lastUpdateProcessSlot = _nextUpdateProcessSlot;
				}
				processIndex.i = _nextUpdateProcessSlot++;
				UpdateProcesses[processIndex.i] = coroutine;
				if (tag != null)
				{
					AddTagOnInstance(tag, handle);
				}
				if (layer.HasValue)
				{
					AddLayerOnInstance(layer.Value, handle);
				}
				_indexToHandle.Add(processIndex, handle);
				_handleToIndex.Add(handle, processIndex);
				while (prewarm)
				{
					if (!UpdateProcesses[processIndex.i].MoveNext())
					{
						if (_indexToHandle.ContainsKey(processIndex))
						{
							KillCoroutinesOnInstance(_indexToHandle[processIndex]);
						}
						prewarm = false;
					}
					else if (UpdateProcesses[processIndex.i] != null && float.IsNaN(UpdateProcesses[processIndex.i].Current))
					{
						if (ReplacementFunction != null)
						{
							UpdateProcesses[processIndex.i] = ReplacementFunction(UpdateProcesses[processIndex.i], _indexToHandle[processIndex]);
							ReplacementFunction = null;
						}
						prewarm = !UpdatePaused[processIndex.i];
					}
					else
					{
						prewarm = false;
					}
				}
				break;
			case Segment.FixedUpdate:
				if (_nextFixedUpdateProcessSlot >= FixedUpdateProcesses.Length)
				{
					IEnumerator<float>[] fixedUpdateProcesses = FixedUpdateProcesses;
					bool[] fixedUpdatePaused = FixedUpdatePaused;
					FixedUpdateProcesses = new IEnumerator<float>[FixedUpdateProcesses.Length + 64 * _expansions++];
					FixedUpdatePaused = new bool[FixedUpdateProcesses.Length];
					for (int num3 = 0; num3 < fixedUpdateProcesses.Length; num3++)
					{
						FixedUpdateProcesses[num3] = fixedUpdateProcesses[num3];
						FixedUpdatePaused[num3] = fixedUpdatePaused[num3];
					}
				}
				if (UpdateTimeValues(processIndex.seg))
				{
					_lastFixedUpdateProcessSlot = _nextFixedUpdateProcessSlot;
				}
				processIndex.i = _nextFixedUpdateProcessSlot++;
				FixedUpdateProcesses[processIndex.i] = coroutine;
				if (tag != null)
				{
					AddTagOnInstance(tag, handle);
				}
				if (layer.HasValue)
				{
					AddLayerOnInstance(layer.Value, handle);
				}
				_indexToHandle.Add(processIndex, handle);
				_handleToIndex.Add(handle, processIndex);
				while (prewarm)
				{
					if (!FixedUpdateProcesses[processIndex.i].MoveNext())
					{
						if (_indexToHandle.ContainsKey(processIndex))
						{
							KillCoroutinesOnInstance(_indexToHandle[processIndex]);
						}
						prewarm = false;
					}
					else if (FixedUpdateProcesses[processIndex.i] != null && float.IsNaN(FixedUpdateProcesses[processIndex.i].Current))
					{
						if (ReplacementFunction != null)
						{
							FixedUpdateProcesses[processIndex.i] = ReplacementFunction(FixedUpdateProcesses[processIndex.i], _indexToHandle[processIndex]);
							ReplacementFunction = null;
						}
						prewarm = !FixedUpdatePaused[processIndex.i];
					}
					else
					{
						prewarm = false;
					}
				}
				break;
			case Segment.LateUpdate:
				if (_nextLateUpdateProcessSlot >= LateUpdateProcesses.Length)
				{
					IEnumerator<float>[] lateUpdateProcesses = LateUpdateProcesses;
					bool[] lateUpdatePaused = LateUpdatePaused;
					LateUpdateProcesses = new IEnumerator<float>[LateUpdateProcesses.Length + 64 * _expansions++];
					LateUpdatePaused = new bool[LateUpdateProcesses.Length];
					for (int l = 0; l < lateUpdateProcesses.Length; l++)
					{
						LateUpdateProcesses[l] = lateUpdateProcesses[l];
						LateUpdatePaused[l] = lateUpdatePaused[l];
					}
				}
				if (UpdateTimeValues(processIndex.seg))
				{
					_lastLateUpdateProcessSlot = _nextLateUpdateProcessSlot;
				}
				processIndex.i = _nextLateUpdateProcessSlot++;
				LateUpdateProcesses[processIndex.i] = coroutine;
				if (tag != null)
				{
					AddTagOnInstance(tag, handle);
				}
				if (layer.HasValue)
				{
					AddLayerOnInstance(layer.Value, handle);
				}
				_indexToHandle.Add(processIndex, handle);
				_handleToIndex.Add(handle, processIndex);
				while (prewarm)
				{
					if (!LateUpdateProcesses[processIndex.i].MoveNext())
					{
						if (_indexToHandle.ContainsKey(processIndex))
						{
							KillCoroutinesOnInstance(_indexToHandle[processIndex]);
						}
						prewarm = false;
					}
					else if (LateUpdateProcesses[processIndex.i] != null && float.IsNaN(LateUpdateProcesses[processIndex.i].Current))
					{
						if (ReplacementFunction != null)
						{
							LateUpdateProcesses[processIndex.i] = ReplacementFunction(LateUpdateProcesses[processIndex.i], _indexToHandle[processIndex]);
							ReplacementFunction = null;
						}
						prewarm = !LateUpdatePaused[processIndex.i];
					}
					else
					{
						prewarm = false;
					}
				}
				break;
			case Segment.SlowUpdate:
				if (_nextSlowUpdateProcessSlot >= SlowUpdateProcesses.Length)
				{
					IEnumerator<float>[] slowUpdateProcesses = SlowUpdateProcesses;
					bool[] slowUpdatePaused = SlowUpdatePaused;
					SlowUpdateProcesses = new IEnumerator<float>[SlowUpdateProcesses.Length + 64 * _expansions++];
					SlowUpdatePaused = new bool[SlowUpdateProcesses.Length];
					for (int m = 0; m < slowUpdateProcesses.Length; m++)
					{
						SlowUpdateProcesses[m] = slowUpdateProcesses[m];
						SlowUpdatePaused[m] = slowUpdatePaused[m];
					}
				}
				if (UpdateTimeValues(processIndex.seg))
				{
					_lastSlowUpdateProcessSlot = _nextSlowUpdateProcessSlot;
				}
				processIndex.i = _nextSlowUpdateProcessSlot++;
				SlowUpdateProcesses[processIndex.i] = coroutine;
				if (tag != null)
				{
					AddTagOnInstance(tag, handle);
				}
				if (layer.HasValue)
				{
					AddLayerOnInstance(layer.Value, handle);
				}
				_indexToHandle.Add(processIndex, handle);
				_handleToIndex.Add(handle, processIndex);
				while (prewarm)
				{
					if (!SlowUpdateProcesses[processIndex.i].MoveNext())
					{
						if (_indexToHandle.ContainsKey(processIndex))
						{
							KillCoroutinesOnInstance(_indexToHandle[processIndex]);
						}
						prewarm = false;
					}
					else if (SlowUpdateProcesses[processIndex.i] != null && float.IsNaN(SlowUpdateProcesses[processIndex.i].Current))
					{
						if (ReplacementFunction != null)
						{
							SlowUpdateProcesses[processIndex.i] = ReplacementFunction(SlowUpdateProcesses[processIndex.i], _indexToHandle[processIndex]);
							ReplacementFunction = null;
						}
						prewarm = !SlowUpdatePaused[processIndex.i];
					}
					else
					{
						prewarm = false;
					}
				}
				break;
			case Segment.RealtimeUpdate:
				if (_nextRealtimeUpdateProcessSlot >= RealtimeUpdateProcesses.Length)
				{
					IEnumerator<float>[] realtimeUpdateProcesses = RealtimeUpdateProcesses;
					bool[] realtimeUpdatePaused = RealtimeUpdatePaused;
					RealtimeUpdateProcesses = new IEnumerator<float>[RealtimeUpdateProcesses.Length + 64 * _expansions++];
					RealtimeUpdatePaused = new bool[RealtimeUpdateProcesses.Length];
					for (int k = 0; k < realtimeUpdateProcesses.Length; k++)
					{
						RealtimeUpdateProcesses[k] = realtimeUpdateProcesses[k];
						RealtimeUpdatePaused[k] = realtimeUpdatePaused[k];
					}
				}
				if (UpdateTimeValues(processIndex.seg))
				{
					_lastRealtimeUpdateProcessSlot = _nextRealtimeUpdateProcessSlot;
				}
				processIndex.i = _nextRealtimeUpdateProcessSlot++;
				RealtimeUpdateProcesses[processIndex.i] = coroutine;
				if (tag != null)
				{
					AddTagOnInstance(tag, handle);
				}
				if (layer.HasValue)
				{
					AddLayerOnInstance(layer.Value, handle);
				}
				_indexToHandle.Add(processIndex, handle);
				_handleToIndex.Add(handle, processIndex);
				while (prewarm)
				{
					if (!RealtimeUpdateProcesses[processIndex.i].MoveNext())
					{
						if (_indexToHandle.ContainsKey(processIndex))
						{
							KillCoroutinesOnInstance(_indexToHandle[processIndex]);
						}
						prewarm = false;
					}
					else if (RealtimeUpdateProcesses[processIndex.i] != null && float.IsNaN(RealtimeUpdateProcesses[processIndex.i].Current))
					{
						if (ReplacementFunction != null)
						{
							RealtimeUpdateProcesses[processIndex.i] = ReplacementFunction(RealtimeUpdateProcesses[processIndex.i], _indexToHandle[processIndex]);
							ReplacementFunction = null;
						}
						prewarm = !RealtimeUpdatePaused[processIndex.i];
					}
					else
					{
						prewarm = false;
					}
				}
				break;
			case Segment.EndOfFrame:
				if (_nextEndOfFrameProcessSlot >= EndOfFrameProcesses.Length)
				{
					IEnumerator<float>[] endOfFrameProcesses = EndOfFrameProcesses;
					bool[] endOfFramePaused = EndOfFramePaused;
					EndOfFrameProcesses = new IEnumerator<float>[EndOfFrameProcesses.Length + 64 * _expansions++];
					EndOfFramePaused = new bool[EndOfFrameProcesses.Length];
					for (int n = 0; n < endOfFrameProcesses.Length; n++)
					{
						EndOfFrameProcesses[n] = endOfFrameProcesses[n];
						EndOfFramePaused[n] = endOfFramePaused[n];
					}
				}
				if (UpdateTimeValues(processIndex.seg))
				{
					_lastEndOfFrameProcessSlot = _nextEndOfFrameProcessSlot;
				}
				processIndex.i = _nextEndOfFrameProcessSlot++;
				EndOfFrameProcesses[processIndex.i] = coroutine;
				if (tag != null)
				{
					AddTagOnInstance(tag, handle);
				}
				if (layer.HasValue)
				{
					AddLayerOnInstance(layer.Value, handle);
				}
				_indexToHandle.Add(processIndex, handle);
				_handleToIndex.Add(handle, processIndex);
				_eofWatcherHandle = RunCoroutineSingletonOnInstance(_EOFPumpWatcher(), _eofWatcherHandle, SingletonBehavior.Abort);
				break;
			case Segment.ManualTimeframe:
				if (_nextManualTimeframeProcessSlot >= ManualTimeframeProcesses.Length)
				{
					IEnumerator<float>[] manualTimeframeProcesses = ManualTimeframeProcesses;
					bool[] manualTimeframePaused = ManualTimeframePaused;
					ManualTimeframeProcesses = new IEnumerator<float>[ManualTimeframeProcesses.Length + 64 * _expansions++];
					ManualTimeframePaused = new bool[ManualTimeframeProcesses.Length];
					for (int i = 0; i < manualTimeframeProcesses.Length; i++)
					{
						ManualTimeframeProcesses[i] = manualTimeframeProcesses[i];
						ManualTimeframePaused[i] = manualTimeframePaused[i];
					}
				}
				if (UpdateTimeValues(processIndex.seg))
				{
					_lastManualTimeframeProcessSlot = _nextManualTimeframeProcessSlot;
				}
				processIndex.i = _nextManualTimeframeProcessSlot++;
				ManualTimeframeProcesses[processIndex.i] = coroutine;
				if (tag != null)
				{
					AddTagOnInstance(tag, handle);
				}
				if (layer.HasValue)
				{
					AddLayerOnInstance(layer.Value, handle);
				}
				_indexToHandle.Add(processIndex, handle);
				_handleToIndex.Add(handle, processIndex);
				break;
			default:
				handle = default(CoroutineHandle);
				break;
			}
			localTime = num;
			deltaTime = num2;
			return handle;
		}

		public static int KillCoroutines()
		{
			return (!(_instance == null)) ? _instance.KillCoroutinesOnInstance() : 0;
		}

		public int KillCoroutinesOnInstance()
		{
			int result = _nextUpdateProcessSlot + _nextLateUpdateProcessSlot + _nextFixedUpdateProcessSlot + _nextSlowUpdateProcessSlot + _nextRealtimeUpdateProcessSlot + _nextEditorUpdateProcessSlot + _nextEditorSlowUpdateProcessSlot + _nextEndOfFrameProcessSlot + _nextManualTimeframeProcessSlot;
			UpdateProcesses = new IEnumerator<float>[256];
			UpdatePaused = new bool[256];
			UpdateCoroutines = 0;
			_nextUpdateProcessSlot = 0;
			LateUpdateProcesses = new IEnumerator<float>[8];
			LateUpdatePaused = new bool[8];
			LateUpdateCoroutines = 0;
			_nextLateUpdateProcessSlot = 0;
			FixedUpdateProcesses = new IEnumerator<float>[64];
			FixedUpdatePaused = new bool[64];
			FixedUpdateCoroutines = 0;
			_nextFixedUpdateProcessSlot = 0;
			SlowUpdateProcesses = new IEnumerator<float>[64];
			SlowUpdatePaused = new bool[64];
			SlowUpdateCoroutines = 0;
			_nextSlowUpdateProcessSlot = 0;
			RealtimeUpdateProcesses = new IEnumerator<float>[8];
			RealtimeUpdatePaused = new bool[8];
			RealtimeUpdateCoroutines = 0;
			_nextRealtimeUpdateProcessSlot = 0;
			EditorUpdateProcesses = new IEnumerator<float>[8];
			EditorUpdatePaused = new bool[8];
			EditorUpdateCoroutines = 0;
			_nextEditorUpdateProcessSlot = 0;
			EditorSlowUpdateProcesses = new IEnumerator<float>[8];
			EditorSlowUpdatePaused = new bool[8];
			EditorSlowUpdateCoroutines = 0;
			_nextEditorSlowUpdateProcessSlot = 0;
			EndOfFrameProcesses = new IEnumerator<float>[8];
			EndOfFramePaused = new bool[8];
			EndOfFrameCoroutines = 0;
			_nextEndOfFrameProcessSlot = 0;
			ManualTimeframeProcesses = new IEnumerator<float>[8];
			ManualTimeframePaused = new bool[8];
			ManualTimeframeCoroutines = 0;
			_nextManualTimeframeProcessSlot = 0;
			_processTags.Clear();
			_taggedProcesses.Clear();
			_processLayers.Clear();
			_layeredProcesses.Clear();
			_handleToIndex.Clear();
			_indexToHandle.Clear();
			_waitingTriggers.Clear();
			_expansions = (ushort)(_expansions / 2 + 1);
			Links.Clear();
			ResetTimeCountOnInstance();
			return result;
		}

		public static int KillCoroutines(CoroutineHandle handle)
		{
			return ActiveInstances.ContainsKey(handle.Key) ? GetInstance(handle.Key).KillCoroutinesOnInstance(handle) : 0;
		}

		public int KillCoroutinesOnInstance(CoroutineHandle handle)
		{
			int num = 0;
			if (_handleToIndex.ContainsKey(handle))
			{
				if (_waitingTriggers.ContainsKey(handle))
				{
					CloseWaitingProcess(handle);
				}
				if (Nullify(handle))
				{
					num++;
				}
				RemoveGraffiti(handle);
			}
			if (Links.ContainsKey(handle))
			{
				HashSet<CoroutineHandle>.Enumerator enumerator = Links[handle].GetEnumerator();
				Links.Remove(handle);
				while (enumerator.MoveNext())
				{
					num += KillCoroutines(enumerator.Current);
				}
			}
			return num;
		}

		public static int KillCoroutines(GameObject gameObj)
		{
			return (!(_instance == null)) ? _instance.KillCoroutinesOnInstance(gameObj.GetInstanceID()) : 0;
		}

		public int KillCoroutinesOnInstance(GameObject gameObj)
		{
			return KillCoroutinesOnInstance(gameObj.GetInstanceID());
		}

		public static int KillCoroutines(int layer)
		{
			return (!(_instance == null)) ? _instance.KillCoroutinesOnInstance(layer) : 0;
		}

		public int KillCoroutinesOnInstance(int layer)
		{
			int num = 0;
			while (_layeredProcesses.ContainsKey(layer))
			{
				HashSet<CoroutineHandle>.Enumerator enumerator = _layeredProcesses[layer].GetEnumerator();
				enumerator.MoveNext();
				if (Nullify(enumerator.Current))
				{
					if (_waitingTriggers.ContainsKey(enumerator.Current))
					{
						CloseWaitingProcess(enumerator.Current);
					}
					num++;
				}
				RemoveGraffiti(enumerator.Current);
				if (Links.ContainsKey(enumerator.Current))
				{
					HashSet<CoroutineHandle>.Enumerator enumerator2 = Links[enumerator.Current].GetEnumerator();
					Links.Remove(enumerator.Current);
					while (enumerator2.MoveNext())
					{
						num += KillCoroutines(enumerator2.Current);
					}
				}
			}
			return num;
		}

		public static int KillCoroutines(string tag)
		{
			return (!(_instance == null)) ? _instance.KillCoroutinesOnInstance(tag) : 0;
		}

		public int KillCoroutinesOnInstance(string tag)
		{
			if (tag == null)
			{
				return 0;
			}
			int num = 0;
			while (_taggedProcesses.ContainsKey(tag))
			{
				HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
				enumerator.MoveNext();
				if (Nullify(_handleToIndex[enumerator.Current]))
				{
					if (_waitingTriggers.ContainsKey(enumerator.Current))
					{
						CloseWaitingProcess(enumerator.Current);
					}
					num++;
				}
				RemoveGraffiti(enumerator.Current);
				if (Links.ContainsKey(enumerator.Current))
				{
					HashSet<CoroutineHandle>.Enumerator enumerator2 = Links[enumerator.Current].GetEnumerator();
					Links.Remove(enumerator.Current);
					while (enumerator2.MoveNext())
					{
						num += KillCoroutines(enumerator2.Current);
					}
				}
			}
			return num;
		}

		public static int KillCoroutines(GameObject gameObj, string tag)
		{
			return (!(_instance == null)) ? _instance.KillCoroutinesOnInstance(gameObj.GetInstanceID(), tag) : 0;
		}

		public int KillCoroutinesOnInstance(GameObject gameObj, string tag)
		{
			return KillCoroutinesOnInstance(gameObj.GetInstanceID(), tag);
		}

		public static int KillCoroutines(int layer, string tag)
		{
			return (!(_instance == null)) ? _instance.KillCoroutinesOnInstance(layer, tag) : 0;
		}

		public int KillCoroutinesOnInstance(int layer, string tag)
		{
			if (tag == null)
			{
				return KillCoroutinesOnInstance(layer);
			}
			if (!_layeredProcesses.ContainsKey(layer) || !_taggedProcesses.ContainsKey(tag))
			{
				return 0;
			}
			int num = 0;
			HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (CoindexIsNull(_handleToIndex[enumerator.Current]) || !_layeredProcesses[layer].Contains(enumerator.Current) || !Nullify(enumerator.Current))
				{
					continue;
				}
				if (_waitingTriggers.ContainsKey(enumerator.Current))
				{
					CloseWaitingProcess(enumerator.Current);
				}
				num++;
				RemoveGraffiti(enumerator.Current);
				if (Links.ContainsKey(enumerator.Current))
				{
					HashSet<CoroutineHandle>.Enumerator enumerator2 = Links[enumerator.Current].GetEnumerator();
					Links.Remove(enumerator.Current);
					while (enumerator2.MoveNext())
					{
						KillCoroutines(enumerator2.Current);
					}
				}
				if (!_taggedProcesses.ContainsKey(tag) || !_layeredProcesses.ContainsKey(layer))
				{
					break;
				}
				enumerator = _taggedProcesses[tag].GetEnumerator();
			}
			return num;
		}

		public static Timing GetInstance(byte ID)
		{
			return (!ActiveInstances.ContainsKey(ID)) ? null : ActiveInstances[ID];
		}

		public static float WaitForSeconds(float waitTime)
		{
			if (float.IsNaN(waitTime))
			{
				waitTime = 0f;
			}
			return LocalTime + waitTime;
		}

		public float WaitForSecondsOnInstance(float waitTime)
		{
			if (float.IsNaN(waitTime))
			{
				waitTime = 0f;
			}
			return localTime + waitTime;
		}

		private bool UpdateTimeValues(Segment segment)
		{
			switch (segment)
			{
			case Segment.Update:
				if (_currentUpdateFrame != Time.frameCount)
				{
					deltaTime = Time.deltaTime;
					_lastUpdateTime += deltaTime;
					localTime = _lastUpdateTime;
					_currentUpdateFrame = Time.frameCount;
					return true;
				}
				deltaTime = Time.deltaTime;
				localTime = _lastUpdateTime;
				return false;
			case Segment.LateUpdate:
				if (_currentLateUpdateFrame != Time.frameCount)
				{
					deltaTime = Time.deltaTime;
					_lastLateUpdateTime += deltaTime;
					localTime = _lastLateUpdateTime;
					_currentLateUpdateFrame = Time.frameCount;
					return true;
				}
				deltaTime = Time.deltaTime;
				localTime = _lastLateUpdateTime;
				return false;
			case Segment.FixedUpdate:
				if (_currentFixedUpdateFrame != Time.frameCount)
				{
					deltaTime = Time.deltaTime;
					_lastFixedUpdateTime += deltaTime;
					localTime = _lastFixedUpdateTime;
					_currentFixedUpdateFrame = Time.frameCount;
					return true;
				}
				deltaTime = Time.deltaTime;
				localTime = _lastFixedUpdateTime;
				return false;
			case Segment.SlowUpdate:
				if (_currentSlowUpdateFrame != Time.frameCount)
				{
					deltaTime = (_lastSlowUpdateDeltaTime = Time.realtimeSinceStartup - _lastSlowUpdateTime);
					localTime = (_lastSlowUpdateTime = Time.realtimeSinceStartup);
					_currentSlowUpdateFrame = Time.frameCount;
					return true;
				}
				localTime = _lastSlowUpdateTime;
				deltaTime = _lastSlowUpdateDeltaTime;
				return false;
			case Segment.RealtimeUpdate:
				if (_currentRealtimeUpdateFrame != Time.frameCount)
				{
					deltaTime = Time.unscaledDeltaTime;
					_lastRealtimeUpdateTime += deltaTime;
					localTime = _lastRealtimeUpdateTime;
					_currentRealtimeUpdateFrame = Time.frameCount;
					return true;
				}
				deltaTime = Time.unscaledDeltaTime;
				localTime = _lastRealtimeUpdateTime;
				return false;
			case Segment.EndOfFrame:
				if (_currentEndOfFrameFrame != Time.frameCount)
				{
					deltaTime = Time.deltaTime;
					_lastEndOfFrameTime += deltaTime;
					localTime = _lastEndOfFrameTime;
					_currentEndOfFrameFrame = Time.frameCount;
					return true;
				}
				deltaTime = Time.deltaTime;
				localTime = _lastEndOfFrameTime;
				return false;
			case Segment.ManualTimeframe:
			{
				float num = ((SetManualTimeframeTime != null) ? SetManualTimeframeTime(_lastManualTimeframeTime) : Time.time);
				if ((double)_lastManualTimeframeTime + 0.0001 < (double)num && (double)_lastManualTimeframeTime - 0.0001 > (double)num)
				{
					localTime = num;
					deltaTime = localTime - _lastManualTimeframeTime;
					if (deltaTime > Time.maximumDeltaTime)
					{
						deltaTime = Time.maximumDeltaTime;
					}
					_lastManualTimeframeDeltaTime = deltaTime;
					_lastManualTimeframeTime = num;
					return true;
				}
				deltaTime = _lastManualTimeframeDeltaTime;
				localTime = _lastManualTimeframeTime;
				return false;
			}
			default:
				return true;
			}
		}

		private float GetSegmentTime(Segment segment)
		{
			switch (segment)
			{
			case Segment.Update:
				if (_currentUpdateFrame == Time.frameCount)
				{
					return _lastUpdateTime;
				}
				return _lastUpdateTime + Time.deltaTime;
			case Segment.LateUpdate:
				if (_currentUpdateFrame == Time.frameCount)
				{
					return _lastLateUpdateTime;
				}
				return _lastLateUpdateTime + Time.deltaTime;
			case Segment.FixedUpdate:
				if (_currentFixedUpdateFrame == Time.frameCount)
				{
					return _lastFixedUpdateTime;
				}
				return _lastFixedUpdateTime + Time.deltaTime;
			case Segment.SlowUpdate:
				return Time.realtimeSinceStartup;
			case Segment.RealtimeUpdate:
				if (_currentRealtimeUpdateFrame == Time.frameCount)
				{
					return _lastRealtimeUpdateTime;
				}
				return _lastRealtimeUpdateTime + Time.unscaledDeltaTime;
			case Segment.EndOfFrame:
				if (_currentUpdateFrame == Time.frameCount)
				{
					return _lastEndOfFrameTime;
				}
				return _lastEndOfFrameTime + Time.deltaTime;
			case Segment.ManualTimeframe:
				return _lastManualTimeframeTime;
			default:
				return 0f;
			}
		}

		public void ResetTimeCountOnInstance()
		{
			localTime = 0f;
			_lastUpdateTime = 0f;
			_lastFixedUpdateTime = 0f;
			_lastRealtimeUpdateTime = 0f;
			_EOFPumpRan = false;
		}

		public static int PauseCoroutines()
		{
			return (!(_instance == null)) ? _instance.PauseCoroutinesOnInstance() : 0;
		}

		public int PauseCoroutinesOnInstance()
		{
			int num = 0;
			for (int i = 0; i < _nextUpdateProcessSlot; i++)
			{
				if (!UpdatePaused[i] && UpdateProcesses[i] != null)
				{
					num++;
					UpdatePaused[i] = true;
					if (UpdateProcesses[i].Current > GetSegmentTime(Segment.Update))
					{
						UpdateProcesses[i] = _InjectDelay(UpdateProcesses[i], UpdateProcesses[i].Current - GetSegmentTime(Segment.Update));
					}
				}
			}
			for (int i = 0; i < _nextLateUpdateProcessSlot; i++)
			{
				if (!LateUpdatePaused[i] && LateUpdateProcesses[i] != null)
				{
					num++;
					LateUpdatePaused[i] = true;
					if (LateUpdateProcesses[i].Current > GetSegmentTime(Segment.LateUpdate))
					{
						LateUpdateProcesses[i] = _InjectDelay(LateUpdateProcesses[i], LateUpdateProcesses[i].Current - GetSegmentTime(Segment.LateUpdate));
					}
				}
			}
			for (int i = 0; i < _nextFixedUpdateProcessSlot; i++)
			{
				if (!FixedUpdatePaused[i] && FixedUpdateProcesses[i] != null)
				{
					num++;
					FixedUpdatePaused[i] = true;
					if (FixedUpdateProcesses[i].Current > GetSegmentTime(Segment.FixedUpdate))
					{
						FixedUpdateProcesses[i] = _InjectDelay(FixedUpdateProcesses[i], FixedUpdateProcesses[i].Current - GetSegmentTime(Segment.FixedUpdate));
					}
				}
			}
			for (int i = 0; i < _nextSlowUpdateProcessSlot; i++)
			{
				if (!SlowUpdatePaused[i] && SlowUpdateProcesses[i] != null)
				{
					num++;
					SlowUpdatePaused[i] = true;
					if (SlowUpdateProcesses[i].Current > GetSegmentTime(Segment.SlowUpdate))
					{
						SlowUpdateProcesses[i] = _InjectDelay(SlowUpdateProcesses[i], SlowUpdateProcesses[i].Current - GetSegmentTime(Segment.SlowUpdate));
					}
				}
			}
			for (int i = 0; i < _nextRealtimeUpdateProcessSlot; i++)
			{
				if (!RealtimeUpdatePaused[i] && RealtimeUpdateProcesses[i] != null)
				{
					num++;
					RealtimeUpdatePaused[i] = true;
					if (RealtimeUpdateProcesses[i].Current > GetSegmentTime(Segment.RealtimeUpdate))
					{
						RealtimeUpdateProcesses[i] = _InjectDelay(RealtimeUpdateProcesses[i], RealtimeUpdateProcesses[i].Current - GetSegmentTime(Segment.RealtimeUpdate));
					}
				}
			}
			for (int i = 0; i < _nextEditorUpdateProcessSlot; i++)
			{
				if (!EditorUpdatePaused[i] && EditorUpdateProcesses[i] != null)
				{
					num++;
					EditorUpdatePaused[i] = true;
					if (EditorUpdateProcesses[i].Current > GetSegmentTime(Segment.EditorUpdate))
					{
						EditorUpdateProcesses[i] = _InjectDelay(EditorUpdateProcesses[i], EditorUpdateProcesses[i].Current - GetSegmentTime(Segment.EditorUpdate));
					}
				}
			}
			for (int i = 0; i < _nextEditorSlowUpdateProcessSlot; i++)
			{
				if (!EditorSlowUpdatePaused[i] && EditorSlowUpdateProcesses[i] != null)
				{
					num++;
					EditorSlowUpdatePaused[i] = true;
					if (EditorSlowUpdateProcesses[i].Current > GetSegmentTime(Segment.EditorSlowUpdate))
					{
						EditorSlowUpdateProcesses[i] = _InjectDelay(EditorSlowUpdateProcesses[i], EditorSlowUpdateProcesses[i].Current - GetSegmentTime(Segment.EditorSlowUpdate));
					}
				}
			}
			for (int i = 0; i < _nextEndOfFrameProcessSlot; i++)
			{
				if (!EndOfFramePaused[i] && EndOfFrameProcesses[i] != null)
				{
					num++;
					EndOfFramePaused[i] = true;
					if (EndOfFrameProcesses[i].Current > GetSegmentTime(Segment.EndOfFrame))
					{
						EndOfFrameProcesses[i] = _InjectDelay(EndOfFrameProcesses[i], EndOfFrameProcesses[i].Current - GetSegmentTime(Segment.EndOfFrame));
					}
				}
			}
			for (int i = 0; i < _nextManualTimeframeProcessSlot; i++)
			{
				if (!ManualTimeframePaused[i] && ManualTimeframeProcesses[i] != null)
				{
					num++;
					ManualTimeframePaused[i] = true;
					if (ManualTimeframeProcesses[i].Current > GetSegmentTime(Segment.ManualTimeframe))
					{
						ManualTimeframeProcesses[i] = _InjectDelay(ManualTimeframeProcesses[i], ManualTimeframeProcesses[i].Current - GetSegmentTime(Segment.ManualTimeframe));
					}
				}
			}
			Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>.Enumerator enumerator = Links.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (_handleToIndex.ContainsKey(enumerator.Current.Key))
				{
					HashSet<CoroutineHandle>.Enumerator enumerator2 = enumerator.Current.Value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						num += PauseCoroutines(enumerator2.Current);
					}
				}
			}
			return num;
		}

		public static int PauseCoroutines(CoroutineHandle handle)
		{
			return ActiveInstances.ContainsKey(handle.Key) ? GetInstance(handle.Key).PauseCoroutinesOnInstance(handle) : 0;
		}

		public int PauseCoroutinesOnInstance(CoroutineHandle handle)
		{
			int num = 0;
			if (_handleToIndex.ContainsKey(handle) && !CoindexIsNull(_handleToIndex[handle]) && !SetPause(_handleToIndex[handle]))
			{
				num++;
			}
			if (Links.ContainsKey(handle))
			{
				HashSet<CoroutineHandle> hashSet = Links[handle];
				Links.Remove(handle);
				HashSet<CoroutineHandle>.Enumerator enumerator = hashSet.GetEnumerator();
				while (enumerator.MoveNext())
				{
					num += PauseCoroutines(enumerator.Current);
				}
				Links.Add(handle, hashSet);
			}
			return num;
		}

		public static int PauseCoroutines(GameObject gameObj)
		{
			return (!(_instance == null)) ? _instance.PauseCoroutinesOnInstance(gameObj) : 0;
		}

		public int PauseCoroutinesOnInstance(GameObject gameObj)
		{
			return (!(gameObj == null)) ? PauseCoroutinesOnInstance(gameObj.GetInstanceID()) : 0;
		}

		public static int PauseCoroutines(int layer)
		{
			return (!(_instance == null)) ? _instance.PauseCoroutinesOnInstance(layer) : 0;
		}

		public int PauseCoroutinesOnInstance(int layer)
		{
			if (!_layeredProcesses.ContainsKey(layer))
			{
				return 0;
			}
			int num = 0;
			HashSet<CoroutineHandle>.Enumerator enumerator = _layeredProcesses[layer].GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (!CoindexIsNull(_handleToIndex[enumerator.Current]) && !SetPause(_handleToIndex[enumerator.Current]))
				{
					num++;
				}
				if (Links.ContainsKey(enumerator.Current))
				{
					HashSet<CoroutineHandle> hashSet = Links[enumerator.Current];
					Links.Remove(enumerator.Current);
					HashSet<CoroutineHandle>.Enumerator enumerator2 = hashSet.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						num += PauseCoroutines(enumerator2.Current);
					}
					Links.Add(enumerator.Current, hashSet);
				}
			}
			return num;
		}

		public static int PauseCoroutines(string tag)
		{
			return (!(_instance == null)) ? _instance.PauseCoroutinesOnInstance(tag) : 0;
		}

		public int PauseCoroutinesOnInstance(string tag)
		{
			if (tag == null || !_taggedProcesses.ContainsKey(tag))
			{
				return 0;
			}
			int num = 0;
			HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (!CoindexIsNull(_handleToIndex[enumerator.Current]) && !SetPause(_handleToIndex[enumerator.Current]))
				{
					num++;
				}
				if (Links.ContainsKey(enumerator.Current))
				{
					HashSet<CoroutineHandle> hashSet = Links[enumerator.Current];
					Links.Remove(enumerator.Current);
					HashSet<CoroutineHandle>.Enumerator enumerator2 = hashSet.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						num += PauseCoroutines(enumerator2.Current);
					}
					Links.Add(enumerator.Current, hashSet);
				}
			}
			return num;
		}

		public static int PauseCoroutines(GameObject gameObj, string tag)
		{
			return (!(_instance == null)) ? _instance.PauseCoroutinesOnInstance(gameObj.GetInstanceID(), tag) : 0;
		}

		public int PauseCoroutinesOnInstance(GameObject gameObj, string tag)
		{
			return (!(gameObj == null)) ? PauseCoroutinesOnInstance(gameObj.GetInstanceID(), tag) : 0;
		}

		public static int PauseCoroutines(int layer, string tag)
		{
			return (!(_instance == null)) ? _instance.PauseCoroutinesOnInstance(layer, tag) : 0;
		}

		public int PauseCoroutinesOnInstance(int layer, string tag)
		{
			if (tag == null)
			{
				return PauseCoroutinesOnInstance(layer);
			}
			if (!_taggedProcesses.ContainsKey(tag) || !_layeredProcesses.ContainsKey(layer))
			{
				return 0;
			}
			int num = 0;
			HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (!_processLayers.ContainsKey(enumerator.Current) || _processLayers[enumerator.Current] != layer || CoindexIsNull(_handleToIndex[enumerator.Current]))
				{
					continue;
				}
				if (!SetPause(_handleToIndex[enumerator.Current]))
				{
					num++;
				}
				if (Links.ContainsKey(enumerator.Current))
				{
					HashSet<CoroutineHandle> hashSet = Links[enumerator.Current];
					Links.Remove(enumerator.Current);
					HashSet<CoroutineHandle>.Enumerator enumerator2 = hashSet.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						num += PauseCoroutines(enumerator2.Current);
					}
					Links.Add(enumerator.Current, hashSet);
				}
			}
			return num;
		}

		public static int ResumeCoroutines()
		{
			return (!(_instance == null)) ? _instance.ResumeCoroutinesOnInstance() : 0;
		}

		public int ResumeCoroutinesOnInstance()
		{
			int num = 0;
			ProcessIndex key = default(ProcessIndex);
			key.i = 0;
			key.seg = Segment.Update;
			while (key.i < _nextUpdateProcessSlot)
			{
				if (UpdatePaused[key.i] && UpdateProcesses[key.i] != null && !_allWaiting.Contains(_indexToHandle[key]))
				{
					UpdatePaused[key.i] = false;
					num++;
				}
				key.i++;
			}
			key.i = 0;
			key.seg = Segment.LateUpdate;
			while (key.i < _nextLateUpdateProcessSlot)
			{
				if (LateUpdatePaused[key.i] && LateUpdateProcesses[key.i] != null && !_allWaiting.Contains(_indexToHandle[key]))
				{
					LateUpdatePaused[key.i] = false;
					num++;
				}
				key.i++;
			}
			key.i = 0;
			key.seg = Segment.FixedUpdate;
			while (key.i < _nextFixedUpdateProcessSlot)
			{
				if (FixedUpdatePaused[key.i] && FixedUpdateProcesses[key.i] != null && !_allWaiting.Contains(_indexToHandle[key]))
				{
					FixedUpdatePaused[key.i] = false;
					num++;
				}
				key.i++;
			}
			key.i = 0;
			key.seg = Segment.SlowUpdate;
			while (key.i < _nextSlowUpdateProcessSlot)
			{
				if (SlowUpdatePaused[key.i] && SlowUpdateProcesses[key.i] != null && !_allWaiting.Contains(_indexToHandle[key]))
				{
					SlowUpdatePaused[key.i] = false;
					num++;
				}
				key.i++;
			}
			key.i = 0;
			key.seg = Segment.RealtimeUpdate;
			while (key.i < _nextRealtimeUpdateProcessSlot)
			{
				if (RealtimeUpdatePaused[key.i] && RealtimeUpdateProcesses[key.i] != null && !_allWaiting.Contains(_indexToHandle[key]))
				{
					RealtimeUpdatePaused[key.i] = false;
					num++;
				}
				key.i++;
			}
			key.i = 0;
			key.seg = Segment.EditorUpdate;
			while (key.i < _nextEditorUpdateProcessSlot)
			{
				if (EditorUpdatePaused[key.i] && EditorUpdateProcesses[key.i] != null && !_allWaiting.Contains(_indexToHandle[key]))
				{
					EditorUpdatePaused[key.i] = false;
					num++;
				}
				key.i++;
			}
			key.i = 0;
			key.seg = Segment.EditorSlowUpdate;
			while (key.i < _nextEditorSlowUpdateProcessSlot)
			{
				if (EditorSlowUpdatePaused[key.i] && EditorSlowUpdateProcesses[key.i] != null && !_allWaiting.Contains(_indexToHandle[key]))
				{
					EditorSlowUpdatePaused[key.i] = false;
					num++;
				}
				key.i++;
			}
			key.i = 0;
			key.seg = Segment.EndOfFrame;
			while (key.i < _nextEndOfFrameProcessSlot)
			{
				if (EndOfFramePaused[key.i] && EndOfFrameProcesses[key.i] != null && !_allWaiting.Contains(_indexToHandle[key]))
				{
					EndOfFramePaused[key.i] = false;
					num++;
				}
				key.i++;
			}
			key.i = 0;
			key.seg = Segment.ManualTimeframe;
			while (key.i < _nextManualTimeframeProcessSlot)
			{
				if (ManualTimeframePaused[key.i] && ManualTimeframeProcesses[key.i] != null && !_allWaiting.Contains(_indexToHandle[key]))
				{
					ManualTimeframePaused[key.i] = false;
					num++;
				}
				key.i++;
			}
			Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>.Enumerator enumerator = Links.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (_handleToIndex.ContainsKey(enumerator.Current.Key))
				{
					HashSet<CoroutineHandle>.Enumerator enumerator2 = enumerator.Current.Value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						num += ResumeCoroutines(enumerator2.Current);
					}
				}
			}
			return num;
		}

		public static int ResumeCoroutines(CoroutineHandle handle)
		{
			return ActiveInstances.ContainsKey(handle.Key) ? GetInstance(handle.Key).ResumeCoroutinesOnInstance(handle) : 0;
		}

		public int ResumeCoroutinesOnInstance(CoroutineHandle handle)
		{
			Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>.Enumerator enumerator = _waitingTriggers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				HashSet<CoroutineHandle>.Enumerator enumerator2 = enumerator.Current.Value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current == handle)
					{
						return 0;
					}
				}
			}
			int num = 0;
			if (_handleToIndex.ContainsKey(handle) && !CoindexIsNull(_handleToIndex[handle]) && SetPause(_handleToIndex[handle], false))
			{
				num++;
			}
			if (Links.ContainsKey(handle))
			{
				HashSet<CoroutineHandle> hashSet = Links[handle];
				Links.Remove(handle);
				HashSet<CoroutineHandle>.Enumerator enumerator3 = hashSet.GetEnumerator();
				while (enumerator3.MoveNext())
				{
					num += ResumeCoroutines(enumerator3.Current);
				}
				Links.Add(handle, hashSet);
			}
			return num;
		}

		public static int ResumeCoroutines(GameObject gameObj)
		{
			return (!(_instance == null)) ? _instance.ResumeCoroutinesOnInstance(gameObj.GetInstanceID()) : 0;
		}

		public int ResumeCoroutinesOnInstance(GameObject gameObj)
		{
			return (!(gameObj == null)) ? ResumeCoroutinesOnInstance(gameObj.GetInstanceID()) : 0;
		}

		public static int ResumeCoroutines(int layer)
		{
			return (!(_instance == null)) ? _instance.ResumeCoroutinesOnInstance(layer) : 0;
		}

		public int ResumeCoroutinesOnInstance(int layer)
		{
			if (!_layeredProcesses.ContainsKey(layer))
			{
				return 0;
			}
			int num = 0;
			HashSet<CoroutineHandle>.Enumerator enumerator = _layeredProcesses[layer].GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (!CoindexIsNull(_handleToIndex[enumerator.Current]) && !_allWaiting.Contains(enumerator.Current) && SetPause(_handleToIndex[enumerator.Current], false))
				{
					num++;
				}
				if (Links.ContainsKey(enumerator.Current))
				{
					HashSet<CoroutineHandle> hashSet = Links[enumerator.Current];
					Links.Remove(enumerator.Current);
					HashSet<CoroutineHandle>.Enumerator enumerator2 = hashSet.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						num += ResumeCoroutines(enumerator2.Current);
					}
					Links.Add(enumerator.Current, hashSet);
				}
			}
			return num;
		}

		public static int ResumeCoroutines(string tag)
		{
			return (!(_instance == null)) ? _instance.ResumeCoroutinesOnInstance(tag) : 0;
		}

		public int ResumeCoroutinesOnInstance(string tag)
		{
			if (tag == null || !_taggedProcesses.ContainsKey(tag))
			{
				return 0;
			}
			int num = 0;
			HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (!CoindexIsNull(_handleToIndex[enumerator.Current]) && !_allWaiting.Contains(enumerator.Current) && SetPause(_handleToIndex[enumerator.Current], false))
				{
					num++;
				}
				if (Links.ContainsKey(enumerator.Current))
				{
					HashSet<CoroutineHandle> hashSet = Links[enumerator.Current];
					Links.Remove(enumerator.Current);
					HashSet<CoroutineHandle>.Enumerator enumerator2 = hashSet.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						num += ResumeCoroutines(enumerator2.Current);
					}
					Links.Add(enumerator.Current, hashSet);
				}
			}
			return num;
		}

		public static int ResumeCoroutines(GameObject gameObj, string tag)
		{
			return (!(_instance == null)) ? _instance.ResumeCoroutinesOnInstance(gameObj.GetInstanceID(), tag) : 0;
		}

		public int ResumeCoroutinesOnInstance(GameObject gameObj, string tag)
		{
			return (!(gameObj == null)) ? ResumeCoroutinesOnInstance(gameObj.GetInstanceID(), tag) : 0;
		}

		public static int ResumeCoroutines(int layer, string tag)
		{
			return (!(_instance == null)) ? _instance.ResumeCoroutinesOnInstance(layer, tag) : 0;
		}

		public int ResumeCoroutinesOnInstance(int layer, string tag)
		{
			if (tag == null)
			{
				return ResumeCoroutinesOnInstance(layer);
			}
			if (!_layeredProcesses.ContainsKey(layer) || !_taggedProcesses.ContainsKey(tag))
			{
				return 0;
			}
			int num = 0;
			HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (CoindexIsNull(_handleToIndex[enumerator.Current]) || !_layeredProcesses[layer].Contains(enumerator.Current) || _allWaiting.Contains(enumerator.Current))
				{
					continue;
				}
				if (SetPause(_handleToIndex[enumerator.Current], false))
				{
					num++;
				}
				if (Links.ContainsKey(enumerator.Current))
				{
					HashSet<CoroutineHandle> hashSet = Links[enumerator.Current];
					Links.Remove(enumerator.Current);
					HashSet<CoroutineHandle>.Enumerator enumerator2 = hashSet.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						num += ResumeCoroutines(enumerator2.Current);
					}
					Links.Add(enumerator.Current, hashSet);
				}
			}
			return num;
		}

		public static string GetTag(CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			return (!(instance != null) || !instance._handleToIndex.ContainsKey(handle) || !instance._processTags.ContainsKey(handle)) ? null : instance._processTags[handle];
		}

		public static int? GetLayer(CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			return (!(instance != null) || !instance._handleToIndex.ContainsKey(handle) || !instance._processLayers.ContainsKey(handle)) ? ((int?)null) : new int?(instance._processLayers[handle]);
		}

		public static Segment GetSegment(CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			return (!(instance != null) || !instance._handleToIndex.ContainsKey(handle)) ? Segment.Invalid : instance._handleToIndex[handle].seg;
		}

		public static bool SetTag(CoroutineHandle handle, string newTag, bool overwriteExisting = true)
		{
			Timing instance = GetInstance(handle.Key);
			if (instance == null || !instance._handleToIndex.ContainsKey(handle) || instance.CoindexIsNull(instance._handleToIndex[handle]) || (!overwriteExisting && instance._processTags.ContainsKey(handle)))
			{
				return false;
			}
			instance.RemoveTagOnInstance(handle);
			instance.AddTagOnInstance(newTag, handle);
			return true;
		}

		public static bool SetLayer(CoroutineHandle handle, int newLayer, bool overwriteExisting = true)
		{
			Timing instance = GetInstance(handle.Key);
			if (instance == null || !instance._handleToIndex.ContainsKey(handle) || instance.CoindexIsNull(instance._handleToIndex[handle]) || (!overwriteExisting && instance._processLayers.ContainsKey(handle)))
			{
				return false;
			}
			instance.RemoveLayerOnInstance(handle);
			instance.AddLayerOnInstance(newLayer, handle);
			return true;
		}

		public static bool SetSegment(CoroutineHandle handle, Segment newSegment)
		{
			Timing instance = GetInstance(handle.Key);
			if (instance == null || !instance._handleToIndex.ContainsKey(handle) || instance.CoindexIsNull(instance._handleToIndex[handle]))
			{
				return false;
			}
			instance.RunCoroutineInternal(instance.CoindexExtract(instance._handleToIndex[handle]), newSegment, (!instance._processLayers.ContainsKey(handle)) ? ((int?)null) : new int?(instance._processLayers[handle]), (!instance._processTags.ContainsKey(handle)) ? null : instance._processTags[handle], handle, false);
			return true;
		}

		public static bool RemoveTag(CoroutineHandle handle)
		{
			return SetTag(handle, null);
		}

		public static bool RemoveLayer(CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			if (instance == null || !instance._handleToIndex.ContainsKey(handle) || instance.CoindexIsNull(instance._handleToIndex[handle]))
			{
				return false;
			}
			instance.RemoveLayerOnInstance(handle);
			return true;
		}

		public static bool IsRunning(CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			return instance != null && instance._handleToIndex.ContainsKey(handle) && !instance.CoindexIsNull(instance._handleToIndex[handle]);
		}

		public static bool IsPaused(CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			return instance != null && instance._handleToIndex.ContainsKey(handle) && !instance.CoindexIsNull(instance._handleToIndex[handle]) && !instance.CoindexIsPaused(instance._handleToIndex[handle]);
		}

		private void AddTagOnInstance(string tag, CoroutineHandle handle)
		{
			_processTags.Add(handle, tag);
			if (_taggedProcesses.ContainsKey(tag))
			{
				_taggedProcesses[tag].Add(handle);
				return;
			}
			_taggedProcesses.Add(tag, new HashSet<CoroutineHandle> { handle });
		}

		private void AddLayerOnInstance(int layer, CoroutineHandle handle)
		{
			_processLayers.Add(handle, layer);
			if (_layeredProcesses.ContainsKey(layer))
			{
				_layeredProcesses[layer].Add(handle);
				return;
			}
			_layeredProcesses.Add(layer, new HashSet<CoroutineHandle> { handle });
		}

		private void RemoveTagOnInstance(CoroutineHandle handle)
		{
			if (_processTags.ContainsKey(handle))
			{
				if (_taggedProcesses[_processTags[handle]].Count > 1)
				{
					_taggedProcesses[_processTags[handle]].Remove(handle);
				}
				else
				{
					_taggedProcesses.Remove(_processTags[handle]);
				}
				_processTags.Remove(handle);
			}
		}

		private void RemoveLayerOnInstance(CoroutineHandle handle)
		{
			if (_processLayers.ContainsKey(handle))
			{
				if (_layeredProcesses[_processLayers[handle]].Count > 1)
				{
					_layeredProcesses[_processLayers[handle]].Remove(handle);
				}
				else
				{
					_layeredProcesses.Remove(_processLayers[handle]);
				}
				_processLayers.Remove(handle);
			}
		}

		private void RemoveGraffiti(CoroutineHandle handle)
		{
			if (_processLayers.ContainsKey(handle))
			{
				if (_layeredProcesses[_processLayers[handle]].Count > 1)
				{
					_layeredProcesses[_processLayers[handle]].Remove(handle);
				}
				else
				{
					_layeredProcesses.Remove(_processLayers[handle]);
				}
				_processLayers.Remove(handle);
			}
			if (_processTags.ContainsKey(handle))
			{
				if (_taggedProcesses[_processTags[handle]].Count > 1)
				{
					_taggedProcesses[_processTags[handle]].Remove(handle);
				}
				else
				{
					_taggedProcesses.Remove(_processTags[handle]);
				}
				_processTags.Remove(handle);
			}
		}

		private IEnumerator<float> CoindexExtract(ProcessIndex coindex)
		{
			switch (coindex.seg)
			{
			case Segment.Update:
			{
				IEnumerator<float> result = UpdateProcesses[coindex.i];
				UpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.FixedUpdate:
			{
				IEnumerator<float> result = FixedUpdateProcesses[coindex.i];
				FixedUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.LateUpdate:
			{
				IEnumerator<float> result = LateUpdateProcesses[coindex.i];
				LateUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.SlowUpdate:
			{
				IEnumerator<float> result = SlowUpdateProcesses[coindex.i];
				SlowUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.RealtimeUpdate:
			{
				IEnumerator<float> result = RealtimeUpdateProcesses[coindex.i];
				RealtimeUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.EditorUpdate:
			{
				IEnumerator<float> result = EditorUpdateProcesses[coindex.i];
				EditorUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.EditorSlowUpdate:
			{
				IEnumerator<float> result = EditorSlowUpdateProcesses[coindex.i];
				EditorSlowUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.EndOfFrame:
			{
				IEnumerator<float> result = EndOfFrameProcesses[coindex.i];
				EndOfFrameProcesses[coindex.i] = null;
				return result;
			}
			case Segment.ManualTimeframe:
			{
				IEnumerator<float> result = ManualTimeframeProcesses[coindex.i];
				ManualTimeframeProcesses[coindex.i] = null;
				return result;
			}
			default:
				return null;
			}
		}

		private bool CoindexIsNull(ProcessIndex coindex)
		{
			switch (coindex.seg)
			{
			case Segment.Update:
				return UpdateProcesses[coindex.i] == null;
			case Segment.FixedUpdate:
				return FixedUpdateProcesses[coindex.i] == null;
			case Segment.LateUpdate:
				return LateUpdateProcesses[coindex.i] == null;
			case Segment.SlowUpdate:
				return SlowUpdateProcesses[coindex.i] == null;
			case Segment.RealtimeUpdate:
				return RealtimeUpdateProcesses[coindex.i] == null;
			case Segment.EditorUpdate:
				return EditorUpdateProcesses[coindex.i] == null;
			case Segment.EditorSlowUpdate:
				return EditorSlowUpdateProcesses[coindex.i] == null;
			case Segment.EndOfFrame:
				return EndOfFrameProcesses[coindex.i] == null;
			case Segment.ManualTimeframe:
				return ManualTimeframeProcesses[coindex.i] == null;
			default:
				return true;
			}
		}

		private IEnumerator<float> CoindexPeek(ProcessIndex coindex)
		{
			switch (coindex.seg)
			{
			case Segment.Update:
				return UpdateProcesses[coindex.i];
			case Segment.FixedUpdate:
				return FixedUpdateProcesses[coindex.i];
			case Segment.LateUpdate:
				return LateUpdateProcesses[coindex.i];
			case Segment.SlowUpdate:
				return SlowUpdateProcesses[coindex.i];
			case Segment.RealtimeUpdate:
				return RealtimeUpdateProcesses[coindex.i];
			case Segment.EditorUpdate:
				return EditorUpdateProcesses[coindex.i];
			case Segment.EditorSlowUpdate:
				return EditorSlowUpdateProcesses[coindex.i];
			case Segment.EndOfFrame:
				return EndOfFrameProcesses[coindex.i];
			case Segment.ManualTimeframe:
				return ManualTimeframeProcesses[coindex.i];
			default:
				return null;
			}
		}

		private bool Nullify(CoroutineHandle handle)
		{
			return Nullify(_handleToIndex[handle]);
		}

		private bool Nullify(ProcessIndex coindex)
		{
			switch (coindex.seg)
			{
			case Segment.Update:
			{
				bool result = UpdateProcesses[coindex.i] != null;
				UpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.FixedUpdate:
			{
				bool result = FixedUpdateProcesses[coindex.i] != null;
				FixedUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.LateUpdate:
			{
				bool result = LateUpdateProcesses[coindex.i] != null;
				LateUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.SlowUpdate:
			{
				bool result = SlowUpdateProcesses[coindex.i] != null;
				SlowUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.RealtimeUpdate:
			{
				bool result = RealtimeUpdateProcesses[coindex.i] != null;
				RealtimeUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.EditorUpdate:
			{
				bool result = UpdateProcesses[coindex.i] != null;
				EditorUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.EditorSlowUpdate:
			{
				bool result = EditorSlowUpdateProcesses[coindex.i] != null;
				EditorSlowUpdateProcesses[coindex.i] = null;
				return result;
			}
			case Segment.EndOfFrame:
			{
				bool result = EndOfFrameProcesses[coindex.i] != null;
				EndOfFrameProcesses[coindex.i] = null;
				return result;
			}
			case Segment.ManualTimeframe:
			{
				bool result = ManualTimeframeProcesses[coindex.i] != null;
				ManualTimeframeProcesses[coindex.i] = null;
				return result;
			}
			default:
				return false;
			}
		}

		private bool SetPause(ProcessIndex coindex, bool newPausedState = true)
		{
			if (CoindexPeek(coindex) == null)
			{
				return false;
			}
			switch (coindex.seg)
			{
			case Segment.Update:
			{
				bool result = UpdatePaused[coindex.i];
				UpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && UpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					UpdateProcesses[coindex.i] = _InjectDelay(UpdateProcesses[coindex.i], UpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return result;
			}
			case Segment.FixedUpdate:
			{
				bool result = FixedUpdatePaused[coindex.i];
				FixedUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && FixedUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					FixedUpdateProcesses[coindex.i] = _InjectDelay(FixedUpdateProcesses[coindex.i], FixedUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return result;
			}
			case Segment.LateUpdate:
			{
				bool result = LateUpdatePaused[coindex.i];
				LateUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && LateUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					LateUpdateProcesses[coindex.i] = _InjectDelay(LateUpdateProcesses[coindex.i], LateUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return result;
			}
			case Segment.SlowUpdate:
			{
				bool result = SlowUpdatePaused[coindex.i];
				SlowUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && SlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					SlowUpdateProcesses[coindex.i] = _InjectDelay(SlowUpdateProcesses[coindex.i], SlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return result;
			}
			case Segment.RealtimeUpdate:
			{
				bool result = RealtimeUpdatePaused[coindex.i];
				RealtimeUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && RealtimeUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					RealtimeUpdateProcesses[coindex.i] = _InjectDelay(RealtimeUpdateProcesses[coindex.i], RealtimeUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return result;
			}
			case Segment.EditorUpdate:
			{
				bool result = EditorUpdatePaused[coindex.i];
				EditorUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && EditorUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					EditorUpdateProcesses[coindex.i] = _InjectDelay(EditorUpdateProcesses[coindex.i], EditorUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return result;
			}
			case Segment.EditorSlowUpdate:
			{
				bool result = EditorSlowUpdatePaused[coindex.i];
				EditorSlowUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && EditorSlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					EditorSlowUpdateProcesses[coindex.i] = _InjectDelay(EditorSlowUpdateProcesses[coindex.i], EditorSlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return result;
			}
			case Segment.EndOfFrame:
			{
				bool result = EndOfFramePaused[coindex.i];
				EndOfFramePaused[coindex.i] = newPausedState;
				if (newPausedState && EndOfFrameProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					EndOfFrameProcesses[coindex.i] = _InjectDelay(EndOfFrameProcesses[coindex.i], EndOfFrameProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return result;
			}
			case Segment.ManualTimeframe:
			{
				bool result = ManualTimeframePaused[coindex.i];
				ManualTimeframePaused[coindex.i] = newPausedState;
				if (newPausedState && ManualTimeframeProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					ManualTimeframeProcesses[coindex.i] = _InjectDelay(ManualTimeframeProcesses[coindex.i], ManualTimeframeProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return result;
			}
			default:
				return false;
			}
		}

		private IEnumerator<float> CreatePause(ProcessIndex coindex, IEnumerator<float> coptr, bool newPausedState = true)
		{
			if (CoindexPeek(coindex) == null)
			{
				return null;
			}
			switch (coindex.seg)
			{
			case Segment.Update:
				UpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && UpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					coptr = _InjectDelay(UpdateProcesses[coindex.i], UpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return coptr;
			case Segment.FixedUpdate:
				FixedUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && FixedUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					coptr = _InjectDelay(FixedUpdateProcesses[coindex.i], FixedUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return coptr;
			case Segment.LateUpdate:
				LateUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && LateUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					coptr = _InjectDelay(LateUpdateProcesses[coindex.i], LateUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return coptr;
			case Segment.SlowUpdate:
				SlowUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && SlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					coptr = _InjectDelay(SlowUpdateProcesses[coindex.i], SlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return coptr;
			case Segment.RealtimeUpdate:
				RealtimeUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && RealtimeUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					coptr = _InjectDelay(RealtimeUpdateProcesses[coindex.i], RealtimeUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return coptr;
			case Segment.EditorUpdate:
				EditorUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && EditorUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					coptr = _InjectDelay(EditorUpdateProcesses[coindex.i], EditorUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return coptr;
			case Segment.EditorSlowUpdate:
				EditorSlowUpdatePaused[coindex.i] = newPausedState;
				if (newPausedState && EditorSlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					coptr = _InjectDelay(EditorSlowUpdateProcesses[coindex.i], EditorSlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return coptr;
			case Segment.EndOfFrame:
				EndOfFramePaused[coindex.i] = newPausedState;
				if (newPausedState && EndOfFrameProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					coptr = _InjectDelay(EndOfFrameProcesses[coindex.i], EndOfFrameProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return coptr;
			case Segment.ManualTimeframe:
				ManualTimeframePaused[coindex.i] = newPausedState;
				if (newPausedState && ManualTimeframeProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
				{
					coptr = _InjectDelay(ManualTimeframeProcesses[coindex.i], ManualTimeframeProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
				}
				return coptr;
			default:
				return coptr;
			}
		}

		private bool CoindexIsPaused(ProcessIndex coindex)
		{
			switch (coindex.seg)
			{
			case Segment.Update:
				return UpdatePaused[coindex.i];
			case Segment.FixedUpdate:
				return FixedUpdatePaused[coindex.i];
			case Segment.LateUpdate:
				return LateUpdatePaused[coindex.i];
			case Segment.SlowUpdate:
				return SlowUpdatePaused[coindex.i];
			case Segment.RealtimeUpdate:
				return RealtimeUpdatePaused[coindex.i];
			case Segment.EditorUpdate:
				return EditorUpdatePaused[coindex.i];
			case Segment.EditorSlowUpdate:
				return EditorSlowUpdatePaused[coindex.i];
			case Segment.EndOfFrame:
				return EndOfFramePaused[coindex.i];
			case Segment.ManualTimeframe:
				return ManualTimeframePaused[coindex.i];
			default:
				return false;
			}
		}

		private void CoindexReplace(ProcessIndex coindex, IEnumerator<float> replacement)
		{
			switch (coindex.seg)
			{
			case Segment.Update:
				UpdateProcesses[coindex.i] = replacement;
				break;
			case Segment.FixedUpdate:
				FixedUpdateProcesses[coindex.i] = replacement;
				break;
			case Segment.LateUpdate:
				LateUpdateProcesses[coindex.i] = replacement;
				break;
			case Segment.SlowUpdate:
				SlowUpdateProcesses[coindex.i] = replacement;
				break;
			case Segment.RealtimeUpdate:
				RealtimeUpdateProcesses[coindex.i] = replacement;
				break;
			case Segment.EditorUpdate:
				EditorUpdateProcesses[coindex.i] = replacement;
				break;
			case Segment.EditorSlowUpdate:
				EditorSlowUpdateProcesses[coindex.i] = replacement;
				break;
			case Segment.EndOfFrame:
				EndOfFrameProcesses[coindex.i] = replacement;
				break;
			case Segment.ManualTimeframe:
				ManualTimeframeProcesses[coindex.i] = replacement;
				break;
			}
		}

		public static float WaitUntilDone(IEnumerator<float> newCoroutine)
		{
			return WaitUntilDone(RunCoroutine(newCoroutine), true);
		}

		public static float WaitUntilDone(IEnumerator<float> newCoroutine, string tag)
		{
			return WaitUntilDone(RunCoroutine(newCoroutine, tag), true);
		}

		public static float WaitUntilDone(IEnumerator<float> newCoroutine, int layer)
		{
			return WaitUntilDone(RunCoroutine(newCoroutine, layer), true);
		}

		public static float WaitUntilDone(IEnumerator<float> newCoroutine, int layer, string tag)
		{
			return WaitUntilDone(RunCoroutine(newCoroutine, layer, tag), true);
		}

		public static float WaitUntilDone(IEnumerator<float> newCoroutine, Segment segment)
		{
			return WaitUntilDone(RunCoroutine(newCoroutine, segment), true);
		}

		public static float WaitUntilDone(IEnumerator<float> newCoroutine, Segment segment, string tag)
		{
			return WaitUntilDone(RunCoroutine(newCoroutine, segment, tag), true);
		}

		public static float WaitUntilDone(IEnumerator<float> newCoroutine, Segment segment, int layer)
		{
			return WaitUntilDone(RunCoroutine(newCoroutine, segment, layer), true);
		}

		public static float WaitUntilDone(IEnumerator<float> newCoroutine, Segment segment, int layer, string tag)
		{
			return WaitUntilDone(RunCoroutine(newCoroutine, segment, layer, tag), true);
		}

		public static float WaitUntilDone(CoroutineHandle otherCoroutine)
		{
			return WaitUntilDone(otherCoroutine, true);
		}

		public static float WaitUntilDone(CoroutineHandle otherCoroutine, bool warnOnIssue)
		{
			Timing instance = GetInstance(otherCoroutine.Key);
			if (instance != null && instance._handleToIndex.ContainsKey(otherCoroutine))
			{
				if (instance.CoindexIsNull(instance._handleToIndex[otherCoroutine]))
				{
					return 0f;
				}
				if (!instance._waitingTriggers.ContainsKey(otherCoroutine))
				{
					instance.CoindexReplace(instance._handleToIndex[otherCoroutine], instance._StartWhenDone(otherCoroutine, instance.CoindexPeek(instance._handleToIndex[otherCoroutine])));
					instance._waitingTriggers.Add(otherCoroutine, new HashSet<CoroutineHandle>());
				}
				_tmpBool = warnOnIssue;
				_tmpHandle = otherCoroutine;
				ReplacementFunction = instance.WaitUntilDoneWrapper;
				return float.NaN;
			}
			return 0f;
		}

		private IEnumerator<float> WaitUntilDoneWrapper(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			bool tmpBool = _tmpBool;
			CoroutineHandle tmpHandle = _tmpHandle;
			if (handle == tmpHandle)
			{
				return coptr;
			}
			if (handle.Key != tmpHandle.Key)
			{
				return coptr;
			}
			_waitingTriggers[tmpHandle].Add(handle);
			if (!_allWaiting.Contains(tmpHandle))
			{
				_allWaiting.Add(tmpHandle);
			}
			coptr = CreatePause(_handleToIndex[handle], coptr);
			return coptr;
		}

		public static void WaitForOtherHandles(CoroutineHandle handle, CoroutineHandle otherHandle, bool warnOnIssue = true)
		{
			if (!IsRunning(handle) || !IsRunning(otherHandle) || handle == otherHandle || handle.Key != otherHandle.Key)
			{
				return;
			}
			Timing instance = GetInstance(handle.Key);
			if (instance != null && instance._handleToIndex.ContainsKey(handle) && instance._handleToIndex.ContainsKey(otherHandle) && !instance.CoindexIsNull(instance._handleToIndex[otherHandle]))
			{
				if (!instance._waitingTriggers.ContainsKey(otherHandle))
				{
					instance.CoindexReplace(instance._handleToIndex[otherHandle], instance._StartWhenDone(otherHandle, instance.CoindexPeek(instance._handleToIndex[otherHandle])));
					instance._waitingTriggers.Add(otherHandle, new HashSet<CoroutineHandle>());
				}
				instance._waitingTriggers[otherHandle].Add(handle);
				if (!instance._allWaiting.Contains(otherHandle))
				{
					instance._allWaiting.Add(otherHandle);
				}
				instance.SetPause(instance._handleToIndex[handle]);
			}
		}

		public static void WaitForOtherHandles(CoroutineHandle handle, IEnumerable<CoroutineHandle> otherHandles, bool warnOnIssue = true)
		{
			if (!IsRunning(handle))
			{
				return;
			}
			Timing instance = GetInstance(handle.Key);
			IEnumerator<CoroutineHandle> enumerator = otherHandles.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (IsRunning(enumerator.Current) && !(handle == enumerator.Current) && handle.Key == enumerator.Current.Key)
				{
					if (!instance._waitingTriggers.ContainsKey(enumerator.Current))
					{
						instance.CoindexReplace(instance._handleToIndex[enumerator.Current], instance._StartWhenDone(enumerator.Current, instance.CoindexPeek(instance._handleToIndex[enumerator.Current])));
						instance._waitingTriggers.Add(enumerator.Current, new HashSet<CoroutineHandle>());
					}
					instance._waitingTriggers[enumerator.Current].Add(handle);
					if (!instance._allWaiting.Contains(enumerator.Current))
					{
						instance._allWaiting.Add(enumerator.Current);
					}
					instance.SetPause(instance._handleToIndex[handle]);
				}
			}
		}

		private IEnumerator<float> _StartWhenDone(CoroutineHandle handle, IEnumerator<float> proc)
		{
			if (!_waitingTriggers.ContainsKey(handle))
			{
				yield break;
			}
			try
			{
				if (proc.Current > localTime)
				{
					yield return proc.Current;
				}
				while (proc.MoveNext())
				{
					yield return proc.Current;
				}
			}
			finally
			{
				CloseWaitingProcess(handle);
			}
		}

		private void CloseWaitingProcess(CoroutineHandle handle)
		{
			if (!_waitingTriggers.ContainsKey(handle))
			{
				return;
			}
			HashSet<CoroutineHandle>.Enumerator enumerator = _waitingTriggers[handle].GetEnumerator();
			_waitingTriggers.Remove(handle);
			while (enumerator.MoveNext())
			{
				if (_handleToIndex.ContainsKey(enumerator.Current) && !HandleIsInWaitingList(enumerator.Current))
				{
					SetPause(_handleToIndex[enumerator.Current], false);
					_allWaiting.Remove(handle);
				}
			}
		}

		private bool HandleIsInWaitingList(CoroutineHandle handle)
		{
			Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>.Enumerator enumerator = _waitingTriggers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value.Contains(handle))
				{
					return true;
				}
			}
			return false;
		}

		public static float WaitUntilDone(WWW wwwObject)
		{
			if (wwwObject == null || wwwObject.isDone)
			{
				return 0f;
			}
			_tmpRef = wwwObject;
			ReplacementFunction = WaitUntilDoneWwwHelper;
			return float.NaN;
		}

		private static IEnumerator<float> WaitUntilDoneWwwHelper(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			return _StartWhenDone(_tmpRef as WWW, coptr);
		}

		private static IEnumerator<float> _StartWhenDone(WWW wwwObject, IEnumerator<float> pausedProc)
		{
			while (!wwwObject.isDone)
			{
				yield return 0f;
			}
			ReplacementFunction = (IEnumerator<float> P_0, CoroutineHandle P_1) => pausedProc;
			yield return float.NaN;
		}

		public static float WaitUntilDone(AsyncOperation operation)
		{
			if (operation == null || operation.isDone)
			{
				return float.NaN;
			}
			_tmpRef = operation;
			ReplacementFunction = WaitUntilDoneAscOpHelper;
			return float.NaN;
		}

		private static IEnumerator<float> WaitUntilDoneAscOpHelper(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			return _StartWhenDone(_tmpRef as AsyncOperation, coptr);
		}

		private static IEnumerator<float> _StartWhenDone(AsyncOperation operation, IEnumerator<float> pausedProc)
		{
			while (!operation.isDone)
			{
				yield return 0f;
			}
			ReplacementFunction = (IEnumerator<float> P_0, CoroutineHandle P_1) => pausedProc;
			yield return float.NaN;
		}

		public static float WaitUntilDone(CustomYieldInstruction operation)
		{
			if (operation == null || !operation.keepWaiting)
			{
				return float.NaN;
			}
			_tmpRef = operation;
			ReplacementFunction = WaitUntilDoneCustYieldHelper;
			return float.NaN;
		}

		private static IEnumerator<float> WaitUntilDoneCustYieldHelper(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			return _StartWhenDone(_tmpRef as CustomYieldInstruction, coptr);
		}

		private static IEnumerator<float> _StartWhenDone(CustomYieldInstruction operation, IEnumerator<float> pausedProc)
		{
			while (operation.keepWaiting)
			{
				yield return 0f;
			}
			ReplacementFunction = (IEnumerator<float> P_0, CoroutineHandle P_1) => pausedProc;
			yield return float.NaN;
		}

		public static float WaitUntilTrue(Func<bool> evaluatorFunc)
		{
			if (evaluatorFunc == null || evaluatorFunc())
			{
				return float.NaN;
			}
			_tmpRef = evaluatorFunc;
			ReplacementFunction = WaitUntilTrueHelper;
			return float.NaN;
		}

		private static IEnumerator<float> WaitUntilTrueHelper(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			return _StartWhenDone(_tmpRef as Func<bool>, false, coptr);
		}

		public static float WaitUntilFalse(Func<bool> evaluatorFunc)
		{
			if (evaluatorFunc == null || !evaluatorFunc())
			{
				return float.NaN;
			}
			_tmpRef = evaluatorFunc;
			ReplacementFunction = WaitUntilFalseHelper;
			return float.NaN;
		}

		private static IEnumerator<float> WaitUntilFalseHelper(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			return _StartWhenDone(_tmpRef as Func<bool>, true, coptr);
		}

		private static IEnumerator<float> _StartWhenDone(Func<bool> evaluatorFunc, bool continueOn, IEnumerator<float> pausedProc)
		{
			while (evaluatorFunc() == continueOn)
			{
				yield return 0f;
			}
			ReplacementFunction = (IEnumerator<float> P_0, CoroutineHandle P_1) => pausedProc;
			yield return float.NaN;
		}

		private IEnumerator<float> _InjectDelay(IEnumerator<float> proc, float waitTime)
		{
			yield return WaitForSecondsOnInstance(waitTime);
			ReplacementFunction = (IEnumerator<float> P_0, CoroutineHandle P_1) => proc;
			yield return float.NaN;
		}

		public bool LockCoroutine(CoroutineHandle coroutine, CoroutineHandle key)
		{
			if (coroutine.Key != _instanceID || key == default(CoroutineHandle) || key.Key != 0)
			{
				return false;
			}
			if (!_waitingTriggers.ContainsKey(key))
			{
				_waitingTriggers.Add(key, new HashSet<CoroutineHandle> { coroutine });
			}
			else
			{
				_waitingTriggers[key].Add(coroutine);
			}
			SetPause(_handleToIndex[coroutine]);
			return true;
		}

		public bool UnlockCoroutine(CoroutineHandle coroutine, CoroutineHandle key)
		{
			if (coroutine.Key != _instanceID || key == default(CoroutineHandle) || !_handleToIndex.ContainsKey(coroutine) || !_waitingTriggers.ContainsKey(key))
			{
				return false;
			}
			_waitingTriggers[key].Remove(coroutine);
			SetPause(_handleToIndex[coroutine], HandleIsInWaitingList(coroutine));
			return true;
		}

		public static int LinkCoroutines(CoroutineHandle master, CoroutineHandle slave)
		{
			if (!IsRunning(slave) || !master.IsValid)
			{
				return 0;
			}
			if (!IsRunning(master))
			{
				KillCoroutines(slave);
				return 1;
			}
			if (Links.ContainsKey(master))
			{
				if (!Links[master].Contains(slave))
				{
					Links[master].Add(slave);
					return 1;
				}
				return 0;
			}
			Links.Add(master, new HashSet<CoroutineHandle> { slave });
			return 1;
		}

		public static int UnlinkCoroutines(CoroutineHandle master, CoroutineHandle slave, bool twoWay = false)
		{
			int num = 0;
			if (Links.ContainsKey(master) && Links[master].Contains(slave))
			{
				if (Links[master].Count <= 1)
				{
					Links.Remove(master);
				}
				else
				{
					Links[master].Remove(slave);
				}
				num++;
			}
			if (twoWay && Links.ContainsKey(slave) && Links[slave].Contains(master))
			{
				if (Links[slave].Count <= 1)
				{
					Links.Remove(slave);
				}
				else
				{
					Links[slave].Remove(master);
				}
				num++;
			}
			return num;
		}

		public static float GetMyHandle(Action<CoroutineHandle> reciever)
		{
			_tmpRef = reciever;
			ReplacementFunction = GetHandleHelper;
			return float.NaN;
		}

		private static IEnumerator<float> GetHandleHelper(IEnumerator<float> input, CoroutineHandle handle)
		{
			Action<CoroutineHandle> action = _tmpRef as Action<CoroutineHandle>;
			if (action != null)
			{
				action(handle);
			}
			return input;
		}

		public static float SwitchCoroutine(Segment newSegment)
		{
			_tmpSegment = newSegment;
			ReplacementFunction = SwitchCoroutineRepS;
			return float.NaN;
		}

		private static IEnumerator<float> SwitchCoroutineRepS(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			instance.RunCoroutineInternal(coptr, _tmpSegment, (!instance._processLayers.ContainsKey(handle)) ? ((int?)null) : new int?(instance._processLayers[handle]), (!instance._processTags.ContainsKey(handle)) ? null : instance._processTags[handle], handle, false);
			return null;
		}

		public static float SwitchCoroutine(Segment newSegment, string newTag)
		{
			_tmpSegment = newSegment;
			_tmpRef = newTag;
			ReplacementFunction = SwitchCoroutineRepST;
			return float.NaN;
		}

		private static IEnumerator<float> SwitchCoroutineRepST(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			instance.RunCoroutineInternal(coptr, _tmpSegment, (!instance._processLayers.ContainsKey(handle)) ? ((int?)null) : new int?(instance._processLayers[handle]), _tmpRef as string, handle, false);
			return null;
		}

		public static float SwitchCoroutine(Segment newSegment, int newLayer)
		{
			_tmpSegment = newSegment;
			_tmpInt = newLayer;
			ReplacementFunction = SwitchCoroutineRepSL;
			return float.NaN;
		}

		private static IEnumerator<float> SwitchCoroutineRepSL(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			instance.RunCoroutineInternal(coptr, _tmpSegment, _tmpInt, (!instance._processTags.ContainsKey(handle)) ? null : instance._processTags[handle], handle, false);
			return null;
		}

		public static float SwitchCoroutine(Segment newSegment, int newLayer, string newTag)
		{
			_tmpSegment = newSegment;
			_tmpInt = newLayer;
			_tmpRef = newTag;
			ReplacementFunction = SwitchCoroutineRepSLT;
			return float.NaN;
		}

		private static IEnumerator<float> SwitchCoroutineRepSLT(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			GetInstance(handle.Key).RunCoroutineInternal(coptr, _tmpSegment, _tmpInt, _tmpRef as string, handle, false);
			return null;
		}

		public static float SwitchCoroutine(string newTag)
		{
			_tmpRef = newTag;
			ReplacementFunction = SwitchCoroutineRepT;
			return float.NaN;
		}

		private static IEnumerator<float> SwitchCoroutineRepT(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			instance.RemoveTagOnInstance(handle);
			if (_tmpRef is string)
			{
				instance.AddTagOnInstance((string)_tmpRef, handle);
			}
			return coptr;
		}

		public static float SwitchCoroutine(int newLayer)
		{
			_tmpInt = newLayer;
			ReplacementFunction = SwitchCoroutineRepL;
			return float.NaN;
		}

		private static IEnumerator<float> SwitchCoroutineRepL(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			RemoveLayer(handle);
			GetInstance(handle.Key).AddLayerOnInstance(_tmpInt, handle);
			return coptr;
		}

		public static float SwitchCoroutine(int newLayer, string newTag)
		{
			_tmpInt = newLayer;
			_tmpRef = newTag;
			ReplacementFunction = SwitchCoroutineRepLT;
			return float.NaN;
		}

		private static IEnumerator<float> SwitchCoroutineRepLT(IEnumerator<float> coptr, CoroutineHandle handle)
		{
			Timing instance = GetInstance(handle.Key);
			instance.RemoveLayerOnInstance(handle);
			instance.AddLayerOnInstance(_tmpInt, handle);
			instance.RemoveTagOnInstance(handle);
			if (_tmpRef is string)
			{
				instance.AddTagOnInstance((string)_tmpRef, handle);
			}
			return coptr;
		}

		public static CoroutineHandle CallDelayed(float delay, Action action)
		{
			return (action != null) ? RunCoroutine(Instance._DelayedCall(delay, action, null)) : default(CoroutineHandle);
		}

		public CoroutineHandle CallDelayedOnInstance(float delay, Action action)
		{
			return (action != null) ? RunCoroutineOnInstance(_DelayedCall(delay, action, null)) : default(CoroutineHandle);
		}

		public static CoroutineHandle CallDelayed(float delay, Action action, GameObject gameObject)
		{
			return (action != null) ? RunCoroutine(Instance._DelayedCall(delay, action, gameObject), gameObject) : default(CoroutineHandle);
		}

		public CoroutineHandle CallDelayedOnInstance(float delay, Action action, GameObject gameObject)
		{
			return (action != null) ? RunCoroutineOnInstance(_DelayedCall(delay, action, gameObject), gameObject) : default(CoroutineHandle);
		}

		private IEnumerator<float> _DelayedCall(float delay, Action action, GameObject cancelWith)
		{
			yield return WaitForSecondsOnInstance(delay);
			if (object.ReferenceEquals(cancelWith, null) || cancelWith != null)
			{
				action();
			}
		}

		public static CoroutineHandle CallPeriodically(float timeframe, float period, Action action, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(period, action, null)) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				RunCoroutine(Instance._WatchCall(timeframe, coroutineHandle, null, onDone));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, Action action, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(period, action, null)) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				RunCoroutineOnInstance(_WatchCall(timeframe, coroutineHandle, null, onDone));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallPeriodically(float timeframe, float period, Action action, GameObject gameObject, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(period, action, gameObject), gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(timeframe, coroutineHandle, gameObject, onDone), gameObject));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, Action action, GameObject gameObject, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(period, action, gameObject), gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(timeframe, coroutineHandle, gameObject, onDone), gameObject));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallPeriodically(float timeframe, float period, Action action, Segment timing, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(period, action, null), timing) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				RunCoroutine(Instance._WatchCall(timeframe, coroutineHandle, null, onDone), timing);
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, Action action, Segment timing, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(period, action, null), timing) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(timeframe, coroutineHandle, null, onDone), timing));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallPeriodically(float timeframe, float period, Action action, Segment timing, GameObject gameObject, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(period, action, gameObject), timing, gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(timeframe, coroutineHandle, gameObject, onDone), timing, gameObject));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, Action action, Segment timing, GameObject gameObject, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(period, action, gameObject), timing, gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(timeframe, coroutineHandle, gameObject, onDone), timing, gameObject));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallContinuously(float timeframe, Action action, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(0f, action, null)) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(timeframe, coroutineHandle, null, onDone)));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallContinuouslyOnInstance(float timeframe, Action action, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(0f, action, null)) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(timeframe, coroutineHandle, null, onDone)));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallContinuously(float timeframe, Action action, GameObject gameObject, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(0f, action, gameObject), gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(timeframe, coroutineHandle, gameObject, onDone), gameObject));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallContinuouslyOnInstance(float timeframe, Action action, GameObject gameObject, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(0f, action, gameObject), gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(timeframe, coroutineHandle, gameObject, onDone), gameObject));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallContinuously(float timeframe, Action action, Segment timing, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(0f, action, null), timing) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(timeframe, coroutineHandle, null, onDone), timing));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallContinuouslyOnInstance(float timeframe, Action action, Segment timing, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(0f, action, null), timing) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(timeframe, coroutineHandle, null, onDone), timing));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallContinuously(float timeframe, Action action, Segment timing, GameObject gameObject, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(0f, action, gameObject), timing, gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(timeframe, coroutineHandle, gameObject, onDone), timing, gameObject));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallContinuouslyOnInstance(float timeframe, Action action, Segment timing, GameObject gameObject, Action onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(0f, action, gameObject), timing, gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(timeframe, coroutineHandle, gameObject, onDone), timing, gameObject));
			}
			return coroutineHandle;
		}

		private IEnumerator<float> _WatchCall(float timeframe, CoroutineHandle handle, GameObject gObject, Action onDone)
		{
			yield return WaitForSecondsOnInstance(timeframe);
			KillCoroutinesOnInstance(handle);
			if (onDone != null && (object.ReferenceEquals(gObject, null) || gObject != null))
			{
				onDone();
			}
		}

		private IEnumerator<float> _CallContinuously(float period, Action action, GameObject gObject)
		{
			while (object.ReferenceEquals(gObject, null) || gObject != null)
			{
				yield return WaitForSecondsOnInstance(period);
				if (object.ReferenceEquals(gObject, null) || (gObject != null && gObject.activeInHierarchy))
				{
					action();
				}
			}
		}

		public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period, Action<T> action, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(reference, period, action, null)) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(reference, timeframe, coroutineHandle, null, onDone)));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period, Action<T> action, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(reference, period, action, null)) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, coroutineHandle, null, onDone)));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period, Action<T> action, GameObject gameObject, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(reference, period, action, gameObject), gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(reference, timeframe, coroutineHandle, gameObject, onDone), gameObject));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period, Action<T> action, GameObject gameObject, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(reference, period, action, gameObject), gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, coroutineHandle, gameObject, onDone), gameObject));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period, Action<T> action, Segment timing, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(reference, period, action, null), timing) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(reference, timeframe, coroutineHandle, null, onDone), timing));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period, Action<T> action, Segment timing, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(reference, period, action, null), timing) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, coroutineHandle, null, onDone), timing));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period, Action<T> action, Segment timing, GameObject gameObject, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(reference, period, action, gameObject), timing, gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(reference, timeframe, coroutineHandle, gameObject, onDone), timing, gameObject));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period, Action<T> action, Segment timing, GameObject gameObject, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(reference, period, action, gameObject), timing, gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, coroutineHandle, gameObject, onDone), timing, gameObject));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, Action<T> action, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(reference, 0f, action, null)) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(reference, timeframe, coroutineHandle, null, onDone)));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, Action<T> action, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(reference, 0f, action, null)) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, coroutineHandle, null, onDone)));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, Action<T> action, GameObject gameObject, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(reference, 0f, action, gameObject), gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(reference, timeframe, coroutineHandle, gameObject, onDone), gameObject));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, Action<T> action, GameObject gameObject, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(reference, 0f, action, gameObject), gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, coroutineHandle, gameObject, onDone), gameObject));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, Action<T> action, Segment timing, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(reference, 0f, action, null), timing) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(reference, timeframe, coroutineHandle, null, onDone), timing));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, Action<T> action, Segment timing, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(reference, 0f, action, null), timing) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, coroutineHandle, null, onDone), timing));
			}
			return coroutineHandle;
		}

		public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, Action<T> action, Segment timing, GameObject gameObject, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutine(Instance._CallContinuously(reference, 0f, action, gameObject), timing, gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutine(Instance._WatchCall(reference, timeframe, coroutineHandle, gameObject, onDone), timing, gameObject));
			}
			return coroutineHandle;
		}

		public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, Action<T> action, Segment timing, GameObject gameObject, Action<T> onDone = null)
		{
			CoroutineHandle coroutineHandle = ((action != null) ? RunCoroutineOnInstance(_CallContinuously(reference, 0f, action, gameObject), timing, gameObject) : default(CoroutineHandle));
			if (!float.IsPositiveInfinity(timeframe))
			{
				LinkCoroutines(coroutineHandle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, coroutineHandle, gameObject, onDone), timing, gameObject));
			}
			return coroutineHandle;
		}

		private IEnumerator<float> _WatchCall<T>(T reference, float timeframe, CoroutineHandle handle, GameObject gObject, Action<T> onDone)
		{
			yield return WaitForSecondsOnInstance(timeframe);
			KillCoroutinesOnInstance(handle);
			if (onDone != null && (object.ReferenceEquals(gObject, null) || gObject != null))
			{
				onDone(reference);
			}
		}

		private IEnumerator<float> _CallContinuously<T>(T reference, float period, Action<T> action, GameObject gObject)
		{
			while (object.ReferenceEquals(gObject, null) || gObject != null)
			{
				yield return WaitForSecondsOnInstance(period);
				if (object.ReferenceEquals(gObject, null) || (gObject != null && gObject.activeInHierarchy))
				{
					action(reference);
				}
			}
		}

		[Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
		public new Coroutine StartCoroutine(IEnumerator routine)
		{
			return null;
		}

		[Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
		public new Coroutine StartCoroutine(string methodName, object value)
		{
			return null;
		}

		[Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
		public new Coroutine StartCoroutine(string methodName)
		{
			return null;
		}

		[Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
		public new Coroutine StartCoroutine_Auto(IEnumerator routine)
		{
			return null;
		}

		[Obsolete("Unity coroutine function, use KillCoroutine instead.", true)]
		public new void StopCoroutine(string methodName)
		{
		}

		[Obsolete("Unity coroutine function, use KillCoroutine instead.", true)]
		public new void StopCoroutine(IEnumerator routine)
		{
		}

		[Obsolete("Unity coroutine function, use KillCoroutine instead.", true)]
		public new void StopCoroutine(Coroutine routine)
		{
		}

		[Obsolete("Unity coroutine function, use KillAllCoroutines instead.", true)]
		public new void StopAllCoroutines()
		{
		}

		[Obsolete("Use your own GameObject for this.", true)]
		public new static void Destroy(UnityEngine.Object obj)
		{
		}

		[Obsolete("Use your own GameObject for this.", true)]
		public new static void Destroy(UnityEngine.Object obj, float f)
		{
		}

		[Obsolete("Use your own GameObject for this.", true)]
		public new static void DestroyObject(UnityEngine.Object obj)
		{
		}

		[Obsolete("Use your own GameObject for this.", true)]
		public new static void DestroyObject(UnityEngine.Object obj, float f)
		{
		}

		[Obsolete("Use your own GameObject for this.", true)]
		public new static void DestroyImmediate(UnityEngine.Object obj)
		{
		}

		[Obsolete("Use your own GameObject for this.", true)]
		public new static void DestroyImmediate(UnityEngine.Object obj, bool b)
		{
		}

		[Obsolete("Just.. no.", true)]
		public new static T FindObjectOfType<T>() where T : UnityEngine.Object
		{
			return (T)null;
		}

		[Obsolete("Just.. no.", true)]
		public new static UnityEngine.Object FindObjectOfType(Type t)
		{
			return null;
		}

		[Obsolete("Just.. no.", true)]
		public new static T[] FindObjectsOfType<T>() where T : UnityEngine.Object
		{
			return null;
		}

		[Obsolete("Just.. no.", true)]
		public new static UnityEngine.Object[] FindObjectsOfType(Type t)
		{
			return null;
		}

		[Obsolete("Just.. no.", true)]
		public new static void print(object message)
		{
		}
	}
}
