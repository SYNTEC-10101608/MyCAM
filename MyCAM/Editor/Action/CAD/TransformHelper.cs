using MyCAM.Data;
using OCC.AIS;
using OCC.gp;
using OCCViewer;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class TransformHelper
	{
		public TransformHelper( Viewer viewer, DataManager dataManager, ViewManager viewManager, gp_Trsf trsf )
		{
			m_Viewer = viewer;
			m_DataManager = dataManager;
			m_ViewManager = viewManager;
			m_3PTransform = trsf;
		}

		public void TransformData()
		{
			List<ITransformableObject> transformableList = new List<ITransformableObject>();
			List<string> transformObjIDList = new List<string>();

			// get part to transform
			foreach( string szPartID in m_DataManager.PartIDList ) {
				if( DataGettingHelper.GetTransformableObject( szPartID, out ITransformableObject transformable ) ) {
					transformableList.Add( transformable );
					transformObjIDList.Add( szPartID );
				}
			}

			// get path to transform
			foreach( string szPathID in m_DataManager.PathIDList ) {
				if( DataGettingHelper.GetTransformableObject( szPathID, out ITransformableObject transformable ) ) {
					transformableList.Add( transformable );
					transformObjIDList.Add( szPathID );
				}
			}

			// do transform
			foreach( var oneTransformable in transformableList ) {
				oneTransformable.DoTransform( m_3PTransform );
			}

			// update viewer
			foreach( var szObjID in transformObjIDList ) {
				AIS_Shape oneAIS = AIS_Shape.DownCast( m_ViewManager.ViewObjectMap[ szObjID ].AISHandle );
				if( oneAIS == null || oneAIS.IsNull() ) {
					continue;
				}
				if( !DataGettingHelper.GetShapeObject( szObjID, out IShapeObject szObjShape ) ) {
					continue;
				}
				oneAIS.SetShape( szObjShape.Shape );
				m_Viewer.GetAISContext().Redisplay( oneAIS, false );
			}
			m_Viewer.UpdateView();
		}

		Viewer m_Viewer;
		DataManager m_DataManager;
		ViewManager m_ViewManager;
		gp_Trsf m_3PTransform;
	}
}
