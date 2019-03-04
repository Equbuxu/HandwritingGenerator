using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    public class FormPreparer
    {
        private bool generated = false;
        private const int smallImageWidth = 350;
        private const int largeImageWidth = 1500;

        private string path;
        private byte BWThreshold;
        private Bitmap originalForm;
        private Bitmap smallBWForm;
        private Bitmap largeForm;

        private Bitmap result;

        public FormPreparer(string formPath)
        {
            path = formPath;
        }

        public Bitmap CreatePrepared()
        {
            if (generated)
                return result;
            generated = true;

            //BWThreshold, originalForm, smallBWForm, largeForm
            PrepareData();
            List<Vector> impreciseMarkers = FindMarkersOnSmallForm();
            List<Vector> preciseMarkers = PrecisifyMarkers(impreciseMarkers);
            List<Vector> identifiedMarkers = IdentifyMarkers(preciseMarkers);
            List<Vector> fullyRescaledMarkers = identifiedMarkers.Select((a) => a * (originalForm.Width / (double)largeImageWidth)).ToList();
            Bitmap fixedOrientationForm = RotateCrop(fullyRescaledMarkers, originalForm);
            BitmapUtils.Autocontrast(fixedOrientationForm);
            fixedOrientationForm.Save("DebugOut/prepared.png");
            result = fixedOrientationForm;

            return result;
        }

        /// <summary>
        /// There must be 5 markers
        /// Returns 5 markers in order: topleft, topright, botleft, botright, fifth
        /// </summary>
        private List<Vector> IdentifyMarkers(List<Vector> markers)
        {
            //I've already forgot how this thing works. Hopefully there are no bugs

            if (markers.Count != 5)
            {
                throw new ArgumentException("invalid marker list");
            }

            //find distances
            List<Vector> smallestDistVectors = new List<Vector>();
            List<Vector> largestDistVectors = new List<Vector>();
            double smallestDist = double.MaxValue;
            double largestDist = 0;
            foreach (Vector marker in markers)
            {
                foreach (Vector otherMarker in markers)
                {
                    if (marker == otherMarker)
                        continue;

                    double dist = (marker - otherMarker).Length;
                    if (dist < smallestDist) //precise math since only one pair is expected
                    {
                        smallestDist = dist;
                        smallestDistVectors.Clear();
                        smallestDistVectors.Add(marker);
                        smallestDistVectors.Add(otherMarker);
                    }

                    if (Precision.DefBigger(dist, largestDist))
                    {
                        largestDist = dist;
                        largestDistVectors.Clear();
                        if (!largestDistVectors.Contains(marker))
                            largestDistVectors.Add(marker);
                        if (!largestDistVectors.Contains(otherMarker))
                            largestDistVectors.Add(otherMarker);
                    }
                    else if (Precision.ApproxEquals(dist, largestDist))
                    {
                        if (!largestDistVectors.Contains(marker))
                            largestDistVectors.Add(marker);
                        if (!largestDistVectors.Contains(otherMarker))
                            largestDistVectors.Add(otherMarker);
                    }
                }
            }


            //identify markers
            List<Vector> result = new List<Vector>();
            for (int i = 0; i < 5; i++)
                result.Add(null);

            result[0] = smallestDistVectors.Intersect(largestDistVectors).First(); //topleft 
            result[4] = smallestDistVectors[0] == result[0] ? smallestDistVectors[1] : smallestDistVectors[0]; //fifth

            double refAngle = (result[4] - result[0]).Angle;

            foreach (Vector vector in largestDistVectors)
            {
                if (vector == result[4] || vector == result[0])
                    continue;
                double angle = (vector - result[0]).Angle;
                if (Precision.ApproxEquals(angle, refAngle))
                    result[1] = vector; //topright
                else if (Precision.ApproxEquals(angle, MathUtils.NormalizeAngle(refAngle + Math.PI / 2)))
                    result[2] = vector; //botleft
                else if (Precision.ApproxEquals(angle, MathUtils.NormalizeAngle(refAngle + 0.9959388)))
                    result[3] = vector; //botright
            }
            if (result.Contains(null))
                return null;
            return result;
        }

        /// <summary>
        /// Takes the positions of markers from the small form and finds them on the large form
        /// </summary>
        private List<Vector> PrecisifyMarkers(List<Vector> impreciseMarkers)
        {
            double scalingFactor = largeImageWidth / (double)smallImageWidth;

            List<Vector> rescaledMarkers = impreciseMarkers.Select((a) => a * scalingFactor).ToList();
            List<Vector> precisifiedMarkers = new List<Vector>();

            Bitmap visited = new Bitmap(largeImageWidth, largeImageWidth * originalForm.Height / originalForm.Width);
            foreach (Vector impreciseMarker in rescaledMarkers)
            {
                List<Vector> connComponentPoints = new List<Vector>();
                TraverseConnectedComponentDFS(connComponentPoints, largeForm, visited, (int)impreciseMarker.X, (int)impreciseMarker.Y, BWThreshold);
                double totalX = 0, totalY = 0;
                connComponentPoints.ForEach((a) => { totalX += a.X; totalY += a.Y; });
                precisifiedMarkers.Add(new Vector(totalX / connComponentPoints.Count, totalY / connComponentPoints.Count));
            }

            return precisifiedMarkers;
        }

        private List<Vector> FindMarkersOnSmallForm()
        {
            //Find all black areas
            List<List<Vector>> blackAreas = new List<List<Vector>>();

            Bitmap visited = new Bitmap(smallBWForm.Width, smallBWForm.Height); //R > 0 => visited

            for (int j = 0; j < smallBWForm.Height; j++)
            {
                for (int i = 0; i < smallBWForm.Width; i++)
                {
                    if (smallBWForm.GetPixel(i, j).R == 0 && visited.GetPixel(i, j).R == 0)
                    {
                        List<Vector> area = new List<Vector>();
                        TraverseConnectedComponentDFS(area, smallBWForm, visited, i, j, BWThreshold);
                        blackAreas.Add(area);
                    }
                }
            }

            //Choose biggest areas
            blackAreas.Sort((a, b) => b.Count - a.Count);

            if (blackAreas.Count < 5)
                return null;

            //Find their centers
            List<Vector> squareCenters = new List<Vector>();

            for (int i = 0; i < 5; i++)
            {
                double sumX = 0;
                double sumY = 0;
                int count = 0;
                foreach (Vector p in blackAreas[i])
                {
                    sumX += p.X;
                    sumY += p.Y;
                    count++;
                }

                squareCenters.Add(new Vector(sumX / count, sumY / count));
            }

            return squareCenters;
        }

        private void PrepareData()
        {
            originalForm = new Bitmap(path);

            Bitmap smallForm = BitmapUtils.Resize(originalForm, smallImageWidth, originalForm.Height * smallImageWidth / originalForm.Width);
            BWThreshold = BitmapUtils.GetMinMaxMiddleColorThreshold(smallForm);
            CreateSmallExpandedBW(smallForm);

            largeForm = BitmapUtils.Resize(originalForm, largeImageWidth, originalForm.Height * largeImageWidth / originalForm.Width);
        }

        private void CreateSmallExpandedBW(Bitmap smallForm)
        {
            smallBWForm = BitmapUtils.MakeGrayscale(smallForm);
            smallBWForm = BitmapUtils.MakeBlackAndWhite(smallBWForm, BWThreshold);
            smallBWForm.Save("DebugOut/blackwhite.png");
            smallBWForm = BitmapUtils.ExpandWhite(smallBWForm);
            smallBWForm.Save("DebugOut/Expandede.png");
        }

        /// <summary>
        /// Rotates and crops an image according to markers
        /// </summary>
        private static Bitmap RotateCrop(List<Vector> markers, Bitmap image)
        {
            Vector[] corners = new Vector[3];
            corners[0] = new Vector(0, 0);
            corners[1] = new Vector(image.Width, 0);
            corners[2] = new Vector(0, image.Height);
            //corners[3] = new Vector(image.Width, image.Height);

            double rotAngle = -(markers[1] - markers[0]).Angle;
            Vector translVector = new Vector(0, 0) - markers[0].Rotate(rotAngle);
            corners = corners.Select((vec) => vec.Rotate(rotAngle) + translVector).ToArray();

            int newWidth = (int)Math.Round((markers[0] - markers[1]).Length);
            //int newHeight = (int)(newWidth * 1.5435835);
            int newHeight = (int)Math.Round((markers[2] - markers[0]).Length);
            Bitmap result = new Bitmap(newWidth, newHeight);
            Graphics gr = Graphics.FromImage(result);
            Point[] destPoints = corners.Select((vec) => new Point((int)Math.Round(vec.X), (int)Math.Round(vec.Y))).ToArray();
            gr.DrawImage(image, destPoints, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            gr.Dispose();
            return result;
        }

        /// <summary>
        /// Fills result list with points belonging to the component
        /// </summary>
        private static void TraverseConnectedComponentDFS(List<Vector> result, Bitmap map, Bitmap visited, int x, int y, byte threshold)
        {
            if (x < 0 || x >= map.Width || y < 0 || y >= map.Height || IsVisited(visited, x, y) || !BitmapUtils.IsBelowThreshold(map, x, y, threshold))
                return;
            result.Add(new Vector(x, y));
            visited.SetPixel(x, y, Color.Red);

            TraverseConnectedComponentDFS(result, map, visited, x + 1, y, threshold);
            TraverseConnectedComponentDFS(result, map, visited, x, y + 1, threshold);
            TraverseConnectedComponentDFS(result, map, visited, x - 1, y, threshold);
            TraverseConnectedComponentDFS(result, map, visited, x, y - 1, threshold);
        }

        /// <summary>
        /// R > 0 => visited
        /// </summary>
        private static bool IsVisited(Bitmap map, int x, int y)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return true;
            return map.GetPixel(x, y).R > 0;
        }
    }
}
