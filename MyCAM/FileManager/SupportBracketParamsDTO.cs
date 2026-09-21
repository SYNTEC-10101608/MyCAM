using MyCAM.Data;
using OCC.TopAbs;
using OCC.TopoDS;
using System;
using System.Xml.Serialization;

namespace MyCAM.FileManager
{
	// DTO for support bracket design params, persisted inside .sfa
	[Serializable]
	public class SupportBracketParamsDTO
	{
		// constructor for XmlSerializer
		public SupportBracketParamsDTO()
		{
		}

		public SupportBracketParamsDTO( SupportBracketData parameters )
		{
			if( parameters == null ) {
				return;
			}
			Version = parameters.Version;
			XExtension = parameters.XExtension;
			YExtension = parameters.YExtension;
			BaseThick = parameters.BaseThick;
			XPositions = parameters.XPositions ?? new double[ 0 ];
			YPositions = parameters.YPositions ?? new double[ 0 ];
			CrossGap = parameters.CrossGap;
			XPinCount = parameters.XPinCount;
			YPinCount = parameters.YPinCount;
			WallThick = parameters.WallThick;
			RibHeight = parameters.RibHeight;
			ZExtension = parameters.ZExtension;
			PinLength = parameters.PinLength;
			FilletRadius = parameters.FilletRadius;
		}

		// bundle params + the input shell captured at design time
		public SupportBracketParamsDTO( SupportBracketData parameters, TopoDS_Shell inputShell )
			: this( parameters )
		{
			if( inputShell != null && !inputShell.IsNull() ) {
				InputShell = new TopoShapeDTO( inputShell );
			}
		}

		// keep version for future compatibility handling
		[XmlAttribute]
		public string Version
		{
			get; set;
		}

		// input shell captured at design time (null when absent)
		public TopoShapeDTO InputShell
		{
			get; set;
		}

		public double XExtension
		{
			get; set;
		}

		public double YExtension
		{
			get; set;
		}

		public double BaseThick
		{
			get; set;
		}

		[XmlArray( "XPositions" )]
		[XmlArrayItem( "X" )]
		public double[] XPositions
		{
			get; set;
		} = new double[ 0 ];

		[XmlArray( "YPositions" )]
		[XmlArrayItem( "Y" )]
		public double[] YPositions
		{
			get; set;
		} = new double[ 0 ];

		public double CrossGap
		{
			get; set;
		}

		public int XPinCount
		{
			get; set;
		}

		public int YPinCount
		{
			get; set;
		}

		public double WallThick
		{
			get; set;
		}

		public double RibHeight
		{
			get; set;
		}

		public double ZExtension
		{
			get; set;
		}

		public double PinLength
		{
			get; set;
		}

		public double FilletRadius
		{
			get; set;
		}

		public SupportBracketData ToData()
		{
			return new SupportBracketData
			{
				Version = string.IsNullOrEmpty( Version ) ? new SupportBracketData().Version : Version,
				XExtension = XExtension,
				YExtension = YExtension,
				BaseThick = BaseThick,
				XPositions = XPositions ?? new double[ 0 ],
				YPositions = YPositions ?? new double[ 0 ],
				CrossGap = CrossGap,
				XPinCount = XPinCount,
				YPinCount = YPinCount,
				WallThick = WallThick,
				RibHeight = RibHeight,
				ZExtension = ZExtension,
				PinLength = PinLength,
				FilletRadius = FilletRadius,
			};
		}

		// restore shell from BRep string; null when not present or invalid
		public TopoDS_Shell GetInputShell()
		{
			if( InputShell == null || string.IsNullOrEmpty( InputShell.TopoShapeBRepData ) ) {
				return null;
			}
			try {
				TopoDS_Shape shape = TopoShapeDTO.BRepStringToShape( InputShell.TopoShapeBRepData );
				if( shape != null && !shape.IsNull() && shape.ShapeType() == TopAbs_ShapeEnum.TopAbs_SHELL ) {
					return TopoDS.ToShell( shape );
				}
			}
			catch {
				// invalid BRep data, treat as no shell
			}
			return null;
		}
	}
}
