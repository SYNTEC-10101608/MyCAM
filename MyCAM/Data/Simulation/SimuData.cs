using MyCAM.Post;
using OCC.gp;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class SimuData
	{
		public class TreeData
		{
			public class MachineTreeDef
			{
				public MachineComponentType Type;
				public MachineTreeDef[] Children;

				public MachineTreeDef( MachineComponentType type, params MachineTreeDef[] children )
				{
					Type = type;
					Children = children;
				}
			}

			public static readonly MachineTreeDef DefaultMixTreeDef =
				new MachineTreeDef( MachineComponentType.Base,
					new MachineTreeDef( MachineComponentType.Slave,
						new MachineTreeDef( MachineComponentType.WorkPiece ) ),
					new MachineTreeDef( MachineComponentType.XAxis,
						new MachineTreeDef( MachineComponentType.YAxis,
							new MachineTreeDef( MachineComponentType.ZAxis,
								new MachineTreeDef( MachineComponentType.Master,
									new MachineTreeDef( MachineComponentType.Laser, new MachineTreeDef( MachineComponentType.Laser ) ) ) ) ) )
				);

			public static readonly MachineTreeDef DefaultTableTreeDef =
				new MachineTreeDef( MachineComponentType.Base,
					new MachineTreeDef( MachineComponentType.Master,
						new MachineTreeDef( MachineComponentType.Slave,
							new MachineTreeDef( MachineComponentType.WorkPiece ) ) ),
					new MachineTreeDef( MachineComponentType.YAxis,
						new MachineTreeDef( MachineComponentType.XAxis,
							new MachineTreeDef( MachineComponentType.ZAxis,
								new MachineTreeDef( MachineComponentType.Laser ) ) ) )
				);

			public static readonly MachineTreeDef DefaultSpindleTreeDef =
				new MachineTreeDef( MachineComponentType.Base,
					new MachineTreeDef( MachineComponentType.XAxis,
						new MachineTreeDef( MachineComponentType.YAxis,
							new MachineTreeDef( MachineComponentType.ZAxis,
								new MachineTreeDef( MachineComponentType.Master,
									new MachineTreeDef( MachineComponentType.Slave,
										new MachineTreeDef( MachineComponentType.Laser ) ) ) ) ) ),
					new MachineTreeDef( MachineComponentType.WorkPiece )
				);
		}

		public class SpeedData
		{
			public readonly struct TickInfo
			{
				public int Interval
				{
					get;
				}

				public int FrameIncrease
				{
					get;
				}

				public TickInfo( int interval, int frameIncrease )
				{
					Interval = interval;
					FrameIncrease = frameIncrease;
				}
			}

			public static readonly List<TickInfo> SpeedRateSheet = new List<TickInfo>
			{
				new TickInfo(100, 1),
				new TickInfo(50, 1),
				new TickInfo(30, 1),
				new TickInfo(10, 1),
				new TickInfo(10, 2),
				new TickInfo(10, 5),
			};
		}

		public class SimuRequiredData
		{
			internal struct SimuInputSet
			{
				public List<PostData> EachPathIKPostDataList;
				public List<int> GroupStartPathIndices;

				// each group's last path end point is for per-group exit path calculation
				public List<IProcessPoint> GroupLastPathLastPntList;
				public EntryAndExitData EntryAndExitData;
				public PostSolver PostSolver;
				public FiveAxisMachineData MachineData;
				public CollisionSolver CollisionEngine;

				// it will be used before collision calculation, use AABB to filter out some impossible collision pairs
				public Dictionary<string, (double[] Min, double[] Max)> AABBMap;

				public SimuInputSet( List<PostData> eachPathIKPostDataList, List<int> groupStartPathIndices, List<IProcessPoint> groupLastPathLastPntList, EntryAndExitData entryAndExitData, PostSolver postSolver, FiveAxisMachineData machineData, CollisionSolver fCLTest )
				{
					EachPathIKPostDataList = eachPathIKPostDataList;
					GroupStartPathIndices = groupStartPathIndices;
					GroupLastPathLastPntList = groupLastPathLastPntList;
					EntryAndExitData = entryAndExitData;
					PostSolver = postSolver;
					MachineData = machineData;
					CollisionEngine = fCLTest;
					AABBMap = null;
				}
			}
		}

		public class ResultData
		{
			public readonly struct PathStartEndIndex
			{
				public int StartIndex
				{
					get;
				}

				public int EndIndex
				{
					get;
				}

				public PathStartEndIndex( int startIndex, int endIndex )
				{
					StartIndex = startIndex;
					EndIndex = endIndex;
				}
			}

			public struct SimuCalResult
			{
				public Dictionary<MachineComponentType, List<gp_Trsf>> FrameTrasfMap;

				public Dictionary<MachineComponentType, List<bool>> FrameCollisionMap;

				public int FrameCount;

				public List<PathStartEndIndex> PathStartEndIdxList;

				public SimuCalResult( Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap, Dictionary<MachineComponentType, List<bool>> frameCollisionMap, int totalFrameCount, List<PathStartEndIndex> pathStartEndIndexList )
				{
					FrameTrasfMap = frameTransformMap;
					FrameCollisionMap = frameCollisionMap;
					FrameCount = totalFrameCount;
					PathStartEndIdxList = pathStartEndIndexList;
				}

			}
		}
	}
}
