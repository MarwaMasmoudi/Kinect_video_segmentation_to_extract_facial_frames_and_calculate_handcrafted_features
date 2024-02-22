using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.Face;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Windows;

namespace Microsoft.Samples.Kinect.FaceBasics
{
   public  class FramesResult
    {
        public FramesResult() { }
        public FramesResult(FaceFrameResult faceFrame , RecordedImage recordedIm , TimeSpan tim , int index)
        {
            FaceFrameResult = faceFrame;
            RecordedImage = recordedIm;
            Time = tim;
            faceIndex = index;
        }
        public FramesResult( TimeSpan tim)
        {
            Time = tim;
        }


        public void SaveImage(string floderPath)
        {
            if (!System.IO.Directory.Exists(floderPath))
            {
                System.IO.Directory.CreateDirectory(floderPath);
            }
            try
            {
                FrameImage.Save(string.Format(@"{0}\{1}", floderPath, FrameImageFileName)  );
            }
            catch { }
            try
            {
                FaceImage.Save(string.Format(@"{0}\{1}", floderPath, FaceImageFileName));
            }
            catch { }
        }
        public void SaveImage()
        {
            string floderPath = Environment.CurrentDirectory+@"\img";
            SaveImage( floderPath);
        }

        public  void ReadImage()
        {
            if (FaceImage == null || FrameImage == null)
            {
                if (!System.IO.Directory.Exists(Parametres.TempDirectory ))
                {
                    System.IO.Directory.CreateDirectory(Parametres.TempDirectory);
                }

                string filename = Parametres.TempDirectory + @"\" + Time.ToString().Replace(':', ' ').Replace('.', ' ') + "_" + FaceFrameResult.TrackingId + ".jpeg";
                try
                {
                    FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    RecordedImage.JpegImage.Save(fs);
                    fs.Dispose();
                }
                catch { }
                try
                {
                    Image i   = Image.FromFile(filename);
                    Bitmap bitmap = (Bitmap)i ;
                    MemoryStream ms = new MemoryStream();

                    bitmap.Save(ms , i.RawFormat);
                    FrameImage = Image.FromStream(ms);

                    MemoryStream ms2 = new MemoryStream();
                    Rectangle r = new Rectangle(Convert.ToInt32(FaceBox.X), Convert.ToInt32(FaceBox.Y), Convert.ToInt32(FaceBox.Width), Convert.ToInt32(FaceBox.Height));

                    Image i2 = Image.FromFile(filename);
                    Bitmap bitmap1 = (Bitmap)i2;
                    Bitmap bitmap2 = bitmap1.Clone(r, bitmap1.PixelFormat);

                    bitmap2.Save(ms2,  i2.RawFormat);
                    FaceImage = Image.FromStream(ms2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                //try { 
                //System.IO.File.Delete(filename);}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.Message);
                //}

            }
        }


        FaceFrameResult faceFrameResult  ;
        RecordedImage recordedImage;
        TimeSpan time;
        int faceIndex;
        Rect faceBox;

        Image frameImage;
        Image faceImage;
        bool emotion = true ;
        bool confirmed = true;

        public FaceFrameResult FaceFrameResult { get => faceFrameResult; set => faceFrameResult = value; }
        public RecordedImage RecordedImage { get => recordedImage; set => recordedImage = value; }
        public TimeSpan Time { get => time; set => time = value; }
        public int FaceIndex { get => faceIndex; set => faceIndex = value; }
        public Rect FaceBox { get => faceBox; set => faceBox = value; }
        public Image FrameImage { get => frameImage; set => frameImage = value; }
        public Image FaceImage { get => faceImage; set => faceImage = value; }
        public bool Emotion { get => emotion; set => emotion = value; }
        public int EmotionBite { get { return Emotion ? 1 : 0; } }
        public bool Confirmed { get => confirmed; set => confirmed = value; }


        public int LeftEyeClosed
        {
            get
            {
                return GetDetectionResultValue(FaceFrameResult.FaceProperties[FaceProperty.LeftEyeClosed]);
            }
        }
        public int LookingAway
        {
            get
            {
                return GetDetectionResultValue(FaceFrameResult.FaceProperties[FaceProperty.LookingAway]);
            }
        }
        public int MouthMoved
        {
            get
            {
                return GetDetectionResultValue(FaceFrameResult.FaceProperties[FaceProperty.MouthMoved]);
            }
        }
        public int MouthOpen
        {
            get
            {
                return GetDetectionResultValue(FaceFrameResult.FaceProperties[FaceProperty.MouthOpen]);
            }
        }
        public int RightEyeClosed
        {
            get
            {
                return GetDetectionResultValue(FaceFrameResult.FaceProperties[FaceProperty.RightEyeClosed]);
            }
        }

        int GetDetectionResultValue(Microsoft.Kinect.DetectionResult v )
        {
            switch (v)
            {
                case Microsoft.Kinect.DetectionResult.Unknown: return 0;
                case Microsoft.Kinect.DetectionResult.No: return 1;
                case Microsoft.Kinect.DetectionResult.Maybe: return 2;
                case Microsoft.Kinect.DetectionResult.Yes: return 3;
                default: return -1;
            }
        }

        public string FrameImageFileName
        {
            get
            {
                return string.Format("{0}_{1}.jpeg",  Time.ToString().Replace(':', ' ').Replace('.', ' ') , FaceFrameResult.TrackingId );
            }
        }
        public string FaceImageFileName
        {
            get
            {
                return string.Format("{0}_{1}_F.jpeg", Time.ToString().Replace(':', ' ').Replace('.', ' '), FaceFrameResult.TrackingId);
            }
        }

        public Polygons.ConvexPolygon2D Polygon1
        {
            get
            {
                List<Polygons.Point2D> points = new List<Polygons.Point2D>();
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].Y));
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].Y));
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.Nose].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.Nose].Y));
                return new Polygons.ConvexPolygon2D(points.ToArray());
            }
        }

        public Polygons.ConvexPolygon2D Polygon2
        {
            get
            {
                List<Polygons.Point2D> points = new List<Polygons.Point2D>();
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerLeft].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerLeft].Y));
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerRight].Y));
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.Nose].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.Nose].Y));
                return new Polygons.ConvexPolygon2D(points.ToArray());
            }
        }
        public Polygons.ConvexPolygon2D Polygon3
        {
            get
            {
                List<Polygons.Point2D> points = new List<Polygons.Point2D>();
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].Y));
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].Y));
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerLeft].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerLeft].Y));
                points.Add(new Polygons.Point2D(FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerRight].Y));

                return new Polygons.ConvexPolygon2D(points.ToArray());
            }
        }



        public Double DistanceEyeLeft_EyeRight
        {  get
            {
                return Parametres.Distance(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].X,FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].Y, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].Y);
            }
        }


        public Double DistanceEyeLeft_Nose
        {
            get
            {
                return Parametres.Distance(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].Y, FaceFrameResult.FacePointsInColorSpace[FacePointType.Nose].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.Nose].Y);
            }
        }

        public Double DistanceEyeLeft_MouthCornerLeft
        {
            get
            {
                return Parametres.Distance(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].Y, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerLeft].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerLeft].Y);

            }
        }

        public Double DistanceEyeLeft_MouthCornerRight
        {
            get
            {
                return Parametres.Distance(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeLeft].Y, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerRight].Y);
        
            }
        }

        public Double DistanceEyeRight_Nose
        {
            get
            {
                return Parametres.Distance(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].Y, FaceFrameResult.FacePointsInColorSpace[FacePointType.Nose].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.Nose].Y);
         
            }
        }

        public Double DistanceEyeRight_MouthCornerRight
        {
            get
            {
                return Parametres.Distance(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].Y, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerRight].Y);
        
            }
        }

        public Double DistanceEyeright_MouthCornerLeft
        {
            get
            {
                return Parametres.Distance(FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.EyeRight].Y, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerLeft].X, FaceFrameResult.FacePointsInColorSpace[FacePointType.MouthCornerLeft].Y);

            }
        }
        

        public override string ToString()
        {
            //return string.Format("Time:{0} FFR:{1} RI:{2}", Time, FaceFrameResult.RelativeTime, recordedImage.Time);
            return string.Format("{0} {1} Emotion", (Confirmed? "Confirmed" : "Not Confirmed") , (Emotion? "with" : "without") );
        }
    }
}
