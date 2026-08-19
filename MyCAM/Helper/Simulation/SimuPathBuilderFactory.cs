using MyCAM.Data;
using System;

namespace MyCAM.Helper
{
    // create simulation path builder based on machine product
    public static class SimuPathBuilderFactory
    {
        public static SimuPathBuilderBase Create( IMachineDataBase machineData )
        {
            switch( machineData ) {
                case FiveAxisMachineData _:
                    return new FiveAxisSimuPathBuilder();
                case ArticRobotMachineData _:

                    // arm builder will be added when articulated robot simulation is implemented
                    throw new NotSupportedException( "Articulated robot simulation is not implemented yet." );
                default:
                    throw new NotSupportedException( $"Unsupported machine product: {machineData?.Product}" );
            }
        }
    }
}
