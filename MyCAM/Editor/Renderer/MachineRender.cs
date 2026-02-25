using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.Simulation;
using OCC.AIS;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Poly;
using OCC.Quantity;
using OCC.TColStd;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;


namespace MyCAM.Editor.Renderer
{
	internal class MachineRender : CAMRendererBase
	{
		readonly Dictionary<MachineComponentType, List<AIS_InteractiveObject>> m_MachineAISDict = new Dictionary<MachineComponentType, List<AIS_InteractiveObject>>();
		readonly Dictionary<MachineComponentType, List<gp_Trsf>> m_FrameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
		readonly Dictionary<MachineComponentType, List<bool>> m_FrameCollisionMap = new Dictionary<MachineComponentType, List<bool>>();
		MachineAIS m_MachineAIS = new MachineAIS();

		public MachineRender( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
			GetMeshesListToAIS( dataManager.MachineMeshes );
			BuildLaserAIS();

			// set dictionary after all ais is ready
			SetMachineAIS();
		}

		// let outer know if we have valid AIS to display
		public bool IsWithMachineAIS
		{
			get
			{
				return m_MachineAISDict != null && m_MachineAISDict.Count >= MIN_Machine_Part;
			}
		}

		public void SetSimuData( Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap, Dictionary<MachineComponentType, List<bool>> frameCollisionMap )
		{
			// set transform map
			if( frameTransformMap != null ) {
				m_FrameTransformMap.Clear();
				foreach( var keyValue in frameTransformMap ) {
					List<gp_Trsf> copyTransList = new List<gp_Trsf>( keyValue.Value.Count );
					foreach( var trsf in keyValue.Value ) {
						copyTransList.Add( trsf.MakeCopy() );
					}
					m_FrameTransformMap.Add( keyValue.Key, copyTransList );
				}
			}

			// set collision map
			if( frameCollisionMap != null ) {
				m_FrameCollisionMap.Clear();
				foreach( var keyValue in frameCollisionMap ) {
					var copyCollisionList = new List<bool>( keyValue.Value );
					m_FrameCollisionMap.Add( keyValue.Key, copyCollisionList );
				}
			}
		}

