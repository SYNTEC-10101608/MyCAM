using MyCAM.Data;
using OCC.AIS;
using OCC.BRep;
using OCC.BRepMesh;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.IFSelect;
using OCC.IGESControl;
using OCC.Poly;
using OCC.Quantity;
using OCC.STEPControl;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopLoc;
using OCC.TopoDS;
using OCC.XSControl;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
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

	internal class CADEditor
	{
		public CADEditor( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
		{
			if( viewer == null || treeView == null || cadManager == null || viewManager == null ) {
				throw new ArgumentNullException( "CADEditor consturcting argument null." );
			}

			// data manager
			m_CADManager = cadManager;
			m_CADManager.PartChanged += OnPartChanged;
			m_CADManager.FeatureAdded += OnFeatureAdded;

			// user interface
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// default action
			m_DefaultAction = new DefaultAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager, ESelectObjectType.Part );

			TestColDet();
		}

		// this is a temp function to test collision detection using bullet tool
		void TestColDet()
		{
			// create a box as shapeA
			BRepPrimAPI_MakeBox boxMaker = new BRepPrimAPI_MakeBox( new gp_Pnt( -50, -50, -50 ), 100, 100, 100 );
			TopoDS_Shape shapeA = boxMaker.Shape();
			AIS_Shape m_ShapeA = new AIS_Shape( shapeA );
			m_ShapeA.SetColor( COLOR_DEFAULT );
			m_ShapeA.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			//m_ShapeA.SetTransparency( 0.5 );
			m_Viewer.GetAISContext().Display( m_ShapeA, true );
			MeshShape( shapeA, out List<double> vertexListA, out List<int> indexListA );

			// create a sphere as shapeB
			BRepPrimAPI_MakeSphere sphereMaker = new BRepPrimAPI_MakeSphere( new gp_Pnt( 0, 0, 0 ), 50 );
			TopoDS_Shape shapeB = sphereMaker.Shape();
			AIS_Shape m_ShapeB = new AIS_Shape( shapeB );
			m_ShapeB.SetColor( COLOR_DEFAULT );
			m_ShapeB.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			//m_ShapeB.SetTransparency( 0.5 );
			m_Viewer.GetAISContext().Display( m_ShapeB, true );
			MeshShape( shapeB, out List<double> vertexListB, out List<int> indexListB );

			// create a cylinder as shapeC
			BRepPrimAPI_MakeCylinder cylinderMaker = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, -300 ), new gp_Dir( 0, 0, 1 ) ), 50, 600 );
			TopoDS_Shape shapeC = cylinderMaker.Shape();
			AIS_Shape m_ShapeC = new AIS_Shape( shapeC );
			m_ShapeC.SetColor( COLOR_DEFAULT );
			m_ShapeC.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_ShapeC.SetTransparency( 0.5 );
			m_Viewer.GetAISContext().Display( m_ShapeC, true );
			MeshShape( shapeC, out List<double> vertexListC, out List<int> indexListC );

			// create list of trsf presenting shape position on each frame
			const int frameCount = 20;
			List<gp_Trsf> trsfAList = new List<gp_Trsf>();
			List<gp_Trsf> trsfBList = new List<gp_Trsf>();
			List<gp_Trsf> trsfCList = new List<gp_Trsf>();
			for( int i = 0; i <= frameCount; i++ ) {
				double pos = -300 + i * 30;
				gp_Trsf trsfA = new gp_Trsf();
				trsfA.SetTranslation( new gp_Vec( -75, 300, 0 ) );
				trsfAList.Add( trsfA );
				gp_Trsf trsfB = new gp_Trsf();
				trsfB.SetTranslation( new gp_Vec( 75, -300, 0 ) );
				trsfBList.Add( trsfB );
				gp_Trsf trsfC = new gp_Trsf();
				gp_Quaternion quat = new gp_Quaternion( new gp_Vec( 1, 0, 0), Math.PI * i / frameCount );
				trsfC.SetRotationPart( quat );
				trsfC.SetTranslationPart( new gp_Vec( pos, 0, 0 ) );
				trsfCList.Add( trsfC );
			}

			// create collision detection tool
			FCLTest fclTest = new FCLTest();
			fclTest.AddModel( "ShapeA", indexListA.ToArray(), vertexListA.ToArray() );
			fclTest.AddModel( "ShapeB", indexListB.ToArray(), vertexListB.ToArray() );
			fclTest.AddModel( "ShapeC", indexListC.ToArray(), vertexListC.ToArray() );

			// check collision at each frame
			List<bool> colDetA = new List<bool>();
			List<bool> colDetB = new List<bool>();
			List<bool> colDetC = new List<bool>();
			for( int i = 0; i < frameCount; i++ ) {

				// check collision
				bool bColAB = fclTest.CheckCollision( "ShapeA", "ShapeB", ConvertTransform( trsfAList[ i ] ), ConvertTransform( trsfBList[ i ] ) );
				bool bColAC = fclTest.CheckCollision( "ShapeA", "ShapeC", ConvertTransform( trsfAList[ i ] ), ConvertTransform( trsfCList[ i ] ) );
				bool bColBC = fclTest.CheckCollision( "ShapeB", "ShapeC", ConvertTransform( trsfBList[ i ] ), ConvertTransform( trsfCList[ i ] ) );
				colDetA.Add( bColAB || bColAC );
				colDetB.Add( bColAB || bColBC );
				colDetC.Add( bColAC || bColBC );
			}

			// simulate frame by frame
			m_Viewer.TopView();
			m_Viewer.KeyDown += ( e ) =>
			{
				if( e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ) {
					if( e.KeyCode == Keys.Down ) {
						m_FrameIndex++;
						if( m_FrameIndex >= frameCount ) {
							m_FrameIndex = 0;
						}
					}
					else if( e.KeyCode == Keys.Up ) {
						m_FrameIndex--;
						if( m_FrameIndex < 0 ) {
							m_FrameIndex = frameCount - 1;
						}
					}

					// apply transformation to shapes
					m_ShapeA.SetLocalTransformation( trsfAList[ m_FrameIndex ] );
					m_ShapeB.SetLocalTransformation( trsfBList[ m_FrameIndex ] );
					m_ShapeC.SetLocalTransformation( trsfCList[ m_FrameIndex ] );

					// show color to indicate collision
					m_ShapeA.SetColor( colDetA[ m_FrameIndex ] ? COLOR_COLDET : COLOR_DEFAULT );
					m_ShapeB.SetColor( colDetB[ m_FrameIndex ] ? COLOR_COLDET : COLOR_DEFAULT );
					m_ShapeC.SetColor( colDetC[ m_FrameIndex ] ? COLOR_COLDET : COLOR_DEFAULT );
					m_Viewer.UpdateView();
				}
			};
		}
		int m_FrameIndex = 0;
		readonly Quantity_Color COLOR_COLDET = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED );
		readonly Quantity_Color COLOR_DEFAULT = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE );

		void MeshShape( TopoDS_Shape shape, out List<double> vertexList, out List<int> indexList )
		{
			vertexList = new List<double>();
			indexList = new List<int>();

			// mesh the shape
			BRepMesh_IncrementalMesh meshMaker = new BRepMesh_IncrementalMesh( shape, 0.1 );
			meshMaker.Perform();
			if( !meshMaker.IsDone() ) {
				MessageBox.Show( "Error: Mesh shape failed." );
				return;
			}

			// get the mesh data
			TopExp_Explorer faceExp = new TopExp_Explorer( shape, TopAbs_ShapeEnum.TopAbs_FACE );
			while( faceExp.More() ) {
				TopoDS_Face face = TopoDS.ToFace( faceExp.Current() );
				int startIndexOfFace = vertexList.Count / 3;

				// get triangulation data
				TopLoc_Location loc = new TopLoc_Location();
				Poly_Triangulation tri = BRep_Tool.Triangulation( face, ref loc );
				for( int i = 1; i <= tri.NbNodes(); i++ ) {
					gp_Pnt p = tri.Node( i );
					vertexList.Add( p.X() );
					vertexList.Add( p.Y() );
					vertexList.Add( p.Z() );
				}

				// the start vertex index of this face
				for( int i = 1; i <= tri.NbTriangles(); i++ ) {
					Poly_Triangle triangle = tri.Triangle( i );
					int index1 = 0;
					int index2 = 0;
					int index3 = 0;
					triangle.Get( ref index1, ref index2, ref index3 );
					indexList.Add( startIndexOfFace + index1 - 1 ); // convert to zero-based index
					indexList.Add( startIndexOfFace + index2 - 1 );
					indexList.Add( startIndexOfFace + index3 - 1 );
				}
				faceExp.Next();
			}
		}

		double[] ConvertTransform( gp_Trsf trsf )
		{
			gp_Mat matR = trsf.GetRotation().GetMatrix();
			gp_XYZ vecT = trsf.TranslationPart();
			double[] result = new double[ 12 ];

			// the rotation part
			result[ 0 ] = matR.Value( 1, 1 );
			result[ 1 ] = matR.Value( 1, 2 );
			result[ 2 ] = matR.Value( 1, 3 );
			result[ 3 ] = matR.Value( 2, 1 );
			result[ 4 ] = matR.Value( 2, 2 );
			result[ 5 ] = matR.Value( 2, 3 );
			result[ 6 ] = matR.Value( 3, 1 );
			result[ 7 ] = matR.Value( 3, 2 );
			result[ 8 ] = matR.Value( 3, 3 );

			// the translation part
			result[ 9 ] = vecT.X();
			result[ 10 ] = vecT.Y();
			result[ 11 ] = vecT.Z();
			return result;
		}

		// user interface
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;

		// data manager
		DataManager m_CADManager;

		// action
		IEditorAction m_DefaultAction;
		IEditorAction m_CurrentAction;

		// editor
		public void EditStart()
		{
			// init tree
			m_TreeView.Nodes.Add( m_ViewManager.PartNode );

			// start default action
			m_CurrentAction = m_DefaultAction;
			m_DefaultAction.Start();
		}

		public void EditEnd()
		{
			// clear tree
			m_TreeView.Nodes.Clear();

			// end all action
			if( m_CurrentAction.ActionType == EditActionType.Default ) {
				m_CurrentAction.End();
			}
			else {
				m_CurrentAction.End();
				m_DefaultAction.End();
			}
		}

		// APIs
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
		}

		public void AddPoint( AddPointType type )
		{
			AddPointAction action = new AddPointAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager, type );
			StartEditAction( action );
		}

		public void ThreePointTransform()
		{
			ThreePtTransformAction action = new ThreePtTransformAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			StartEditAction( action );
		}

		public void StartManaulTransform()
		{
			ManualTransformAction action = new ManualTransformAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			StartEditAction( action );
		}

		public void ApplyManualTransform( EConstraintType type, bool bReverse = false )
		{
			if( m_CurrentAction.ActionType != EditActionType.ManualTransform ) {
				return;
			}
			( (ManualTransformAction)m_CurrentAction ).ApplyTransform( type, bReverse );
		}

		public void EndManualTransform()
		{
			if( m_CurrentAction.ActionType != EditActionType.ManualTransform ) {
				return;
			}
			( (ManualTransformAction)m_CurrentAction ).TransformDone();
		}

		public void GoToCAM()
		{
			//// build CAD data
			//List<CADData> cadDataList = new List<CADData>();
			//foreach( string szID in m_CADManager.PathIDList ) {
			//	PathData pathData = (PathData)m_CADManager.ShapeDataMap[ szID ];
			//	CADData cadData = new CADData( TopoDS.ToWire( pathData.Shape ), pathData.Edge5DList, pathData.Transform );
			//	cadDataList.Add( cadData );
			//}

			//// show CAMEditForm
			//CAMEditForm camEditForm = new CAMEditForm();
			//camEditForm.Size = new System.Drawing.Size( 1200, 800 );
			//CAMEditModel camEditModel = new CAMEditModel( m_CADManager.PartShape, cadDataList );
			//camEditForm.Init( camEditModel );
			//camEditForm.ShowDialog();
			//if( camEditForm.DialogResult != DialogResult.OK ) {
			//	return;
			//}
		}

		// manager events
		void OnPartChanged()
		{
			// clear the tree view and viewer
			m_ViewManager.PartNode.Nodes.Clear();
			m_ViewManager.PathNode.Nodes.Clear();
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}

			// update view manager data
			m_ViewManager.ViewObjectMap.Clear();
			m_ViewManager.TreeNodeMap.Clear();
			foreach( var szNewDataID in m_CADManager.PartIDList ) {
				ShapeData data = m_CADManager.ShapeDataMap[ szNewDataID ];

				// add node to the tree view
				TreeNode node = new TreeNode( data.UID );
				m_ViewManager.PartNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( data.UID, node );

				// add shape to the viewer
				AIS_Shape aisShape = ViewHelper.CreatePartAIS( data.Shape );
				m_ViewManager.ViewObjectMap.Add( data.UID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// update tree view and viewer
			m_ViewManager.PartNode.ExpandAll();
			m_Viewer.UpdateView();
		}

		void OnFeatureAdded( List<string> newFeatureIDs )
		{
			foreach( string szID in newFeatureIDs ) {
				if( string.IsNullOrEmpty( szID ) ) {
					return;
				}

				// add a new node to the tree view
				TreeNode node = new TreeNode( szID );
				m_ViewManager.PartNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( szID, node );

				// add a new shape to the viewer
				AIS_Shape aisShape = ViewHelper.CreateFeatureAIS( m_CADManager.ShapeDataMap[ szID ].Shape );
				m_ViewManager.ViewObjectMap.Add( szID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// update tree view and viewer
			m_ViewManager.PartNode.ExpandAll();
			m_Viewer.UpdateView();
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
			m_CADManager.AddPart( oneShape );
		}

		// edit actions
		void StartEditAction( IEditorAction action )
		{
			// to prevent from non-necessary default action start
			m_IsNextAction = true;

			// end the current action
			m_CurrentAction.End();
			m_IsNextAction = false;

			// start the action
			m_CurrentAction = action;
			m_CurrentAction.Start();
			m_CurrentAction.EndAction += OnEditActionEnd;
		}

		void OnEditActionEnd( IEditorAction action )
		{
			// start default action if all edit actions are done
			if( !m_IsNextAction ) {
				m_CurrentAction = m_DefaultAction;
				m_CurrentAction.Start();
			}
		}

		bool m_IsNextAction = false;
	}
}
