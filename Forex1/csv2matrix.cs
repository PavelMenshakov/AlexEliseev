using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forex1
{
    public class csv2matrix
    {
        public static double[,] matrix(string nameOfFile)           //Делаем из файла матрицу double
        {
            int col, row;                                   // размеры исходной матрицы

            string text = System.IO.File.ReadAllText(@nameOfFile);   //d:\Program Files\Alpari Limited MT5\MQL5\Files\NeuroSolutions\probe3.csv
            string textSplited = text.Replace('\r', ' ');
            string[] textSplitedt = textSplited.Split(new Char[] { '\n' }); // разбиваем выборку на строки
            string[] textSplitedtmp = textSplitedt;
            string[] arrText;

            arrText = textSplitedtmp[0].Split(new Char[] { ',' });

            col = arrText.Length;
            row = textSplitedt.Length - 1;
            double[,] arrDouble = new double[row, col];

            for (int y = 1; y < row - 1; y++)
            {
                arrText = textSplitedtmp[y].Split(new Char[] { ',' });  // разбиваем строку на элементы
                for (int x = 0; x <= col - 1; x++)
                    arrDouble[y - 1, x] = Convert.ToDouble(arrText[x].Replace('.',','));   //Заполняем массив исходными данными без названий столбцов
            }

            // arrText = textSplitedtmp[0].Split(new Char[] { ',' }); // Массив названий индикаторов

            return arrDouble;
        }

        public static string[] namesOfCol(string nameOfFile)
        {
            string text = System.IO.File.ReadAllText(@nameOfFile);   //d:\Program Files\Alpari Limited MT5\MQL5\Files\NeuroSolutions\probe3.csv
            string textSplited = text.Replace('\r', ' ');
            string[] textSplitedt = textSplited.Split(new Char[] { '\n' }); // разбиваем выборку на строки

            string[] s = textSplitedt[0].Split(new Char[] { ',' }); // Массив названий индикаторов

            return s;
        }

    }
}
