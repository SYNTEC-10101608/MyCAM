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
		[XmlArrayItem( "ContourPathObjectDTO", typeof( ContourPathObjectDTO ) )]
		[XmlArrayItem( "CirclePathObjectDTO", typeof( CirclePathObjectDTO ) )]
		[XmlArrayItem( "RectanglePathObjectDTO", typeof( RectanglePathObjectDTO ) )]
		[XmlArrayItem( "PolygonPathObjectDTO", typeof( PolygonPathObjectDTO ) )]
		[XmlArrayItem( "RunwayPathObjectDTO", typeof( RunwayPathObjectDTO ) )]
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
		}

		public EntryAndExitDataDTO EntryAndExitData
		{
			get;
			set;
		} = new EntryAndExitDataDTO();

		public CalibrationDataDTO PartCalibrationData
		{
			get;
			set;
		} = new CalibrationDataDTO();

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
			ToPartCalibrationDataDTO( dataManager.CalibrationData );
		}

		internal void DataMgrDTO2Data( out Dictionary<string, IObject> objectDataMap, out List<string> partIDList, out List<string> pathIDList, out ShapeIDsStruct shapeIDs, out EntryAndExitData entryAndExitData, out CalibrationData calibrationData )
		{
			objectDataMap = ObjectMapDTOToObjectMap();
			partIDList = PartIDListDTOToPartList();
			pathIDList = PathIDListDTOToPathList();
			shapeIDs = ShapeIDDTOToShapeIDStruct();
			entryAndExitData = EntryAndExitDTOToEntryAndExitData();
			calibrationData = PartCalibrationDTOToCalibrationData();
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
					switch( path.PathType ) {
						case PathType.Contour:
							if( path is ContourPathObject contourPath ) {
								ContourPathObjectDTO pathDataDTO = new ContourPathObjectDTO( contourPath );
								ObjectDataMap.Add( pathDataDTO );
							}
							break;
						case PathType.Circle:
							if( path is CirclePathObject circlePath ) {
								CirclePathObjectDTO circleDTO = new CirclePathObjectDTO( circlePath );
								ObjectDataMap.Add( circleDTO );
							}
							break;
						case PathType.Rectangle:
							if( path is RectanglePathObject rectanglePath ) {
								RectanglePathObjectDTO rectangleDTO = new RectanglePathObjectDTO( rectanglePath );
								ObjectDataMap.Add( rectangleDTO );
							}
							break;
						case PathType.Triangle:
						case PathType.Square:
						case PathType.Pentagon:
						case PathType.Hexagon:
							if( path is PolygonPathObject polygonPath ) {
								PolygonPathObjectDTO polygonDTO = new PolygonPathObjectDTO( polygonPath );
								ObjectDataMap.Add( polygonDTO );
							}
							break;
						case PathType.Runway:
							if( path is RunwayPathObject runwayPath ) {
								RunwayPathObjectDTO runwayDTO = new RunwayPathObjectDTO( runwayPath );
								ObjectDataMap.Add( runwayDTO );
							}
							break;
					}
					continue;
				}

				// is part data
				if( objectData.Value is PartObject part ) {
					PartObjectDTO partDataDTO = new PartObjectDTO( part );
					ObjectDataMap.Add( partDataDTO );
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

		void ToPartCalibrationDataDTO( CalibrationData calibrationData )
		{
			PartCalibrationData = new CalibrationDataDTO( calibrationData );
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
					else if( pathDataDTO is CirclePathObjectDTO circlePathObject ) {
						objectMap.Add( pathDataDTO.UID, circlePathObject.PathDTOToCirclePathObject() );
					}
					else if( pathDataDTO is RectanglePathObjectDTO rectanglePathObject ) {
						objectMap.Add( pathDataDTO.UID, rectanglePathObject.PathDTOToRectanglePathObject() );
					}
					else if( pathDataDTO is PolygonPathObjectDTO polygonPathObject ) {
						objectMap.Add( pathDataDTO.UID, polygonPathObject.PathDTOToPolygonPathObject() );
					}
					else if( pathDataDTO is RunwayPathObjectDTO runwayPathObject ) {
						objectMap.Add( pathDataDTO.UID, runwayPathObject.PathDTOToRunwayPathObject() );
					}
					continue;
				}
				if( entry is PartObjectDTO partDataDTO ) {
					PartObject partObject = partDataDTO.PartDTOToPartData();
					objectMap.Add( partDataDTO.UID, partObject );
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
				throw new ArgumentException( "PathIDList deserialization failed." );
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

		CalibrationData PartCalibrationDTOToCalibrationData()
		{
			return PartCalibrationData.ToCalibrationData();
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
		}

		public TopoShapeDTO Shape
		{
			get; set;
		}

		public ObjectType ObjectType
		{
			get; set;
		}

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
	[XmlInclude( typeof( CirclePathObjectDTO ) )]
	[XmlInclude( typeof( RectanglePathObjectDTO ) )]
	[XmlInclude( typeof( PolygonPathObjectDTO ) )]
	[XmlInclude( typeof( RunwayPathObjectDTO ) )]
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
		}

		public PathType PathType
		{
			get;
			set;
		}
	}

	public class ContourPathObjectDTO : PathObjectDTO
	{
		public ContourGeomDataDTO GeomData
		{
			get;
			set;
		}

		internal ContourPathObjectDTO()
		{
		}

		internal ContourPathObjectDTO( PathObject pathObject )
		{
			if( pathObject == null ) {
				return;
			}
			UID = pathObject.UID;
			Shape = new TopoShapeDTO( pathObject.Shape );
			ObjectType = ObjectType.Path;
			PathType = PathType.Contour;
			if( pathObject is ContourPathObject contourPathObject ) {
				GeomData = new ContourGeomDataDTO( (ContourGeomData)contourPathObject.GeomData );
			}
			else {
				GeomData = new ContourGeomDataDTO();
			}
			CraftData = new CraftDataDTO( pathObject.CraftData );
		}

		// DTO → ContourPathObject
		internal ContourPathObject PathDTOToContourPathObject()
		{
			// protection
			if( Shape == null || string.IsNullOrEmpty( UID ) || GeomData == null ) {
				throw new ArgumentNullException( "PathObject deserialization failed." );
			}
			TopoDS_Shape shape = TopoShapeDTO.BRepStringToShape( Shape.TopoShapeBRepData );
			CraftData craftData = CraftData.ToCraftData();
			ContourGeomData geomData = GeomData.ToContourGeomData();
			return new ContourPathObject( UID, shape, geomData, craftData );
		}
	}

	public class CirclePathObjectDTO : PathObjectDTO
	{
		public CircleGeomDataDTO GeomData
		{
			get;
			set;
		}

		public ContourPathObjectDTO ContourPathObject
		{
			get;
			set;
		}

		internal CirclePathObjectDTO()
		{
		}

		internal CirclePathObjectDTO( PathObject pathObject )
		{
			if( pathObject == null ) {
				return;
			}
			UID = pathObject.UID;
			Shape = new TopoShapeDTO( pathObject.Shape );
			ObjectType = ObjectType.Path;
			PathType = PathType.Circle;
			if( pathObject is StdPatternObjectBase standardPatternPathObject ) {
				GeomData = new CircleGeomDataDTO( (CircleGeomData)standardPatternPathObject.GeomData );
				ContourPathObject = new ContourPathObjectDTO( standardPatternPathObject.ContourPathObject );
			}
			else {
				GeomData = new CircleGeomDataDTO();
				ContourPathObject = new ContourPathObjectDTO();
			}
			CraftData = new CraftDataDTO( pathObject.CraftData );
		}

		// DTO → CirclePathObject
		internal CirclePathObject PathDTOToCirclePathObject()
		{
			// protection
			if( Shape == null || string.IsNullOrEmpty( UID ) || GeomData == null || ContourPathObject == null ) {
				throw new ArgumentNullException( "CirclePathObject deserialization failed." );
			}
			TopoDS_Shape shape = TopoShapeDTO.BRepStringToShape( Shape.TopoShapeBRepData );
			CraftData craftData = CraftData.ToCraftData();
			CircleGeomData geomData = GeomData.ToCircleGeomData();
			ContourPathObject contourPathObject = ContourPathObject.PathDTOToContourPathObject();
			return new CirclePathObject( UID, shape, geomData, craftData, contourPathObject );
		}
	}

	public class RectanglePathObjectDTO : PathObjectDTO
	{
		public RectangleGeomDataDTO GeomData
		{
			get;
			set;
		}

		public ContourPathObjectDTO ContourPathObject
		{
			get;
			set;
		}

		internal RectanglePathObjectDTO()
		{
		}

		internal RectanglePathObjectDTO( PathObject pathObject )
		{
			if( pathObject == null ) {
				return;
			}
			UID = pathObject.UID;
			Shape = new TopoShapeDTO( pathObject.Shape );
			ObjectType = ObjectType.Path;
			PathType = PathType.Rectangle;
			if( pathObject is StdPatternObjectBase standardPatternPathObject ) {
				GeomData = new RectangleGeomDataDTO( (RectangleGeomData)standardPatternPathObject.GeomData );
				ContourPathObject = new ContourPathObjectDTO( standardPatternPathObject.ContourPathObject );
			}
			else {
				GeomData = new RectangleGeomDataDTO();
				ContourPathObject = new ContourPathObjectDTO();
			}
			CraftData = new CraftDataDTO( pathObject.CraftData );
		}

		// DTO → RectanglePathObject
		internal RectanglePathObject PathDTOToRectanglePathObject()
		{
			// protection
			if( Shape == null || string.IsNullOrEmpty( UID ) || GeomData == null || ContourPathObject == null ) {
				throw new ArgumentNullException( "RectanglePathObject deserialization failed." );
			}
			TopoDS_Shape shape = TopoShapeDTO.BRepStringToShape( Shape.TopoShapeBRepData );
			CraftData craftData = CraftData.ToCraftData();
			RectangleGeomData geomData = GeomData.ToRectangleGeomData();
			ContourPathObject contourPathObject = ContourPathObject.PathDTOToContourPathObject();
			return new RectanglePathObject( UID, shape, geomData, craftData, contourPathObject );
		}
	}

	public class PolygonPathObjectDTO : PathObjectDTO
	{
		public PolygonGeomDataDTO GeomData
		{
			get;
			set;
		}

		public ContourPathObjectDTO ContourPathObject
		{
			get;
			set;
		}

		internal PolygonPathObjectDTO()
		{
		}

		internal PolygonPathObjectDTO( PathObject pathObject )
		{
			if( pathObject == null ) {
				return;
			}
			UID = pathObject.UID;
			Shape = new TopoShapeDTO( pathObject.Shape );
			ObjectType = ObjectType.Path;
			if( pathObject is StdPatternObjectBase standardPatternPathObject ) {
				PathType = pathObject.PathType; // Use the actual PathType from PathObject (which gets it from PolygonGeomData)
				GeomData = new PolygonGeomDataDTO( (PolygonGeomData)standardPatternPathObject.GeomData );
				ContourPathObject = new ContourPathObjectDTO( standardPatternPathObject.ContourPathObject );
			}
			else {
				PathType = PathType.Triangle; // Default
				GeomData = new PolygonGeomDataDTO();
				ContourPathObject = new ContourPathObjectDTO();
			}
			CraftData = new CraftDataDTO( pathObject.CraftData );
		}

		// DTO → PolygonPathObject
		internal PolygonPathObject PathDTOToPolygonPathObject()
		{
			// protection
			if( Shape == null || string.IsNullOrEmpty( UID ) || GeomData == null || ContourPathObject == null ) {
				throw new ArgumentNullException( "PolygonPathObject deserialization failed." );
			}
			TopoDS_Shape shape = TopoShapeDTO.BRepStringToShape( Shape.TopoShapeBRepData );
			CraftData craftData = CraftData.ToCraftData();
			PolygonGeomData geomData = GeomData.ToPolygonGeomData();
			ContourPathObject contourPathObject = ContourPathObject.PathDTOToContourPathObject();
			return new PolygonPathObject( UID, shape, geomData, craftData, contourPathObject );
		}
	}

	public class RunwayPathObjectDTO : PathObjectDTO
	{
		public RunwayGeomDataDTO GeomData
		{
			get;
			set;
		}

		public ContourPathObjectDTO ContourPathObject
		{
			get;
			set;
		}

		internal RunwayPathObjectDTO()
		{
		}

		internal RunwayPathObjectDTO( PathObject pathObject )
		{
			if( pathObject == null ) {
				return;
			}
			UID = pathObject.UID;
			Shape = new TopoShapeDTO( pathObject.Shape );
			ObjectType = ObjectType.Path;
			PathType = PathType.Runway;
			if( pathObject is StdPatternObjectBase standardPatternPathObject ) {
				GeomData = new RunwayGeomDataDTO( (RunwayGeomData)standardPatternPathObject.GeomData );
				ContourPathObject = new ContourPathObjectDTO( standardPatternPathObject.ContourPathObject );
			}
			else {
				GeomData = new RunwayGeomDataDTO();
				ContourPathObject = new ContourPathObjectDTO();
			}
			CraftData = new CraftDataDTO( pathObject.CraftData );
		}

		// DTO → RunwayPathObject
		internal RunwayPathObject PathDTOToRunwayPathObject()
		{
			// protection
			if( Shape == null || string.IsNullOrEmpty( UID ) || GeomData == null || ContourPathObject == null ) {
				throw new ArgumentNullException( "RunwayPathObject deserialization failed." );
			}
			TopoDS_Shape shape = TopoShapeDTO.BRepStringToShape( Shape.TopoShapeBRepData );
			CraftData craftData = CraftData.ToCraftData();
			RunwayGeomData geomData = GeomData.ToRunwayGeomData();
			ContourPathObject contourPathObject = ContourPathObject.PathDTOToContourPathObject();
			return new RunwayPathObject( UID, shape, geomData, craftData, contourPathObject );
		}
	}

	public class ContourGeomDataDTO
	{
		public bool? IsClosed
		{
			get;
			set;
		}

		[XmlArray( "CADPointListDTO" )]
		[XmlArrayItem( "CADPointDTO" )]
		public List<CADPointDTO> CADPointList
		{
			get;
			set;
		} = new List<CADPointDTO>();

		[XmlArray( "ConnectPointMapDTO" )]
		[XmlArrayItem( "ConnectPointPairDTO" )]
		public List<ConnectPointPairDTO> ConnectPointMap
		{
			get;
			set;
		} = new List<ConnectPointPairDTO>();

		internal ContourGeomDataDTO()
		{
		}

		internal ContourGeomDataDTO( ContourGeomData geomData )
		{
			if( geomData == null ) {
				return;
			}
			IsClosed = geomData.IsClosed;

			// Convert CADPointList
			foreach( var point in geomData.CADPointList ) {
				CADPointList.Add( new CADPointDTO( point ) );
			}

			// Convert ConnectPointMap to list of pairs
			var pointIndexMap = new Dictionary<CADPoint, int>();
			for( int i = 0; i < geomData.CADPointList.Count; i++ ) {
				pointIndexMap[ geomData.CADPointList[ i ] ] = i;
			}
			foreach( var kvp in geomData.ConnectPointMap ) {
				if( pointIndexMap.TryGetValue( kvp.Key, out int keyIndex ) ) {
					ConnectPointMap.Add( new ConnectPointPairDTO( keyIndex, new CADPointDTO( kvp.Value ) ) );
				}
			}
		}

		internal ContourGeomData ToContourGeomData()
		{
			if( CADPointList == null || ConnectPointMap == null || !IsClosed.HasValue ) {
				throw new ArgumentException( "ContourGeomData deserialization failed." );
			}
			if( CADPointList.Count == 0 ) {
				throw new ArgumentException( "ContourGeomData deserialization failed: CADPointList is empty." );
			}
			List<CADPoint> cadPointList = CADPointList.Select( cadPointDTO => cadPointDTO.ToCADPoint() ).ToList();

			// Reconstruct ConnectPointMap
			// Key is from CADPointList by index, Value is separate CADPoint
			Dictionary<CADPoint, CADPoint> connectPointMap = new Dictionary<CADPoint, CADPoint>();
			foreach( var pair in ConnectPointMap ) {
				if( pair.KeyIndex.HasValue && pair.KeyIndex.Value >= 0 && pair.KeyIndex.Value < cadPointList.Count && pair.ValuePoint != null ) {
					CADPoint valuePoint = pair.ValuePoint.ToCADPoint();
					connectPointMap[ cadPointList[ pair.KeyIndex.Value ] ] = valuePoint;
				}
			}

			return new ContourGeomData( cadPointList, connectPointMap, IsClosed.Value );
		}
	}

	public class CircleGeomDataDTO
	{
		public double? Diameter
		{
			get;
			set;
		}

		public double? RotatedAngle_deg
		{
			get;
			set;
		}

		public bool? IsCoordinateReversed
		{
			get;
			set;
		}

		public gp_Ax1DTO RefCenterDir
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal CircleGeomDataDTO()
		{
		}

		internal CircleGeomDataDTO( CircleGeomData geomData )
		{
			if( geomData == null ) {
				return;
			}
			Diameter = geomData.Diameter;
			RotatedAngle_deg = geomData.RotatedAngle_deg;
			IsCoordinateReversed = geomData.IsCoordinateReversed;
			RefCenterDir = new gp_Ax1DTO( geomData.RefCenterDir );
		}

		internal CircleGeomData ToCircleGeomData()
		{
			if( RefCenterDir == null || !Diameter.HasValue || !RotatedAngle_deg.HasValue || !IsCoordinateReversed.HasValue ) {
				throw new ArgumentException( "CircleGeomData deserialization failed." );
			}
			gp_Ax1 refCenterDir = RefCenterDir.ToAx1();
			return new CircleGeomData( refCenterDir, Diameter.Value, RotatedAngle_deg.Value, IsCoordinateReversed.Value );
		}
	}

	public class RectangleGeomDataDTO
	{
		public double? Width
		{
			get;
			set;
		}

		public double? Length
		{
			get;
			set;
		}

		public double? CornerRadius
		{
			get;
			set;
		}

		public double? RotatedAngle_deg
		{
			get;
			set;
		}

		public bool? IsCoordinateReversed
		{
			get;
			set;
		}

		public gp_Ax1DTO RefCenterDir
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal RectangleGeomDataDTO()
		{
		}

		internal RectangleGeomDataDTO( RectangleGeomData geomData )
		{
			if( geomData == null ) {
				return;
			}
			Width = geomData.Width;
			Length = geomData.Length;
			CornerRadius = geomData.CornerRadius;
			RotatedAngle_deg = geomData.RotatedAngle_deg;
			IsCoordinateReversed = geomData.IsCoordinateReversed;
			RefCenterDir = new gp_Ax1DTO( geomData.RefCenterDir );
		}

		internal RectangleGeomData ToRectangleGeomData()
		{
			if( RefCenterDir == null || !Width.HasValue || !Length.HasValue || !CornerRadius.HasValue || !RotatedAngle_deg.HasValue || !IsCoordinateReversed.HasValue ) {
				throw new ArgumentException( "RectangleGeomData deserialization failed." );
			}
			gp_Ax1 refCenterDir = RefCenterDir.ToAx1();
			return new RectangleGeomData( refCenterDir, Width.Value, Length.Value, CornerRadius.Value, RotatedAngle_deg.Value, IsCoordinateReversed.Value );
		}
	}

	public class PolygonGeomDataDTO
	{
		public int? Sides
		{
			get;
			set;
		}

		public double? SideLength
		{
			get;
			set;
		}

		public double? CornerRadius
		{
			get;
			set;
		}

		public double? RotatedAngle_deg
		{
			get;
			set;
		}

		public bool? IsCoordinateReversed
		{
			get;
			set;
		}

		public gp_Ax1DTO RefCenterDir
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal PolygonGeomDataDTO()
		{
		}

		internal PolygonGeomDataDTO( PolygonGeomData geomData )
		{
			if( geomData == null ) {
				return;
			}
			Sides = geomData.Sides;
			SideLength = geomData.SideLength;
			CornerRadius = geomData.CornerRadius;
			RotatedAngle_deg = geomData.RotatedAngle_deg;
			IsCoordinateReversed = geomData.IsCoordinateReversed;
			RefCenterDir = new gp_Ax1DTO( geomData.RefCenterDir );
		}

		internal PolygonGeomData ToPolygonGeomData()
		{
			if( RefCenterDir == null || !Sides.HasValue || !SideLength.HasValue || !CornerRadius.HasValue || !RotatedAngle_deg.HasValue || !IsCoordinateReversed.HasValue ) {
				throw new ArgumentException( "PolygonGeomData deserialization failed." );
			}
			gp_Ax1 refCenterDir = RefCenterDir.ToAx1();
			return new PolygonGeomData( refCenterDir, Sides.Value, SideLength.Value, CornerRadius.Value, RotatedAngle_deg.Value, IsCoordinateReversed.Value );
		}
	}

	public class RunwayGeomDataDTO
	{
		public double? Length
		{
			get;
			set;
		}

		public double? Width
		{
			get;
			set;
		}

		public double? RotatedAngle_deg
		{
			get;
			set;
		}

		public bool? IsCoordinateReversed
		{
			get;
			set;
		}

		public gp_Ax1DTO RefCenterDir
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal RunwayGeomDataDTO()
		{
		}

		internal RunwayGeomDataDTO( RunwayGeomData geomData )
		{
			if( geomData == null ) {
				return;
			}
			Length = geomData.Length;
			Width = geomData.Width;
			RotatedAngle_deg = geomData.RotatedAngle_deg;
			IsCoordinateReversed = geomData.IsCoordinateReversed;
			RefCenterDir = new gp_Ax1DTO( geomData.RefCenterDir );
		}

		internal RunwayGeomData ToRunwayGeomData()
		{
			if( RefCenterDir == null || !Length.HasValue || !Width.HasValue || !RotatedAngle_deg.HasValue || !IsCoordinateReversed.HasValue ) {
				throw new ArgumentException( "RunwayGeomData deserialization failed." );
			}
			gp_Ax1 refCenterDir = RefCenterDir.ToAx1();
			return new RunwayGeomData( refCenterDir, Length.Value, Width.Value, RotatedAngle_deg.Value, IsCoordinateReversed.Value );
		}
	}

	public class gp_Ax1DTO
	{
		// Location (origin point)
		public double? Location_X
		{
			get; set;
		}
		public double? Location_Y
		{
			get; set;
		}
		public double? Location_Z
		{
			get; set;
		}

		// Direction (axis direction)
		public double? Direction_X
		{
			get; set;
		}
		public double? Direction_Y
		{
			get; set;
		}
		public double? Direction_Z
		{
			get; set;
		}

		// parameterless constructor (for XmlSerializer)
		public gp_Ax1DTO()
		{
		}

		internal gp_Ax1DTO( gp_Ax1 ax1 )
		{
			if( ax1 == null ) {
				return;
			}

			gp_Pnt location = ax1.Location();
			Location_X = location.X();
			Location_Y = location.Y();
			Location_Z = location.Z();

			gp_Dir direction = ax1.Direction();
			Direction_X = direction.X();
			Direction_Y = direction.Y();
			Direction_Z = direction.Z();
		}

		internal gp_Ax1 ToAx1()
		{
			if( !Location_X.HasValue || !Location_Y.HasValue || !Location_Z.HasValue ||
				!Direction_X.HasValue || !Direction_Y.HasValue || !Direction_Z.HasValue ) {
				throw new ArgumentException( "gp_Ax1 deserialization failed." );
			}
			gp_Pnt location = new gp_Pnt( Location_X.Value, Location_Y.Value, Location_Z.Value );
			gp_Dir direction = new gp_Dir( Direction_X.Value, Direction_Y.Value, Direction_Z.Value );
			return new gp_Ax1( location, direction );
		}
	}

	public class ConnectPointPairDTO
	{
		public int? KeyIndex
		{
			get;
			set;
		}

		public CADPointDTO ValuePoint
		{
			get;
			set;
		}

		internal ConnectPointPairDTO()
		{
		}

		internal ConnectPointPairDTO( int keyIndex, CADPointDTO valuePoint )
		{
			KeyIndex = keyIndex;
			ValuePoint = valuePoint;
		}
	}

	public class CraftDataDTO
	{
		// properties
		public int? TechLayer
		{
			get;
			set;
		}

		public bool? IsPathReverse
		{
			get;
			set;
		}

		public bool? IsToolVecReverse
		{
			get;
			set;
		}

		public int? StartPoint
		{
			get;
			set;
		}

		public gp_TrsfDTO CumulativeTrsfMatrix
		{
			get;
			set;
		}

		public double? CompensatedDistance
		{
			get;
			set;
		}

		public TraverseDataDTO TraverseData
		{
			get;
			set;
		}

		public LeadDataDTO LeadData
		{
			get;
			set;
		}

		public double? OverCutLength
		{
			get;
			set;
		}

		public EToolVecInterpolateType? InterpolateType
		{
			get;
			set;
		}

		public List<ToolVecMapDTO> ToolVecModifyMap
		{
			get;
			set;
		} = new List<ToolVecMapDTO>();

		public List<ToolVecMap2DTO> ToolVecModifyMap_New
		{
			get;
			set;
		} = new List<ToolVecMap2DTO>();

		public StartPntToolVecParamDTO StartPntToolVecData
		{
			get;
			set;
		}

		internal CraftDataDTO()
		{
		}

		internal CraftDataDTO( CraftData craftData )
		{
			if( craftData == null ) {
				return;
			}
			TechLayer = craftData.TechLayer;
			IsPathReverse = craftData.IsPathReverse;
			IsToolVecReverse = craftData.IsToolVecReverse;
			StartPoint = craftData.StartPointIndex;
			CumulativeTrsfMatrix = craftData.CumulativeTrsfMatrix != null
									? new gp_TrsfDTO( craftData.CumulativeTrsfMatrix )
									: new gp_TrsfDTO();
			CompensatedDistance = craftData.CompensatedDistance;
			LeadData = craftData.LeadData != null
						? new LeadDataDTO( craftData.LeadData )
						: new LeadDataDTO();
			TraverseData = craftData.TraverseData != null
							? new TraverseDataDTO( craftData.TraverseData )
							: new TraverseDataDTO();
			OverCutLength = craftData.OverCutLength;
			ToolVecModifyMap_New = craftData.ToolVecModifyMap.ToDictionary()
				.Select( kvp => new ToolVecMap2DTO( kvp.Key, kvp.Value.AngleData, kvp.Value.InterpolateType ) )
				.ToList();

			if( craftData.StartPntToolVecData != null ) {
				StartPntToolVecData = new StartPntToolVecParamDTO( craftData.StartPntToolVecData );
			}
			else {
				// create empty node
				StartPntToolVecData = new StartPntToolVecParamDTO();
			}
		}

		internal CraftData ToCraftData()
		{
			// Validate required fields
			if( LeadData == null || TraverseData == null ||
				!StartPoint.HasValue || !IsPathReverse.HasValue || !OverCutLength.HasValue ||
				!IsToolVecReverse.HasValue || !CompensatedDistance.HasValue ) {
				throw new ArgumentException( "CraftData deserialization failed: Missing required fields." );
			}

			// Backward compatibility for TechLayer
			if( !TechLayer.HasValue ) {
				TechLayer = DEFAULT_TECH_LAYER;
			}

			LeadData leadData = LeadData.ToLeadData();
			TraverseData traverseData = TraverseData.ToTraverseData();
			Dictionary<int, ToolVecModifyData> toolVecModifyMap = BuildToolVecModifyMap( out List<ToolVecMapDTO> sourceMap_OldVersion );
			StartPntToolVecParam startPntToolVecParam = BuildStartPntToolVecParam( sourceMap_OldVersion, IsPathReverse.Value );
			CraftData craftData = new CraftData( TechLayer.Value, StartPoint.Value, IsPathReverse.Value,
												 leadData, OverCutLength.Value, toolVecModifyMap,
												 startPntToolVecParam, IsToolVecReverse.Value, traverseData );

			// old version may have start/end point in ToolVecModifyMap, those are move to StartPntToolVecData
			RemoveOldMapStartEndPoint( craftData );

			// Set properties not in constructor
			if( CumulativeTrsfMatrix == null ) {
				throw new ArgumentException( "CraftData.CumulativeTrsfMatrix deserialization failed." );
			}
			craftData.CumulativeTrsfMatrix = CumulativeTrsfMatrix.ToTrsf();

			craftData.CompensatedDistance = CompensatedDistance.Value;

			return craftData;
		}

		void RemoveOldMapStartEndPoint( CraftData craftData )
		{
			if( craftData.ToolVecModifyMap.ContainsKey( StartPoint.Value ) ) {
				craftData.ToolVecModifyMap.Remove( StartPoint.Value );
			}
			if( craftData.ToolVecModifyMap.ContainsKey( LAST_POINT_INDEX ) ) {
				craftData.ToolVecModifyMap.Remove( LAST_POINT_INDEX );
			}
		}

		Dictionary<int, ToolVecModifyData> ToToolVecModifyMap()
		{
			Dictionary<int, ToolVecModifyData> toolVecModifyMap = new Dictionary<int, ToolVecModifyData>();
			if( ToolVecModifyMap_New == null ) {
				return toolVecModifyMap;
			}

			// Read from ToolVecModifyMap_New, each entry has its own InterpolateType
			foreach( var dto in ToolVecModifyMap_New ) {
				if( dto.Index.HasValue ) {
					toolVecModifyMap[ dto.Index.Value ] = dto.ToToolVecModifyData();
				}
			}
			return toolVecModifyMap;
		}

		Dictionary<int, ToolVecModifyData> ToToolVecModifyMap( EToolVecInterpolateType globalInterpolateType )
		{
			Dictionary<int, ToolVecModifyData> toolVecModifyMap = new Dictionary<int, ToolVecModifyData>();
			if( ToolVecModifyMap == null ) {
				return toolVecModifyMap;
			}

			// read from ToolVecModifyMap(old data), apply global InterpolateType to all entries
			foreach( ToolVecMapDTO dto in ToolVecModifyMap ) {
				if( dto.Index.HasValue ) {
					toolVecModifyMap[ dto.Index.Value ] = dto.ToToolVecModifyData( globalInterpolateType );
				}
				else {
					throw new ArgumentException( "CraftData deserialization failed: ToolVecModifyMap entry missing Index." );
				}
			}
			return toolVecModifyMap;
		}

		Dictionary<int, ToolVecModifyData> BuildToolVecModifyMap( out List<ToolVecMapDTO> sourceMap_OldVersion )
		{
			sourceMap_OldVersion = null;
			// if do not have any control pnt , toolVecModifyMap will still not be empty(construct protection)
			bool isOldVersion = ToolVecModifyMap != null && ToolVecModifyMap.Count > 0;

			if( isOldVersion ) {
				// need InterpolateType from global field
				if( !InterpolateType.HasValue ) {
					throw new ArgumentException( "CraftData deserialization failed: Old version file missing InterpolateType field." );
				}

				EToolVecInterpolateType globalInterpolateType = InterpolateType.Value;
				sourceMap_OldVersion = ToolVecModifyMap;

				// Use new method for legacy ToolVecModifyMap deserialization
				return ToToolVecModifyMap( globalInterpolateType );
			}
			else {
				// Use new method for ToolVecModifyMap_New deserialization
				return ToToolVecModifyMap();
			}
		}

		StartPntToolVecParam BuildStartPntToolVecParam( List<ToolVecMapDTO> sourceMap_OldVersion, bool isPathReverse )
		{
			// new version with StartPntToolVecData
			if( StartPntToolVecData != null ) {
				return StartPntToolVecData.ToStartPntToolVecParam();
			}

			// old version without map
			if( sourceMap_OldVersion == null ) {
				return new StartPntToolVecParam();
			}

			// old version only have 1 interpolate type for all points
			if( !InterpolateType.HasValue ) {
				throw new ArgumentException( "CraftData deserialization failed: Old version file missing InterpolateType field." );
			}
			EToolVecInterpolateType globalInterpolateType = InterpolateType.Value;

			// find start and end point data from old version
			ToolVecMapDTO startData = sourceMap_OldVersion.FirstOrDefault( d => d.Index.HasValue && d.Index.Value == StartPoint );
			ToolVecMapDTO endData = sourceMap_OldVersion.FirstOrDefault( d => d.Index.HasValue && d.Index.Value == LAST_POINT_INDEX );

			if( startData == null && endData == null ) {
				return new StartPntToolVecParam();
			}

			// get start / end data from DTO
			ToolVecModifyData startPnt = CreateToolVecModifyDataFromDTO( startData, globalInterpolateType );
			ToolVecModifyData endPnt = CreateToolVecModifyDataFromDTO( endData, globalInterpolateType );

			// swap start and end when path is reversed
			if( isPathReverse ) {
				ToolVecModifyData temp = startPnt;
				startPnt = endPnt;
				endPnt = temp;
			}
			return new StartPntToolVecParam( startPnt, endPnt );
		}

		ToolVecModifyData CreateToolVecModifyDataFromDTO( ToolVecMapDTO dto, EToolVecInterpolateType interpolateType )
		{
			if( dto == null ) {
				return new ToolVecModifyData( interpolateType );
			}

			ToolVecAngleData angleData = new ToolVecAngleData(
				dto.RA_deg ?? 0,
				dto.RB_deg ?? 0,
				dto.MasterAngle_deg ?? 0,
				dto.SlaveAngle_deg ?? 0
			);

			return new ToolVecModifyData( angleData, interpolateType );
		}

		const int DEFAULT_TECH_LAYER = 1;
		const int LAST_POINT_INDEX = -1;
	}

	public class TraverseDataDTO
	{
		public double? LiftUpDistance
		{
			get;
			set;
		}

		public double? CutDownDistance
		{
			get;
			set;
		}

		public double? FollowSafeDistance
		{
			get;
			set;
		}

		public double? FrogLeapDistance
		{
			get; set;
		}

		public double? SafePlaneDistance
		{
			get; set;
		}

		public bool? IsSafePlaneChecked
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
			IsSafePlaneChecked = traverseData.IsSafePlaneEnable;
			SafePlaneDistance = traverseData.SafePlaneDistance;
		}

		internal TraverseData ToTraverseData()
		{
			if( !LiftUpDistance.HasValue || !CutDownDistance.HasValue || !FollowSafeDistance.HasValue ||
				!FrogLeapDistance.HasValue || !IsSafePlaneChecked.HasValue || !SafePlaneDistance.HasValue ) {
				throw new ArgumentException( "TraverseData deserialization failed." );
			}
			return new TraverseData( LiftUpDistance.Value, CutDownDistance.Value, FollowSafeDistance.Value, FrogLeapDistance.Value, IsSafePlaneChecked.Value, SafePlaneDistance.Value );
		}
	}

	public class EntryAndExitDataDTO
	{
		public double? EntryDistance
		{
			get;
			set;
		}

		public double? ExitDistance
		{
			get;
			set;
		}

		public double? FollowSafeDistance
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
			if( !EntryDistance.HasValue || !ExitDistance.HasValue || !FollowSafeDistance.HasValue ) {
				return new EntryAndExitData();
			}
			return new EntryAndExitData( EntryDistance.Value, ExitDistance.Value, FollowSafeDistance.Value );
		}
	}

	public class CalibrationDataDTO
	{
		public gp_PntDTO REFPnt1
		{
			get;
			set;
		}

		public gp_PntDTO REFPnt2
		{
			get;
			set;
		}

		public gp_PntDTO REFPnt3
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal CalibrationDataDTO()
		{
		}

		internal CalibrationDataDTO( CalibrationData calibrationData )
		{
			if( calibrationData == null || calibrationData.IsBeenSet == false ) {
				return;
			}
			REFPnt1 = new gp_PntDTO( calibrationData.Ref_Pnt1 );
			REFPnt2 = new gp_PntDTO( calibrationData.Ref_Pnt2 );
			REFPnt3 = new gp_PntDTO( calibrationData.Ref_Pnt3 );
		}

		internal CalibrationData ToCalibrationData()
		{
			if( REFPnt1 == null || REFPnt2 == null || REFPnt3 == null ) {
				return new CalibrationData();
			}
			return new CalibrationData( REFPnt1.ToPnt(), REFPnt2.ToPnt(), REFPnt3.ToPnt() );
		}
	}

	public class gp_PntDTO
	{
		public double? X
		{
			get;
			set;
		}

		public double? Y
		{
			get;
			set;
		}

		public double? Z
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal gp_PntDTO()
		{
		}

		internal gp_PntDTO( gp_Pnt point )
		{
			if( point == null ) {
				return;
			}
			X = point.X();
			Y = point.Y();
			Z = point.Z();
		}

		internal gp_Pnt ToPnt()
		{
			if( !X.HasValue || !Y.HasValue || !Z.HasValue ) {
				throw new ArgumentException( "gp_Pnt deserialization failed." );
			}
			return new gp_Pnt( X.Value, Y.Value, Z.Value );
		}
	}

	public class LeadDataDTO
	{
		public double? LeadInStraightLength
		{
			get;
			set;
		}

		public double? LeadInArcLength
		{
			get;
			set;
		}

		public double? LeadOutStraightLength
		{
			get;
			set;
		}

		public double? LeadOutArcLength
		{
			get;
			set;
		}

		public double? LeadInAngle
		{
			get;
			set;
		}

		public double? LeadOutAngle
		{
			get;
			set;
		}

		public bool? IsChangeLeadDirection
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal LeadDataDTO()
		{
		}

		// constructor from LeadData
		internal LeadDataDTO( LeadData leadData )
		{
			if( leadData == null ) {
				return;
			}
			LeadInStraightLength = leadData.LeadIn.StraightLength;
			LeadOutStraightLength = leadData.LeadOut.StraightLength;
			LeadInArcLength = leadData.LeadIn.ArcLength;
			LeadOutArcLength = leadData.LeadOut.ArcLength;
			LeadInAngle = leadData.LeadIn.Angle_deg;
			LeadOutAngle = leadData.LeadOut.Angle_deg;
			IsChangeLeadDirection = leadData.IsChangeLeadDirection;
		}

		internal LeadData ToLeadData()
		{
			if( !LeadInStraightLength.HasValue || !LeadInArcLength.HasValue || !LeadInAngle.HasValue ||
				!LeadOutStraightLength.HasValue || !LeadOutArcLength.HasValue || !LeadOutAngle.HasValue ||
				!IsChangeLeadDirection.HasValue ) {
				throw new ArgumentException( "LeadData deserialization failed." );
			}
			return new LeadData( LeadInStraightLength.Value, LeadInArcLength.Value, LeadInAngle.Value, LeadOutStraightLength.Value, LeadOutArcLength.Value, LeadOutAngle.Value, IsChangeLeadDirection.Value );
		}
	}

	public class CADPointDTO
	{
		// properties
		public double? X
		{
			get;
			set;
		}

		public double? Y
		{
			get;
			set;
		}

		public double? Z
		{
			get;
			set;
		}

		public double? Normal1_X
		{
			get;
			set;
		}

		public double? Normal1_Y
		{
			get;
			set;
		}

		public double? Normal1_Z
		{
			get;
			set;
		}

		public double? Normal2_X
		{
			get;
			set;
		}

		public double? Normal2_Y
		{
			get;
			set;
		}

		public double? Normal2_Z
		{
			get;
			set;
		}

		public double? Tangent_X
		{
			get;
			set;
		}

		public double? Tangent_Y
		{
			get;
			set;
		}

		public double? Tangent_Z
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
			if( !X.HasValue || !Y.HasValue || !Z.HasValue ||
				!Normal1_X.HasValue || !Normal1_Y.HasValue || !Normal1_Z.HasValue ||
				!Normal2_X.HasValue || !Normal2_Y.HasValue || !Normal2_Z.HasValue ||
				!Tangent_X.HasValue || !Tangent_Y.HasValue || !Tangent_Z.HasValue ) {
				throw new ArgumentException( "CADPoint deserialization failed." );
			}
			gp_Pnt point = new gp_Pnt( X.Value, Y.Value, Z.Value );
			gp_Dir n1 = new gp_Dir( Normal1_X.Value, Normal1_Y.Value, Normal1_Z.Value );
			gp_Dir n2 = new gp_Dir( Normal2_X.Value, Normal2_Y.Value, Normal2_Z.Value );
			gp_Dir t = new gp_Dir( Tangent_X.Value, Tangent_Y.Value, Tangent_Z.Value );
			return new CADPoint( point, n1, n2, t );
		}
	}

	public class TopoShapeDTO
	{
		public string TopoShapeBRepData
		{
			get;
			set;
		}

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

	// Legacy DTO: used in old save files where ToolVecModifyMap contained ToolVecMapDTO elements (no InterpolateType field)
	public class ToolVecMapDTO
	{
		public int? Index
		{
			get;
			set;
		}

		public double? RA_deg
		{
			get;
			set;
		}

		public double? RB_deg
		{
			get;
			set;
		}

		public double? MasterAngle_deg
		{
			get;
			set;
		}

		public double? SlaveAngle_deg
		{
			get;
			set;
		}

		// parameterless constructor (for XmlSerializer)
		internal ToolVecMapDTO()
		{
		}

		internal ToolVecMapDTO( int index, double ra_deg, double rb_deg, double masterAngle_deg, double slaveAngle_deg )
		{
			Index = index;
			RA_deg = ra_deg;
			RB_deg = rb_deg;
			MasterAngle_deg = masterAngle_deg;
			SlaveAngle_deg = slaveAngle_deg;
		}

		// DTO conversion method, similar to ToLeadData()
		internal ToolVecModifyData ToToolVecModifyData( EToolVecInterpolateType interpolateType )
		{
			if( !Index.HasValue || !RA_deg.HasValue || !RB_deg.HasValue || !MasterAngle_deg.HasValue || !SlaveAngle_deg.HasValue ) {
				throw new ArgumentException( "ToolVecMapDTO deserialization failed: Index is required." );
			}
			ToolVecAngleData angleData = new ToolVecAngleData(
				RA_deg ?? 0,
				RB_deg ?? 0,
				MasterAngle_deg ?? 0,
				SlaveAngle_deg ?? 0
			);

			return new ToolVecModifyData( angleData, interpolateType );
		}
	}

	public class ToolVecMap2DTO
	{
		public int? Index
		{
			get;
			set;
		}

		public ToolVecAngleDataDTO AngleDataDTO
		{
			get;
			set;
		} = new ToolVecAngleDataDTO();

		public EToolVecInterpolateType? InterpolateType
		{
			get;
			set;
		}

		// parameterless constructor(for XmlSerializer)
		internal ToolVecMap2DTO()
		{
		}

		internal ToolVecMap2DTO( int index, ToolVecAngleData toolVecAngleData, EToolVecInterpolateType interpolateType )
		{
			Index = index;
			AngleDataDTO = new ToolVecAngleDataDTO( toolVecAngleData );
			InterpolateType = interpolateType;
		}

		/// <summary>
		/// Convert ToolVecMap2DTO to ToolVecModifyData
		/// </summary>
		/// <returns>ToolVecModifyData object</returns>
		internal ToolVecModifyData ToToolVecModifyData()
		{
			if( !Index.HasValue ) {
				throw new ArgumentException( "ToolVecMap2DTO deserialization failed: Index is required." );
			}

			// Use the InterpolateType from the entry, default to Normal if null
			EToolVecInterpolateType entryInterpolateType = InterpolateType ?? EToolVecInterpolateType.Normal;
			ToolVecAngleData angleData = AngleDataDTO?.ToToolVecAngleData();

			return new ToolVecModifyData( angleData, entryInterpolateType );
		}
	}
}

