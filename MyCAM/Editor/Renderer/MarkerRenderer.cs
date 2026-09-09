using MyCAM.Data;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.gp;
using OCC.Quantity;
using OCC.TCollection;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor.Renderer
{
	public enum MarkerType
	{
		MicroJoint,
		ExtendedGCode,
		RCorner
	}

	public class MarkerData
	{
		public MarkerType Type
		{
			get; set;
		}

		public gp_Pnt Position
		{
			get; set;
		}

		public string LabelText
		{
			get; set;
		}

		public Quantity_NameOfColor Color
		{
			get; set;
		}
	}

	public class MarkerContext
	{
		public ContourCache ContourCache
		{
			get; set;
		}

		public CraftData CraftData
		{
			get; set;
		}

		public IReadOnlyList<CAMPoint> CAMPointList
		{
			get; set;
		}
	}

	public interface IMarkerProvider
	{
		MarkerType Type
		{
			get;
		}

		IEnumerable<MarkerData> GetMarkers( MarkerContext context );
	}

	public class MicroJointMarkerProvider : IMarkerProvider
	{
		public MarkerType Type
		{
			get
			{
				return MarkerType.MicroJoint;
			}
		}

		public IEnumerable<MarkerData> GetMarkers( MarkerContext context )
		{
			List<MarkerData> markerDataList = new List<MarkerData>();
			if( context?.CAMPointList == null ) {
				return markerDataList;
			}
			foreach( CAMPoint camPoint in context.CAMPointList ) {
				if( camPoint.IsMicroJointStart ) {
					markerDataList.Add( new MarkerData()
					{
						Type = MarkerType.MicroJoint,
						Position = camPoint.Point,
						LabelText = "MJ▲",
						Color = Quantity_NameOfColor.Quantity_NOC_GREEN
					} );
				}
				if( camPoint.IsMicroJointEnd ) {
					markerDataList.Add( new MarkerData()
					{
						Type = MarkerType.MicroJoint,
						Position = camPoint.Point,
						LabelText = "MJ▼",
						Color = Quantity_NameOfColor.Quantity_NOC_RED
					} );
				}
			}
			return markerDataList;
		}
	}

	public class RCornerMarkerProvider : IMarkerProvider
	{
		public MarkerType Type
		{
			get
			{
				return MarkerType.RCorner;
			}
		}

		public IEnumerable<MarkerData> GetMarkers( MarkerContext context )
		{
			List<MarkerData> markerDataList = new List<MarkerData>();
			if( context?.CAMPointList == null || context?.CraftData == null || context?.ContourCache == null ) {
				return markerDataList;
			}

			// use RCornerMap and the cache's CAM-range conversion to find R corner positions
			foreach( RCornerData range in context.CraftData.RCornerMap.Values ) {
				if( !context.ContourCache.TryGetRCornerCAMRange( range, out int machiningStartCamIdx, out int machiningEndCamIdx ) ) {
					continue;
				}

				if( machiningStartCamIdx >= 0 && machiningStartCamIdx < context.CAMPointList.Count ) {
					AddStartMarker( markerDataList, context.CAMPointList[ machiningStartCamIdx ].Point );
				}
				if( machiningEndCamIdx >= 0 && machiningEndCamIdx < context.CAMPointList.Count ) {
					AddEndMarker( markerDataList, context.CAMPointList[ machiningEndCamIdx ].Point );
				}
			}
			return markerDataList;
		}

		static void AddStartMarker( List<MarkerData> list, gp_Pnt position )
		{
			list.Add( new MarkerData()
			{
				Type = MarkerType.RCorner,
				Position = position,
				LabelText = "RC▲",
				Color = Quantity_NameOfColor.Quantity_NOC_GREEN
			} );
		}

		static void AddEndMarker( List<MarkerData> list, gp_Pnt position )
		{
			list.Add( new MarkerData()
			{
				Type = MarkerType.RCorner,
				Position = position,
				LabelText = "RC▼",
				Color = Quantity_NameOfColor.Quantity_NOC_RED
			} );
		}
	}

	public class ExtendedGCodeMarkerProvider : IMarkerProvider
	{
		public MarkerType Type
		{
			get
			{
				return MarkerType.ExtendedGCode;
			}
		}

		public IEnumerable<MarkerData> GetMarkers( MarkerContext context )
		{
			List<MarkerData> markerDataList = new List<MarkerData>();
			if( context?.CraftData == null || context.ContourCache == null ) {
				return markerDataList;
			}
			if( context.CraftData.ExtendedGCodeMap.Count == 0 ) {
				return markerDataList;
			}

			foreach( var kvp in context.CraftData.ExtendedGCodeMap ) {
				ExtendedGCodeType gCodeType = kvp.Key.Item1;
				int? nIndex = kvp.Key.Item2;

				if( !TryGetMarkerPoint( context, gCodeType, nIndex, out gp_Pnt point ) ) {
					continue;
				}
				markerDataList.Add( new MarkerData()
				{
					Type = MarkerType.ExtendedGCode,
					Position = OffsetPosition( point, gCodeType ),
					LabelText = GetLabelText( gCodeType ),
					Color = GetLabelColor( gCodeType )
				} );
			}
			return markerDataList;
		}

		bool TryGetMarkerPoint( MarkerContext context, ExtendedGCodeType gCodeType, int? nIndex, out gp_Pnt point )
		{
			point = null;
			ContourCache contourCache = context.ContourCache;
			List<CAMPoint> mainPathPointList = contourCache.MainPathPointList;
			if( mainPathPointList == null || mainPathPointList.Count == 0 ) {
				return false;
			}

			switch( gCodeType ) {

				// traverse head is the point before lead in, traverse tail is the point after lead out
				case ExtendedGCodeType.TraverseHead:
					point = GetFirstPoint( contourCache.LeadInPointList ) ?? mainPathPointList.First().Point;
					return true;

				case ExtendedGCodeType.TraverseTail:
					point = GetLastPoint( contourCache.LeadOutPointList )
						?? GetLastPoint( contourCache.OverCutPointList )
						?? mainPathPointList.Last().Point;
					return true;

				// path head is the main path start, path tail is the main path end
				case ExtendedGCodeType.PathHead:
					point = mainPathPointList.First().Point;
					return true;

				case ExtendedGCodeType.PathTail:
					point = GetLastPoint( contourCache.OverCutPointList ) ?? mainPathPointList.Last().Point;
					return true;

				// control point index is recorded on CAD index, need to convert to CAM index
				case ExtendedGCodeType.ControlPoint:
					return TryGetControlPoint( contourCache, nIndex, out point );

				default:
					return false;
			}
		}

		bool TryGetControlPoint( ContourCache contourCache, int? nIndex, out gp_Pnt point )
		{
			point = null;
			if( nIndex == null ) {
				return false;
			}

			List<CAMPoint> mainPathPointList = contourCache.MainPathPointList;
			if( mainPathPointList == null || mainPathPointList.Count == 0 ) {
				return false;
			}

			if( SpecialPointIndex.IsClosedPointIndex( nIndex.Value ) ) {
				point = mainPathPointList.Last().Point;
				return true;
			}

			if( !contourCache.CADToCAMIndexMap.TryGetValue( nIndex.Value, out int nCAMIndex ) ) {
				return false;
			}
			if( nCAMIndex < 0 || nCAMIndex >= mainPathPointList.Count ) {
				return false;
			}
			point = mainPathPointList[ nCAMIndex ].Point;
			return true;
		}

		gp_Pnt GetFirstPoint( List<CAMPoint> camPointList )
		{
			if( camPointList == null || camPointList.Count == 0 ) {
				return null;
			}
			return camPointList.First().Point;
		}

		gp_Pnt GetLastPoint( List<CAMPoint> camPointList )
		{
			if( camPointList == null || camPointList.Count == 0 ) {
				return null;
			}
			return camPointList.Last().Point;
		}

		// first version: separate the overlapped labels by a fixed Z offset
		gp_Pnt OffsetPosition( gp_Pnt point, ExtendedGCodeType gCodeType )
		{
			double dOffset_mm = 0;
			switch( gCodeType ) {
				case ExtendedGCodeType.TraverseHead:
					dOffset_mm = TRAVERSE_HEAD_OFFSET;
					break;
				case ExtendedGCodeType.TraverseTail:
					dOffset_mm = TRAVERSE_TAIL_OFFSET;
					break;
				case ExtendedGCodeType.PathHead:
					dOffset_mm = PATH_HEAD_OFFSET;
					break;
				case ExtendedGCodeType.PathTail:
					dOffset_mm = PATH_TAIL_OFFSET;
					break;
				default:
					dOffset_mm = 0;
					break;
			}
			if( dOffset_mm == 0 ) {
				return point;
			}
			return new gp_Pnt( point.X(), point.Y(), point.Z() + dOffset_mm );
		}

		string GetLabelText( ExtendedGCodeType gCodeType )
		{
			switch( gCodeType ) {
				case ExtendedGCodeType.TraverseHead:
					return "G-T▲";
				case ExtendedGCodeType.TraverseTail:
					return "G-T▼";
				case ExtendedGCodeType.PathHead:
					return "G-P▲";
				case ExtendedGCodeType.PathTail:
					return "G-P▼";
				case ExtendedGCodeType.ControlPoint:
					return "G";
				default:
					return string.Empty;
			}
		}

		Quantity_NameOfColor GetLabelColor( ExtendedGCodeType gCodeType )
		{
			switch( gCodeType ) {
				case ExtendedGCodeType.TraverseHead:
				case ExtendedGCodeType.TraverseTail:
					return Quantity_NameOfColor.Quantity_NOC_CYAN1;
				case ExtendedGCodeType.PathHead:
				case ExtendedGCodeType.PathTail:
					return Quantity_NameOfColor.Quantity_NOC_YELLOW;
				default:
					return Quantity_NameOfColor.Quantity_NOC_RED;
			}
		}

		// label offset to avoid overlapping on the same point
		const double TRAVERSE_HEAD_OFFSET = 6.0;
		const double TRAVERSE_TAIL_OFFSET = -6.0;
		const double PATH_HEAD_OFFSET = 3.0;
		const double PATH_TAIL_OFFSET = -3.0;
	}

	internal class MarkerRenderer : CAMRendererBase
	{
		readonly Dictionary<MarkerType, IMarkerProvider> m_ProviderDict = new Dictionary<MarkerType, IMarkerProvider>();
		readonly Dictionary<MarkerType, bool> m_VisibilityDict = new Dictionary<MarkerType, bool>();
		readonly Dictionary<MarkerType, Dictionary<string, List<AIS_InteractiveObject>>> m_MarkerAISDict = new Dictionary<MarkerType, Dictionary<string, List<AIS_InteractiveObject>>>();

		public MarkerRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
			RegisterProvider( new MicroJointMarkerProvider() );
			RegisterProvider( new ExtendedGCodeMarkerProvider() );
			RegisterProvider( new RCornerMarkerProvider() );
		}

		public void RegisterProvider( IMarkerProvider provider )
		{
			if( provider == null ) {
				return;
			}
			m_ProviderDict[ provider.Type ] = provider;
			if( !m_VisibilityDict.ContainsKey( provider.Type ) ) {
				m_VisibilityDict[ provider.Type ] = true;
			}
			if( !m_MarkerAISDict.ContainsKey( provider.Type ) ) {
				m_MarkerAISDict[ provider.Type ] = new Dictionary<string, List<AIS_InteractiveObject>>();
			}
		}

		public void SetMarkerVisible( MarkerType type, bool isVisible, bool bUpdate = false )
		{
			m_VisibilityDict[ type ] = isVisible;
			if( isVisible ) {
				DisplayMarkerType( type );
			}
			else {
				EraseMarkerType( type );
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		public bool IsMarkerVisible( MarkerType type )
		{
			return m_VisibilityDict.TryGetValue( type, out bool isVisible ) && isVisible;
		}

		public override void Show( bool bUpdate = false )
		{
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
			if( m_IsPauseRefreshAndHide || m_IsPauseRefresh ) {
				return;
			}

			Remove( pathIDList );
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			foreach( string pathID in pathIDList ) {
				RefreshPathMarkers( pathID );
			}

			if( bUpdate ) {
				UpdateView();
			}
		}

		public override void Remove( bool bUpdate = false )
		{
			Remove( m_DataManager.PathIDList, bUpdate );
		}

		public void Remove( List<string> pathIDList, bool bUpdate = false )
		{
			foreach( string pathID in pathIDList ) {
				RemovePathMarkers( pathID );
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Trans( gp_Trsf trsf, bool bUpdate = false )
		{
			foreach( var typeKvp in m_MarkerAISDict ) {
				foreach( var pathKvp in typeKvp.Value ) {
					foreach( AIS_InteractiveObject markerAIS in pathKvp.Value ) {
						SetTransformation( markerAIS, trsf );
					}
				}
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Reset( bool bUpdate = false )
		{
			gp_Trsf trsf = new gp_Trsf();
			Trans( trsf, bUpdate );
		}

		public override void SetPauseRefreshAndHide( bool isPause )
		{
			if( m_IsPauseRefreshAndHide == isPause ) {
				return;
			}
			base.SetPauseRefreshAndHide( isPause );
			if( isPause ) {
				foreach( MarkerType type in m_MarkerAISDict.Keys ) {
					EraseMarkerType( type );
				}
			}
			else {
				foreach( MarkerType type in m_MarkerAISDict.Keys ) {
					if( IsMarkerVisible( type ) ) {
						DisplayMarkerType( type );
					}
				}
			}
		}

		bool TryCreateMarkerContext( string pathID, out MarkerContext context )
		{
			context = null;
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return false;
			}

			ContourCache contourCache = null;
			IReadOnlyList<CAMPoint> camPointList = null;
			if( pathType == PathType.Contour ) {
				if( DataGettingHelper.GetContourCacheByID( pathID, out contourCache ) ) {
					camPointList = contourCache.MainPathPointList;
				}
			}
			else if( DataGettingHelper.IsStdPattern( pathType ) ) {
				if( DataGettingHelper.GetStdPatternCacheByID( pathID, out IStdPatternCache stdPatternCache ) ) {
					camPointList = stdPatternCache.KeyCAMPointList?.Cast<CAMPoint>().ToList();
				}
			}

			DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );

			context = new MarkerContext()
			{
				ContourCache = contourCache,
				CraftData = craftData,
				CAMPointList = camPointList
			};
			return true;
		}

		void RefreshPathMarkers( string pathID, gp_Trsf trsf = null )
		{
			if( !TryCreateMarkerContext( pathID, out MarkerContext context ) ) {
				return;
			}
			foreach( var kvp in m_ProviderDict ) {
				RefreshMarkerType( pathID, kvp.Key, context, trsf );
			}
		}

		void RefreshMarkerType( string pathID, MarkerType type, MarkerContext context, gp_Trsf trsf = null )
		{
			if( !m_ProviderDict.TryGetValue( type, out IMarkerProvider provider ) ) {
				return;
			}

			RemovePathMarkers( pathID, type );

			List<AIS_InteractiveObject> markerAISList = new List<AIS_InteractiveObject>();
			foreach( MarkerData markerData in provider.GetMarkers( context ) ) {
				AIS_InteractiveObject markerAIS = CreateAIS( markerData, trsf );
				if( markerAIS == null ) {
					continue;
				}
				markerAISList.Add( markerAIS );
				if( IsMarkerVisible( type ) ) {
					m_Viewer.GetAISContext().Display( markerAIS, false );
					m_Viewer.GetAISContext().Deactivate( markerAIS );
				}
			}
			if( markerAISList.Count > 0 ) {
				m_MarkerAISDict[ type ][ pathID ] = markerAISList;
			}
		}

		AIS_InteractiveObject CreateAIS( MarkerData markerData, gp_Trsf trsf = null )
		{
			if( markerData?.Position == null ) {
				return null;
			}

			AIS_TextLabel label = new AIS_TextLabel();
			label.SetText( new TCollection_ExtendedString( markerData.LabelText ) );
			label.SetColor( new Quantity_Color( markerData.Color ) );
			label.SetPosition( markerData.Position );
			label.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );

			if( trsf != null ) {
				SetTransformation( label, trsf );
			}

			return label;
		}

		void DisplayMarkerType( MarkerType type )
		{
			if( !m_MarkerAISDict.TryGetValue( type, out var pathAISDict ) ) {
				return;
			}
			foreach( var kvp in pathAISDict ) {
				foreach( AIS_InteractiveObject markerAIS in kvp.Value ) {
					m_Viewer.GetAISContext().Display( markerAIS, false );
					m_Viewer.GetAISContext().Deactivate( markerAIS );
				}
			}
		}

		void EraseMarkerType( MarkerType type )
		{
			if( !m_MarkerAISDict.TryGetValue( type, out var pathAISDict ) ) {
				return;
			}
			foreach( var kvp in pathAISDict ) {
				foreach( AIS_InteractiveObject markerAIS in kvp.Value ) {
					m_Viewer.GetAISContext().Erase( markerAIS, false );
				}
			}
		}

		void RemovePathMarkers( string pathID )
		{
			foreach( MarkerType type in m_MarkerAISDict.Keys.ToList() ) {
				RemovePathMarkers( pathID, type );
			}
		}

		void RemovePathMarkers( string pathID, MarkerType type )
		{
			if( !m_MarkerAISDict.TryGetValue( type, out var pathAISDict ) ) {
				return;
			}
			if( !pathAISDict.TryGetValue( pathID, out List<AIS_InteractiveObject> markerAISList ) ) {
				return;
			}
			foreach( AIS_InteractiveObject markerAIS in markerAISList ) {
				m_Viewer.GetAISContext().Remove( markerAIS, false );
			}
			pathAISDict.Remove( pathID );
		}

		private void SetTransformation( AIS_InteractiveObject markerAIS, gp_Trsf trsf )
		{
			if( markerAIS == null || trsf == null ) {
				return;
			}
			markerAIS.SetLocalTransformation( trsf );
		}
	}
}
