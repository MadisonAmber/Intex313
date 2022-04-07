using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex313.Models
{
    public class FormData
    {
        public float MilePoint { get; set; }
        public float Lat_Utm_Y { get; set; }
        public float Long_Utm_X { get; set; }
        public float Crash_Time { get; set; }
        public float Work_Zone { get; set; }
        public float Pedestrian { get; set; }
        public float Motorcycle { get; set; }
        public float Intersection { get; set; }
        public float Teenage_Driver { get; set; }
        public float Older_Driver { get; set; }
        public float Night_Condition { get; set; }
        public float Roadway_Departure { get; set; }

        public Tensor<float> AsTensor()
        {
            float[] data = new float[]
            {
                MilePoint, Lat_Utm_Y, Long_Utm_X, Crash_Time, Work_Zone, Pedestrian, Motorcycle, Intersection, Teenage_Driver, Older_Driver, Night_Condition, Roadway_Departure
            };
            int[] dimensions = new int[] { 1, 12 };
            return new DenseTensor<float>(data, dimensions);
        }

    }
}
