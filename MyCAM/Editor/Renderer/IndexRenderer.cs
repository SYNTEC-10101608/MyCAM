using MyCAM.PathCache;
using MyCAM.Data;
using OCC.AIS;
using OCC.gp;
using OCC.Quantity;
using OCC.TCollection;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;

namespace MyCAM.Editor.Renderer
{
	/// <summary>
	/// Renderer for path index labels
	/// </summary>
	internal class IndexRenderer : CAMRendererBase
	{
		readonly List<AIS_TextLabel> m_IndexList = new List<AIS_TextLabel>();

		public IndexRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void Show( bool bUpdate = false )
		{
			Remove();

			// no need to show
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			// create text label
			int nCurrentIndex = 0;
			foreach( string pathID in m_DataManager.PathIDList ) {
				gp_Pnt location = GetMainPathStartPoint( pathID );
				if( location == null ) {
					continue;
				}
				string szIndex = ( ++nCurrentIndex ).ToString();

				// create text label ais
				AIS_TextLabel textLabel = new AIS_TextLabel();
				textLabel.SetText( new TCollection_ExtendedString( szIndex ) );
				textLabel.SetPosition( location );
				textLabel.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
				textLabel.SetHeight( 20 );
				textLabel.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
				m_IndexList.Add( textLabel );
			}

			// display text label
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_Viewer.GetAISContext().Display( textLabel, false );
				m_Viewer.GetAISContext().Deactivate( textLabel );
			}

			if( bUpdate ) {
				UpdateView();
			}
		}

		public override void Remove( bool bUpdate = false )
		{
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_Viewer.GetAISContext().Remove( textLabel, false );
			}
			m_IndexList.Clear();
			if( bUpdate ) {
				UpdateView();
			}
		}

		gp_Pnt GetMainPathStartPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetMainPathStartPointCache( pathID, out IMainPathStartPointCache startPnt ) ) {
				return null;
			}
			return startPnt.GetMainPathStartCAMPoint().Point;
		}
	}
}
