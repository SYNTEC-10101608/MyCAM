using MyCAM.Data;
using OCC.BRep;
using OCC.BRepTools;
using OCC.gp;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace MyCAM.FileManager
{
	// use this class to serialize/deserialize DataManager
	[Serializable]
	[XmlRoot( "DataManagerDTO" )]
	public class DataManagerDTO
	{
		#region XML structure

		[XmlArray( "ObjectMapDTO" )]
		[XmlArrayItem( "PartObjectDTO", typeof( PartObjectDTO ) )]
		[XmlArrayItem( "PathObjectDTO", typeof( PathObjectDTO ) )]
		public List<IObjectDTO> ObjectDataMap
		{
			get;
			private set;
		} = new List<IObjectDTO>();

		[XmlArray( "PartIDListDTO" )]
		[XmlArrayItem( "PartID" )]
		public List<string> PartIDList
		{
			get;
			private set;
		} = new List<string>();

		[XmlArray( "PathIDListDTO" )]
		[XmlArrayItem( "PathID" )]
		public List<string> PathIDList
		{
			get;
			private set;
		} = new List<string>();

		public ShapeIDsDTO ShapeIDList
		{
			get;
			set;
		} = new ShapeIDsDTO();

		public EntryAndExitDataDTO EntryAndExitData
		{
			get;
			set;
		} = new EntryAndExitDataDTO();

		#endregion

		// constructor for XmlSerializer
		internal DataManagerDTO()
		{
		}

		internal DataManagerDTO( DataManager dataManager )
		{
			if( dataManager == null ) {
				return;
			}
			ToObjectDataDTO( dataManager.ObjectMap );
			ToPartIDListDTO( dataManager.PartIDList );
			ToPathIDListDTO( dataManager.PathIDList );
			ToShapeIDDTO( dataManager.GetShapeIDsForDTO() );
			ToEntryAndExitDataDTO( dataManager.EntryAndExitData );
		}

		internal void DataMgrDTO2Data( out Dictionary<string, IObject> objectDataMap, out List<string> partIDList, out List<string> pathIDList, out ShapeIDsStruct shapeIDs, out EntryAndExitData entryAndExitData )
		{
			objectDataMap = ObjectMapDTOToObjectMap();
			partIDList = PartIDListDTOToPartList();
			pathIDList = PathIDListDTOToPathList();
			shapeIDs = ShapeIDDTOToShapeIDStruct();
			entryAndExitData = EntryAndExitDTOToEntryAndExitData();
		}

		#region Generate DTO

		// ObjectDataMap → ObjectDataMapDTO
		void ToObjectDataDTO( Dictionary<string, IObject> objectDataMap )
		{
			ObjectDataMap = new List<IObjectDTO>();
			if( objectDataMap == null ) {
				return;
			}
			foreach( var objectData in objectDataMap ) {

				// current index is path data
				if( objectData.Value is PathObject path ) {
					if( path.PathType == PathType.Contour ) {
						ContourPathObjectDTO pathDataDTO = new ContourPathObjectDTO( path );
						ObjectDataMap.Add( pathDataDTO );
					}
					continue;
				}

				// is part data
				if( objectData.Value is PartObject part ) {
					PartObjectDTO partDataDTO = new PartObjectDTO( part );
					ObjectDataMap.Add( partDataDTO );
					continue;
				}
			}
		}

		// PartIDList → PartIDListDTO
		void ToPartIDListDTO( List<string> partIDList )
		{
			PartIDList = new List<string>();
			if( partIDList != null ) {
				PartIDList.AddRange( partIDList );
			}
		}

		// PathIDList → PathIDListDTO
		void ToPathIDListDTO( List<string> pathIDList )
		{
			PathIDList = new List<string>();
			if( pathIDList != null ) {
				PathIDList.AddRange( pathIDList );
			}
		}

		void ToShapeIDDTO( ShapeIDsStruct shapeIDs )
		{
			ShapeIDList = new ShapeIDsDTO( shapeIDs );
		}

		void ToEntryAndExitDataDTO( EntryAndExitData entryAndExitData )
		{
			EntryAndExitData = new EntryAndExitDataDTO( entryAndExitData );
		}

		#endregion

		#region Generate Data by DTO

		Dictionary<string, IObject> ObjectMapDTOToObjectMap()
		{
			if( ObjectDataMap == null || ObjectDataMap.Count == 0 ) {
				throw new ArgumentException( "ObjectMap deserialization failed." );
			}
			Dictionary<string, IObject> objectMap = new Dictionary<string, IObject>();
			foreach( var entry in ObjectDataMap ) {

				// need to identify pathDataDTO first, because partdata include pathdata
				if( entry is PathObjectDTO pathDataDTO ) {
					if( pathDataDTO is ContourPathObjectDTO contourPathObject ) {
						objectMap.Add( pathDataDTO.UID, contourPathObject.PathDTOToContourPathObject() );
					}
					continue;
				}
				if( entry is PartObjectDTO partDataDTO ) {
					PartObject partObject = partDataDTO.PartDTOToPartData();
					objectMap.Add( partDataDTO.UID, partObject );
					continue;
				}
			}
			return objectMap;
		}

		List<string> PartIDListDTOToPartList()
		{
			List<string> partIDList = new List<string>();
			if( PartIDList == null || PartIDList.Count == 0 ) {
				throw new ArgumentException( "PartIDList deserialization failed." );
			}
			foreach( var partID in PartIDList ) {
				partIDList.Add( partID );
			}
			return partIDList;
		}

		List<string> PathIDListDTOToPathList()
		{
			List<string> pathIDList = new List<string>();
			if( PathIDList == null ) {
				throw new ArgumentNullException( "PathIDList deserialization failed." );
			}
			foreach( var pathID in PathIDList ) {
				pathIDList.Add( pathID );
			}
			return pathIDList;
		}

		ShapeIDsStruct ShapeIDDTOToShapeIDStruct()
		{
			return ShapeIDList.ToShapeIDStruct();
		}

		EntryAndExitData EntryAndExitDTOToEntryAndExitData()
		{
			return EntryAndExitData.ToEntryAndExitData();
		}

		#endregion
	}

	[XmlInclude( typeof( PartObjectDTO ) )]
	[XmlInclude( typeof( PathObjectDTO ) )]
	public abstract class IObjectDTO
	{
		// properties
		public string UID
		{
			get; set;
		} = string.Empty;

		public TopoShapeDTO Shape
		{
			get; set;
		} = new TopoShapeDTO();

		public ObjectType ObjectType
		{
			get; set;
		} = ObjectType.Part;

		// constructor for XmlSerializer
		internal IObjectDTO()
		{
		}
	}

	public class PartObjectDTO : IObjectDTO
	{
		// constructor for XmlSerializer
		internal PartObjectDTO()
		{
		}

		// PartData → DTO
		internal PartObjectDTO( PartObject partData )
		{
			if( partData == null ) {
				return;
			}
			UID = partData.UID;
			Shape = new TopoShapeDTO( partData.Shape );
			ObjectType = ObjectType.Part;
		}

		// DTO → PartData
		internal PartObject PartDTOToPartData()
		{
			// protection
			if( Shape == null || string.IsNullOrEmpty( UID ) ) {
				throw new ArgumentNullException( "PartData deserialization failed." );
			}
			TopoDS_Shape topoShape = TopoShapeDTO.BRepStringToShape( Shape.TopoShapeBRepData );
			return new PartObject( UID, topoShape );
		}
	}

	[XmlInclude( typeof( ContourPathObjectDTO ) )]
	public abstract class PathObjectDTO : IObjectDTO
	{
		// constructor for XmlSerializer
		internal PathObjectDTO()
		{
		}

		public CraftDataDTO CraftData
		{
			get;
			set;
		} = new CraftDataDTO();

		public PathType PathType
		{
			get;
			set;
		} = PathType.Contour;
	}

	public class ContourPathObjectDTO : PathObjectDTO
	{
		[XmlArray( "CADPointListDTO" )]
		[XmlArrayItem( "CADPointDTO" )]
		public List<CADPointDTO> CADPointList
		{
			get;
			set;
		} = new List<CADPointDTO>();

		internal ContourPathObjectDTO()
		{
		}

		internal ContourPathObjectDTO( PathObject pathObject )
		{
			//if( pathObject == null ) {
			//	return;
			//}
			//UID = pathObject.UID;
			//Shape = new TopoShapeDTO( pathObject.Shape );
			//ObjectType = ObjectType.Path;
			//PathType = PathType.Contour;
			//if( pathObject is ContourPathObject ) {
			//	foreach( var point in ( (ContourPathObject)pathObject ).CADPointList ) {
			//		CADPointList.Add( new CADPointDTO( point ) );
			//	}
			//}
			//else {
			//	CADPointList = new List<CADPointDTO>();
			//}
			//CraftData = new CraftDataDTO( pathObject.CraftData );
		}

		// DTO → ContourPathObject
		internal ContourPathObject PathDTOToContourPathObject()
		{
			// protection
			//if( Shape == null || string.IsNullOrEmpty( UID ) ) {
			//	throw new ArgumentNullException( "PathObject deserialization failed." );
			//}
			//TopoDS_Shape shape = TopoShapeDTO.BRepStringToShape( Shape.TopoShapeBRepData );
			//CraftData craftData = CraftData.ToCraftData();
			//List<CADPoint> cadPointList = CADPointList.Select( cadPointDTO => cadPointDTO.ToCADPoint() ).ToList();
			//return new ContourPathObject( UID, shape, cadPointList, craftData );
			return null;
		}
	}

	public class CraftDataDTO
	{
		// properties
		public string UID
		{
			get;
			set;
		} = string.Empty;

		public bool IsReverse
		{
			get;
			set;
		}

		public bool IsToolVecReverse
		{
			get;
			set;
		}

		public int StartPoint
		{
			get;
			set;
		}

		public TraverseDataDTO TraverseData
		{
			get;
			set;
		} = new TraverseDataDTO();

		public LeadParamDTO LeadParam
		{
			get;
			set;
		} = new LeadParamDTO();

		public double OverCutLength
		{
			get;
			set;
		}

		public List<ToolVecMapDTO> ToolVecModifyMap
		{
			get;
			set;
		} = new List<ToolVecMapDTO>();

		internal CraftDataDTO()
		{
		}

		internal CraftDataDTO( CraftData craftData )
		{
			if( craftData == null ) {
				return;
			}
			UID = craftData.UID;
			IsReverse = craftData.IsReverse;
			IsToolVecReverse = craftData.IsToolVecReverse;
			StartPoint = craftData.StartPointIndex;
			LeadParam = craftData.LeadLineParam != null
						? new LeadParamDTO( craftData.LeadLineParam )
						: new LeadParamDTO();
			TraverseData = craftData.TraverseData != null
							? new TraverseDataDTO( craftData.TraverseData )
							: new TraverseDataDTO();
			OverCutLength = craftData.OverCutLength;
			ToolVecModifyMap = ( craftData.ToolVecModifyMap ?? new Dictionary<int, Tuple<double, double>>() )
				.Select( kvp => new ToolVecMapDTO( kvp.Key, kvp.Value.Item1, kvp.Value.Item2 ) )
				.ToList();
		}

		internal CraftData ToCraftData()
		{
			if( ToolVecModifyMap == null || LeadParam == null || TraverseData == null ) {
				throw new ArgumentException( "ContourCacheInfo deserialization failed." );
			}
			Dictionary<int, Tuple<double, double>> toolVecModifyMap = ToolVecModifyMap.ToDictionary( ToolVecModifyData => ToolVecModifyData.Index, ToolVecModifyData => Tuple.Create( ToolVecModifyData.Value1, ToolVecModifyData.Value2 ) );
			LeadData leadParam = LeadParam.ToLeadData();
			TraverseData traverseData = TraverseData.ToTraverseData();
			return new CraftData( UID, StartPoint, IsReverse, leadParam, OverCutLength, toolVecModifyMap, IsToolVecReverse, traverseData );
		}
	}

	public class TraverseDataDTO
	{
		public double LiftUpDistance
		{
			get;
			set;
		}

		public double CutDownDistance
		{
			get;
			set;
		}

		public double FollowSafeDistance
		{
			get;
			set;
		}

		public double FrogLeapDistance
		{
			get; set;
		}

		// parameterless constructor (for XmlSerializer)
		internal TraverseDataDTO()
		{
		}

		internal TraverseDataDTO( TraverseData traverseData )
		{
			if( traverseData == null ) {
				return;
			}
			LiftUpDistance = traverseData.LiftUpDistance;
			CutDownDistance = traverseData.CutDownDistance;
			FollowSafeDistance = traverseData.FollowSafeDistance;
			FrogLeapDistance = traverseData.FrogLeapDistance;
		}

		internal TraverseData ToTraverseData()
		{
			return new TraverseData( LiftUpDistance, CutDownDistance, FollowSafeDistance, FrogLeapDistance );
		}
	}

	public class EntryAndExitDataDTO
	{
		public double EntryDistance
		{
			get;
			set;
		}

		public double ExitDistance
		{
			get;
			set;
		}

		public double FollowSafeDistance
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal EntryAndExitDataDTO()
		{
		}

		internal EntryAndExitDataDTO( EntryAndExitData entryAndExitData )
		{
			if( entryAndExitData == null ) {
				return;
			}
			EntryDistance = entryAndExitData.EntryDistance;
			ExitDistance = entryAndExitData.ExitDistance;
			FollowSafeDistance = entryAndExitData.FollowSafeDistance;
		}

		internal EntryAndExitData ToEntryAndExitData()
		{
			return new EntryAndExitData( EntryDistance, ExitDistance, FollowSafeDistance );
		}
	}

	public class LeadParamDTO
	{
		public int LeadInType
		{
			get;
			set;
		}

		public int LeadOutType
		{
			get;
			set;
		}

		public double LeadInLength
		{
			get;
			set;
		}

		public double LeadOutLength
		{
			get;
			set;
		}

		public double LeadInAngle
		{
			get;
			set;
		}

		public double LeadOutAngle
		{
			get;
			set;
		}

		public bool IsChangeLeadDirection
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal LeadParamDTO()
		{
		}

		// constructor from LeadData
		internal LeadParamDTO( LeadData leadParam )
		{
			if( leadParam == null ) {
				return;
			}
			LeadInType = (int)leadParam.LeadIn.Type;
			LeadOutType = (int)leadParam.LeadOut.Type;
			LeadInLength = leadParam.LeadIn.Length;
			LeadOutLength = leadParam.LeadOut.Length;
			LeadInAngle = leadParam.LeadIn.Angle;
			LeadOutAngle = leadParam.LeadOut.Angle;
			IsChangeLeadDirection = leadParam.IsChangeLeadDirection;
		}

		internal LeadData ToLeadData()
		{
			LeadLineType leadInType;
			if( Enum.IsDefined( typeof( LeadLineType ), LeadInType ) ) {
				leadInType = (LeadLineType)LeadInType;
			}
			else {
				throw new ArgumentException( "LeadData deserialization failed." );
			}
			LeadLineType leadOutType;
			if( Enum.IsDefined( typeof( LeadLineType ), LeadOutType ) ) {
				leadOutType = (LeadLineType)LeadOutType;
			}
			else {
				throw new ArgumentException( "LeadData deserialization failed." );
			}
			return new LeadData( leadInType, leadOutType, LeadInLength, LeadInAngle, LeadOutLength, LeadOutAngle, IsChangeLeadDirection );
		}
	}

	public class CADPointDTO
	{
		// properties
		public double X
		{
			get;
			set;
		}

		public double Y
		{
			get;
			set;
		}

		public double Z
		{
			get;
			set;
		}

		public double Normal1_X
		{
			get;
			set;
		}

		public double Normal1_Y
		{
			get;
			set;
		}

		public double Normal1_Z
		{
			get;
			set;
		}

		public double Normal2_X
		{
			get;
			set;
		}

		public double Normal2_Y
		{
			get;
			set;
		}

		public double Normal2_Z
		{
			get;
			set;
		}

		public double Tangent_X
		{
			get;
			set;
		}

		public double Tangent_Y
		{
			get;
			set;
		}

		public double Tangent_Z
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal CADPointDTO()
		{
		}

		// CADPoint → DTO
		internal CADPointDTO( CADPoint cadPoint )
		{
			if( cadPoint == null ) {
				return;
			}
			X = cadPoint.Point.X();
			Y = cadPoint.Point.Y();
			Z = cadPoint.Point.Z();
			Normal1_X = cadPoint.NormalVec_1st.X();
			Normal1_Y = cadPoint.NormalVec_1st.Y();
			Normal1_Z = cadPoint.NormalVec_1st.Z();
			Normal2_X = cadPoint.NormalVec_2nd.X();
			Normal2_Y = cadPoint.NormalVec_2nd.Y();
			Normal2_Z = cadPoint.NormalVec_2nd.Z();
			Tangent_X = cadPoint.TangentVec.X();
			Tangent_Y = cadPoint.TangentVec.Y();
			Tangent_Z = cadPoint.TangentVec.Z();
		}

		// DTO → CADPoint
		internal CADPoint ToCADPoint()
		{
			gp_Pnt point = new gp_Pnt( X, Y, Z );
			gp_Dir n1 = new gp_Dir( Normal1_X, Normal1_Y, Normal1_Z );
			gp_Dir n2 = new gp_Dir( Normal2_X, Normal2_Y, Normal2_Z );
			gp_Dir t = new gp_Dir( Tangent_X, Tangent_Y, Tangent_Z );
			return new CADPoint( point, n1, n2, t );
		}
	}

	public class TopoShapeDTO
	{
		public string TopoShapeBRepData
		{
			get;
			set;
		} = string.Empty;

		// parameterless constructor (for XmlSerializer, required)
		internal TopoShapeDTO()
		{
		}

		internal TopoShapeDTO( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				TopoShapeBRepData = string.Empty;
				return;
			}
			TopoShapeBRepData = ShapeToBRepString( shape );
		}

		// shape → string
		internal static string ShapeToBRepString( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				return string.Empty;
			}
			string tempFile = Path.GetTempFileName();
			try {
				BRepTools.Write( shape, tempFile );

				// read the BRep text data from the file
				string brepText = File.ReadAllText( tempFile );

				// encode the BRep text into Base64 string
				return Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( brepText ) );
			}
			catch( Exception ) {
				return string.Empty;
			}
			finally {

				//  clean up the temporary file
				if( File.Exists( tempFile ) ) {
					File.Delete( tempFile );
				}
			}
		}

		// string → shape
		internal static TopoDS_Shape BRepStringToShape( string brepString )
		{
			if( string.IsNullOrEmpty( brepString ) ) {
				throw new ArgumentNullException( "TopoShape deserialization failed." );
			}
			string tempFile = Path.GetTempFileName();
			try {
				// decode the Base64 string into plain BRep text (UTF-8 encoded)
				string brepText = System.Text.Encoding.UTF8.GetString( Convert.FromBase64String( brepString ) );
				File.WriteAllText( tempFile, brepText );

				// create an empty TopoDS_Shape object and a BRep_Builder
				TopoDS_Shape shape = new TopoDS_Shape();
				BRep_Builder builder = new BRep_Builder();

				// read the BRep data from the temporary file into the TopoDS_Shape
				BRepTools.Read( ref shape, tempFile, builder );
				if( shape == null || shape.IsNull() ) {
					throw new ArgumentNullException( "TopoShape deserialization failed." );
				}
				return shape;
			}
			catch( Exception ex ) {
				throw new FormatException( "TopoShape deserialization failed", ex );
			}
			finally {
				// clean up the temporary file
				if( File.Exists( tempFile ) )
					File.Delete( tempFile );
			}
		}
	}

	public class ToolVecMapDTO
	{
		public int Index
		{
			get;
			set;
		}

		public double Value1
		{
			get;
			set;
		}

		public double Value2
		{
			get;
			set;
		}

		// parameterless constructor(for XmlSerializer)
		internal ToolVecMapDTO()
		{
		}

		internal ToolVecMapDTO( int index, double value1, double value2 )
		{
			Index = index;
			Value1 = value1;
			Value2 = value2;
		}
	}

	public class ShapeIDsDTO
	{
		public int SolidID
		{
			get; set;
		}

		public int ShellID
		{
			get; set;
		}

		public int FaceID
		{
			get; set;
		}

		public int WireID
		{
			get; set;
		}

		public int EdgeID
		{
			get; set;
		}

		public int VertexID
		{
			get; set;
		}

		public int PathID
		{
			get; set;
		}

		internal ShapeIDsDTO()
		{
		}

		internal ShapeIDsDTO( ShapeIDsStruct shapeIDsStruct )
		{
			SolidID = shapeIDsStruct.Solid_ID;
			ShellID = shapeIDsStruct.Shell_ID;
			FaceID = shapeIDsStruct.Face_ID;
			WireID = shapeIDsStruct.Wire_ID;
			EdgeID = shapeIDsStruct.Edge_ID;
			VertexID = shapeIDsStruct.Vertex_ID;
			PathID = shapeIDsStruct.Path_ID;
		}

		internal ShapeIDsStruct ToShapeIDStruct()
		{
			return new ShapeIDsStruct()
			{
				Solid_ID = SolidID,
				Shell_ID = ShellID,
				Face_ID = FaceID,
				Wire_ID = WireID,
				Edge_ID = EdgeID,
				Vertex_ID = VertexID,
				Path_ID = PathID,
			};
		}
	}
}
