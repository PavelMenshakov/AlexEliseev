using AForge.Neuro;
using AForge.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Entity;
using Forex1.Model;
using AForge;

namespace Forex1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Buttons


        private void button1_Click(object sender, EventArgs e)
        {

            string traningdatafile = @"d:\Program Files\Alpari Limited MT5\MQL5\Files\NeuroSolutions\probe3.csv";    //обучающая выборка
            string testdatafile = @"d:\Program Files\Alpari Limited MT5\MQL5\Files\NeuroSolutions\probe33.csv";     //тестовая выборка

            // читаем файл обучающей выборки
            double[,] arrDouble = csv2matrix.matrix(traningdatafile);
            string[] arrText = csv2matrix.namesOfCol(traningdatafile);
            int col = arrDouble.GetLength(1), row = arrDouble.GetLength(0);
            
            arrDouble = privedenie(arrDouble, arrText);
            row--; //уменьшаем на 1 строку изза отрезанных заголовков столбцов

            double[,] output = new double[row, 1];
            double[,] input = new double[row, col - 1]; // входной массив меньше на первый столбец т.к. этот столбец выходной массив

            double[][] inp = new double[row][];
            double[][] outp = new double[row][];

            for (int y = 0; y < row; y++)
            {
                inp[y] = new double[col - 1];                   // для нейросети
                outp[y] = new double[] { arrDouble[y, 0] };     // для нейросети
                
                output[y, 0] = arrDouble[y, 0];                 // для дальнейшей обработки


                for (int x = 1; x < col; x++)
                {
                    input[y, x - 1] = arrDouble[y, x];            //Заполняем массив исходныx данными без результирующего столбца
                    inp[y][x - 1] = arrDouble[y, x];              // для нейросети
                }
            }

            // Создаем сеть
            ActivationNetwork network = new ActivationNetwork(
                new BipolarSigmoidFunction(2),
                col - 1, //  inputs in the network
                (col - 1)*2, //  neurons in the first layer
                1);        // 1 выход
            AForge.Neuro.Learning.BackPropagationLearning learning = new AForge.Neuro.Learning.BackPropagationLearning(network);
            learning.LearningRate = 0.1;

            bool needToStop = false;
            int iteration = 0;

            //Создаем окно графика
            AForge.Controls.Chart chart = new AForge.Controls.Chart();
            chart.AddDataSeries("Read", Color.Red, Chart.SeriesType.ConnectedDots, 1);
            chart.AddDataSeries("Test", Color.Green, Chart.SeriesType.ConnectedDots, 1);
            groupBox1.Controls.Add(chart);
            chart.Size = new System.Drawing.Size(570, 630);
            chart.RangeX = new Range(0, 1000);

            double error = 1;
            double[,] arrDoubleTest = csv2matrix.matrix(testdatafile);
            double[,] arrTest = new double[arrDoubleTest.GetLength(0), arrDoubleTest.GetLength(1) - 1];

            double[,] arrchartRead = new double[arrDoubleTest.GetLength(0), 2]; //Массивы для графика     Исходный, вычитанный из файла
            double[,] arrchartTest = new double[arrDoubleTest.GetLength(0), 2]; //                        и вычисленный с помощтю нейросети

            for (int x = 0; x < arrDoubleTest.GetLength(1) - 1; x++)            //уменьшаем тестовую матрицу из файла на первый столбец
                for (int y = 0; y < arrDoubleTest.GetLength(0); y++)
                    arrTest[y, x] = arrDoubleTest[y, x + 1];
            arrTest = privedenieTest(arrTest, arrText);                         //приведение величин проверочной матрицы к виду   0..1

            col = arrTest.GetLength(1);     // размеры тестовой матрицы
            row = arrTest.GetLength(0);

            for (int y = 0; y < arrDoubleTest.GetLength(0); y++)               //заполняем массив для вывода графика, проверочные значения
            {
                arrchartTest[y, 0] = y;
                arrchartRead[y, 0] = y;
                arrchartRead[y, 1] = arrDoubleTest[y, 0];
            }
            
            
            while (!needToStop)
            {

                error = learning.RunEpoch(inp, outp);

                if (error == 0)
                    break;
                else if (iteration < 500)     //iteration < row - 1
                {
                    iteration++;
                    progressBar1.Value =  Convert.ToInt32(iteration * 0.2);
                }
                else
                    needToStop = true;
            
                //network.Compute   Проверяем обученную сеть
            
                double[] result = new double[1];
                double[] res = new double[row];
                double[] inpu = new double[col];

                for (int y = 0; y < row; y++)                                       //Проверяем как работает наша нейросеть 
                {
                    for (int x=0; x < col; x++)
                        inpu[x] = arrTest[y,x];
                    result = network.Compute(inpu);
                    arrchartTest[y,1] = result[0];                                      //заполняем вторую часть массива для гарфика, вычисленные значения
                }

                // График  

                chart.UpdateDataSeries("Read", arrchartRead);
                chart.UpdateDataSeries("Test", arrchartTest);

            }
        }

        private void UpdateDataListView()
        {
            // remove all current records
            //dataList.Items.Clear();
            // add new records
            //for (int i = 0, n = data.GetLength(0); i < n; i++)
            //{
            //    dataList.Items.Add(data[i].ToString());
            //}
        }

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    string name = textBox1.Text;
        //    double min = double.Parse(textBox2.Text.Replace(',', '.')), max = double.Parse(textBox3.Text.Replace(',', '.'));
        //    addRecord(name, min, max);
        //}



        #endregion

        public double[,] privedenie(double [,] ma, string[] names)        // Функция приведения значений матрицы от 0 до 1 по столбцам 
        {
            double[] min = new double[ma.GetLength(1)], max = new double[ma.GetLength(1)];
            double mint;

            for (int x = 0; x < ma.GetLength(1); x++)
            {
                min[x] = ma[0, x];
                max[x] = ma[0, x];
                for (int y = 1; y < ma.GetLength(0); y++)
                {
                    if (min[x] > ma[y, x]) min[x] = ma[y, x];      // ищем минимальное и максимальное значение индикатора в выборке
                    if (max[x] < ma[y, x]) max[x] = ma[y, x];
                }

                addRecord(names[x], min[x], max[x]);                //сохраняем данные для последующего приведения в тестовой выборке и торговле

                if (min[x] < 0)
                {
                    mint = min[x]*(-1);
                    for (int y = 0; y < ma.GetLength(0); y++)
                        ma[y, x] += mint;                           // сдвигаем показания индикатора чтоб минимально было 0 если есть отрицательные значения
                }
                for (int y = 0; y < ma.GetLength(0); y++)
                {
                    ma[y, x] = ma[y, x] / (min[x] < 0 ? ( max[x] + (min[x]*(-1)) ) : max[x]);  // делим все значения индикатора на максимальное и получаем значения в диапазоне от 0 до 1
                }
 
                
            }
            return ma;
        }
        
        public double[,] privedenieTest(double[,] ma, string[] names)        // Функция приведения значений матрицы от 0 до 1 по столбцам 
        {
            double[,] minmax = new double[ma.GetLength(1),2];
            double[] minmaxtemp = new double[2];
            double mint;


            for (int x = 0; x < ma.GetLength(1); x++)
            {

                minmaxtemp = getValuesByName(names[x+1]);                            //сохраняем данные для последующего приведения в тестовой выборке и торговле
                minmax[x, 0] = minmaxtemp[0];
                minmax[x, 1] = minmaxtemp[1];

                if (minmax[x,0] < 0)
                {
                    mint = minmax[x,0] * (-1);
                    for (int y = 0; y < ma.GetLength(0); y++)
                        ma[y, x] += mint;                           // сдвигаем показания индикатора чтоб минимально было 0 если есть отрицательные значения
                }
                for (int y = 0; y < ma.GetLength(0); y++)
                {
                    ma[y, x] = ma[y, x] / (minmax[x,0] < 0 ? (minmax[x,1] + (minmax[x,0] * (-1))) : minmax[x,1]);  // делим все значения индикатора на максимальное и получаем значения в диапазоне от 0 до 1
                }


            }
            return ma;
        }





        #region Entity Base
        //контекст для базки на ентити
        public class ValuesContext : DbContext
        {
            public DbSet<minMaxValues> mmValues { get; set; }
        }


        //добавляшка записей 
        public bool addRecord(string name, double min, double max)
        {
            using (var mmContext = new ValuesContext())
            {
                var record = new minMaxValues { ValueName= name, MinValue = min, MaxValue = max};
                mmContext.mmValues.Add(record);
                mmContext.SaveChanges();
            }
            return true;
        }


        public bool updateRecordById(int id, string name, double min, double max)
        {
            using (var mmContext = new ValuesContext())
            {
                var record = new minMaxValues { ValueName = name, MinValue = min, MaxValue = max };

                var item = mmContext.mmValues.First(x => x.ValueId == id );
                
                    if (record.ValueName != null)   item.ValueName = record.ValueName;
                    if (record.MinValue != null)  item.MinValue = record.MinValue;
                    if (record.MaxValue != null)  item.MaxValue = record.MaxValue;

                mmContext.SaveChanges();
            }
            return true;
        }

        public double[] getValuesByName(string name)
        {
            double[] values = new double[2];
            using (var mmContext = new ValuesContext())
            {
                var query = from v in mmContext.mmValues
                            where v.ValueName == name
                            select v;
                foreach (var item in query) 
                {
                    values[0] = item.MinValue;
                    values[1] = item.MaxValue;
                }

            }

            return values;
        }

        public double[] getValuesById(int id)
        {
            double[] values = new double[2];
            using (var mmContext = new ValuesContext())
            {
                var query = from v in mmContext.mmValues
                            where v.ValueId == id
                            select v;
                foreach (var item in query)
                {
                    values[0] = item.MinValue;
                    values[1] = item.MaxValue;
                }

            }
            return values;
        }




        #endregion



    }
}
