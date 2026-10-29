using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetApp;

namespace UnitTests
{
    [TestClass]
    public class UnitTests
    {
        //перевіряє базові арифметичні операції, div, mod, унарні знаки 
        [TestMethod]
        public void Test1_ArithmeticDivModAndUnaryOperations()
        {
            var manager = new SpreadsheetManager(5, 5);

            manager.UpdateCell("A1", "=2 + 3 * 4");
            manager.UpdateCell("B1", "=(2 + 3) * 4");
            manager.UpdateCell("C1", "=17 div 5");
            manager.UpdateCell("D1", "=17 mod 5");
            manager.UpdateCell("E1", "=-5 + +3");
            manager.RecalculateAll();

            Assert.AreEqual(14.0, manager.CurrentTable.GetCell("A1")!.Value, 1e-9);
            Assert.AreEqual(20.0, manager.CurrentTable.GetCell("B1")!.Value, 1e-9);
            Assert.AreEqual(3.0, manager.CurrentTable.GetCell("C1")!.Value, 1e-9);
            Assert.AreEqual(2.0, manager.CurrentTable.GetCell("D1")!.Value, 1e-9);
            Assert.AreEqual(-2.0, manager.CurrentTable.GetCell("E1")!.Value, 1e-9);
        }

        //перевіряє функції max(x, y), min(x, y) та ланцюжки посилань на клітинки
        [TestMethod]
        public void Test2_MaxMinFunctionsAndCellReferences()
        {
            var manager = new SpreadsheetManager(5, 5);

            manager.UpdateCell("A1", "10");
            manager.UpdateCell("B1", "=A1 + 5");
            manager.UpdateCell("C1", "=max(A1, B1 * 2)");
            manager.UpdateCell("D1", "=min(C1, A1 + B1)");
            manager.RecalculateAll();

            Assert.AreEqual(10.0, manager.CurrentTable.GetCell("A1")!.Value, 1e-9);
            Assert.AreEqual(15.0, manager.CurrentTable.GetCell("B1")!.Value, 1e-9);
            Assert.AreEqual(30.0, manager.CurrentTable.GetCell("C1")!.Value, 1e-9);
            Assert.AreEqual(25.0, manager.CurrentTable.GetCell("D1")!.Value, 1e-9);
            Assert.IsFalse(manager.CurrentTable.GetCell("D1")!.HasError);
        }

        //перевіряє обробку помилок: по типу: ділення на нуль, циклічні посилання, синтаксичні помилки
        [TestMethod]
        public void Test3_ErrorHandling_DivisionByZeroCycleAndSyntaxError()
        {
            var manager = new SpreadsheetManager(5, 5);

            manager.UpdateCell("A1", "=10 / 0");
            manager.UpdateCell("B1", "=10 div 0");
            manager.UpdateCell("A2", "=B2 + 1");
            manager.UpdateCell("B2", "=A2 + 1");
            manager.UpdateCell("C1", "=5 + * 2");
            manager.RecalculateAll();

            var cellA1 = manager.CurrentTable.GetCell("A1")!;
            var cellB1 = manager.CurrentTable.GetCell("B1")!;
            var cellA2 = manager.CurrentTable.GetCell("A2")!;
            var cellB2 = manager.CurrentTable.GetCell("B2")!;
            var cellC1 = manager.CurrentTable.GetCell("C1")!;

            Assert.IsTrue(cellA1.HasError);
            StringAssert.Contains(cellA1.ErrorMessage, "на нуль");

            Assert.IsTrue(cellB1.HasError);
            StringAssert.Contains(cellB1.ErrorMessage, "на нуль");

            Assert.IsTrue(cellA2.HasError);
            StringAssert.Contains(cellA2.ErrorMessage, "циклічне посилання");

            Assert.IsTrue(cellB2.HasError);
            StringAssert.Contains(cellB2.ErrorMessage, "циклічне посилання");

            Assert.IsTrue(cellC1.HasError);
            StringAssert.Contains(cellC1.ErrorMessage, "Синтаксична помилка");
        }
    }
}
