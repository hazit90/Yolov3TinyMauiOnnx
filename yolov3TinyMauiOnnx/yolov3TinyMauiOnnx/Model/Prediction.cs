using System;
using System.Drawing;

namespace yolov3TinyMauiOnnx.Model
{
	public class Prediction
	{

        List<RectangleF> Boxes { get; set; }
        List<RectangleF> nmsOutScores;
        List<RectangleF> nmsOutIndicies;

        public Prediction()
		{
            Boxes = new List<RectangleF>();
            nmsOutScores = new List<RectangleF>();
            nmsOutIndicies = new List<RectangleF>();
        }
        //public Box Box { get; set; }
        //public string Label { get; set; }
        //public float Confidence { get; set; }
	}
}

