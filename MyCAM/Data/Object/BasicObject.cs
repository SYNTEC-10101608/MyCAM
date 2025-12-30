using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public interface IObject
	{
		string UID
		{
			get;
		}

		ObjectType ObjectType
		{
			get;
		}
	}

	public interface IShapeObject
	{
		TopoDS_Shape Shape
		{
			get;
		}
	}

	public interface ITransformableObject
	{
		void DoTransform( gp_Trsf transform );
	}

	public interface ISewableObject : IShapeObject
	{
		void SewShape( double sewTol );
	}

	public class PartObject : IObject, IShapeObject, ITransformableObject, ISewableObject
	{
		public PartObject( string szUID, TopoDS_Shape shape )
		{
			UID = szUID;
			Shape = shape;
		}

		public string UID
		{
			get; private set;
		}

		public TopoDS_Shape Shape
		{
			get; private set;
		}

		public ObjectType ObjectType
		{
			get
			{
				return ObjectType.Part;
			}
		}

		// TODO: the following method should move out of a data record class
		public virtual void SewShape( double sewTol )
		{
			TopoDS_Shape result = ShapeTool.SewShape( new List<TopoDS_Shape>() { Shape }, sewTol );
			if( result == null || result.IsNull() ) {
				return;
			}
			Shape = result;
		}

		public virtual void DoTransform( gp_Trsf transform )
		{
			BRepBuilderAPI_Transform shapeTransform = new BRepBuilderAPI_Transform( Shape, transform );
			if( !shapeTransform.IsDone() ) {
				return;
			}
			Shape = shapeTransform.Shape();
		}
	}

	public abstract class PathObject : IObject, IShapeObject, ITransformableObject
	{
		protected PathObject( string szUID, TopoDS_Shape shape )
		{
			UID = szUID;
			Shape = shape;
		}

		public string UID
		{
			get; private set;
		}

		public TopoDS_Shape Shape
		{
			get; private set;
		}

		public ObjectType ObjectType
		{
			get
			{
				return ObjectType.Path;
			}
		}

		public CraftData CraftData
		{
			get
			{
				return m_CraftData;
			}
		}

		public abstract PathType PathType
		{
			get;
		}

		// TODO: the following method should move out of a data record class
		public virtual void DoTransform( gp_Trsf transform )
		{
			BRepBuilderAPI_Transform shapeTransform = new BRepBuilderAPI_Transform( Shape, transform );
			if( !shapeTransform.IsDone() ) {
				return;
			}
			Shape = shapeTransform.Shape();
		}

		protected CraftData m_CraftData;
	}

	public class PathEdge5D
	{
		public PathEdge5D( TopoDS_Edge pathEdge, TopoDS_Face componentFace )
		{
			PathEdge = pathEdge;
			ComponentFace = componentFace;
		}

		public TopoDS_Edge PathEdge
		{
			get; private set;
		}

		public TopoDS_Face ComponentFace
		{
			get; private set;
		}
	}

	public enum ObjectType
	{
		Part = 0,
		Path = 1,
	}

	public enum PathType
	{
		Contour = 0,
		Circle = 1,
		Rectangle = 2,
		Runway = 3,
		Triangle = 4,
		Square = 5,
		Pentagon = 6,
		Hexagon = 7,
	}
}
