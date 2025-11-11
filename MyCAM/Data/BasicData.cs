using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopoDS;

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

		public virtual void DoTransform( gp_Trsf transform )
		{
			BRepBuilderAPI_Transform shapeTransform = new BRepBuilderAPI_Transform( Shape, transform );
			Shape = shapeTransform.Shape();
		}
	}

	public abstract class PathObject : IObject
	{
		protected PathObject( string szUID, TopoDS_Shape shape, PathType pathShapeType )
		{
			UID = szUID;
			Shape = shape;

			// re: implement in derived class, like ObjectType
			PathType = pathShapeType;
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

		// re: why virtual here? let it abstract if no default implementation
		public virtual CraftData CraftData
		{
			get; set;
		}

		// re: let it abstract
		public PathType PathType
		{
			get; protected set;
		}

		public virtual void DoTransform( gp_Trsf transform )
		{
			BRepBuilderAPI_Transform shapeTransform = new BRepBuilderAPI_Transform( Shape, transform );
			Shape = shapeTransform.Shape();
		}

		// re: move to standard pattern
		public virtual void UpdateShape( TopoDS_Shape newShape )
		{
			Shape = newShape;
		}

		// re: implement clone by yourself, dont use MemberwiseClone directly
		public virtual PathObject Clone()
		{
			PathObject newPathData = (PathObject)this.MemberwiseClone();
			if( this.Shape != null ) {
				BRepBuilderAPI_Copy copyShape = new BRepBuilderAPI_Copy( this.Shape );
				newPathData.UpdateShape( copyShape.Shape() );
			}
			return newPathData;
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
