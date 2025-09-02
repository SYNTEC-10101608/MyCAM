﻿using System;
using System.Collections.Generic;
using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Post
{
	internal static class PostHelper
	{

		public static bool SolvePath( PostSolver postSolver, CAMData camData,
			 out PostData pathMCSPostData, out PostData pathG54PostData )
		{
			// for simulation
			pathMCSPostData = new PostData();

			// for write NC file
			pathG54PostData = new PostData();
			if( postSolver == null || camData == null ) {
				return false;
			}

			// to ensure joint space continuity of process path
			double dLastPointProcess_M = 0, dLastPointProcess_S = 0;

			// lead-in
			if( camData.LeadLineParam.LeadIn.Type != LeadType.LeadLineType.None && camData.LeadInCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, camData.LeadInCAMPointList,
					out List<PostPoint> leadInPost, out List<PostPoint> leadInMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathMCSPostData.LeadInPostPointList.AddRange( leadInMCS );
				pathG54PostData.LeadInPostPointList.AddRange( leadInPost );
			}

			// main path
			if( !SolveProcessPath( postSolver, camData.CAMPointList,
				out List<PostPoint> mainPost, out List<PostPoint> mainMCS,
				ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
				return false;
			}
			pathMCSPostData.MainPathPostPointList.AddRange( mainMCS );
			pathG54PostData.MainPathPostPointList.AddRange( mainPost );

			// over-cut
			if( camData.OverCutLength != 0 && camData.OverCutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, camData.OverCutCAMPointList,
					out List<PostPoint> overCutPost, out List<PostPoint> overCutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathMCSPostData.OverCutPostPointList.AddRange( overCutMCS );
				pathG54PostData.OverCutPostPointList.AddRange( overCutPost );
			}

			// lead-out
			if( camData.LeadLineParam.LeadOut.Type != LeadType.LeadLineType.None && camData.LeadOutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, camData.LeadOutCAMPointList,
					out List<PostPoint> leadOutPost, out List<PostPoint> leadOutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathMCSPostData.LeadOutPostPointList.AddRange( leadOutMCS );
				pathG54PostData.LeadOutPostPointList.AddRange( leadOutPost );
			}
			return true;
		}

		public static List<PostPoint> GetConcatenatedPostList( PostData postData )
		{
			List<PostPoint> result = new List<PostPoint>();
			if( postData == null ) {
				return result;
			}
			if( postData.LeadInPostPointList.Count > 0 ) {
				result.AddRange( postData.LeadInPostPointList );
			}
			if( postData.MainPathPostPointList.Count > 0 ) {
				result.AddRange( postData.MainPathPostPointList );
			}
			if( postData.OverCutPostPointList.Count > 0 ) {
				result.AddRange( postData.OverCutPostPointList );
			}
			if( postData.LeadOutPostPointList.Count > 0 ) {
				result.AddRange( postData.LeadOutPostPointList );
			}
			return result;
		}

		static bool SolveProcessPath( PostSolver postSolver, List<CAMPoint> camPointList,
			out List<PostPoint> resultG54, out List<PostPoint> resultMCS, ref double dLastProcessPathM, ref double dLastProcessPathS )
		{
			resultG54 = new List<PostPoint>();
			resultMCS = new List<PostPoint>();

			// solve IK
			List<Tuple<double, double>> rotateAngleList = new List<Tuple<double, double>>();
			List<bool> sigularTagList = new List<bool>();
			foreach( CAMPoint point in camPointList ) {
				IKSolveResult ikResult = postSolver.SolveIK( point, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
				if( ikResult == IKSolveResult.IvalidInput || ikResult == IKSolveResult.NoSolution ) {
					return false;
				}
				rotateAngleList.Add( new Tuple<double, double>( dLastProcessPathM, dLastProcessPathS ) );
				if( ikResult == IKSolveResult.NoError ) {
					sigularTagList.Add( false );
				}
				else if( ikResult == IKSolveResult.MasterInfinityOfSolution || ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
					sigularTagList.Add( true );
				}
			}

			// TODO: filter the sigular points
			// solve FK
			for( int i = 0; i < camPointList.Count; i++ ) {
				gp_Pnt pointG54 = camPointList[ i ].CADPoint.Point;
				gp_Vec tcpOffset = postSolver.SolveFK( rotateAngleList[ i ].Item1, rotateAngleList[ i ].Item2, pointG54 );
				gp_Pnt pointMCS = pointG54.Translated( tcpOffset );

				// add G54 frame data
				PostPoint frameDataG54 = new PostPoint()
				{
					X = pointG54.X(),
					Y = pointG54.Y(),
					Z = pointG54.Z(),
					Master = rotateAngleList[ i ].Item1,
					Slave = rotateAngleList[ i ].Item2
				};
				resultG54.Add( frameDataG54 );

				// add MCS frame data
				PostPoint frameDataMCS = new PostPoint()
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
