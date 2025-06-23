using OCC.AIS;
using OCC.Graphic3d;
using OCC.IFSelect;
using OCC.IGESControl;
using OCC.Quantity;
using OCC.STEPControl;
using OCC.TopAbs;
using OCC.TopoDS;
using OCC.XSControl;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	public enum FileFormat
	{
		BREP = 0,
		STEP = 1,
		IGES = 2
	}

	public enum AddPointType
	{
		CircArcCenter = 0,
		EdgeMidPoint = 1,
	}

	public enum EConstraintType
	{
		Axial,
		AxialParallel,
		Plane,
		PlaneParallel,
	}

	internal static class ViewHelper
	{
		public static AIS_Shape CreatePartAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			aisShape.SetMaterial( aspect );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			Quantity_Color color = new Quantity_Color();
			aisShape.Color( ref color );
			Quantity_NameOfColor colorName = color.Name();
			return aisShape;
		}

		public static AIS_Shape CreateFaceAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY50 ) );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			return aisShape;
		}
	}

	internal class CADEditor
	{
		public CADEditor( Viewer viewer, TreeView treeView )
		{
			if( viewer == null || treeView == null ) {
				throw new ArgumentNullException( "CADEditor consturcting argument null." );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;

			// CAD manager
			m_CADManager = new CADManager();
			m_CADManager.AddCADModelDone += OnAddCADModelDone;

			// default action
			m_DefaultAction = new DefaultAction( m_Viewer, m_TreeView, m_CADManager );
			m_CurrentAction = m_DefaultAction;
			m_DefaultAction.Start();
		}

		// user interface
		Viewer m_Viewer;
		TreeView m_TreeView;

		// CAD manager
		CADManager m_CADManager;

		// action
		ICADAction m_DefaultAction;
		ICADAction m_CurrentAction;

		// APIs
		public Action StartProcess;
		public Action EndProcess;

		public void ImportFile( FileFormat format )
		{
			OpenFileDialog openDialog = new OpenFileDialog();

			// file dialog filter
			string filter = "";
			switch( format ) {
				case FileFormat.BREP:
					filter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
					break;
				case FileFormat.STEP:
					filter = "STEP Files (*.stp *.step)|*.stp; *.step";
					break;
				case FileFormat.IGES:
					filter = "IGES Files (*.igs *.iges)|*.igs; *.iges";
					break;
				default:
					break;
			}
			openDialog.Filter = filter + "|All files (*.*)|*.*";

			// show file dialog
			if( openDialog.ShowDialog() != DialogResult.OK ) {
				return;
			}

			// get the file name
			string szFileName = openDialog.FileName;
			if( string.IsNullOrEmpty( szFileName ) ) {
				return;
			}

			// read file data and show a progress form
			ReadFileData( format, szFileName );
			//using( var progressForm = new ProgressForm() ) {
			//	StartProcess?.Invoke();
			//	progressForm.StartPosition = FormStartPosition.CenterParent;
			//	progressForm.Show();
			//	Application.DoEvents();

			//	// read the file data
			//	ReadFileData( format, szFileName );
			//	progressForm.Close();
			//	EndProcess?.Invoke();
			//}
		}

		public void AddPoint( AddPointType type )
		{
			AddPointAction action = new AddPointAction( m_Viewer, m_TreeView, m_CADManager, type );
			EditActionStart( action );
		}

		public void ThreePointTransform()
		{
			ThreePtTransformAction action = new ThreePtTransformAction( m_Viewer, m_TreeView, m_CADManager );
			EditActionStart( action );
		}

		public void StartManaulTransform()
		{
			ManualTransformAction action = new ManualTransformAction( m_Viewer, m_TreeView, m_CADManager );
			EditActionStart( action );
		}

		public void ApplyManualTransform( EConstraintType type, bool bReverse = false )
		{
			if( m_CurrentAction.ActionType != CADActionType.ManualTransform ) {
				return;
			}
			( (ManualTransformAction)m_CurrentAction ).ApplyTransform( type, bReverse );
		}

		public void EndManualTransform()
		{
			if( m_CurrentAction.ActionType != CADActionType.ManualTransform ) {
				return;
			}
			( (ManualTransformAction)m_CurrentAction ).TransformDone();
		}

		public void StartSelectFace()
		{
			SelectFaceAction action = new SelectFaceAction( m_Viewer, m_TreeView, m_CADManager );
			EditActionStart( action );
		}

		public void SelectD1ContFace()
		{
			if( m_CurrentAction.ActionType != CADActionType.SelectFace ) {
				return;
			}
			( (SelectFaceAction)m_CurrentAction ).SelectD1ContFace();
		}

		public void EndSelectFace()
		{
			if( m_CurrentAction.ActionType != CADActionType.SelectFace ) {
				return;
			}
			( (SelectFaceAction)m_CurrentAction ).SelectDone();
		}

		// manager events
		void OnAddCADModelDone( ShapeData model )
		{
			string szUID = model.UID;
			TopoDS_Shape shape = model.Shape;
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() ) {
				return;
			}

			// update the tree view
			TreeNode newNode = new TreeNode( szUID );
			m_CADManager.TreeNodeMap[ szUID ] = newNode;
			m_TreeView.Nodes.Add( newNode );

			// update the viewer
			AIS_Shape aisShape = ViewHelper.CreatePartAIS( shape );
			m_CADManager.ViewObjectMap[ szUID ] = new ViewObject( aisShape );
			m_Viewer.GetAISContext().Display( aisShape, true );
		}

		// private methods
		void ReadFileData( FileFormat format, string szFileName )
		{
			// read the file
			XSControl_Reader Reader;
			switch( format ) {
				case FileFormat.BREP:
					Reader = new XSControl_Reader();
					break;
				case FileFormat.STEP:
					Reader = new STEPControl_Reader();
					break;
				case FileFormat.IGES:
					Reader = new IGESControl_Reader();
					break;
				default:
					Reader = new XSControl_Reader();
					break;
			}
			IFSelect_ReturnStatus status = Reader.ReadFile( szFileName );

			// check the status
			if( status != IFSelect_ReturnStatus.IFSelect_RetDone ) {
				MessageBox.Show( ToString() + "Error: Import" );
				return;
			}
			Reader.TransferRoots();

			// prevent from empty shape or null shape
			if( Reader.NbShapes() == 0 ) {
				MessageBox.Show( ToString() + "Error: Import" );
				return;
			}
			TopoDS_Shape oneShape = Reader.OneShape();
			if( oneShape == null || oneShape.IsNull() ) {
				MessageBox.Show( ToString() + "Error: Import" );
				return;
			}
			oneShape = ShapeTool.SewShape( new List<TopoDS_Shape>() { oneShape }/*, 1e-1*/ );

			// add the read shape to the manager
			AddToManager( oneShape );
		}

		void AddToManager( TopoDS_Shape oneShape )
		{
			List<TopoDS_Shape> shapeList = ArrangeShapeData( oneShape );
			foreach( TopoDS_Shape shape in shapeList ) {
				if( shape == null || shape.IsNull() ) {
					continue;
				}
				m_CADManager.AddCADModel( shape );
			}
		}

		List<TopoDS_Shape> ArrangeShapeData( TopoDS_Shape oneShape )
		{
			if( oneShape == null || oneShape.IsNull() ) {
				return new List<TopoDS_Shape>();
			}
			if( oneShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_COMPOUND ) {
				return new List<TopoDS_Shape>() { oneShape };
			}
			List<TopoDS_Shape> result = new List<TopoDS_Shape>();
			foreach( TopoDS_Shape subShape in oneShape.elementsAsList ) {
				result.AddRange( ArrangeShapeData( subShape ) );
			}
			return result;
		}

		// edit actions
		void EditActionStart( ICADAction action )
		{
			m_CurrentAction.End();
			m_CurrentAction = action;
			m_CurrentAction.Start();
			m_CurrentAction.EndAction += EditActionEnd;
		}

		void EditActionEnd( ICADAction action )
		{
			if( action == null
				|| action.ActionType == CADActionType.None || action.ActionType == CADActionType.Default ) {
				return;
			}
			m_CurrentAction = m_DefaultAction;
			m_CurrentAction.Start();
		}
	}
}