		public override void Show( bool bUpdate = false )
		{
			if( m_MachineAISDict == null || m_MachineAISDict.Count == 0 ) {
				return;
			}
			foreach( KeyValuePair<MachineComponentType, List<AIS_InteractiveObject>> keyValue in m_MachineAISDict ) {
				if( keyValue.Key == MachineComponentType.UnKnow ) {
					continue;
				}
				List<AIS_InteractiveObject> aisList = keyValue.Value;
				if( aisList == null || aisList.Count == 0 ) {
					continue;
				}

				foreach( AIS_InteractiveObject aisObj in aisList ) {
					if( aisObj == null ) {
						continue;
					}

					// machine part
					if( aisObj is AIS_Triangulation tri ) {
						m_Viewer.GetAISContext().Display( tri, false );
						m_Viewer.GetAISContext().Deactivate( tri );
					}

					// laser light
					else {
						m_Viewer.GetAISContext().Display( aisObj, false );
						m_Viewer.GetAISContext().Deactivate( aisObj );
					}
				}
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void ShowToolVecEditResult( Dictionary<MachineComponentType, List<gp_Trsf>> transMap, bool bUpdate = false )
		{
			foreach( var keyValue in m_MachineAISDict ) {
				MachineComponentType componentType = keyValue.Key;
				if( transMap.TryGetValue( componentType, out var transList ) ) {
					if( transList != null && transList.Count > 0 ) {
						ApplyTransForComponent( componentType, transList.Last() );
					}
				}
			}
		}

		public override void Remove( bool bUpdate = false )
		{
			foreach( KeyValuePair<MachineComponentType, List<AIS_InteractiveObject>> keyValue in m_MachineAISDict ) {
				foreach( AIS_InteractiveObject aisObject in keyValue.Value ) {
					if( aisObject == null ) {
						continue;
					}
					// set ais back to ori coordinate
					ResetAIS( aisObject, keyValue.Key );
					m_Viewer.GetAISContext().Remove( aisObject, false );
				}
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		const int MIN_Machine_Part = 2;

		// set back to initial position and color
		void ResetAIS( AIS_InteractiveObject aisShape, MachineComponentType type )
		{
			if( aisShape == null ) {
				return;
			}
			aisShape.SetLocalTransformation( new gp_Trsf() );
			if( aisShape is AIS_Triangulation tri ) {
				SetMeshColor( tri, type, false );
			}
		}

		public void TransAndSetColor( int frameIndex, bool isNeedUpdate = false )
		{
			TransForm( frameIndex );
			SetColor( frameIndex );
			if( isNeedUpdate ) {
				m_Viewer.UpdateView();
			}
		}

		void SetColor( int frameIndex )
		{
			foreach( var keyValue in m_MachineAISDict ) {
				RefreshComponentColor( keyValue.Key, frameIndex );
			}
		}

		void TransForm( int frameIndex )
		{
			if( m_MachineAISDict == null || m_MachineAISDict.Count == 0 ) {
				return;
			}
			foreach( var keyValue in m_MachineAISDict ) {
				MachineComponentType componentType = keyValue.Key;
				if( m_FrameTransformMap.TryGetValue( componentType, out var transList ) ) {
					if( frameIndex >= 0 && frameIndex < transList.Count ) {
						ApplyTransForComponent( componentType, transList[ frameIndex ] );
					}
				}
			}
		}

		void RefreshComponentColor( MachineComponentType type, int frameIndex )
		{
			if( !m_FrameCollisionMap.ContainsKey( type ) || m_FrameCollisionMap[ type ].Count <= frameIndex ) {
				return;
			}
			bool isCollision = m_FrameCollisionMap[ type ][ frameIndex ];
			foreach( var aisObj in GetMachineShapes( type ) ) {
				AIS_Triangulation tri = aisObj as AIS_Triangulation;
				if( tri != null ) {
					SetMeshColor( tri, type, isCollision );
					m_Viewer.GetAISContext().Redisplay( tri, false );
				}
				else {
					m_Viewer.GetAISContext().Redisplay( aisObj, false );
				}
			}
		}

		void SetMeshColor( AIS_Triangulation meshAIS, MachineComponentType type, bool isCollision )
		{
			if( meshAIS == null ||
				meshAIS.GetTriangulation() == null ||
				type == MachineComponentType.UnKnow ) {
				return;
			}
			Quantity_Color color;
			double transparency;

			if( isCollision ) {
				color = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED );
				transparency = 0.0;
			}
			else {
				color = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_LIGHTGRAY );
				transparency = 0.4;
			}
			int R = (int)( color.Red() * 255 );
			int G = (int)( color.Green() * 255 );
			int B = (int)( color.Blue() * 255 );
			int colorValue = ( B << 16 ) | ( G << 8 ) | R;
			int nbNodes = meshAIS.GetTriangulation().NbNodes();
			var colorArray = new TColStd_HArray1OfInteger( 1, nbNodes, colorValue );
			meshAIS.SetColors( colorArray );
			meshAIS.SetTransparency( transparency );
		}

		void ApplyTransForComponent( MachineComponentType type, gp_Trsf trsf )
		{
			foreach( var aisList in GetMachineShapes( type ) ) {
				aisList?.SetLocalTransformation( trsf );
			}
		}

		IEnumerable<AIS_InteractiveObject> GetMachineShapes( MachineComponentType type )
		{
			if( m_MachineAISDict.TryGetValue( type, out var list ) ) {
				return list;
			}
			return Enumerable.Empty<AIS_InteractiveObject>();
		}

		#region convert mesh to AIS

		void GetMeshesListToAIS( MachineMeshes machineMeshes )
		{
			MachineAIS machineAppearance = new MachineAIS();
			if( machineMeshes == null ) {
				AnnounceWrning( MeshesToAISResult.ArgumentNull );
				return;
			}
			foreach( KeyValuePair<MachineComponentType, Poly_Triangulation> keyValue in machineMeshes.Meshes ) {

				// do not have mash data
				if( keyValue.Value == null ) {
					AnnounceWrning( MeshesToAISResult.InvalidMesh, keyValue.Key );
					continue;
				}
				if( machineAppearance.AISList.ContainsKey( keyValue.Key ) == false ) {
					AnnounceWrning( MeshesToAISResult.InvalidMesh, keyValue.Key );
					continue;
				}
				machineAppearance.AISList[ keyValue.Key ] = ConvertMeshToAIS( keyValue.Value );
			}
			m_MachineAIS = machineAppearance;
		}

		void BuildLaserAIS()
		{
			BRepPrimAPI_MakeCylinder makeTool1 = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, -1 ) ), 0.2, m_DataManager.MachineData.ToolLength );
			AIS_Shape LaserAIS = new AIS_Shape( makeTool1.Shape() );
			LaserAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			LaserAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PURPLE ) );
			LaserAIS.SetTransparency( 0.8f );
			m_MachineAIS.AISList[ MachineComponentType.Laser ] = LaserAIS;
		}

		enum MeshesToAISResult
		{
			Success,
			ArgumentNull,
			InvalidMesh,
			UnknownError
		}

		AIS_Triangulation ConvertMeshToAIS( Poly_Triangulation mesh )
		{
			if( mesh == null || mesh.NbNodes() <= 0 || mesh.NbTriangles() <= 0 ) {
				return null;
			}
			AIS_Triangulation resultAIS = new AIS_Triangulation( mesh );

			// set material aspect, this matter since the default material gives wrong color effect
			Graphic3d_MaterialAspect baseAspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NameOfMaterial_UserDefined );
			resultAIS.SetMaterial( baseAspect );
			return resultAIS;
		}

		void AnnounceWrning( MeshesToAISResult result, MachineComponentType componentType = MachineComponentType.UnKnow )
		{
			if( result != MeshesToAISResult.Success ) {
				switch( result ) {
					case MeshesToAISResult.ArgumentNull:
						MyApp.Logger.ShowOnLogPanel( $"機構網格資訊錯誤", MyApp.NoticeType.Warning );
						break;
					case MeshesToAISResult.InvalidMesh:
						//MyApp.Logger.ShowOnLogPanel( $"{componentType} 原始網格資訊讀取失敗", MyApp.NoticeType.Warning );
						break;
					case MeshesToAISResult.UnknownError:
					default:
						//MyApp.Logger.ShowOnLogPanel( $"{componentType} STL讀取失敗，發生未知錯誤", MyApp.NoticeType.Warning );
						break;
				}
				return;
			}
		}

		void SetMachineAIS()
		{
			if( m_MachineAIS == null || m_MachineAIS.AISList == null || m_MachineAIS.AISList.Count == 0 ) {
				return;
			}
			m_MachineAISDict.Clear();
			foreach( KeyValuePair<MachineComponentType, AIS_InteractiveObject> kv in m_MachineAIS.AISList ) {
				if( kv.Value == null ) {
					continue;
				}
				m_MachineAISDict[ kv.Key ] = new List<AIS_InteractiveObject> { kv.Value };
			}
		}

		#endregion
	}
}
