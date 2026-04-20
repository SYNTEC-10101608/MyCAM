using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System.Collections.Generic;

namespace MyCAM.Editor.Renderer
{
    /// <summary>
    /// Renderer for contour edit control points.
    /// Displays CAD-space marks and (when offset exists) CAM-space marks with a connecting line.
    /// </summary>
    internal class EditPointRenderer : ICAMRenderer
    {
        readonly Viewer m_Viewer;
        readonly string m_PathID;
        bool m_IsShow = true;

        readonly List<AIS_Shape> m_CADMarkList = new List<AIS_Shape>();
        readonly List<AIS_Shape> m_CAMMarkList = new List<AIS_Shape>();
        readonly List<AIS_Line> m_OffsetLineList = new List<AIS_Line>();

        public EditPointRenderer( Viewer viewer, string pathID )
        {
            m_Viewer = viewer;
            m_PathID = pathID;
        }

        public void Show( bool bUpdate = false )
        {
            Remove();

            if( !m_IsShow ) {
                if( bUpdate ) {
                    UpdateView();
                }
                return;
            }

            if( !DataGettingHelper.GetContourCacheByID( m_PathID, out ContourCache contourCache ) ) {
                return;
            }
            if( !DataGettingHelper.GetCraftDataByID( m_PathID, out CraftData craftData ) ) {
                return;
            }

            var aisContext = m_Viewer.GetAISContext();

            foreach( var kvp in craftData.CADPointModifyMap ) {
                int cadIndex = kvp.Key;
                CADPointModifyData modifyData = kvp.Value;

                // get CAD point position
                var cadPointList = contourCache.TrsfCADPointList;
                if( cadIndex < 0 || cadIndex >= cadPointList.Count ) {
                    continue;
                }
                gp_Pnt cadPnt = cadPointList[ cadIndex ].Point;

                // draw CAD mark
                AIS_Shape cadMark = CreatePointMark( cadPnt, CAD_MARK_COLOR );
                aisContext.Display( cadMark, false );
                aisContext.Deactivate( cadMark );
                m_CADMarkList.Add( cadMark );

                // skip CAM mark and line when there is no offset
                bool hasOffset = modifyData.DX != 0 || modifyData.DY != 0 || modifyData.DZ != 0;
                if( !hasOffset ) {
                    continue;
                }

                // get CAM point via CAD¡÷CAM index mapping
                var cadToCAMIndexMap = contourCache.CADToCAMIndexMap;
                if( !cadToCAMIndexMap.ContainsKey( cadIndex ) ) {
                    continue;
                }
                int camIndex = cadToCAMIndexMap[ cadIndex ];
                var camPointList = contourCache.MainPathPointList;
                if( camIndex < 0 || camIndex >= camPointList.Count ) {
                    continue;
                }
                gp_Pnt camPnt = camPointList[ camIndex ].Point;

                // draw CAM mark
                AIS_Shape camMark = CreatePointMark( camPnt, CAM_MARK_COLOR );
                aisContext.Display( camMark, false );
                aisContext.Deactivate( camMark );
                m_CAMMarkList.Add( camMark );

                // draw offset line between CAD and CAM point
                AIS_Line offsetLine = DrawHelper.GetLineAIS( cadPnt, camPnt, OFFSET_LINE_COLOR );
                aisContext.Display( offsetLine, false );
                aisContext.Deactivate( offsetLine );
                m_OffsetLineList.Add( offsetLine );
            }

            if( bUpdate ) {
                UpdateView();
            }
        }

        public void Remove( bool bUpdate = false )
        {
            var aisContext = m_Viewer.GetAISContext();

            foreach( AIS_Shape mark in m_CADMarkList ) {
                aisContext.Remove( mark, false );
            }
            m_CADMarkList.Clear();

            foreach( AIS_Shape mark in m_CAMMarkList ) {
                aisContext.Remove( mark, false );
            }
            m_CAMMarkList.Clear();

            foreach( AIS_Line line in m_OffsetLineList ) {
                aisContext.Remove( line, false );
            }
            m_OffsetLineList.Clear();

            if( bUpdate ) {
                UpdateView();
            }
        }

        public void SetShow( bool isShow )
        {
            m_IsShow = isShow;
        }

        public void UpdateView()
        {
            m_Viewer.UpdateView();
        }

        AIS_Shape CreatePointMark( gp_Pnt point, Quantity_NameOfColor color )
        {
            BRepBuilderAPI_MakeVertex mkVertex = new BRepBuilderAPI_MakeVertex( point );
            TopoDS_Vertex vertex = mkVertex.Vertex();
            return ViewHelper.CreateFeatureAIS( vertex, color );
        }

        const Quantity_NameOfColor CAD_MARK_COLOR = Quantity_NameOfColor.Quantity_NOC_ORANGE;
        const Quantity_NameOfColor CAM_MARK_COLOR = Quantity_NameOfColor.Quantity_NOC_GREEN;
        const Quantity_NameOfColor OFFSET_LINE_COLOR = Quantity_NameOfColor.Quantity_NOC_WHITE;
    }
}
