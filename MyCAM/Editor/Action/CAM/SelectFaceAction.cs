using MyCAM.Data;
using OCC.AIS;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SelectFaceAction : KeyMouseActionBase
	{
		class FaceHandle
		{
			public TopoDS_Face Face
			{
				get; set;
			}

			public AIS_Shape AIS
			{
				get; set;
			}
		}

		public SelectFaceAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			double d1ThresholdDegrees = DEFAULT_D1_THRESHOLD_DEGREES )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_D1ToleranceRadians = d1ThresholdDegrees * System.Math.PI / 180.0;
			m_VisibleFaceAISPairList = new List<FaceHandle>();
			m_EdgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				if( !DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObject ) ) {
					continue;
				}
				TopoDS_Shape oneshape = shapeObject.Shape;

				// collect all faces
				TopExp_Explorer exp = new TopExp_Explorer( oneshape, TopAbs_ShapeEnum.TopAbs_FACE );
				for( ; exp.More(); exp.Next() ) {
					TopoDS_Face face = TopoDS.ToFace( exp.Current() );
					AIS_Shape aisShape = SelectViewHelper.CreateFaceAIS( face );
					m_VisibleFaceAISPairList.Add( new FaceHandle() { Face = face, AIS = aisShape } );
				}

				// add into map
				TopExp.MapShapesAndAncestors( oneshape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref m_EdgeFaceMap );
			}
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SelectFace;
			}
		}

		// update D1 continuity tolerance in real time (degrees), e.g. when user edits the AddPath dialog's D1 threshold
		public void SetD1ThresholdDegrees( double d1ThresholdDegrees )
		{
			m_D1ToleranceRadians = d1ThresholdDegrees * System.Math.PI / 180.0;
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// disable tree view
			m_TreeView.Enabled = false;

			// hide all shape
			m_ViewManager.EraseAll();

			// show part for selction
			ShowPart();
			m_Viewer.UpdateView();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// enable tree view
			m_TreeView.Enabled = true;

			// show all shape ( path is not allow to hide )
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_ViewManager.DisplayPart( partID );
			}
			m_ViewManager.DisplayPaths( m_DataManager.PathIDList );
			m_ViewManager.DeactiveAll();

			// hide part for selction
			HidePart();
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( m_isFrozen ) {
				return;
			}

			if( e.Button == MouseButtons.Left ) {
				if( m_Viewer.GetAISContext().DetectedOwner().IsNull()
						|| m_Viewer.GetAISContext().DetectedOwner().HasSelectable() == false ) {
					if( ( Control.ModifierKeys & Keys.Control ) != Keys.Control ) {
						m_Viewer.GetAISContext().ClearSelected( true );
						SelectionChanged?.Invoke();
					}
					return;
				}
				AIS_InteractiveObject detectedObject = m_Viewer.GetAISContext().DetectedInteractive();
				if( ( Control.ModifierKeys & Keys.Control ) != Keys.Control ) {
					m_Viewer.GetAISContext().ClearSelected( false );
				}
				m_Viewer.GetAISContext().AddOrRemoveSelected( detectedObject, false );
				m_Viewer.UpdateView();
				SelectionChanged?.Invoke();
			}
		}

		protected override void ViewerMouseDoubleClick( MouseEventArgs e )
		{
			if( m_isFrozen ) {
				return;
			}
			if( e.Button == MouseButtons.Left ) {
				if( m_Viewer.GetAISContext().DetectedOwner().IsNull()
					|| m_Viewer.GetAISContext().DetectedOwner().HasSelectable() == false ) {
					return;
				}
				AIS_InteractiveObject detectedObject = m_Viewer.GetAISContext().DetectedInteractive();
				SelectD1ContFaceFromDetected( detectedObject );
				SelectionChanged?.Invoke();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			// Since this action is trigger by Addpath dlg ,no need to leave by Esc
		}

		void SelectD1ContFaceFromDetected( AIS_InteractiveObject detectedObject )
		{
			// get the face from the detected AIS object
			AIS_Shape detectedAISShape = AIS_Shape.DownCast( detectedObject );
			if( detectedAISShape == null || detectedAISShape.IsNull() ) {
				return;
			}
			TopoDS_Shape detectedShape = detectedAISShape.Shape();
			if( detectedShape == null || detectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_FACE ) {
				return;
			}
			TopoDS_Face detectedFace = TopoDS.ToFace( detectedShape );

			// BFS from only the detected face
			List<TopoDS_Face> d1ContinuousFaceList = BFSFindD1ContFaces( new List<TopoDS_Face> { detectedFace } );

			// clear current selection if Ctrl is not held
			if( ( Control.ModifierKeys & Keys.Control ) != Keys.Control ) {
				m_Viewer.GetAISContext().ClearSelected( false );
			}
			else {
				// undo the single-click toggle on the detected face
				m_Viewer.GetAISContext().AddOrRemoveSelected( detectedAISShape, false );
			}

			// select all D1 continuous faces
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				foreach( TopoDS_Face oneFace in d1ContinuousFaceList ) {
					if( faceAISPair.Face.IsEqual( oneFace ) ) {
						m_Viewer.GetAISContext().AddOrRemoveSelected( faceAISPair.AIS, false );
					}
				}
			}
			m_Viewer.UpdateView();
		}

		// BFS core: find all D1 continuous faces starting from a set of seed faces
		List<TopoDS_Face> BFSFindD1ContFaces( List<TopoDS_Face> faceBFSQueue )
		{
			return GeometryTool.FindD1ContinuousFaces( faceBFSQueue, m_EdgeFaceMap, m_D1ToleranceRadians );
		}

		public List<TopoDS_Shape> GetResult()
		{
			// delegate to GetResultFromFaces with the current viewer selection
			return GetResultFromFaces( GetSelectedFace() );
		}

		// sew the input face list and return the resulting shape list (could be single shell, multiple shells, or some free faces)
		public List<TopoDS_Shape> GetResultFromFaces( List<TopoDS_Face> extractedFaceList )
		{
			List<TopoDS_Shape> faceGroupList = new List<TopoDS_Shape>();
			if( extractedFaceList == null || extractedFaceList.Count == 0 ) {
				return faceGroupList;
			}

			// sew the faces
			TopoDS_Shape sewResult = ShapeTool.SewShape( extractedFaceList.Cast<TopoDS_Shape>().ToList() );

			// single shell or single face
			if( sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_SHELL
				|| sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_FACE ) {
				faceGroupList.Add( sewResult );
			}

			// some shell and free face exist
			else {
				foreach( TopoDS_Shape shape in sewResult.elementsAsList ) {
					faceGroupList.Add( shape );
				}
			}
			return faceGroupList;
		}

		void ShowPart()
		{
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				m_Viewer.GetAISContext().Display( faceAISPair.AIS, false );
				m_Viewer.GetAISContext().Activate( faceAISPair.AIS, (int)AISActiveMode.Face );
			}
		}

		// Deactivate all face AIS so the user can no longer click/select, but keep the highlight visible.
		public void Freeze()
		{
			m_isFrozen = true;
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				m_Viewer.GetAISContext().Deactivate( faceAISPair.AIS );
			}
			m_Viewer.UpdateView();
		}

		// Reactivate face AIS so the user can click/select again.
		public void Unfreeze()
		{
			m_isFrozen = false;
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				m_Viewer.GetAISContext().Activate( faceAISPair.AIS, (int)AISActiveMode.Face );
			}
			m_Viewer.UpdateView();
		}

		void HidePart()
		{
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				m_Viewer.GetAISContext().Remove( faceAISPair.AIS, false );
			}
		}

		List<TopoDS_Face> GetSelectedFace()
		{
			List<TopoDS_Face> selectedFaceList = new List<TopoDS_Face>();
			m_Viewer.GetAISContext().InitSelected();
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				TopoDS_Shape shape = m_Viewer.GetAISContext().SelectedShape();
				if( shape == null || shape.ShapeType() != TopAbs_ShapeEnum.TopAbs_FACE ) {
					m_Viewer.GetAISContext().NextSelected();
					continue;
				}
				selectedFaceList.Add( TopoDS.ToFace( shape ) );
				m_Viewer.GetAISContext().NextSelected();
			}
			return selectedFaceList;
		}

		List<FaceHandle> m_VisibleFaceAISPairList;
		TopTools_IndexedDataMapOfShapeListOfShape m_EdgeFaceMap;
		bool m_isFrozen = false;

		// Fired whenever the face selection changes (click or double-click).
		public event System.Action SelectionChanged;

		// True when at least one face is currently selected.
		public bool HasSelection => GetSelectedFace().Count > 0;

		// D1 tolerance in radians
		double m_D1ToleranceRadians;

		const double DEFAULT_D1_THRESHOLD_DEGREES = 5.0;
	}
}
