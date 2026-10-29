using MyCAM.Data;
using MyCAM.Post;
using OCC.gp;
using System.Collections.Generic;

namespace MyCAM.Helper
{
    // top-level simulation path builder contract; product-specific IK / FK and interpolation are implemented by subclasses
    public abstract class SimuPathBuilderBase
    {
        // build the full frame transform map for the whole simulation
        internal abstract bool BuildFrameTransMap(
            SimuData.SimuRequiredData.SimuInputSet calNeedData,
            out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap,
            out List<SimuData.ResultData.PathStartEndIndex> pathStartEndIndexList,
            out int frameCount );

        // build a single point's frame transform, used by tool vector edit actions
        public abstract bool BuildSinglePointFrameTransform(
            IMachineDataBase machineData,
            gp_Pnt position,
            PostPointBase orientationPoint,
            out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap );

        // build whole-path IK point list ( traverse + main path + exit ) with product-specific interpolation
        protected abstract bool GetWholePathIKPnt(
            List<PostData> postDataList,
            List<int> groupStartPathIndices,
            List<IProcessPoint> groupLastPathLastPntList,
            EntryAndExitData entryAndExitData,
            PostSolver postSolver,
            out List<PathSimuIKData> pathSimuPntList );

        // convert whole-path IK list into FK ( machine coordinate ) list with product-specific FK solving
        protected abstract bool BuildPathFKList(
            List<PathSimuIKData> pathIKPntList,
            PostSolver postSolver,
            gp_Vec simulationOffset,
            out List<PathSimuFKData> pathSimuPntList );

        // convert a single IK point into an FK point with product-specific FK
        protected abstract bool BuildFKPostPnt(
            PostSolver postSolver,
            PostPointBase ikPostPoint,
            out PostPointBase fkPoint,
            gp_Vec simuOffset = null );

        // build traverse IK point list between paths with product-specific interpolation
        protected abstract List<PostPointBase> GetTraversIKPnt(
            PostData postData,
            PostSolver postSolver,
            bool isFirstPath,
            PostPointBase prePathLastPnt );

        // build main-path IK point list from post data
        protected abstract List<PostPointBase> GetPathIKPnt(
            PostData postData );

        // build exit-path IK point list with product-specific interpolation
        protected abstract List<PostPointBase> GetExitPathIKPnt(
            IProcessPoint groupLastPathLastPnt,
            EntryAndExitData entryAndExitData,
            PostPointBase pathEndPoint,
            PostSolver postSolver );

        // build frog-leap and cut-down traverse IK point list with product-specific interpolation
        protected abstract List<PostPointBase> MovementAndCutDownIKPnt(
            PostPointBase traverseStartPnt,
            PostData pathPostData );

        // build linear traverse IK point list with product-specific interpolation
        protected abstract bool BuildLinearTraverseIKPnt(
            PostPointBase startPostPnt,
            PostPointBase endPostPnt,
            out List<PostPointBase> IKpostPointList );

        // build frog-leap arc traverse IK point list with product-specific interpolation
        protected abstract bool BuildFrogLeapTraverseIKPnt(
            PostPointBase startPostPnt,
            PostPointBase midPostPoint,
            PostPointBase endPostPnt,
            out List<PostPointBase> frogLeapIKPostPnt );
    }
}
