using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using MyCAM.Data;
using OCC.BRep;
using OCC.BRepTools;
using OCC.gp;
using OCC.TopoDS;
using static MyCAM.Data.DataManager;
using static MyCAM.Data.LeadType;

namespace MyCAM.FileManager
{
	// use this class to serialize/deserialize DataManager
	[Serializable]
	[XmlRoot( "DataManagerDTO" )]
	public class DataManagerDTO
	{
		#region XML structure

		[XmlArray( "ShapeDataMapDTO" )]
		[XmlArrayItem( "PartDataDTO", typeof( PartDataDTO ) )]
		[XmlArrayItem( "PathDataDTO", typeof( PathDataDTO ) )]
		public List<PartDataDTO> ShapeDataMap
		{
			get;
			private set;
		} = new List<PartDataDTO>();

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

		public TraverseDataDTO TraverseData
		{
			get;
			set;
		} = new TraverseDataDTO();

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
			ToShapeDataDTO( dataManager.ShapeDataMap );
			ToPartIDListDTO( dataManager.PartIDList );
			ToPathIDListDTO( dataManager.PathIDList );
			ToShapeIDDTO( dataManager.GetShapeIDsForDTO() );
			ToTraverseDataDTO( dataManager.TraverseData );
		}

		internal void DataMgrDTO2Data( out Dictionary<string, ShapeData> shapeMap, out List<string> partIDList, out List<string> pathIDList, out ShapeIDsStruct shapeIDs, out TraverseData traverseData )
		{
			shapeMap = ShapeMapDTOToShapeMap();
			partIDList = PartIDListDTOToPartList();
			pathIDList = PathIDListDTOToPathList();
			shapeIDs = ShapeIDDTOToShapeIDStruct();
			traverseData = TraverseDataDTOToTraverseData();
		}

		#region Generate DTO

