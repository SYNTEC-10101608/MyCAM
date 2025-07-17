using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCCViewer;

namespace MyCAM.CAD
{
	internal class TransformHelper
	{
		public TransformHelper( Viewer viewer, CADManager cadManager, gp_Trsf trsf )
		{
			m_Viewer = viewer;
			m_CADManager = cadManager;
			m_3PTransform = trsf;
		}

		public void TransformData()
		{
			BRepBuilderAPI_Transform partTr = new BRepBuilderAPI_Transform( m_CADManager.PartShape, m_3PTransform );
			m_CADManager.PartShape = partTr.Shape();
			foreach( var oneData in m_CADManager.ShapeDataMap ) {
				oneData.Value.DoTransform( m_3PTransform );
				AIS_Shape oneAIS = AIS_Shape.DownCast( m_CADManager.ViewObjectMap[ oneData.Key ].AISHandle );
				if( oneAIS == null || oneAIS.IsNull() ) {
					continue;
				}
				oneAIS.SetShape( oneData.Value.Shape );
				m_Viewer.GetAISContext().Redisplay( oneAIS, false );
			}
			m_Viewer.UpdateView();
		}

		Viewer m_Viewer;
		CADManager m_CADManager;
		gp_Trsf m_3PTransform;
	}
}