public class ToolVecModifyData2DTO
{
	public ToolVecAngleDataDTO AngleData
	{
		get;
		set;
	}

	public EToolVecInterpolateType? InterpolateType
	{
		get;
		set;
	} = EToolVecInterpolateType.Normal;

	internal ToolVecModifyData2DTO()
	{
		// use empty AngleData to ensure XML node exists, and mark it as null
		AngleData = new ToolVecAngleDataDTO();
		AngleData.IsNull = true;
	}

	internal ToolVecModifyData2DTO( ToolVecModifyData data )
	{
		if( data == null ) {
			// build empty AngleData to ensure XML node exists
			AngleData = new ToolVecAngleDataDTO();
			AngleData.IsNull = true;
			return;
		}

		// even if data.AngleData is null, to ensure XML node exists
		if( data.AngleData != null ) {
			AngleData = new ToolVecAngleDataDTO( data.AngleData );
		}
		else {
			AngleData = new ToolVecAngleDataDTO();
		}
		InterpolateType = data.InterpolateType;
	}

	internal ToolVecModifyData ToToolVecModifyData()
	{
		ToolVecAngleData angleData = AngleData?.ToToolVecAngleData();
		EToolVecInterpolateType interpolateType = InterpolateType ?? EToolVecInterpolateType.Normal;
		return new ToolVecModifyData( angleData, interpolateType );
	}
}

