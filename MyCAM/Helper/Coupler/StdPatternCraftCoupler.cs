using System;
using MyCAM.App;
using MyCAM.Data;

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
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "HandleCouplerCraftFailed", HANDLE_COUPLER_CRAFT_FAILED ), ex.Message ), MyApp.NoticeType.Warning );
			}
		}
		const string HANDLE_COUPLER_CRAFT_FAILED = "StdPatternCraftCoupler.HandleCouplerCraftForStartPoint 失敗: {0}";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "StdPatternCraftCoupler" );
	}
}
