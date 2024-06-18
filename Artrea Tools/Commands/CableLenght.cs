using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ArtreaTools
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CableLenght : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Selecting Lines
            ISelectionFilter selFilter = new LineSellFilter();
            ICollection<Element> selLines = uidoc.Selection.PickElementsByRectangle(selFilter, "Select the lines") as ICollection<Element>;
            
            double sumLenght = 0.0;
            double doubleLenght = 0.0;        

            //Line Lenght
            if (selLines.Count > 0)
            {
                foreach (Element ele in selLines)
                {
                    string lLenght = ele.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsValueString();
                    doubleLenght = Convert.ToDouble(lLenght);

                    sumLenght += doubleLenght;
                }

                // Plus 10%
                double pluslenght = sumLenght * 1.1;

                //Rounding
                double lenghtMeter = pluslenght / 1000;
                int lenghtRound =Convert.ToInt32(Math.Round(lenghtMeter, 0));

                //Statement
                TaskDialog.Show("Cable lenght", "Lenght of lines (+10%): " + lenghtRound.ToString() + "m.", TaskDialogCommonButtons.Ok);

                //Deleting Lines
                using (Transaction trans = new Transaction(doc, "Delete Lines"))
                {
                    trans.Start();
                    
                    foreach(Element line in selLines)
                    {
                        ElementId linesId = line.Id;
                        doc.Delete(linesId);
                        
                    }

                    trans.Commit();
                }

            }
            else
            {
                TaskDialog.Show("Errrr...!", "You have not selected a CableLenght line!");
            }
   
            return Result.Succeeded;
        }
    }

    public class LineSellFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Name == "Lines")
            {
                if (elem.get_Parameter(BuiltInParameter.BUILDING_CURVE_GSTYLE).AsValueString() == "CableLenght")
                {
                    return true;
                }
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