public class ToolVecAngleDataDTO
{
	public double? RA_deg
	{
		get;
		set;
	}

	public double? RB_deg
	{
		get;
		set;
	}

	public double? MasterAngle_deg
	{
		get;
		set;
	}

	public double? SlaveAngle_deg
	{
		get;
		set;
	}

	// null tag to 
	public bool IsNull
	{
		get;
		set;
	} = false;

	internal ToolVecAngleDataDTO()
	{
		IsNull = true;
		return;
	}

	internal ToolVecAngleDataDTO( ToolVecAngleData data )
	{
		if( data == null ) {
			IsNull = true;
			return;
		}
		IsNull = false;
		RA_deg = data.RA_deg;
		RB_deg = data.RB_deg;
		MasterAngle_deg = data.Master_deg;
		SlaveAngle_deg = data.Slave_deg;
	}

	internal ToolVecAngleData ToToolVecAngleData()
	{
		if( IsNull ) {
			return new ToolVecAngleData();
		}
		if( !RA_deg.HasValue || !RB_deg.HasValue || !MasterAngle_deg.HasValue || !SlaveAngle_deg.HasValue ) {
			throw new ArgumentException( "ToolVecMapDTO deserialization failed: Index is required." );
		}
		double RA_Value = RA_deg ?? 0;
		double RB_Value = RB_deg ?? 0;
		double MasterAngle_Value = MasterAngle_deg ?? 0;
		double SlaveAngle_Value = SlaveAngle_deg ?? 0;
		return new ToolVecAngleData( RA_Value, RB_Value, MasterAngle_Value, SlaveAngle_Value );
	}
}

