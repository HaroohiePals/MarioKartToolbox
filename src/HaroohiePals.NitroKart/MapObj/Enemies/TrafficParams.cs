using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.MapObj.Enemies;

record TrafficParams(double Field0, double Field4, double Field8, double FieldC, double Field10, double Field14,
    double Field18, double Field1C, ushort Field2C, ushort Field30)
{
    public static TrafficParams Car = new TrafficParams(12, 20, 21, 19, 12.3, 4.2, 11, 1, 0x190, 0x1AE);
    public static TrafficParams Truck = new TrafficParams(13, 40, 33, 28, 13.6, 4.7, 20, 1.2, 0x18F, 0x1AD);
    public static TrafficParams Bus = new TrafficParams(21, 55, 55, 55, 22.2, 7.5, 31, 2, 0x18E, 0x1AC);
}
