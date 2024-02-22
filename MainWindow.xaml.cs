//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.FaceBasics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Face;
    using Excel = Microsoft.Office.Interop.Excel;
    using System.Text;
    using System.Linq;
    using System.Xml.Serialization;
    using Polygons;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const int ClassEmotion = 1;
        private readonly bool EXCEL_VISIBLE = true;
        private string dataFolder = @"data";
        private string excelFile = string.Empty;
        int frameNo = 1;
        bool ProcessClosingStarted = false;

        bool Recorder = true;

        ColorFrameReader cfr;
        byte[] colorData;
        ColorImageFormat format;
        WriteableBitmap wbmp;
        BitmapSource bmpSource;

        /// <summary>
        /// Thickness of face bounding box and face points
        /// </summary>
        private const double DrawFaceShapeThickness = 8;

        /// <summary>
        /// Font size of face property text 
        /// </summary>
        private const double DrawTextFontSize = 30;

        /// <summary>
        /// Radius of face point circle
        /// </summary>
        private const double FacePointRadius = 1.0;

        /// <summary>
        /// Text layout offset in X axis
        /// </summary>
        private const float TextLayoutOffsetX = -0.1f;

        /// <summary>
        /// Text layout offset in Y axis
        /// </summary>
        private const float TextLayoutOffsetY = -0.15f;

        /// <summary>
        /// Face rotation display angle increment in degrees
        /// </summary>
        private const double FaceRotationIncrementInDegrees = 5.0;

        /// <summary>
        /// Formatted text to indicate that there are no bodies/faces tracked in the FOV
        /// </summary>
        private FormattedText textFaceNotTracked = new FormattedText(
                        "No bodies or faces are tracked ...",
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Georgia"),
                        DrawTextFontSize,
                        Brushes.White);

        /// <summary>
        /// Text layout for the no face tracked message
        /// </summary>
        private Point textLayoutFaceNotTracked = new Point(10.0, 10.0);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array to store bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// Number of bodies tracked
        /// </summary>
        private int bodyCount;

        /// <summary>
        /// Face frame sources
        /// </summary>
        private FaceFrameSource[] faceFrameSources = null;

        /// <summary>
        /// Face frame readers
        /// </summary>
        private FaceFrameReader[] faceFrameReaders = null;

        /// <summary>
        /// Storage for face frame results
        /// </summary>
        private FaceFrameResult[] faceFrameResults = null;
        
        /// <summary>
        /// Width of display (color space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (color space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// Display rectangle
        /// </summary>
        private Rect displayRect;

        /// <summary>
        /// List of brushes for each face tracked
        /// </summary>
        private List<Brush> faceBrush;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;
        /// <summary>
        /// Bitmap to display
        /// </summary>
        /// 

        private WriteableBitmap colorBitmap = null;

        private FaceFrame faceFrame;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            DropTempDirectory();
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();
            
            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the color frame details
            FrameDescription frameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;
            // set the display specifics
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;
           this.displayRect = new Rect(0.0, 0.0, this.displayWidth, this.displayHeight);

            cfr = kinectSensor.ColorFrameSource.OpenReader();
            //cfr.FrameArrived += cfr_FrameArrived;

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // wire handler for body frame arrival
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;



            //FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            //// create the bitmap to display
            //this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);


            // set the maximum number of bodies that would be tracked by Kinect
            this.bodyCount = this.kinectSensor.BodyFrameSource.BodyCount;

            // allocate storage to store body objects
            this.bodies = new Body[this.bodyCount];

            // specify the required face frame results
            FaceFrameFeatures faceFrameFeatures =
                FaceFrameFeatures.BoundingBoxInColorSpace
                | FaceFrameFeatures.PointsInColorSpace
                | FaceFrameFeatures.RotationOrientation
                | FaceFrameFeatures.FaceEngagement
                | FaceFrameFeatures.Glasses
                | FaceFrameFeatures.Happy
                | FaceFrameFeatures.LeftEyeClosed
                | FaceFrameFeatures.RightEyeClosed
                | FaceFrameFeatures.LookingAway
                | FaceFrameFeatures.MouthMoved
                | FaceFrameFeatures.MouthOpen;

            // create a face frame source + reader to track each face in the FOV
            this.faceFrameSources = new FaceFrameSource[this.bodyCount];
            this.faceFrameReaders = new FaceFrameReader[this.bodyCount];

            for (int i = 0; i < this.bodyCount; i++)
            {
                // create the face frame source with the required face frame features and an initial tracking Id of 0
                this.faceFrameSources[i] = new FaceFrameSource(this.kinectSensor, 0, faceFrameFeatures);
                               
                // open the corresponding reader
                this.faceFrameReaders[i] = this.faceFrameSources[i].OpenReader();
            }

            // allocate storage to store face frame results for each face in the FOV
            this.faceFrameResults = new FaceFrameResult[this.bodyCount];
            //Console.WriteLine("Face Frame Result", this.faceFrameResults);
            // populate face result colors - one for each face index
            this.faceBrush = new List<Brush>()
            {
                Brushes.White,
                Brushes.Orange,
                Brushes.Green,
                Brushes.Red,
                Brushes.LightBlue,
                Brushes.Yellow
            };

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            var fd = kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            uint frameSize = fd.BytesPerPixel * fd.LengthInPixels;
            colorData = new byte[frameSize];
            format = ColorImageFormat.Bgra;

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.ImageSource = new DrawingImage(this.drawingGroup);


            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

            Parametres.resultats = new List<FramesResult>();
           
        }


        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
       

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        public DrawingImage ImageSource { get => imageSource; set => imageSource = value; }

        public void InitializeExcel()
        {// if there is existing excel file with the same name at the distination, delete that file
            if (Directory.Exists(dataFolder))
            {
                Directory.Delete(dataFolder, true);
            }

            Directory.CreateDirectory(dataFolder);
        }

        public void FinalizeExcel(string floderPath , List<FramesResult> frames )
        {
            string SourcefileName = Environment.CurrentDirectory + @"\faceframeresult.xlsx";
            excelFile = Path.Combine(floderPath,   "faceframeresult.xlsx");
            var excelApp = new Excel.Application();

            excelApp.Visible = false; // EXCEL_VISIBLE;
            var excelWorkbook = excelApp.Workbooks.Open(SourcefileName,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing,
             Type.Missing, Type.Missing);

            //var workSheet = excelWorkbook.Sheets[1];

            //workSheet.Copy(workSheet);
            

            ////// EXCEL HEADER
            var rowIndex = 3;

            int index = -1;
            foreach (FramesResult ffr in frames)
                if (ffr.Confirmed)
                {
                    index++;
                    rowIndex++;
                    int colIndex = 0;

                    ffr.SaveImage(floderPath);

                    excelApp.Cells[rowIndex, ++colIndex] = ffr.Time.ToString();
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.FaceFrameResult.TrackingId.ToString();
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.LeftEyeClosed;
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.LookingAway; 
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.MouthMoved; 
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.MouthOpen; 
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.RightEyeClosed;

                    // D1: EyeLeft_EyeRight
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.DistanceEyeLeft_EyeRight;  

                    // D2: EyeLeft_Nose
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.DistanceEyeLeft_Nose;

                    // D3: EyeLeft_MouthCornerLeft
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.DistanceEyeLeft_MouthCornerLeft;

                    //D4: EyeLeft_MouthCornerRight
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.DistanceEyeLeft_MouthCornerRight;

                    //D5: EyeRight_Nose
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.DistanceEyeRight_Nose;

                    //D6: EyeRight_MouthCornerRight
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.DistanceEyeRight_MouthCornerRight;

                    //D7: Eyeright_MouthCornerLeft
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.DistanceEyeright_MouthCornerLeft;

                    if (index > 0)
                    {
                        FramesResult ffr_1 = frames[index - 1];
                        ////DOverlap1
                        excelApp.Cells[rowIndex, ++colIndex] =  GeometryHelper.polygonArea(GeometryHelper.GetIntersectionOfPolygons( ffr.Polygon1  , ffr_1.Polygon1))   ;
                        ////DOverlap2
                        excelApp.Cells[rowIndex, ++colIndex] = GeometryHelper.polygonArea(GeometryHelper.GetIntersectionOfPolygons(ffr.Polygon2, ffr_1.Polygon2));
                        ////DOverlap3
                        excelApp.Cells[rowIndex, ++colIndex] = GeometryHelper.polygonArea(GeometryHelper.GetIntersectionOfPolygons(ffr.Polygon3, ffr_1.Polygon3));
                    }
                    else
                    {
                        excelApp.Cells[rowIndex, ++colIndex] = 0;
                        ////DOverlap2
                        excelApp.Cells[rowIndex, ++colIndex] = 0;
                        ////DOverlap3
                        excelApp.Cells[rowIndex, ++colIndex] = 0;
                    }
                    excelApp.Cells[rowIndex, ++colIndex] = ffr.EmotionBite;
                }

            // CLOSE EXCEL FILE
            if (excelWorkbook != null)
            {
                excelWorkbook.SaveAs(excelFile);
                //excelWorkbook.Close();
                excelApp.Quit();


            }

            if (Directory.Exists(dataFolder))
            {
                Directory.Delete(dataFolder, true);
            }
            
        }

        /// <summary>
        /// Converts rotation quaternion to Euler angles 
        /// And then maps them to a specified range of values to control the refresh rate
        /// </summary>
        /// <param name="rotQuaternion">face rotation quaternion</param>
        /// <param name="pitch">rotation about the X-axis</param>
        /// <param name="yaw">rotation about the Y-axis</param>
        /// <param name="roll">rotation about the Z-axis</param>
        private static void ExtractFaceRotationInDegrees(Vector4 rotQuaternion, out int pitch, out int yaw, out int roll)
        {
            double x = rotQuaternion.X;
            double y = rotQuaternion.Y;
            double z = rotQuaternion.Z;
            double w = rotQuaternion.W;

            // convert face rotation quaternion to Euler angles in degrees
            double yawD, pitchD, rollD;
            pitchD = Math.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Math.PI * 180.0;
            yawD = Math.Asin(2 * ((w * y) - (x * z))) / Math.PI * 180.0;
            rollD = Math.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Math.PI * 180.0;

            // clamp the values to a multiple of the specified increment to control the refresh rate
            double increment = FaceRotationIncrementInDegrees;
            pitch = (int)(Math.Floor((pitchD + ((increment / 2.0) * (pitchD > 0 ? 1.0 : -1.0))) / increment) * increment);
            yaw = (int)(Math.Floor((yawD + ((increment / 2.0) * (yawD > 0 ? 1.0 : -1.0))) / increment) * increment);
            roll = (int)(Math.Floor((rollD + ((increment / 2.0) * (rollD > 0 ? 1.0 : -1.0))) / increment) * increment);
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameReaders[i] != null)
                {
                    // wire handler for face frame arrival
                    this.faceFrameReaders[i].FrameArrived += this.Reader_FaceFrameArrived;
                }

                InitializeExcel();
            }

            if (this.bodyFrameReader != null)
            {
                // wire handler for body frame arrival
                this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if(!ProcessClosingStarted)
            {
                ProcessClosingStarted = true;
                Cursor = System.Windows.Input.Cursors.Wait;

                Recorder = false;
                Parametres.Filtrer(Parametres.resultats);
                foreach (FramesResult f in Parametres.resultats)
                    f.ReadImage();

                FormResult frm = new FormResult(Parametres.resultats);
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FinalizeExcel(frm.FloderPath, Parametres.resultats);
                }

                //DropTempDirectory();


                for (int i = 0; i < this.bodyCount; i++)
                {
                    if (this.faceFrameReaders[i] != null)
                    {
                        // FaceFrameReader is IDisposable
                        this.faceFrameReaders[i].Dispose();
                        this.faceFrameReaders[i] = null;
                    }

                    if (this.faceFrameSources[i] != null)
                    {
                        // FaceFrameSource is IDisposable
                        this.faceFrameSources[i].Dispose();
                        this.faceFrameSources[i] = null;
                    }
                }

                if (this.bodyFrameReader != null)
                {
                    // BodyFrameReader is IDisposable
                    this.bodyFrameReader.Dispose();
                    this.bodyFrameReader = null;
                }

                if (this.kinectSensor != null)
                {
                    this.kinectSensor.Close();
                    this.kinectSensor = null;
                }
                DropTempDirectory();
            }
            
        }

        private void DropTempDirectory()
        {
            try
            {
                if (System.IO.Directory.Exists(Parametres.TempDirectory ))
                {
                System.IO.Directory.Delete( Parametres.TempDirectory  , true );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Handles the face frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FaceFrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
           using (FaceFrame faceFrame = e.FrameReference.AcquireFrame())
            {
                if (faceFrame != null)
                {
                   
                    // get the index of the face source from the face source array
                    int index = this.GetFaceSourceIndex(faceFrame.FaceFrameSource);
                    
                    // check if this face frame has valid face frame results
                    if (this.ValidateFaceBoxAndPoints(faceFrame.FaceFrameResult))
                    {
                        // store this face frame result to draw later
                        this.faceFrameResults[index] = faceFrame.FaceFrameResult;
                     

                    }
                    else
                    {
                        // indicates that the latest face frame result from this reader is invalid
                        this.faceFrameResults[index] = null;
                    }


                    //UpdateExcel();
                }
            }
        }

        /// <summary>
        /// Returns the index of the face frame source
        /// </summary>
        /// <param name="faceFrameSource">the face frame source</param>
        /// <returns>the index of the face source in the face source array</returns>
        private int GetFaceSourceIndex(FaceFrameSource faceFrameSource)
        {
            int index = -1;

            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameSources[i] == faceFrameSource)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {  if(Recorder)
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                
                if (bodyFrame != null)
                {
                    // update body data
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    
                    using (DrawingContext dc = this.drawingGroup.Open())
                    {
                        // draw the dark background
                        dc.DrawRectangle(Brushes.Black, null, this.displayRect);

                        bool drawFaceResult = false;

                        // iterate through each face source
                        for (int i = 0; i < this.bodyCount; i++)
                        {
                            // check if a valid face is tracked in this face source
                            if (this.faceFrameSources[i].IsTrackingIdValid)
                            {
                                // check if we have valid face frame results
                                if (this.faceFrameResults[i] != null)
                                {
                                    // draw face frame results

                                    FaceFrameResult f = this.faceFrameResults[i];

                                    FramesResult frmres = new FramesResult(f.RelativeTime);
                                    Parametres.resultats.Add(frmres);

                                    this.DrawFaceFrameResults(i, f, dc ,  frmres);

                                    
                                    cfr_FrameArrived( cfr.AcquireLatestFrame() , f ,  frmres);


                                    if (!drawFaceResult)
                                    {
                                        drawFaceResult = true;
                                    }
                                }
                            }
                            else
                            {
                                // check if the corresponding body is tracked 
                                if (this.bodies[i].IsTracked)
                                {
                                    // update the face frame source to track this body
                                    this.faceFrameSources[i].TrackingId = this.bodies[i].TrackingId;
                                }

                            }

                        }

                        if (!drawFaceResult)
                        {
                            // if no faces were drawn then this indicates one of the following:
                            // a body was not tracked 
                            // a body was tracked but the corresponding face was not tracked
                            // a body and the corresponding face was tracked though the face box or the face points were not valid
                            dc.DrawText(
                                this.textFaceNotTracked,
                                this.textLayoutFaceNotTracked);
                        }

                        this.drawingGroup.ClipGeometry = new RectangleGeometry(this.displayRect);
                    }
                }
            }
        }


        void cfr_FrameArrived( ColorFrame e , FaceFrameResult  f ,   FramesResult frmres)
        {
            if (e == null) return; 
            ColorConverter c = new ColorConverter();
            using (ColorFrame cf = e)
            {
                if (cf == null)  return; 
                cf.CopyConvertedFrameDataToArray(colorData, format);
                var fd = cf.FrameDescription;

                // Creating BitmapSource
                var bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel) / 8;
                var stride = bytesPerPixel * cf.FrameDescription.Width;

                bmpSource = BitmapSource.Create(fd.Width, fd.Height, 96.0, 96.0, PixelFormats.Bgr32, null, colorData, stride);

                // WritableBitmap to show on UI
                wbmp = new WriteableBitmap(bmpSource);
                // kinectImage.Source = wbmp;

                // if record started start saving frames
                //if (recordStarted)

                // JpegBitmapEncoder to save BitmapSource to file
                // imageSerial is the serial of the sequential image
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                //using (var fs = new FileStream(Environment.CurrentDirectory + @"\" + e.FrameReference.AcquireFrame().RelativeTime.ToString().Replace('.' ,'_').Replace(":" , "_")  + ".jpeg", FileMode.Create, FileAccess.Write))
                //{
                //    encoder.Save(fs);
                frmres.RecordedImage = new RecordedImage() { JpegImage = encoder, Time = f.RelativeTime };
                //Parametres.images.Add(new RecordedImage() { JpegImage = encoder, Time = f.RelativeTime });
                //}

            }
        }

        /// <summary>
        /// Draws face frame results
        /// </summary>
        /// <param name="faceIndex">the index of the face frame corresponding to a specific body in the FOV</param>
        /// <param name="faceResult">container of all face frame results</param>
        /// <param name="drawingContext">drawing context to render to</param>
        private void DrawFaceFrameResults(int faceIndex, FaceFrameResult faceResult, DrawingContext drawingContext , FramesResult frmres)
        {
            

             // choose the brush based on the face index
             Brush drawingBrush = this.faceBrush[0];
            if (faceIndex < this.bodyCount)
            {
               
                drawingBrush = this.faceBrush[faceIndex];
               Console.WriteLine("face color index =" + " "+ faceIndex);
            }

            Pen drawingPen = new Pen(drawingBrush, DrawFaceShapeThickness);

            // draw the face bounding box
            var faceBoxSource = faceResult.FaceBoundingBoxInColorSpace;
            Rect faceBox = new Rect(faceBoxSource.Left, faceBoxSource.Top, faceBoxSource.Right - faceBoxSource.Left, faceBoxSource.Bottom - faceBoxSource.Top);
            drawingContext.DrawRectangle(null, drawingPen, faceBox);

            frmres.FaceBox = faceBox;


            Console.WriteLine("the coordinates of the rectangle");
            Console.WriteLine("XLeft ="+ faceBox.Left);
            Console.WriteLine("YTop="+ faceBox.Top);
            Console.WriteLine("XRight=" + faceBox.Right);
            Console.WriteLine("YBottom="+faceBox.Bottom);
            
           
            if (faceResult.FacePointsInColorSpace != null)
            {
                Console.WriteLine("The face Points In color space are:");
                // draw each face point
                foreach (PointF pointF in faceResult.FacePointsInColorSpace.Values)
                {
                    drawingContext.DrawEllipse(null, drawingPen, new Point(pointF.X, pointF.Y), FacePointRadius, FacePointRadius);
                    
                    Console.WriteLine("X=" + pointF.X );
                    Console.WriteLine("Y=" + pointF.Y);
                    
                }
                
                               
            }

            string faceText = string.Empty;

            // extract each face property information and store it in faceText
            if (faceResult.FaceProperties != null)
            {
                
                foreach (var item in faceResult.FaceProperties)
                {
                    faceText += item.Key.ToString() + " : ";
                    
                    // consider a "maybe" as a "no" to restrict 
                    // the detection result refresh rate
                    if (item.Value == DetectionResult.Maybe)
                    {
                        faceText += DetectionResult.No + "\n";
                    }
                    
                    else
                    {
                        
                        faceText += item.Value.ToString() + "\n";
                       
                    }

                    

                }
                Console.WriteLine("The properties of face frame features are" + "\n" + faceText);
            }

            // extract face rotation in degrees as Euler angles
            if (faceResult.FaceRotationQuaternion != null)
            {
                int pitch, yaw, roll;
                ExtractFaceRotationInDegrees(faceResult.FaceRotationQuaternion, out pitch, out yaw, out roll);
                faceText += "FaceYaw : " + yaw + "\n" +
                            "FacePitch : " + pitch + "\n" +
                            "FacenRoll : " + roll + "\n";
                // Console.WriteLine(faceIndex);
                Console.WriteLine();
                Console.WriteLine("The angles of face rotation in degrees"+"\n" + "FaceYaw : "+ yaw + "\n" + "FacePitch : " + pitch + "\n" + "FacenRoll : " + roll);
            }
            if (faceResult.RelativeTime != null)
            {
                TimeSpan timeSpan = faceResult.RelativeTime;
                Console.WriteLine("IDFrame=TimeStamp"+ " "+timeSpan);
            }
            if (faceResult.TrackingId !=0)
            {
                ulong trackingID = faceResult.TrackingId;
                Console.WriteLine("TrackingId" +" "+ trackingID);
            }
           
            // render the face property and face rotation information
            Point faceTextLayout;
            if (this.GetFaceTextPositionInColorSpace(faceIndex, out faceTextLayout))
            {
                drawingContext.DrawText(
                        new FormattedText(
                            faceText,
                            CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight,
                            new Typeface("Georgia"),
                            DrawTextFontSize,
                            drawingBrush),
                        faceTextLayout);
            }
            
            Console.WriteLine("----------------------------------------------------------");

            frmres.FaceFrameResult = faceResult;
            frmres.FaceIndex = faceIndex;
            //Parametres.result.Add(faceResult);
            //Parametres.resultIndexes.Add(faceIndex);
            //UpdateExcel();
        }

        /// <summary>
        /// Computes the face result text position by adding an offset to the corresponding 
        /// body's head joint in camera space and then by projecting it to screen space
        /// </summary>
        /// <param name="faceIndex">the index of the face frame corresponding to a specific body in the FOV</param>
        /// <param name="faceTextLayout">the text layout position in screen space</param>
        /// <returns>success or failure</returns>
        private bool GetFaceTextPositionInColorSpace(int faceIndex, out Point faceTextLayout)
        {
            faceTextLayout = new Point();
            bool isLayoutValid = false;

            Body body = this.bodies[faceIndex];
            if (body.IsTracked)
            {
                var headJoint = body.Joints[JointType.Head].Position;
               

                CameraSpacePoint textPoint = new CameraSpacePoint()
                {
                    X = headJoint.X + TextLayoutOffsetX,
                    Y = headJoint.Y + TextLayoutOffsetY,
                    Z = headJoint.Z

                };
                 
                Console.WriteLine("The position Coordinates  of the person relative to kinect on 3 planes X, Y and Z");
                Console.WriteLine("XKinect=" +" "+ textPoint.X);
                Console.WriteLine("YKinect=" + " " + textPoint.Y);
                Console.WriteLine("ZKinect=" + " " + textPoint.Z);
                ColorSpacePoint textPointInColor = this.coordinateMapper.MapCameraPointToColorSpace(textPoint);

            
                faceTextLayout.X = textPointInColor.X;
                faceTextLayout.Y = textPointInColor.Y;
                isLayoutValid = true;

                
            }

            return isLayoutValid;
        }

        private CameraSpacePoint GetCameraSpacePointInColorSpace(int faceIndex, out Point faceTextLayout)
        {
            faceTextLayout = new Point();
            bool isLayoutValid = false;

            Body body = this.bodies[faceIndex];
            if (body.IsTracked)
            {
                var headJoint = body.Joints[JointType.Head].Position;


                CameraSpacePoint textPoint = new CameraSpacePoint()
                {
                    X = headJoint.X + TextLayoutOffsetX,
                    Y = headJoint.Y + TextLayoutOffsetY,
                    Z = headJoint.Z

                };
                return textPoint;
            }
            else return new CameraSpacePoint();
        }

                /// <summary>
                /// Validates face bounding box and face points to be within screen space
                /// </summary>
                /// <param name="faceResult">the face frame result containing face box and points</param>
                /// <returns>success or failure</returns>
                private bool ValidateFaceBoxAndPoints(FaceFrameResult faceResult)
        {
            bool isFaceValid = faceResult != null;

            if (isFaceValid)
            {
                var faceBox = faceResult.FaceBoundingBoxInColorSpace;
                if (faceBox != null)
                {
                    // check if we have a valid rectangle within the bounds of the screen space
                    isFaceValid = (faceBox.Right - faceBox.Left) > 0 &&
                                  (faceBox.Bottom - faceBox.Top) > 0 &&
                                  faceBox.Right <= this.displayWidth &&
                                  faceBox.Bottom <= this.displayHeight;

                    if (isFaceValid)
                    {
                        var facePoints = faceResult.FacePointsInColorSpace;
                        if (facePoints != null)
                        {
                            foreach (PointF pointF in facePoints.Values)
                            {
                                // check if we have a valid face point within the bounds of the screen space
                                bool isFacePointValid = pointF.X > 0.0f &&
                                                        pointF.Y > 0.0f &&
                                                        pointF.X < this.displayWidth &&
                                                        pointF.Y < this.displayHeight;

                                if (!isFacePointValid)
                                {
                                    isFaceValid = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return isFaceValid;
        }
        //private void CalculDistance ()
        //{
        //    //calculate the distance between the 2 interestpoint
        //    foreach (FaceFrameResult ffr1 in Parametres.result)
        //    { 
                
        //    }
        //}
        

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (this.kinectSensor != null)
            {
                // on failure, set the status text
                this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                                : Properties.Resources.SensorNotAvailableStatusText;
            }
        }
    }

   
}