public class StartPntToolVecParamDTO
{
	public ToolVecModifyData2DTO StartPnt
	{
		get;
		set;
	}

	public ToolVecModifyData2DTO EndPnt
	{
		get;
		set;
	}

	internal StartPntToolVecParamDTO()
	{
	}

	internal StartPntToolVecParamDTO( StartPntToolVecParam param )
	{
		if( param == null ) {

			// build default node
			StartPnt = new ToolVecModifyData2DTO();
			EndPnt = new ToolVecModifyData2DTO();
			return;
		}
		if( param.StartPnt != null ) {
			StartPnt = new ToolVecModifyData2DTO( param.StartPnt );
		}
		else {
			StartPnt = new ToolVecModifyData2DTO();
		}
		if( param.EndPnt != null ) {
			EndPnt = new ToolVecModifyData2DTO( param.EndPnt );
		}
		else {
			EndPnt = new ToolVecModifyData2DTO();
		}
	}

	internal StartPntToolVecParam ToStartPntToolVecParam()
	{
		ToolVecModifyData startPnt = StartPnt?.ToToolVecModifyData();
		ToolVecModifyData endPnt = EndPnt?.ToToolVecModifyData();
		return new StartPntToolVecParam( startPnt, endPnt );
	}
}

public class ShapeIDsDTO
{
	public int? SolidID
	{
		get; set;
	}

