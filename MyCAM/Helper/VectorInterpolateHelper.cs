using System;
using System.Collections.Generic;
using OCC.gp;
using MyCAM.Data;

namespace MyCAM.Helper
{
	internal class VectorInterpolateHelper
	{
		public static void ApplyToolVectorInterpolation(List<CAMPointInfo> camPointInfoList, bool isReverse, bool isClosed)
		{
			if (camPointInfoList == null || camPointInfoList.Count == 0)
				return;

			// find all ctrl pnt
			List<int> ctrlPntIdx = new List<int>();
			for (int i = 0; i < camPointInfoList.Count; i++)
			{
				if (camPointInfoList[i].IsCtrlPnt)
				{
					ctrlPntIdx.Add(i);
				}
			}

			// do not have ctrl pnt
			if (ctrlPntIdx.Count == 0)
			{
				return;
			}

			// only one ctrl pnt, apply to all point
			if (ctrlPntIdx.Count == 1)
			{
				ApplySpecifiedVec(camPointInfoList, ctrlPntIdx[0]);
				return;
			}

			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList(ctrlPntIdx, isClosed);

			// modify the tool vector
			for (int i = 0; i < interpolateIntervalList.Count; i++)
			{
				// get start and end index
				int nStartIndex = interpolateIntervalList[i].Item1;
				int nEndIndex = interpolateIntervalList[i].Item2;
				InterpolateToolVec(nStartIndex, nEndIndex, camPointInfoList, isReverse);
			}
		}

		private static void ApplySpecifiedVec(List<CAMPointInfo> pointInfoList, int nSpecifiedIdx)
		{
			gp_Vec SpecifiedVec = CalCtrlPntToolVec(pointInfoList[nSpecifiedIdx]);

			if (SpecifiedVec == null)
			{
				return;
			}
			foreach (var pointInfo in pointInfoList)
			{
				pointInfo.ToolVec = new gp_Dir(SpecifiedVec);
			}
		}

		private static gp_Vec CalCtrlPntToolVec(CAMPointInfo controlBar)
		{
			if (!controlBar.IsCtrlPnt || controlBar.ABValues == null)
				return null;

			var abValues = controlBar.ABValues;
			gp_Dir oriNormal = controlBar.Point.NormalVec_1st;

			return GetVecFromAB(oriNormal, controlBar.Point.TangentVec,
				abValues.Item1 * Math.PI / 180, abValues.Item2 * Math.PI / 180);
		}

		private static gp_Vec GetVecFromAB(gp_Dir normal_1st, gp_Dir tangentVec, double dRA_rad, double dRB_rad)
		{
			if (dRA_rad == 0 && dRB_rad == 0)
			{
				return new gp_Vec(normal_1st);
			}

			gp_Dir x = tangentVec;
			gp_Dir z = normal_1st;
			gp_Dir y = z.Crossed(x);

			double X = 0;
			double Y = 0;
			double Z = 0;
			if (dRA_rad == 0)
			{
				X = 0;
				Z = 1;
			}
			else
			{
				X = dRA_rad < 0 ? -1 : 1;
				Z = X / Math.Tan(dRA_rad);
			}
			Y = Z * Math.Tan(dRB_rad);
			gp_Dir dir1 = new gp_Dir(x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z);
			return new gp_Vec(dir1.XYZ());
		}

		private static List<Tuple<int, int>> GetInterpolateIntervalList(List<int> ctrlIndex, bool isClosed)
		{
			List<Tuple<int, int>> intervalList = new List<Tuple<int, int>>();
			if (ctrlIndex.Count < 2)
			{
				return intervalList;
			}
			if (isClosed)
			{
				for (int i = 0; i < ctrlIndex.Count; i++)
				{
					int nextIndex = (i + 1) % ctrlIndex.Count;
					intervalList.Add(new Tuple<int, int>(ctrlIndex[i], ctrlIndex[nextIndex]));
				}
			}
			else
			{
				for (int i = 0; i < ctrlIndex.Count - 1; i++)
				{
					intervalList.Add(new Tuple<int, int>(ctrlIndex[i], ctrlIndex[i + 1]));
				}
			}
			return intervalList;
		}

		private static void InterpolateToolVec(int nStartIndex, int nEndIndex, List<CAMPointInfo> pathCAMInfo, bool isReverse)
		{
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + pathCAMInfo.Count : nEndIndex;
			if (pathCAMInfo[nStartIndex].SharingPoint == null || pathCAMInfo[nEndIndex].SharingPoint == null)
			{
				return;
			}

			gp_Dir startPntTanVec = isReverse ? pathCAMInfo[nStartIndex].SharingPoint.TangentVec : pathCAMInfo[nStartIndex].Point.TangentVec;
			gp_Dir endPntTanVec = isReverse ? pathCAMInfo[nEndIndex].SharingPoint.TangentVec : pathCAMInfo[nEndIndex].Point.TangentVec;

			gp_Vec startVec = GetVecFromAB(pathCAMInfo[nStartIndex].Point.NormalVec_1st,
				startPntTanVec,
				pathCAMInfo[nStartIndex].ABValues.Item1 * Math.PI / 180,
				pathCAMInfo[nStartIndex].ABValues.Item2 * Math.PI / 180);
			gp_Vec endVec = GetVecFromAB(pathCAMInfo[nEndIndex].Point.NormalVec_1st,
				endPntTanVec,
				pathCAMInfo[nEndIndex].ABValues.Item1 * Math.PI / 180,
				pathCAMInfo[nEndIndex].ABValues.Item2 * Math.PI / 180);

			double totaldistance = 0;
			for (int i = nStartIndex; i < nEndIndexModify; i++)
			{
				totaldistance += pathCAMInfo[i % pathCAMInfo.Count].DistanceToNext;
			}

			gp_Quaternion q12 = new gp_Quaternion(startVec, endVec);
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp(new gp_Quaternion(), q12);
			double accumulatedDistance = 0;
			for (int i = nStartIndex; i < nEndIndexModify; i++)
			{
				double t = accumulatedDistance / totaldistance;
				accumulatedDistance += pathCAMInfo[i % pathCAMInfo.Count].DistanceToNext;
				gp_Quaternion q = new gp_Quaternion();
				slerp.Interpolate(t, ref q);
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation(q);
				pathCAMInfo[i % pathCAMInfo.Count].ToolVec = new gp_Dir(startVec.Transformed(trsf));
			}
		}
	}
}