		// ShapeDataMap → ShapeDataMapDTO
		void ToShapeDataDTO( Dictionary<string, ShapeData> shapeDataMap )
		{
			ShapeDataMap = new List<PartDataDTO>();
			if( shapeDataMap == null ) {
				return;
			}
			foreach( var shapeData in shapeDataMap ) {

				// current index is path data
				if( shapeData.Value is PathData path ) {
					PathDataDTO pathDataDTO = new PathDataDTO( path );
					ShapeDataMap.Add( pathDataDTO );
					continue;
				}

				// is part data
				if( shapeData.Value is ShapeData ) {
					PartDataDTO partDataDTO = new PartDataDTO( shapeData.Value );
					ShapeDataMap.Add( partDataDTO );
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

		void ToTraverseDataDTO( TraverseData traverseData )
		{
			TraverseData = new TraverseDataDTO( traverseData );
		}

		#endregion

		#region Generate Data by DTO

		Dictionary<string, ShapeData> ShapeMapDTOToShapeMap()
		{
			if( ShapeDataMap == null || ShapeDataMap.Count == 0 ) {
				throw new ArgumentException( "ShapeDataMap deserialization failed." );
			}
			Dictionary<string, ShapeData> shapeMap = new Dictionary<string, ShapeData>();
			foreach( var entry in ShapeDataMap ) {

				// need to identify pathDataDTO first, because partdata include pathdata
				if( entry is PathDataDTO pathDataDTO ) {
					PathData pathData = pathDataDTO.PathDTOToPathData();
					shapeMap.Add( pathDataDTO.UID, pathData );
					continue;
				}
				if( entry is PartDataDTO partDataDTO ) {
					ShapeData shapeData = partDataDTO.PartDTOToPartData();
					shapeMap.Add( partDataDTO.UID, shapeData );
					continue;
				}
			}
			return shapeMap;
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

		TraverseData TraverseDataDTOToTraverseData()
		{
			return TraverseData.ToTraverseData();
		}

		#endregion
	}

	public class PartDataDTO
	{
		// properties
		public string UID
		{
			get; set;
		} = string.Empty;

		public TopoShapeDTO Shape
		{
			get;
			set;
		} = new TopoShapeDTO();

		// constructor for XmlSerializer
		internal PartDataDTO()
		{
		}

		// PartData → DTO
		internal PartDataDTO( ShapeData shapeData )
		{
			if( shapeData == null ) {
				return;
			}
			UID = shapeData.UID;
			Shape = new TopoShapeDTO( shapeData.Shape );
		}

		// DTO → PartData
		internal ShapeData PartDTOToPartData()
		{
			// protection
			if( Shape == null || string.IsNullOrEmpty( UID ) ) {
				throw new ArgumentNullException( "PartData deserialization failed." );
			}
			TopoDS_Shape topoShape = TopoShapeDTO.BRepStringToShape( Shape.TopoShapeBRepData );
			return new ShapeData( UID, topoShape );
		}
	}

	public class PathDataDTO : PartDataDTO
	{
		public CAMDataDTO CAMData
		{
			get;
			set;
		} = new CAMDataDTO();

		// constructor for XmlSerializer
		internal PathDataDTO()
		{
		}

		// PathData → DTO
		internal PathDataDTO( PathData pathData )
		{
			if( pathData == null ) {
				return;
			}
			UID = pathData.UID;
			Shape = new TopoShapeDTO( pathData.Shape );
			CAMData = new CAMDataDTO( pathData.CAMData );
		}

		// DTO → PathData
		internal PathData PathDTOToPathData()
		{
			// protection
			if( Shape == null || string.IsNullOrEmpty( UID ) ) {
				throw new ArgumentNullException( "PathData deserialization failed." );
			}
			TopoDS_Shape shape = TopoShapeDTO.BRepStringToShape( Shape.TopoShapeBRepData );
			CAMData camData = CAMData.ToCAMData();
			return new PathData( UID, shape, camData );
		}
	}

	public class CAMDataDTO
	{
		// properties
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

		public bool IsClosed
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

		public List<CADPointDTO> CADPointList
		{
			get;
			set;
		} = new List<CADPointDTO>();

		internal CAMDataDTO()
		{
		}

		internal CAMDataDTO( CAMData camData )
		{
			if( camData == null ) {
				return;
			}
			IsReverse = camData.IsReverse;
			IsToolVecReverse = camData.IsToolVecReverse;
			StartPoint = camData.StartPoint;
			IsClosed = camData.IsClosed;
			LeadParam = camData.LeadLineParam != null
						? new LeadParamDTO( camData.LeadLineParam )
						: new LeadParamDTO();
			TraverseData = camData.TraverseData != null
							? new TraverseDataDTO( camData.TraverseData )
							: new TraverseDataDTO();
			OverCutLength = camData.OverCutLength;
			ToolVecModifyMap = ( camData.ToolVecModifyMap ?? new Dictionary<int, Tuple<double, double>>() )
				.Select( kvp => new ToolVecMapDTO( kvp.Key, kvp.Value.Item1, kvp.Value.Item2 ) )
				.ToList();
			CADPointList = ( camData.CADPointList ?? new List<CADPoint>() )
				.Select( point => new CADPointDTO( point ) )
				.ToList();
		}

		internal CAMData ToCAMData()
		{
			if( ToolVecModifyMap == null || CADPointList == null || CADPointList.Count == 0 || LeadParam == null || TraverseData == null ) {
				throw new ArgumentException( "CAMData deserialization failed." );
			}
			Dictionary<int, Tuple<double, double>> toolVecModifyMap = ToolVecModifyMap.ToDictionary( ToolVecModifyData => ToolVecModifyData.Index, ToolVecModifyData => Tuple.Create( ToolVecModifyData.Value1, ToolVecModifyData.Value2 ) );
			List<CADPoint> cadPoints = CADPointList.Select( dto => dto.ToCADPoint() ).ToList();
			LeadData leadParam = LeadParam.ToLeadData();
			TraverseData traverseData = TraverseData.ToTraverseData();
			return new CAMData( cadPoints, IsClosed, StartPoint, leadParam, IsReverse, IsToolVecReverse, OverCutLength, toolVecModifyMap, traverseData );
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
		}

		internal TraverseData ToTraverseData()
		{
			return new TraverseData( LiftUpDistance, CutDownDistance, FollowSafeDistance );
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
				if( File.Exists( tempFile ) ) File.Delete( tempFile );
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
				if( File.Exists( tempFile ) ) File.Delete( tempFile );
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
