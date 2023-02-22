using System;
using System.Numerics;
namespace yolov3TinyMauiOnnx.Model;

public class ModelResults
{
    private struct Vec3Int
    {
        public Vec3Int(int x, int y, int z)
        {
            batch_index = x;
            class_index = y;
            box_index = z;
        }

        public int batch_index { get; }
        public int class_index { get; }
        public int box_index { get; }

        public override string ToString() => $"({batch_index}, {class_index}, {box_index})";
    }

    


    public ModelResults(float[] boxes, float[] scores, int[] indices)
    {

        ////convert indices to a nx3 array
        //Vec3Int[] indices3d = new Vec3Int[indices.Length / 3];
        //for (int i = 0; i < indices.Length; i += 3)
        //{
        //    indices3d[i / 3] = new Vec3Int(indices[i], indices[i + 1], indices[i + 2]);
        //}

        ////convert boxes to a nx4 array
        //Vector4[] boxes3d = new Vector4[boxes.Length/4];
        //for (int i = 0; i < boxes.Length; i += 4)
        //{
        //    boxes3d[i / 3] = new Vector4(boxes[i], boxes[i + 1], boxes[i + 2], boxes[i+3]);
        //}


               

        

    }

    public float[] Out_Boxes = null;
    public float[] Out_Scores = null;
    public int[] Out_Classes = null;
}

