using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArtreaTools
{
    /// <summary>
    /// Interaction logic for BarrayWindow.xaml
    /// </summary>
    public partial class BarrayWindow : Window
    {
        Document Doc;
        UIDocument UIdoc;
        UIApplication UIapp;
        public BarrayWindow(Document doc, UIDocument uidoc, UIApplication uiApp)
        {
            InitializeComponent();
            Doc = doc;
            UIdoc = uidoc;
            UIapp = uiApp;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            IList<Element> lightingFixtures = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_LightingFixtures).
                WhereElementIsElementType().ToElements();

            foreach(Element lightingFixture in lightingFixtures)
            {
                LightingFixtures.Items.Add(lightingFixture.Name);
            }
        }

        private void LineSelect(object sender, RoutedEventArgs e)
        {
            IList<Element> lightingFixtures = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_LightingFixtures).
                WhereElementIsElementType().ToElements();

            string selectedlamp = this.LightingFixtures.SelectedItem.ToString();
            FamilySymbol lamp = null;


            foreach (Element lightingFixture in lightingFixtures)
            {
                if (lightingFixture.Name == selectedlamp)
                {
                    lamp = lightingFixture as FamilySymbol;
                }
            }

            Close();

            XYZ selXstart = UIdoc.Selection.PickPoint(ObjectSnapTypes.Intersections, "Kattints a vizszintes tengely kezdőpontjához");
            XYZ selXend = UIdoc.Selection.PickPoint(ObjectSnapTypes.Intersections, "Kattints a vizszintes tengely végpontjához");

            XYZ selYstart = UIdoc.Selection.PickPoint(ObjectSnapTypes.Intersections, "Kattints a függőleges tengely kezdőpontjához");
            XYZ selYend = UIdoc.Selection.PickPoint(ObjectSnapTypes.Intersections, "Kattints a függőleges tengely végpontjához");


            int colNumber = 0;
            int.TryParse(Col, out colNumber);

            int rowNumber = 0;
            int.TryParse(Row, out rowNumber);

            int lampHeight = 0;
            int.TryParse(Elevation, out lampHeight);

            //Convert MMeter to feet
            double elevationFeet = lampHeight / 304.8;

            //Create Lines
            DetailCurve xDetailCurve = HorizontalLine(selXstart, selXend) as DetailCurve;
            DetailCurve yDetailCurve = VerticalLine(selYstart, selYend) as DetailCurve;


            //Get Lines Directions
            Autodesk.Revit.DB.Line xLine = xDetailCurve.GeometryCurve as Autodesk.Revit.DB.Line;
            double xLineDirectionX = xLine.Direction.X;

            Autodesk.Revit.DB.Line yLine = yDetailCurve.GeometryCurve as Autodesk.Revit.DB.Line;
            double yLineDirectionY = yLine.Direction.Y;


            //Get Lines Lenght
            double xLenght = xDetailCurve.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
            //double xLenghtdouble = Convert.ToDouble(xLenght);
            //double xLenghtfeet = xLenghtdouble / 3.28084;

            double yLenght = yDetailCurve.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
            //double yLenghtdouble = Convert.ToDouble(yLenght);
            //double yLenghtfeet = yLenghtdouble / 3.28084;


            //Calculate Lamp Distance and Starting Point
            double horizontalDistance = xLenght / colNumber;
            double verticalDistance = yLenght / rowNumber;

            double horStartDistance = horizontalDistance / 2;
            double verStartDistance = verticalDistance / 2;

            XYZ startPoint = Startpoint(horizontalDistance, horStartDistance, verticalDistance, verStartDistance, xLineDirectionX, yLineDirectionY, selXstart, selYstart).Item1;
            horizontalDistance = Startpoint(horizontalDistance, horStartDistance, verticalDistance, verStartDistance, xLineDirectionX, yLineDirectionY, selXstart, selYstart).Item2;
            verticalDistance = Startpoint(horizontalDistance, horStartDistance, verticalDistance, verStartDistance, xLineDirectionX, yLineDirectionY, selXstart, selYstart).Item3;

            //Create First Lamp

            Element firstLamp = null;
            using (Transaction createFirstLamp = new Transaction(Doc, "CreateFirstLamp"))
            {
                createFirstLamp.Start();

                if (!lamp.IsActive)
                {
                    lamp.Activate();
                }

                //Get level
                Level level = Doc.ActiveView.GenLevel;

                if (Doc.IsWorkshared == false)
                {
                    firstLamp = Doc.Create.NewFamilyInstance(startPoint, lamp, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                }
                if (Doc.IsWorkshared == true)
                {
                    //Create Lamp
                    
                    firstLamp = Doc.Create.NewFamilyInstance(startPoint, lamp,level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    //Select the 200 - Lighting
                    IList<Workset> worksetList = new FilteredWorksetCollector(Doc).OfKind(WorksetKind.UserWorkset).ToWorksets();
                    int lightingWorksetId = 0;

                    foreach (Workset workset in worksetList)
                    {
                        if (workset.Name.Equals("200 - Lighting"))
                        {
                            lightingWorksetId = workset.Id.IntegerValue;
                        }
                    }
                    
                    

                    

                    //Set Level
                    /*
                    Parameter lightLevel = firstLamp.LookupParameter("Level");
                    lightLevel.Set(level.Id);*/

                    //Set Workset
                    Parameter lampparam = firstLamp.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                    lampparam.Set(lightingWorksetId);

                    //198

                    //Set Elevation
                    Parameter lampelevation = firstLamp.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM);
                    lampelevation.Set(elevationFeet);

                    createFirstLamp.Commit();
                }

                //Placing lamps
                ElementId firstId = firstLamp.Id;

                ICollection<ElementId> rowId = FirstRow(Doc, firstId, colNumber, horizontalDistance);
                ICollection<ElementId> anotherRowId = AnotherRow(Doc, rowId, rowNumber, verticalDistance);

                UIdoc.Selection.SetElementIds(anotherRowId);

                //var rotateRad = firstLamp.GetParameter("LocationPoint");
                //TaskDialog.Show("Asdf", anotherRowId.Count.ToString());

                //Rotate Lamps - Not working
               // Rotate(anotherRowId,Doc);

                //Delete Axis lines
                DeleteLines(xDetailCurve, yDetailCurve);

            }
        }
            
        public string Col
        {
            get { return this.column_number.Text; }
        }

        public string Row
        {
            get { return this.row_number.Text; }
        }

        public string Elevation
        {
            get { return this.lamp_height.Text; }
        }

        public DetailCurve HorizontalLine(XYZ selXstart, XYZ selXend)
        {
            var xLine = Autodesk.Revit.DB.Line.CreateBound(selXstart, selXend);

            DetailCurve xCurve = null;

            using (Transaction createHLinesTransaction = new Transaction(Doc, "CreateHorizontalLines"))
            {
                createHLinesTransaction.Start();
                xCurve = Doc.Create.NewDetailCurve(Doc.ActiveView, xLine);
                createHLinesTransaction.Commit();
            }

            return xCurve;
        }

        public DetailCurve VerticalLine(XYZ selYstart, XYZ selYend)
        {
            var yLine = Autodesk.Revit.DB.Line.CreateBound(selYstart, selYend);
                        
            DetailCurve yCurve;

            using (Transaction createVLinesTransaction = new Transaction(Doc, "CreateVerticalLines"))
            {
                createVLinesTransaction.Start();                
                yCurve = Doc.Create.NewDetailCurve(Doc.ActiveView, yLine);                                
                createVLinesTransaction.Commit();
            }

            return yCurve;
        }

        public (XYZ,double, double) Startpoint (double horizontalDistance, double horStartDistance, double verticalDistance, double verStartDistance, double xLineDirectionX, double yLineDirectionY, XYZ selXstart, XYZ selYstart)
        {
            double startPointX = 0.0;
            double startPointY = 0.0;

            if (xLineDirectionX > 0)
            {
                double startPX = selXstart.X + horStartDistance;
                startPointX += startPX;
            }
            else if (xLineDirectionX < 0)
            {
                double startPX = selXstart.X - verStartDistance;
                startPointX += startPX;
                horizontalDistance *= -1;
            }

            if (yLineDirectionY > 0)
            {
                double startPY = selYstart.Y + verStartDistance;
                startPointY += startPY;
            }
            else if (yLineDirectionY < 0)
            {
                double startPY = selYstart.Y - verStartDistance;
                startPointY += startPY;
                verticalDistance *= -1;
            }

            XYZ startPoint = new XYZ(startPointX, startPointY, 0);
            return (startPoint,horizontalDistance,verticalDistance);
        }

        public ICollection<ElementId> FirstRow (Document Doc, ElementId firstId, int colNumber, double horizontalDistance)
        {
           // ICollection<ElementId> copiedLampId = null;
            ICollection<ElementId> firstrowId = null;
            //firstrowId.Add(firstId);
            using (Transaction arrayFirstRow = new Transaction(Doc, "Array First row"))
            {
                arrayFirstRow.Start();

                XYZ columnDistance = new XYZ(horizontalDistance, 0, 0);
                firstrowId = LinearArray.ArrayElementWithoutAssociation(Doc, Doc.ActiveView, firstId, colNumber, columnDistance, ArrayAnchorMember.Second);
                //firstrowId = lampColumn.GetCopiedMemberIds();
                firstrowId.Add(firstId);

                arrayFirstRow.Commit();

                
            }
            return firstrowId;
        }

        public ICollection<ElementId> AnotherRow(Document Doc, ICollection<ElementId> firstrowId, int rowNumber, double verticalDistance)
        {
            ICollection<ElementId> anotherRowId = null;
            //firstrowId.Add(firstId);
            using (Transaction rowArray = new Transaction(Doc, "Vertical array"))
            {
                rowArray.Start();

                XYZ rowDistance = new XYZ(0, verticalDistance, 0);
                anotherRowId = LinearArray.ArrayElementsWithoutAssociation(Doc, Doc.ActiveView, firstrowId, rowNumber, rowDistance, ArrayAnchorMember.Second);

                rowArray.Commit();
            }
            return anotherRowId;
        }

        public void Rotate (ICollection<ElementId> anotherRowId, Document Doc)
        {
            TaskDialog rotateDialog = new TaskDialog("Forgatás");
            rotateDialog.MainContent = "El kell forgatni a lámpákat?";
            rotateDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            TaskDialogResult result = rotateDialog.Show();

            if(TaskDialogResult.Yes == result)
            {
                double angle = 90 * Math.PI / 180;
                bool rotated = false;

                using(Transaction rotateLamps = new Transaction(Doc, "Rotate Lamps"))
                {
                    rotateLamps.Start();
                    
                    foreach (ElementId id in anotherRowId)
                    {
                        Element lamp = Doc.GetElement(id);
                        LocationPoint location = lamp.Location as LocationPoint;
                        
                        
                        XYZ axisStart = location.Point;
                        XYZ vectorEnd = new XYZ(10, 0, 0);
                        XYZ axisEnd = axisStart + vectorEnd;
                        

                        Autodesk.Revit.DB.Line axis = Autodesk.Revit.DB.Line.CreateBound(axisStart, axisEnd);

                        ElementTransformUtils.RotateElement(Doc,id,axis,angle);

                    }
                    rotateLamps.Commit();
 
                }

                if (!rotated)
                {
                    throw new Exception("asdf");
                }
            }  
        }

        public void DeleteLines(DetailCurve xDetailCurve, DetailCurve yDetailCurve)
        {
            using (Transaction deleteLines = new Transaction(Doc,"Delete Lines"))
            {
                deleteLines.Start();
                Doc.Delete(xDetailCurve.Id);
                Doc.Delete(yDetailCurve.Id);
                deleteLines.Commit();
            }
        }
    }
}
