// Created: 2006-06-22 
// 
// Copyright (c) 2006-2013 OPEN CASCADE SAS 
// 
// This file is part of commercial software by OPEN CASCADE SAS. 
// 
// This software is furnished in accordance with the terms and conditions 
// of the contract and with the inclusion of this copyright notice. 
// This software or any other copy thereof may not be provided or otherwise 
// be made available to any third party. 
// No ownership title to the software is transferred hereby. 
// 
// OPEN CASCADE SAS makes no representation or warranties with respect to the 
// performance of this software, and specifically disclaims any responsibility 
// for any damages, special or consequential, connected with its use. 

/* ----------------------------------------------------------------------------
 * This class demonstrates direct usage of wrapped Open CASCADE classes from 
 * C# code. Compare it with the C++ OCCViewer class in standard C# sample.
 * ----------------------------------------------------------------------------- */

using OCC.AIS;
using OCC.Aspect;
using OCC.gp;
using OCC.Graphic3d;
using OCC.OpenGl;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.Standard;
using OCC.V3d;
using OCC.WNT;
using OCCTool;
using System;
using System.Windows.Forms;

namespace OCCViewer
{
	public class Viewer : IDisposable
	{
		V3d_Viewer myViewer;
		V3d_View myView;
		AIS_InteractiveContext myAISContext;
		Graphic3d_GraphicDriver myGraphicDriver;

		const int SELECT_PixelTolerance = 5;

		public bool InitViewer( Control control )
		{
			// init viewer
			try {
				Aspect_DisplayConnection aDisplayConnection = new Aspect_DisplayConnection();
				myGraphicDriver = new OpenGl_GraphicDriver( aDisplayConnection );
			}
			catch( Exception ) {
				return false;
			}

			myViewer = new V3d_Viewer( myGraphicDriver );
			myViewer.SetDefaultLights();
			myViewer.SetLightOn();
			myView = myViewer.CreateView();
			WNT_Window aWNTWindow = new WNT_Window( control.Handle );
			myView.SetWindow( aWNTWindow );
			if( !aWNTWindow.IsMapped() ) {
				aWNTWindow.Map();
			}
			myAISContext = new AIS_InteractiveContext( myViewer );
			myAISContext.UpdateCurrentViewer();
			myView.Redraw();
			myView.MustBeResized();

			// set background color to black
			myView.SetBackgroundColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY90 ) );

