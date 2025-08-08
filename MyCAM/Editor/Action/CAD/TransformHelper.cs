using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCCViewer;
using MyCAM.Data;

namespace MyCAM.Editor
{
	internal class TransformHelper
	{
		public TransformHelper( Viewer viewer, DataManager cadManager, ViewManager viewManager, gp_Trsf trsf )
		{
			m_Viewer = viewer;
			m_CADManager = cadManager;
			m_ViewManager = viewManager;
			m_3PTransform = trsf;
		}

		public void TransformData()
		{
			foreach( var oneData in m_CADManager.ShapeDataMap ) {
				oneData.Value.DoTransform( m_3PTransform );
				AIS_Shape oneAIS = AIS_Shape.DownCast( m_ViewManager.ViewObjectMap[ oneData.Key ].AISHandle );
				if( oneAIS == null || oneAIS.IsNull() ) {
					continue;
				}
				oneAIS.SetShape( oneData.Value.Shape );
				m_Viewer.GetAISContext().Redisplay( oneAIS, false );
			}
			m_Viewer.UpdateView();
		}

		Viewer m_Viewer;
		DataManager m_CADManager;
		ViewManager m_ViewManager;
		gp_Trsf m_3PTransform;
	}
}
