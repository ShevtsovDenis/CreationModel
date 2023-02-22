using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModel
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            var walls = CreateWalls(doc);

            return Result.Succeeded;
        }

        //метод для получения списка уровней
        public List<Level> GetLevels (Document doc)
        {
            var listLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();
            return listLevel;
        }

        //метод для получения точек построения стен
        public List<XYZ> GetPoints (double x, double y)
        {
            double width = UnitUtils.ConvertFromInternalUnits(x, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertFromInternalUnits(y, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            return points;
        }

        //метод для построения стен
        public List<Wall> CreateWalls(Document doc)
        {
            Transaction transaction = new Transaction(doc, "Построение стен");
            transaction.Start();
            List<Wall> walls = new List<Wall>();//пустой список стен, куда добавляем создаваемые стены 
            for (int i = 0; i < 4; i++)
            {
                //получаем уровнь 1
                Level level1 = GetLevels(doc)
                    .Where(x => x.Name.Equals("Уровень 1"))
                    .FirstOrDefault();
                //получаем уровнь 2
                Level level2 = GetLevels(doc)
                    .Where(x => x.Name.Equals("Уровень 2"))
                    .FirstOrDefault();
                //получаем точки для построения
                List<XYZ> points = GetPoints(10000, 5000);
                Line line = Line.CreateBound(points[i], points[i + 1]);//создаем линию, по которой будет строится стена
                Wall wall = Wall.Create(doc, line, level1.Id, false);//создаем стену
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);//записываем в параметр стены значение зависимости сверху
                walls.Add(wall);//добавляем стену в список
            }
            transaction.Commit();
            return walls;
        }
    }
}