			// LASER-1073 make an Axis triedron which fixed on panel LEFT_LOWER
			myView.ZBufferTriedronSetup( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ),
										new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ),
										new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE1 ), 0.8, 0.02, 20 );
			myView.TriedronDisplay( Aspect_TypeOfTriedronPosition.Aspect_TOTP_LEFT_LOWER,
									new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ),
									0.2, V3d_TypeOfVisualization.V3d_ZBUFFER );

			// expand the range of clicking path
			myAISContext.SelectionManager().Selector().SetPixelTolerance( SELECT_PixelTolerance );

			// set detecting mode color and style
			SetDrawerAttribute( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_Dynamic, Quantity_NameOfColor.Quantity_NOC_ORANGE, 0.5f );

			// set selection mode color and style
			SetDrawerAttribute( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_Selected, Quantity_NameOfColor.Quantity_NOC_GREEN );

			// set local detecting mode color and style
			SetDrawerAttribute( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_LocalDynamic, Quantity_NameOfColor.Quantity_NOC_ORANGE, 0.5f, true, true );

			// set local detecting mode color and style
			SetDrawerAttribute( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_LocalSelected, Quantity_NameOfColor.Quantity_NOC_GREEN, 0, true, true );

			Graphic3d_RenderingParams.CRef cref = myView.ChangeRenderingParams();
			cref.Ptr.NbMsaaSamples = 10000;

			// viewer mouse action
			KeyMouseActionEnabled = true;
			control.MouseWheel += ( sender, e ) => OnMouseWheel( e );
			control.MouseDown += ( sender, e ) =>
			{
				control.Focus();
				OnMouseDown( e );
			};
			control.MouseUp += ( sender, e ) => OnMouseUp( e );
			control.MouseMove += ( sender, e ) => OnMouseMove( e );
			control.MouseClick += ( sender, e ) =>
			{
				control.Focus();
				OnMouseClick( e );
			};
			control.MouseDoubleClick += ( sender, e ) => OnMouseDoubleClick( e );
			control.KeyDown += ( sender, e ) => OnKeyDown( e );
			control.Paint += ( sender, e ) => UpdateView();
			return true;
		}

		~Viewer()
		{
			Dispose();
		}

		public V3d_View GetView()
		{
			return myView;
		}

		public virtual void Dispose()
		{
			myAISContext?.Dispose();
			myAISContext = null;
			myView?.Dispose();
			myView = null;
			myViewer?.Dispose();
			myViewer = null;
			myGraphicDriver?.Dispose();
			myGraphicDriver = null;
		}

		public bool Dump( string filename )
		{
			if( myView != null ) {
				myView.Redraw();
				return myView.Dump( filename );
			}
			return false;
		}

		public void RedrawView()
		{
			myView?.Redraw();
		}

		public void UpdateView()
		{
			myView?.MustBeResized();
		}

		public void SetDegenerateModeOn()
		{
			if( myView != null ) {
				myView.SetComputedMode( false );
				myView.Redraw();
			}
		}

		public void SetDegenerateModeOff()
		{
			if( myView != null ) {
				myView.SetComputedMode( true );
				myView.Redraw();
			}
		}

		public void WindowFitAll( int Xmin, int Ymin, int Xmax, int Ymax )
		{
			myView?.WindowFitAll( Xmin, Ymin, Xmax, Ymax );
		}

		public void Place( int x, int y, float zoomFactor )
		{
			myView?.Place( x, y, zoomFactor );
		}

		public void Zoom( int x1, int y1, int x2, int y2 )
		{
			myView?.Zoom( x1, y1, x2, y2 );
		}

		public void Pan( int x, int y )
		{
			myView?.Pan( x, y );
		}

		public void Rotation( int x, int y )
		{
			myView?.Rotation( x, y );
		}

		public void StartRotation( int x, int y )
		{
			myView?.StartRotation( x, y );
		}

		public void Select( int x1, int y1, int x2, int y2 )
		{
			if( myAISContext != null ) {
				myAISContext.SelectRectangle( new Graphic3d_Vec2i( x1, y1 ), new Graphic3d_Vec2i( x2, y2 ), myView );
				myAISContext.UpdateCurrentViewer();
			}
		}

		public void Select()
		{
			if( myAISContext != null ) {
				myAISContext.SelectDetected();
				myAISContext.UpdateCurrentViewer();
			}
		}

		public void MoveTo( int x, int y )
		{
			if( myAISContext != null && myView != null ) {
				myAISContext.MoveTo( x, y, myView, true );
			}
		}

		public void ShiftSelect( int x1, int y1, int x2, int y2 )
		{
			if( myAISContext != null ) {
				myAISContext.SelectRectangle( new Graphic3d_Vec2i( x1, y1 ), new Graphic3d_Vec2i( x2, y2 ), myView, AIS_SelectionScheme.AIS_SelectionScheme_XOR );
				myAISContext.UpdateCurrentViewer();
			}
		}

		public void ShiftSelect()
		{
			if( myAISContext != null ) {
				myAISContext.SelectDetected( AIS_SelectionScheme.AIS_SelectionScheme_XOR );
				myAISContext.UpdateCurrentViewer();
			}
		}

		public void BackgroundColor( ref int r, ref int g, ref int b )
		{
			if( myView != null ) {
				double R1 = 0;
				double G1 = 0;
				double B1 = 0;
				myView.BackgroundColor( Quantity_TypeOfColor.Quantity_TOC_RGB, ref R1, ref G1, ref B1 );
				r = (int)( R1 * 255 );
				g = (int)( G1 * 255 );
				b = (int)( B1 * 255 );
			}
		}

		public void UpdateCurrentViewer()
		{
			myAISContext?.UpdateCurrentViewer();
		}

		public void FrontView()
		{
			myView?.SetProj( V3d_TypeOfOrientation.V3d_Ypos );
		}

		public void BackView()
		{
			myView?.SetProj( V3d_TypeOfOrientation.V3d_Yneg );
		}

		public void TopView()
		{
			myView?.SetProj( V3d_TypeOfOrientation.V3d_Zpos );
		}

		public void BottomView()
		{
			myView?.SetProj( V3d_TypeOfOrientation.V3d_Zneg );
		}

		public void RightView()
		{
			myView?.SetProj( V3d_TypeOfOrientation.V3d_Xpos );
		}

		public void LeftView()
		{
			myView?.SetProj( V3d_TypeOfOrientation.V3d_Xneg );
		}

		public void AxoView()
		{
			myView?.SetProj( V3d_TypeOfOrientation.V3d_XposYnegZpos );
		}

		public void SetViewDir( gp_Dir dir )
		{
			if( myView != null ) {
				myView.SetProj( dir.X(), dir.Y(), dir.Z() );
			}
		}

		public gp_Dir GetViewDir()
		{
			if( myView != null ) {
				double x = 0, y = 0, z = 0;
				myView.Proj( ref x, ref y, ref z );
				return new gp_Dir( x, y, z );
			}
			return new gp_Dir( 0, 0, 0 );
		}

		public double Scale()
		{
			if( myView != null ) {
				return myView.Scale();
			}
			else {
				return -1;
			}
		}

		public void ZoomAllView()
		{
			if( myView != null ) {
				myView.FitAll();
				myView.ZFitAll();
			}
		}

		public void Reset()
		{
			if( myView != null ) {
				myView.Reset();
			}
		}

		public void SetBackgroundColor( int r, int g, int b )
		{
			myView?.SetBackgroundColor( Quantity_TypeOfColor.Quantity_TOC_RGB, r / 255.0, g / 255.0, b / 255.0 );
		}

		public double GetOCCVersion()
		{
			return Standard_Version.Number();
		}

		public void CreateNewView( IntPtr wnd )
		{
			if( myAISContext == null ) {
				return;
			}
			myView = myAISContext.CurrentViewer().CreateView();
			if( myGraphicDriver == null ) {
				Aspect_DisplayConnection aDisplayConnection = new Aspect_DisplayConnection();
				myGraphicDriver = new OpenGl_GraphicDriver( aDisplayConnection );
			}
			Aspect_Window aWNTWindow = new Aspect_Window( wnd, true );
			myView.SetWindow( aWNTWindow );
			int w = 100, h = 100;
			aWNTWindow.Size( ref w, ref h );
			if( !aWNTWindow.IsMapped() ) {
				aWNTWindow.Map();
			}
		}

		public bool SetAISContext( Viewer Viewer )
		{
			myAISContext = Viewer.GetAISContext();
			return myAISContext != null;
		}

		public AIS_InteractiveContext GetAISContext()
		{
			return myAISContext;
		}

		public void StartZoomAtPoint( int nPointX, int nPointY )
		{
			myView?.StartZoomAtPoint( nPointX, nPointY );
		}

		public void ZoomAtPoint( int nMouseStartX, int nMouseStartY, int nMouseEndX, int nMouseEndY )
		{
			myView?.ZoomAtPoint( nMouseStartX, nMouseStartY, nMouseEndX, nMouseEndY );
		}

		//Convert mouse position(pixel coordinate) to draft coordinate
		public void Convert( int Xp, int Yp, ref double X, ref double Y, ref double Z )
		{
			myView?.Convert( Xp, Yp, ref X, ref Y, ref Z );
		}

		//Convert draft coordinate to pixel coordinate
		public void Convert( double X, double Y, double Z, ref int Xp, ref int Yp )
		{
			myView?.Convert( X, Y, Z, ref Xp, ref Yp );
		}

		public void Eye( ref double eyeX, ref double eyeY, ref double eyeZ )
		{
			if( myView != null ) {
				myView.Eye( ref eyeX, ref eyeY, ref eyeZ );
			}
		}

		public void At( ref double atX, ref double atY, ref double atZ )
		{
			if( myView != null ) {
				myView.At( ref atX, ref atY, ref atZ );
			}
		}

		public void OnVertices()
		{
			myAISContext.Deactivate();

			// 1 means vertex select mode
			myAISContext.Activate( 1 );

			// set 0 selection pixel tolerance in order to be more accurate when selecting Vertex
			myAISContext.SelectionManager().Selector().SetPixelTolerance( 0 );
		}

		public void OnCloseAllContexts()
		{
			myAISContext.Deactivate();

			// active 0 means default select mode
			myAISContext.Activate( 0 );

			// expand selection pixel tolerance in normal selection mode
			myAISContext.SelectionManager().Selector().SetPixelTolerance( SELECT_PixelTolerance );
		}

		// mouse action
		int m_nXMousePosition = 0;
		int m_nYMousePosition = 0;
		const double ZOOM_Ratio = 0.0002;

		public bool KeyMouseActionEnabled
		{
			get; set;
		}

		void OnMouseWheel( MouseEventArgs e )
		{
			MouseWheel?.Invoke( e );
			if( !KeyMouseActionEnabled ) {
				return;
			}

			// zoom viewer at start point
			StartZoomAtPoint( e.X, e.Y );

			int nEndX = (int)( e.X + e.X * e.Delta * ZOOM_Ratio );
			int nEndY = (int)( e.Y + e.Y * e.Delta * ZOOM_Ratio );

			// zoom viewer with mouse wheel delta and scaling ratio
			ZoomAtPoint( e.X, e.Y, nEndX, nEndY );
		}

		void OnMouseDown( MouseEventArgs e )
		{
			MouseDown?.Invoke( e );
			if( !KeyMouseActionEnabled ) {
				return;
			}
			switch( e.Button ) {

				// press down middle button, then start translate the viewer
				case MouseButtons.Middle:
					m_nXMousePosition = e.X;
					m_nYMousePosition = e.Y;
					break;

				// press down right button, then start rotatae the viewer
				case MouseButtons.Right:
					StartRotation( e.X, e.Y );
					break;
				default:
					break;
			}
		}

		void OnMouseUp( MouseEventArgs e )
		{
			MouseUp?.Invoke( e );
		}

		void OnMouseMove( MouseEventArgs e )
		{
			MouseMove?.Invoke( e );
			if( !KeyMouseActionEnabled ) {
				return;
			}
			MoveTo( e.X, e.Y );
			switch( e.Button ) {

				// translate the viewer
				case MouseButtons.Middle:
					Pan( e.X - m_nXMousePosition, m_nYMousePosition - e.Y );
					m_nXMousePosition = e.X;
					m_nYMousePosition = e.Y;
					break;

				// rotate the viewer
				case MouseButtons.Right:
					Rotation( e.X, e.Y );
					break;
				default:
					break;
			}
		}

		void OnMouseClick( MouseEventArgs e )
		{
			MouseClick?.Invoke( e );
		}

		void OnMouseDoubleClick( MouseEventArgs e )
		{
			MouseDoubleClick?.Invoke( e );
		}

		void OnKeyDown( KeyEventArgs e )
		{
			KeyDown?.Invoke( e );
			if( !KeyMouseActionEnabled ) {
				return;
			}

			// handle key down events
			switch( e.KeyCode ) {
				case Keys.F5:
					AxoView();
					ZoomAllView();
					UpdateView();
					break;
			}
		}

		void SetDrawerAttribute( Prs3d_TypeOfHighlight type, Quantity_NameOfColor color, double transparency = 0, bool considerWire = false, bool considerPoint = false )
		{
			double WIRE_WIDTH = 2.0;
			double POINT_SCALE = 3.0;
			Prs3d_Drawer drawer = myAISContext.HighlightStyle( type );
			drawer.SetDisplayMode( (int)HightlightDisplayMode.FaceAndWireFrame );
			drawer.SetColor( new Quantity_Color( color ) );
			drawer.SetTransparency( (float)transparency );
			if( considerWire ) {
				drawer.SetWireAspect( new Prs3d_LineAspect( new Quantity_Color( color ), Aspect_TypeOfLine.Aspect_TOL_SOLID, WIRE_WIDTH ) );
			}
			if( considerPoint ) {
				drawer.SetPointAspect( new Prs3d_PointAspect( Aspect_TypeOfMarker.Aspect_TOM_BALL, new Quantity_Color( color ), POINT_SCALE ) );
			}
			myAISContext.SetHighlightStyle( type, drawer );
		}

		public Action<MouseEventArgs> MouseWheel;
		public Action<MouseEventArgs> MouseDown;
		public Action<MouseEventArgs> MouseUp;
		public Action<MouseEventArgs> MouseMove;
		public Action<MouseEventArgs> MouseClick;
		public Action<MouseEventArgs> MouseDoubleClick;
		public Action<KeyEventArgs> KeyDown;
	}
}