	public int? ShellID
	{
		get; set;
	}

	public int? FaceID
	{
		get; set;
	}

	public int? WireID
	{
		get; set;
	}

	public int? EdgeID
	{
		get; set;
	}

	public int? VertexID
	{
		get; set;
	}

	public int? PathID
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
		if( !SolidID.HasValue || !ShellID.HasValue || !FaceID.HasValue ||
			!WireID.HasValue || !EdgeID.HasValue || !VertexID.HasValue || !PathID.HasValue ) {
			throw new ArgumentException( "ShapeIDsStruct deserialization failed." );
		}
		return new ShapeIDsStruct()
		{
			Solid_ID = SolidID.Value,
			Shell_ID = ShellID.Value,
			Face_ID = FaceID.Value,
			Wire_ID = WireID.Value,
			Edge_ID = EdgeID.Value,
			Vertex_ID = VertexID.Value,
			Path_ID = PathID.Value,
		};
	}
}

public class gp_TrsfDTO
{
	// Transformation matrix values (3x4 matrix)
	// Row 1
	public double? M11
	{
		get; set;
	}
	public double? M12
	{
		get; set;
	}
	public double? M13
	{
		get; set;
	}
	public double? M14
	{
		get; set;
	}

	// Row 2
	public double? M21
	{
		get; set;
	}
	public double? M22
	{
		get; set;
	}
	public double? M23
	{
		get; set;
	}
	public double? M24
	{
		get; set;
	}

