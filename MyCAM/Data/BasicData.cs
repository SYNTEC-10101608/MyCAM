using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using System.Collections.Generic;

namespace MyCAM.Data
{
	internal interface IObject
	{
		string UID
		{
			get;
		}

		TopoDS_Shape Shape
		{
			get;
		}

		ObjectType ObjectType
		{
			get;
		}

		void DoTransform( gp_Trsf transform );

		void SewShape( double sewTol );
	}

	internal class PartObject : IObject
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

	public abstract class PathObject : IObject
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

		public abstract CraftData CraftData
		{
			get;
		}

		public abstract PathType PathType
		{
			get;
		}

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

	internal class PathEdge5D
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
		Rectangle = 1,
	}
}
