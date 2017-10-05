using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{

    public class YoloBridge
    {
        private const string VOC_NAMES = @"C:\dev\yolo\darknet\data\voc.names";
        private const string YOLO_CPP_DLL = @"C:\dev\yolo\darknet\build\darknet\x64\yolo_cpp_dll.dll";
        private const string YOLO_VOC_CFG = @"C:\dev\yolo\darknet\build\darknet\x64\yolo-voc.cfg";
        private const string YOLO_VOC_WEIGHTS = @"C:\dev\yolo\darknet\build\darknet\x64\yolo-voc.weights";

        [DllImport(YOLO_CPP_DLL, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void initDetector(string cfg, int cfgLen, string weights, int weightsLen);

        [DllImport(YOLO_CPP_DLL, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void closeDetector();

        [DllImport(YOLO_CPP_DLL, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void detectFrame(uint width, uint height, IntPtr imgData, float thresh,
                                                      out IntPtr hItems, out bbox_t* itemsFound, out int itemCount);

        public struct bbox_t
        {
            int x, y, w, h;    // (x,y) - top-left corner, (w, h) - width & height of bounded box
            float prob;                 // confidence - probability that the object was found correctly
            int obj_id;        // class of object - from range [0, classes-1]
            int track_id;      // tracking id for video (0 - untracked, 1 - inf - tracked object)

            public float Prob { get => prob; set => prob = value; }
            public int Obj_id { get => obj_id; set => obj_id = value; }
            public int Track_id { get => track_id; set => track_id = value; }
            public int X { get => x; set => x = value; }
            public int Y { get => y; set => y = value; }
            public int W { get => w; set => w = value; }
            public int H { get => h; set => h = value; }

            public override String ToString() {
                return "Bbox: obj_id: " + obj_id + ", prob: " + prob + ", track_id: " + track_id +", x: " + x +", y: " + y + ", w: " + w + ", h:" + h + "";
            }
        };

        

        public Bitmap ResizeImage(Bitmap img)
        {
            int qSize = img.Width < img.Height ? img.Width : img.Height;
            Bitmap b = new Bitmap(qSize, qSize);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawImage(img, 0, 0, qSize, qSize);
            }
            return b;
        }

        public string getVOCName(int id) {
            var lines = File.ReadAllLines(VOC_NAMES).Select(s => s.Split('\t')).ToArray();
            return lines.Length > 0 ? lines.ElementAt(id)[0] : "";            
        }
        

        public Bitmap DrawRectangle(Bitmap b, bbox_t bbox) {
            Pen pen = new Pen(Color.Red, 3);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawString(getVOCName(bbox.Obj_id), new Font(FontFamily.GenericSansSerif, 20), new SolidBrush(Color.Red), new PointF(bbox.X, bbox.Y));
                g.DrawRectangle(pen, bbox.X, bbox.Y, bbox.W, bbox.H);
            }                
            return b;
        }

        private unsafe bbox_t[] detectQimage(Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr sourcePoint = data.Scan0;
            IntPtr itemsHandle = IntPtr.Zero;
            bbox_t* itemsFound;
            int itemsCount;
            detectFrame((uint)bmp.Width, (uint)bmp.Height, sourcePoint, 0.2f, out itemsHandle, out itemsFound, out itemsCount);
            bmp.UnlockBits(data);
            bbox_t[] resArray = new bbox_t[itemsCount];
            for (int i = 0; i < resArray.Length; ++i)
            {
                resArray[i] = itemsFound[i];        
            }
            return resArray;
        }

        public bbox_t[] detect(Bitmap source)
        {
            return detectQimage(ResizeImage(source));
        }


        public YoloBridge()
        {
            try
            {
                initDetector(YOLO_VOC_CFG, YOLO_VOC_CFG.Length, YOLO_VOC_WEIGHTS, YOLO_VOC_WEIGHTS.Length);
            }
            catch (Exception exception)
            {

                Console.WriteLine(exception.ToString());
                throw exception;
            }
            
        }

        ~YoloBridge()
        {
            closeDetector();
        }

    }
}
