using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Post
{
	internal static class PostHelper
	{
		public static bool SolvePath( PostSolver m_PostSolver, CAMData camData,
			out List<PostData> resultG54, out List<PostData> resultMCS )
		{
			resultG54 = new List<PostData>();
			resultMCS = new List<PostData>();
			if( camData == null ) {
				return false;
			}

			// solve IK
			List<Tuple<double, double>> rotateAngleList = new List<Tuple<double, double>>();
			List<bool> sigularTagList = new List<bool>();
			double dM = 0;
			double dS = 0;
			foreach( CAMPoint point in camData.CAMPointList ) {
				IKSolveResult ikResult = m_PostSolver.SolveIK( point, dM, dS, out dM, out dS );
				if( ikResult == IKSolveResult.IvalidInput || ikResult == IKSolveResult.NoSolution ) {
					return false;
				}
				rotateAngleList.Add( new Tuple<double, double>( dM, dS ) );
				if( ikResult == IKSolveResult.NoError ) {
					sigularTagList.Add( false );
				}
				else if( ikResult == IKSolveResult.MasterInfinityOfSolution || ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
					sigularTagList.Add( true );
				}
			}

			// TODO: filter the sigular points
			// solve FK
			for( int i = 0; i < camData.CAMPointList.Count; i++ ) {
				gp_Pnt pointG54 = camData.CAMPointList[ i ].CADPoint.Point;
				gp_Vec tcpOffset = m_PostSolver.SolveFK( rotateAngleList[ i ].Item1, rotateAngleList[ i ].Item2, pointG54 );
				gp_Pnt pointMCS = pointG54.Translated( tcpOffset );

				// add G54 frame data
				PostData frameDataG54 = new PostData()
				{
					X = pointG54.X(),
					Y = pointG54.Y(),
					Z = pointG54.Z(),
					Master = rotateAngleList[ i ].Item1,
					Slave = rotateAngleList[ i ].Item2
				};
				resultG54.Add( frameDataG54 );

				// add MCS frame data
				PostData frameDataMCS = new PostData()
				{
					X = pointMCS.X(),
					Y = pointMCS.Y(),
					Z = pointMCS.Z(),
					Master = rotateAngleList[ i ].Item1,
					Slave = rotateAngleList[ i ].Item2
				};
				resultMCS.Add( frameDataMCS );
			}
			return true;
		}
	}
}
