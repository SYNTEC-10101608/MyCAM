using MyCAM.Data;
using System;

namespace MyCAM.Post
{
	internal interface ITraversePostData
	{
		PostPoint ProcessStartPoint
		{
			get;
		}
		PostPoint LiftUpPostPoint
		{
			get;
		}
		PostPoint CutDownPostPoint
		{
			get;
		}
		PostPoint FrogLeapMidPostPoint
		{
			get;
		}
		double FollowSafeDistance
		{
			get;
		}
	}

	internal static class TraverseWriterHelper
	{
		public static void WriteTraverse<T>( ITraverseWriter writer, T postData )
			where T : ITraversePostData
		{
			if( writer == null || postData == null ) {
				throw new ArgumentNullException( "TraverseWriterHelper.WriteTraverse arguments cannot be null" );
			}

			// lift up
			if( postData.LiftUpPostPoint != null ) {
				writer.WriteLinearTraverse( postData.LiftUpPostPoint, 0 );
			}

			// frog leap with cut down
			if( postData.FrogLeapMidPostPoint != null && postData.CutDownPostPoint != null ) {
				writer.WriteFrogLeap( postData.FrogLeapMidPostPoint, postData.CutDownPostPoint, 0 );

				// cut down
				writer.WriteLinearTraverse( postData.ProcessStartPoint, postData.FollowSafeDistance );
			}

			// frog leap without cut down
			else if( postData.FrogLeapMidPostPoint != null && postData.CutDownPostPoint == null ) {
				writer.WriteFrogLeap( postData.FrogLeapMidPostPoint, postData.ProcessStartPoint, postData.FollowSafeDistance );
			}

			// no frog leap
			else if( postData.FrogLeapMidPostPoint == null && postData.CutDownPostPoint != null ) {
				writer.WriteLinearTraverse( postData.CutDownPostPoint, 0 );

				// cut down
				writer.WriteLinearTraverse( postData.ProcessStartPoint, postData.FollowSafeDistance );
			}

			// no frog leap and no cut down
			else {
				writer.WriteLinearTraverse( postData.ProcessStartPoint, postData.FollowSafeDistance );
			}
		}
	}
}