	// Row 3
	public double? M31
	{
		get; set;
	}
	public double? M32
	{
		get; set;
	}
	public double? M33
	{
		get; set;
	}
	public double? M34
	{
		get; set;
	}

	// parameterless constructor (for XmlSerializer)
	public gp_TrsfDTO()
	{
	}

	internal gp_TrsfDTO( gp_Trsf trsf )
	{
		if( trsf == null ) {
			return;
		}

		// Get transformation matrix values
		// gp_Trsf uses Value(row, col) method where indices are 1-based
		M11 = trsf.Value( 1, 1 );
		M12 = trsf.Value( 1, 2 );
		M13 = trsf.Value( 1, 3 );
		M14 = trsf.Value( 1, 4 );
		M21 = trsf.Value( 2, 1 );
		M22 = trsf.Value( 2, 2 );
		M23 = trsf.Value( 2, 3 );
		M24 = trsf.Value( 2, 4 );
		M31 = trsf.Value( 3, 1 );
		M32 = trsf.Value( 3, 2 );
		M33 = trsf.Value( 3, 3 );
		M34 = trsf.Value( 3, 4 );
	}

	internal gp_Trsf ToTrsf()
	{
		if( !M11.HasValue || !M12.HasValue || !M13.HasValue || !M14.HasValue ||
			!M21.HasValue || !M22.HasValue || !M23.HasValue || !M24.HasValue ||
			!M31.HasValue || !M32.HasValue || !M33.HasValue || !M34.HasValue ) {
			throw new ArgumentException( "gp_Trsf deserialization failed." );
		}
		gp_Trsf trsf = new gp_Trsf();

		// Set transformation matrix values
		// gp_Trsf uses SetValues method to set the matrix
		trsf.SetValues(
			M11.Value, M12.Value, M13.Value, M14.Value,
			M21.Value, M22.Value, M23.Value, M24.Value,
			M31.Value, M32.Value, M33.Value, M34.Value
		);

		return trsf;
	}
}

