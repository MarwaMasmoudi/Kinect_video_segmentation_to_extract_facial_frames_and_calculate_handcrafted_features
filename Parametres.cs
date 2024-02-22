using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.Face;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace Microsoft.Samples.Kinect.FaceBasics
{
  public static   class Parametres
    {
        //public static List<FaceFrameResult> result = new List<FaceFrameResult>();
        //public static List<RecordedImage> images = new List<RecordedImage>();
        //internal static List<int> resultIndexes=new List<int>();

        public static List<FramesResult> resultats = new List<FramesResult>(); 
        public static string TempDirectory = Environment.CurrentDirectory + @"\tmp";

        public static  double  Distance( float Ax , float Ay  , float Bx , float By )
        {
            double ax, ay, bx, by;
            ax = Convert.ToDouble(Ax);
            ay = Convert.ToDouble(Ay);
            bx = Convert.ToDouble(Bx);
            by = Convert.ToDouble(By);

            return Math.Sqrt( Math.Pow( ax- bx , 2 ) + Math.Pow( ay-by , 2)) ;
        }

        public static double surfaceTriangle ( Point A , Point B , Point C )
        {
            return  Math.Abs( (A.X * C.Y ) + (B.X * A.Y)+ (C.X * B.Y )  - ((A.X * B.Y ) + (B.X * C.Y )+ (C.X * A.Y )) )  / 2;
        }
        public static double surfacePolygone(Point A, Point B, Point C , Point D)
        {
            return Math.Abs( ( (A.X *B.Y )+(B.X *C.Y )+(C.X *D.Y )+(D.X *A.Y ) ) - ((A.Y *B.X ) + (B.Y *C.X ) + (C.Y *D.X ) + (D.Y *A.X )) ) / 2;
            
        }

        internal static void Filtrer(List<FramesResult> resultats)
        {

            bool allFinded = true;
            int index = 0;
            #region filtrage 

            for (index = 0; index < Parametres.resultats.Count; index++)
            {
                if (Parametres.resultats[index].RecordedImage == null)
                {
                    Parametres.resultats.RemoveAt(index);
                    index--;
                }
                else
                if (index > 0)
                    if (Parametres.resultats[index].Time == Parametres.resultats[index - 1].Time  && Parametres.resultats[index].FaceFrameResult.TrackingId  == Parametres.resultats[index - 1].FaceFrameResult.TrackingId)
                    {
                        Parametres.resultats.RemoveAt(index);
                        index--;
                    }
            }
            #endregion
        }
    }
    public  class RecordedImage
    {
        JpegBitmapEncoder jpegImage = new JpegBitmapEncoder();
        TimeSpan time;

        public JpegBitmapEncoder JpegImage { get => jpegImage; set => jpegImage = value; }
        public TimeSpan Time { get => time; set => time = value; }
    }

}
