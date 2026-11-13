using netDxf;
using netDxf.Entities;
using MyCAM.Data;
using OCC.BRepBuilderAPI;
using OCC.Geom;
using OCC.gp;
using OCC.ShapeAnalysis;
using OCC.TColgp;
using OCC.TColStd;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper.FileIO
{
	/// <summary>
	/// 讀取 DXF 檔案並轉換為 OCCT TopoDS_Shape
	/// </summary>
	internal class DxfReader
	{
		/// <summary>
		/// 讀取 DXF 檔案，回傳合併後的 TopoDS_Shape (Compound)
		/// </summary>
		/// <param name="szFileName">DXF 檔案路徑</param>
		/// <returns>成功時回傳 TopoDS_Shape，失敗時回傳 null</returns>
		public TopoDS_Shape Read( string szFileName, Action<LoadingBarInfo> reportProgress = null )
		{
			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ReadFile ) );
			DxfDocument dxfDoc = DxfDocument.Load( szFileName );
			if( dxfDoc == null )
				return null;

			List<TopoDS_Shape> shapes = new List<TopoDS_Shape>();

			int total = dxfDoc.Entities.Lines.Count() + dxfDoc.Entities.Arcs.Count() + dxfDoc.Entities.Circles.Count()
				+ dxfDoc.Entities.Ellipses.Count() + dxfDoc.Entities.Points.Count() + dxfDoc.Entities.Polylines2D.Count()
				+ dxfDoc.Entities.Polylines3D.Count() + dxfDoc.Entities.Splines.Count();
			int completed = 0;
			Action entityProcessed = () => reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ConvertDxf, ++completed, total ) );
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ConvertDxf, 0, total ) );
			ConvertLines( dxfDoc, shapes, entityProcessed );
			ConvertArcs( dxfDoc, shapes, entityProcessed );
			ConvertCircles( dxfDoc, shapes, entityProcessed );
			ConvertEllipses( dxfDoc, shapes, entityProcessed );
			ConvertPoints( dxfDoc, shapes, entityProcessed );
			ConvertPolylines2D( dxfDoc, shapes, entityProcessed );
			ConvertPolylines3D( dxfDoc, shapes, entityProcessed );
			ConvertSplines( dxfDoc, shapes, entityProcessed );

			if( shapes.Count == 0 )
				return null;

			// Connect edges into wires (closed and open)
			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ConnectDxfContours ) );
			TopoDS_Shape edgeCompound = ShapeTool.MakeCompound( shapes );
			return ConnectEdgesToWires( edgeCompound );
		}

		static IEnumerable<T> TrackEntities<T>( IEnumerable<T> entities, Action entityProcessed )
		{
			foreach( T entity in entities ) {
				yield return entity;

				// Report progress to the loading bar.
				entityProcessed();
			}
		}

		void ConvertLines( DxfDocument dxfDoc, List<TopoDS_Shape> shapes, Action entityProcessed )
		{
			foreach( netDxf.Entities.Line line in TrackEntities( dxfDoc.Entities.Lines, entityProcessed ) ) {
				gp_Pnt p0 = new gp_Pnt( line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z );
				gp_Pnt p1 = new gp_Pnt( line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z );
				if( p0.Distance( p1 ) < 1e-8 )
					continue;
				BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( p0, p1 );
				if( edgeMaker.IsDone() )
					shapes.Add( edgeMaker.Edge() );
			}
		}

		void ConvertArcs( DxfDocument dxfDoc, List<TopoDS_Shape> shapes, Action entityProcessed )
		{
			foreach( Arc arc in TrackEntities( dxfDoc.Entities.Arcs, entityProcessed ) ) {
				gp_Pnt center = new gp_Pnt( arc.Center.X, arc.Center.Y, arc.Center.Z );
				gp_Dir normal = new gp_Dir( arc.Normal.X, arc.Normal.Y, arc.Normal.Z );
				gp_Ax2 ax2 = new gp_Ax2( center, normal );
				gp_Circ circ = new gp_Circ( ax2, arc.Radius );
				double startRad = arc.StartAngle * Math.PI / 180.0;
				double endRad = arc.EndAngle * Math.PI / 180.0;
				BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( circ, startRad, endRad );
				if( edgeMaker.IsDone() )
					shapes.Add( edgeMaker.Edge() );
			}
		}

		void ConvertCircles( DxfDocument dxfDoc, List<TopoDS_Shape> shapes, Action entityProcessed )
		{
			foreach( Circle circle in TrackEntities( dxfDoc.Entities.Circles, entityProcessed ) ) {
				gp_Pnt center = new gp_Pnt( circle.Center.X, circle.Center.Y, circle.Center.Z );
				gp_Dir normal = new gp_Dir( circle.Normal.X, circle.Normal.Y, circle.Normal.Z );
				gp_Ax2 ax2 = new gp_Ax2( center, normal );
				gp_Circ circ = new gp_Circ( ax2, circle.Radius );
				BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( circ );
				if( edgeMaker.IsDone() )
					shapes.Add( edgeMaker.Edge() );
			}
		}

		void ConvertEllipses( DxfDocument dxfDoc, List<TopoDS_Shape> shapes, Action entityProcessed )
		{
			foreach( Ellipse ellipse in TrackEntities( dxfDoc.Entities.Ellipses, entityProcessed ) ) {
				gp_Pnt center = new gp_Pnt( ellipse.Center.X, ellipse.Center.Y, ellipse.Center.Z );
				gp_Dir normal = new gp_Dir( ellipse.Normal.X, ellipse.Normal.Y, ellipse.Normal.Z );
				gp_Dir majorDir = new gp_Dir( Math.Cos( ellipse.Rotation * Math.PI / 180.0 ), Math.Sin( ellipse.Rotation * Math.PI / 180.0 ), 0 );
				gp_Ax2 ax2 = new gp_Ax2( center, normal, majorDir );
				double majorRadius = ellipse.MajorAxis * 0.5;
				double minorRadius = ellipse.MinorAxis * 0.5;
				if( majorRadius < 1e-9 || minorRadius < 1e-9 )
					continue;
				gp_Elips elips = new gp_Elips( ax2, majorRadius, minorRadius );
				BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( elips );
				if( edgeMaker.IsDone() )
					shapes.Add( edgeMaker.Edge() );
			}
		}

		void ConvertPoints( DxfDocument dxfDoc, List<TopoDS_Shape> shapes, Action entityProcessed )
		{
			foreach( netDxf.Entities.Point pt in TrackEntities( dxfDoc.Entities.Points, entityProcessed ) ) {
				gp_Pnt pnt = new gp_Pnt( pt.Position.X, pt.Position.Y, pt.Position.Z );
				BRepBuilderAPI_MakeVertex vtxMaker = new BRepBuilderAPI_MakeVertex( pnt );
				shapes.Add( vtxMaker.Shape() );
			}
		}

		void ConvertPolylines2D( DxfDocument dxfDoc, List<TopoDS_Shape> shapes, Action entityProcessed )
		{
			foreach( Polyline2D poly2d in TrackEntities( dxfDoc.Entities.Polylines2D, entityProcessed ) ) {
				List<EntityObject> exploded = poly2d.Explode();
				foreach( EntityObject ent in exploded ) {
					TopoDS_Edge edge = EntityToEdge( ent );
					if( edge != null && !edge.IsNull() )
						shapes.Add( edge );
				}
			}
		}

		void ConvertPolylines3D( DxfDocument dxfDoc, List<TopoDS_Shape> shapes, Action entityProcessed )
		{
			foreach( Polyline3D poly3d in TrackEntities( dxfDoc.Entities.Polylines3D, entityProcessed ) ) {
				List<EntityObject> exploded = poly3d.Explode();
				foreach( EntityObject ent in exploded ) {
					TopoDS_Edge edge = EntityToEdge( ent );
					if( edge != null && !edge.IsNull() )
						shapes.Add( edge );
				}
			}
		}

		void ConvertSplines( DxfDocument dxfDoc, List<TopoDS_Shape> shapes, Action entityProcessed )
		{
			foreach( Spline spline in TrackEntities( dxfDoc.Entities.Splines, entityProcessed ) ) {
				TopoDS_Edge edge = SplineToEdge( spline );
				if( edge != null && !edge.IsNull() )
					shapes.Add( edge );
			}
		}

		TopoDS_Shape ConnectEdgesToWires( TopoDS_Shape edgeCompound )
		{
			// Collect edges into HSequenceOfShape
			TopTools_HSequenceOfShape edgeSeq = new TopTools_HSequenceOfShape();
			TopExp_Explorer edgeExp = new TopExp_Explorer( edgeCompound, TopAbs_ShapeEnum.TopAbs_EDGE );
			for( ; edgeExp.More(); edgeExp.Next() ) {
				edgeSeq.Append( edgeExp.Current() );
			}

			if( edgeSeq.Length() == 0 )
				return edgeCompound;

			// Use static ConnectEdgesToWires to build wires
			TopTools_HSequenceOfShape wireSeq = new TopTools_HSequenceOfShape();
			ShapeAnalysis_FreeBounds.ConnectEdgesToWires( edgeSeq, 1e-3, false, wireSeq );

			if( wireSeq.Length() == 0 )
				return edgeCompound;

			List<TopoDS_Shape> wires = new List<TopoDS_Shape>();
			for( int i = 1; i <= wireSeq.Length(); i++ ) {
				wires.Add( wireSeq.Value( i ) );
			}

			return ShapeTool.MakeCompound( wires );
		}

		TopoDS_Edge EntityToEdge( EntityObject entity )
		{
			if( entity is netDxf.Entities.Line line ) {
				gp_Pnt p0 = new gp_Pnt( line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z );
				gp_Pnt p1 = new gp_Pnt( line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z );
				if( p0.Distance( p1 ) < 1e-8 )
					return null;
				BRepBuilderAPI_MakeEdge maker = new BRepBuilderAPI_MakeEdge( p0, p1 );
				return maker.IsDone() ? maker.Edge() : null;
			}
			if( entity is Arc arc ) {
				gp_Pnt center = new gp_Pnt( arc.Center.X, arc.Center.Y, arc.Center.Z );
				gp_Dir normal = new gp_Dir( arc.Normal.X, arc.Normal.Y, arc.Normal.Z );
				gp_Ax2 ax2 = new gp_Ax2( center, normal );
				gp_Circ circ = new gp_Circ( ax2, arc.Radius );
				double startRad = arc.StartAngle * Math.PI / 180.0;
				double endRad = arc.EndAngle * Math.PI / 180.0;
				BRepBuilderAPI_MakeEdge maker = new BRepBuilderAPI_MakeEdge( circ, startRad, endRad );
				return maker.IsDone() ? maker.Edge() : null;
			}
			return null;
		}

		TopoDS_Edge SplineToEdge( Spline spline )
		{
			try {
				var ctrlPts = spline.ControlPoints;
				if( ctrlPts == null || ctrlPts.Length < 2 )
					return null;

				int nbPoles = ctrlPts.Length;
				TColgp_Array1OfPnt poles = new TColgp_Array1OfPnt( 1, nbPoles );
				for( int i = 0; i < nbPoles; i++ ) {
					var cp = ctrlPts[ i ];
					poles.SetValue( i + 1, new gp_Pnt( cp.X, cp.Y, cp.Z ) );
				}

				// Build knots and multiplicities
				var knotList = spline.Knots.ToList();
				var uniqueKnots = knotList.Distinct().OrderBy( k => k ).ToList();
				int nbKnots = uniqueKnots.Count;
				TColStd_Array1OfReal knots = new TColStd_Array1OfReal( 1, nbKnots );
				TColStd_Array1OfInteger mults = new TColStd_Array1OfInteger( 1, nbKnots );
				for( int i = 0; i < nbKnots; i++ ) {
					knots.SetValue( i + 1, uniqueKnots[ i ] );
					mults.SetValue( i + 1, knotList.Count( k => k == uniqueKnots[ i ] ) );
				}

				// Weights
				double[] wArr = spline.Weights;
				TColStd_Array1OfReal weights = new TColStd_Array1OfReal( 1, nbPoles );
				for( int i = 0; i < nbPoles; i++ ) {
					weights.SetValue( i + 1, ( wArr != null && i < wArr.Length ) ? wArr[ i ] : 1.0 );
				}

				Geom_BSplineCurve bspline = new Geom_BSplineCurve( poles, weights, knots, mults, (int)spline.Degree, false );
				BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( bspline );
				return edgeMaker.IsDone() ? edgeMaker.Edge() : null;
			}
			catch {
				return null;
			}
		}
	}
}
