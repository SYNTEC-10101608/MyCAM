using MyCAM.App;
using MyCAM.Data;
using System;

namespace MyCAM.Helper
{
	public class StdPatternCraftCoupler
	{
		public void HandleCouplerCraftForStartPoint( ref CraftData craftData, IStdPatternGeomData geomData )
		{
			if( craftData == null || geomData == null ) {
				return;
			}

			try {
				double maxOverCut = OverCutHelper.GetMaxOverCutLength( geomData, craftData.StartPointIndex );
				if( craftData.OverCutLength > maxOverCut ) {
					craftData.OverCutLength = maxOverCut;
				}
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"StdPatternCraftCoupler.HandleCouplerCraftForStartPoint failed: {ex.Message}", MyApp.NoticeType.Warning );
			}
		}
	}
}
