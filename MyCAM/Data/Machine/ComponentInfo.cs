using OCC.gp;

namespace MyCAM.Data
{
    public enum Product
    {
        None = 0,
        FiveAxis = 1,
        ArticRobot = 2,
    }

    public enum AxisType
    {
        Linear = 1,
        Rotary = 2,
    }

    public interface IAxisDef
    {
        AxisType Type
        {
            get;
        }

        // stroke limit start (mm for linear axis, deg for rotary axis)
        double LimitStart
        {
            get;
        }

        // stroke limit end (mm for linear axis, deg for rotary axis)
        double LimitEnd
        {
            get;
        }
    }

    public class LinearAxisDef : IAxisDef
    {
        public LinearAxisDef( gp_Dir direction, double limitStart, double limitEnd )
        {
            Direction = direction ?? new gp_Dir( 0, 0, 1 );
            LimitStart = limitStart;
            LimitEnd = limitEnd;
        }

        public AxisType Type
        {
            get
            {
                return AxisType.Linear;
            }
        }

        // moving direction (which axis it is parallel to)
        public gp_Dir Direction
        {
            get; set;
        }

        public double LimitStart
        {
            get; set;
        }

        public double LimitEnd
        {
            get; set;
        }
    }

    public class RotateAxisDef : IAxisDef
    {
        public RotateAxisDef( gp_Pnt rotateCenter, gp_Dir rotateDir, double limitStart_deg, double limitEnd_deg,
            RotaryDirection rotaryDirection = RotaryDirection.None )
        {
            RotateCenter = rotateCenter ?? new gp_Pnt( 0, 0, 0 );
            RotateDir = rotateDir ?? new gp_Dir( 0, 0, 1 );
            LimitStart = limitStart_deg;
            LimitEnd = limitEnd_deg;
            RotaryDirection = rotaryDirection;
        }

        public AxisType Type
        {
            get
            {
                return AxisType.Rotary;
            }
        }

        public gp_Pnt RotateCenter
        {
            get; set;
        }

        public gp_Dir RotateDir
        {
            get; set;
        }

        // rotate hand rule ( right / left ), used by post / NC generation
        public RotaryDirection RotaryDirection
        {
            get; set;
        }

        public double LimitStart
        {
            get; set;
        }

        public double LimitEnd
        {
            get; set;
        }

        public gp_Ax1 RotateAxis
        {
            get
            {
                return new gp_Ax1( RotateCenter, RotateDir );
            }
        }

        // map a principal RotaryAxis ( X / Y / Z ) to its unit direction
        public static gp_Dir RotaryAxisToDir( RotaryAxis axis )
        {
            switch( axis ) {
                case RotaryAxis.X:
                    return new gp_Dir( 1, 0, 0 );
                case RotaryAxis.Y:
                    return new gp_Dir( 0, 1, 0 );
                case RotaryAxis.Z:
                    return new gp_Dir( 0, 0, 1 );
                default:
                    return new gp_Dir( 0, 0, 1 );
            }
        }
    }

    public interface IComponentInfo
    {
        Product Product
        {
            get;
        }

        MachineComponentType ComponentType
        {
            get;
        }

        // axis definition of this component; null for non-axis component (Base / Laser / WorkPiece)
        IAxisDef AxisDef
        {
            get;
        }
    }

    public abstract class CommonInfo : IComponentInfo
    {
        protected CommonInfo( MachineComponentType type, IAxisDef axisDef = null )
        {
            ComponentType = type;
            AxisDef = axisDef;
        }

        public abstract Product Product
        {
            get;
        }

        public MachineComponentType ComponentType
        {
            get;
        }

        public IAxisDef AxisDef
        {
            get;
        }

        public bool IsAxis
        {
            get
            {
                return AxisDef != null;
            }
        }
    }

    public class FiveAxisComponentInfo : CommonInfo
    {
        public FiveAxisComponentInfo( MachineComponentType type, IAxisDef axisDef = null )
            : base( type, axisDef )
        {
        }

        public override Product Product
        {
            get
            {
                return Product.FiveAxis;
            }
        }
    }

    public class ArticRobotComponentInfo : CommonInfo
    {
        public ArticRobotComponentInfo( MachineComponentType type, IAxisDef axisDef = null )
            : base( type, axisDef )
        {
        }

        public override Product Product
        {
            get
            {
                return Product.ArticRobot;
            }
        }
    }
}
