using MyCAM.Data;
using MyCAM.Helper.Simulation;
using OCC.AIS;
using OCC.gp;
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

		public MachineRender( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public void SetMachineAIS( MachineAIS machineAIS )
		{
			if( machineAIS == null || machineAIS.AISList == null || machineAIS.AISList.Count == 0 ) {
				return;
			}
			m_MachineAISDict.Clear();
			foreach( KeyValuePair<MachineComponentType, AIS_InteractiveObject> kv in machineAIS.AISList ) {
				m_MachineAISDict[ kv.Key ] = kv.Value != null
											? new List<AIS_InteractiveObject> { kv.Value }
											: new List<AIS_InteractiveObject>();
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
						ResetAIS( tri, keyValue.Key );
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

		public override void Remove( bool bUpdate = false )
		{
			foreach( KeyValuePair<MachineComponentType, List<AIS_InteractiveObject>> keyValue in m_MachineAISDict ) {
				foreach( AIS_InteractiveObject aisObject in keyValue.Value ) {
					if( aisObject == null ) {
						continue;
					}
					m_Viewer.GetAISContext().Remove( aisObject, false );
				}
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		// set back to initial position and color
		void ResetAIS( AIS_Triangulation aisShape, MachineComponentType type )
		{
			if( aisShape == null ) {
				return;
			}
			aisShape.SetLocalTransformation( new gp_Trsf() );
			SetMeshColor( aisShape, type, false );
		}

		public void TransAndSetColor( int frameIndex )
		{
			TransForm( frameIndex );
			SetColor( frameIndex );
			m_Viewer.UpdateView();
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
	}
}
