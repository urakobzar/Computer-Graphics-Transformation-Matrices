using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Lab4
{
    public partial class Form1 : Form
    {
        int[,] squareMatrix = new int[4, 3];            // Матрица квадрата
        int[,] coordinateAxesMatrix = new int[4, 3];    // Матрица координат осей
        double[,] shiftMatrix = new double[3, 3];       // Матрица сдвига
        double[,] scalingMatrix = new double[3, 3];     // Матрица масшатбирования
        double[,] rotationMatrix = new double[3, 3];    // Матрица поворота вокруг центра координат
        double[,] reflectionMatrix = new double[3, 3];  // Матрица отражения относительно OY
        int k, l;                                       // Элементы матрицы сдвига по X и Y соответственно
        bool continuousMovement;                        // Условие остановки непрерывного сдвига тела
        Direction direction = Direction.Null;           // Переменная перечисления, хранящая в себе последнее направление
        double s;                                       // Элемент матрицы масштабирования
        bool matrixExist = false;                       // Показывает создано тело или нет
        int[,] variantFigure = new int[5, 3];           // Матрица фигуры по варианту
        bool variantFigureExist = false;                // Флаг, создана ли фигура по варианту
        bool squareExist = false;                       // Флаг, создан ли квадрат
        int thickness;                                  // Толщина линии пера

        /// <summary>
        /// Перечисление возможных направлений сдвига тела
        /// </summary>
        enum Direction
        {
            Null = 0,
            Right=1,
            Left=2,
            Up=3,
            Down=4
        }

        /// <summary>
        /// Инициализация матрцы координатных осей
        /// </summary>
        private void CoordinateAxesInitializing()
        {
            coordinateAxesMatrix[0, 0] = -500;                           // Матрица осей, последний столбец - однородные координаты
            coordinateAxesMatrix[0, 1] = 0;                              //
            coordinateAxesMatrix[0, 2] = 1;                              // [ -500        0       1 ]
            coordinateAxesMatrix[1, 0] = 500;                            // [  500        0       1 ]
            coordinateAxesMatrix[1, 1] = 0;                              // [  0        500       1 ]
            coordinateAxesMatrix[1, 2] = 1;                              // [  0       -500       1 ]
            coordinateAxesMatrix[2, 0] = 0; 
            coordinateAxesMatrix[2, 1] = 500; 
            coordinateAxesMatrix[2, 2] = 1;
            coordinateAxesMatrix[3, 0] = 0; 
            coordinateAxesMatrix[3, 1] = -500; 
            coordinateAxesMatrix[3, 2] = 1;
        }

        /// <summary>
        /// Инициализация матрцы квадрата
        /// </summary>
        private void SquareInitializing()
        {
            squareMatrix[0, 0] = -50;                             // Матрица квадрата, последний столбец - однородные координаты
            squareMatrix[0, 1] = 0;                               // 
            squareMatrix[0, 2] = 1;                               // [ -50        0       1 ]
            squareMatrix[1, 0] = 0;                               // [  0        50       1 ]
            squareMatrix[1, 1] = 50;                              // [  50        0       1 ]
            squareMatrix[1, 2] = 1;                               // [  0       -50       1 ]
            squareMatrix[2, 0] = 50;                               
            squareMatrix[2, 1] = 0;                               
            squareMatrix[2, 2] = 1;                              
            squareMatrix[3, 0] = 0;                              
            squareMatrix[3, 1] = -50;                             
            squareMatrix[3, 2] = 1;                              
        }

        /// <summary>
        /// Инициализация матрицы сдвига
        /// </summary>
        /// <param name="k1">Сдвиг по Х</param>
        /// <param name="l1">Сдвиг по Y</param>
        private void ShiftMatrixInitializing(int k1, int l1)
        {
            shiftMatrix[0, 0] = 1;                         // Матрица сдвига
            shiftMatrix[0, 1] = 0;                         //
            shiftMatrix[0, 2] = 0;                         // [ 1        0       0 ]
            shiftMatrix[1, 0] = 0;                         // [ 0        1       0 ]
            shiftMatrix[1, 1] = 1;                         // [ k1      l1       1 ]
            shiftMatrix[1, 2] = 0;
            shiftMatrix[2, 0] = k1; 
            shiftMatrix[2, 1] = l1; 
            shiftMatrix[2, 2] = 1;
        }

        /// <summary>
        /// Инициализация матрцы масштабирования
        /// </summary>
        private void ScalingMatrixInitializing()
        {
            int m = pictureBox1.Width / 2;
            int n = pictureBox1.Height / 2;
            scalingMatrix[0, 0] = 1;                         // Матрица масштабирования
            scalingMatrix[0, 1] = 0;                         //
            scalingMatrix[0, 2] = 0;                         // [ 1        0       0 ]
            scalingMatrix[1, 0] = 0;                         // [ 0        1       0 ]
            scalingMatrix[1, 1] = 1;                         // [ 0        0       s ]
            scalingMatrix[1, 2] = 0;
            scalingMatrix[2, 0] = m * (s - 1);
            scalingMatrix[2, 1] = n * (s - 1);
            scalingMatrix[2, 2] = s;
        }

        /// <summary>
        /// Инициализация матрцы поворота вокруг центра координат
        /// </summary>
        /// <param name="alpha">Угол поворота</param>
        private void RotationMatrixInitializing(double alpha)
        {
            int m = pictureBox1.Width / 2;
            int n = pictureBox1.Height / 2;
            rotationMatrix[0, 0] = Math.Cos(alpha);           // Матрица поворота вокруг начала координат
            rotationMatrix[0, 1] = Math.Sin(alpha);           //
            rotationMatrix[0, 2] = 0;                         // [        cosA                    sinA                0 ]
            rotationMatrix[1, 0] = -Math.Sin(alpha);          // [       -sinA                    cosA                0 ]
            rotationMatrix[1, 1] = Math.Cos(alpha);           // [ -m*(cos(-1)+n*sinA     -m*sinA+n*(cosA-1)          1 ]
            rotationMatrix[1, 2] = 0;
            rotationMatrix[2, 0] = -m * (Math.Cos(alpha)-1) + n * Math.Sin(alpha);
            rotationMatrix[2, 1] = -m * Math.Sin(alpha) - n * (Math.Cos(alpha) - 1);
            rotationMatrix[2, 2] = 1;
        }

        /// <summary>
        /// Инициализация матрцы отражения относительно ОУ
        /// </summary>
        private void ReflectionMatrixInitializing()
        {
            int m = pictureBox1.Width;
            reflectionMatrix[0, 0] = -1;                        // Матрица отражения
            reflectionMatrix[0, 1] = 0;                         //
            reflectionMatrix[0, 2] = 0;                         // [ -1        0       0 ]
            reflectionMatrix[1, 0] = 0;                         // [  0        1       0 ]
            reflectionMatrix[1, 1] = 1;                         // [  m        0       1 ]
            reflectionMatrix[1, 2] = 0;
            reflectionMatrix[2, 0] = m;
            reflectionMatrix[2, 1] = 0;
            reflectionMatrix[2, 2] = 1;
        }

        /// <summary>
        /// Умножение матриц
        /// </summary>
        /// <param name="a">Исходная матрица</param>
        /// <param name="b">Матрица преобразования</param>
        /// <returns>Преобразованная матрица</returns>
        private int[,] MatrixMuptiply(int[,] a, double[,] b)
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            int[,] result = new int[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = 0;
                    for (int ii = 0; ii < m; ii++)
                    {
                        result[i, j] += a[i, ii] * Convert.ToInt32(b[ii, j]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Умножение матриц
        /// </summary>
        /// <param name="a">Исходная матрица</param>
        /// <param name="b">Матрица масштабирования</param>
        /// <returns>Преобразованная матрица</returns>
        private int[,] ScalingMatrixMuptiply(int[,] a, double[,] b)
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            int[,] result = new int[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = 0;
                    for (int ii = 0; ii < m; ii++)
                    {
                        result[i, j] += a[i, ii] * Convert.ToInt32(b[ii, j]);
                    }
                }
            }
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {                    
                    result[i, j] = Convert.ToInt32(result[i, j]/s);                    
                }
            }
            return result;
        }

        /// <summary>
        /// Рисование квадрата
        /// </summary>
        private void SquareDraw()
        {
            int[,] newSquareMatrix;
            if (!matrixExist)
            {
                SquareInitializing();
                ShiftMatrixInitializing(k, l);
                newSquareMatrix = MatrixMuptiply(squareMatrix, shiftMatrix);
                DrawingSquare(newSquareMatrix);
                k = 0;
                l = 0;
                matrixExist = true;
            }
            else
            {
                ShiftMatrixInitializing(k, l);
                newSquareMatrix = MatrixMuptiply(squareMatrix, shiftMatrix);
                DrawingSquare(newSquareMatrix);
            }
        }

        /// <summary>
        /// Рисование координатных осей
        /// </summary>
        private void CoordinateAxesDraw()
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height); ;
            Graphics cg = Graphics.FromImage(bmp);
            int a = pictureBox1.Width / 2;
            int b = pictureBox1.Height / 2;
            CoordinateAxesInitializing();
            ShiftMatrixInitializing(a, b);
            int[,] osi1 = MatrixMuptiply(coordinateAxesMatrix, shiftMatrix);
            Pen myPen = new Pen(Color.Red, 1);
            cg.DrawLine(myPen, osi1[0, 0], osi1[0, 1], osi1[1, 0], osi1[1, 1]);
            cg.DrawLine(myPen, osi1[2, 0], osi1[2, 1], osi1[3, 0], osi1[3, 1]);
            cg.Dispose();
            myPen.Dispose();
            pictureBox1.BackgroundImage = bmp;
        }

        /// <summary>
        /// Обновление матрицы тела
        /// </summary>
        /// <param name="a">Преобразованная матрица</param>
        private void RefreshCoordinateSquare(int[,] a)
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    squareMatrix[i, j] = a[i, j];
                }
            }
        }

        /// <summary>
        /// Рисование квадрата на холсте
        /// </summary>
        /// <param name="newSquareMatrix"></param>
        private void DrawingSquare(int[,] newSquareMatrix)
        {
            RefreshCoordinateSquare(newSquareMatrix);
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height); ;
            Graphics cg = Graphics.FromImage(bmp);
            Pen myPen = new Pen(Color.Blue, 2);
            cg.DrawLine(myPen, squareMatrix[0, 0], squareMatrix[0, 1], squareMatrix[1, 0], squareMatrix[1, 1]);
            myPen = new Pen(Color.Yellow, 2);
            cg.DrawLine(myPen, squareMatrix[1, 0], squareMatrix[1, 1], squareMatrix[2, 0], squareMatrix[2, 1]);
            myPen = new Pen(Color.Orange, 2);
            cg.DrawLine(myPen, squareMatrix[2, 0], squareMatrix[2, 1], squareMatrix[3, 0], squareMatrix[3, 1]);
            myPen = new Pen(Color.Green, 2);
            cg.DrawLine(myPen, squareMatrix[3, 0], squareMatrix[3, 1], squareMatrix[0, 0], squareMatrix[0, 1]);
            pictureBox1.Image = bmp;
            cg.Dispose();
            myPen.Dispose();
        }

        /// <summary>
        /// Инициализация матрицы фигуры по вариантам
        /// </summary>
        private void VariantFigureInitializing()
        {
            variantFigure[0, 0] = 0;                            // Матрица фигуры по вариантам, последний столбец - однородные координаты
            variantFigure[0, 1] = 50;                           // 
            variantFigure[0, 2] = 1;                            // [  0        50       1 ]
            variantFigure[1, 0] = 0;                            // [  0        20       1 ]
            variantFigure[1, 1] = 20;                           // [ -35       50       1 ]
            variantFigure[1, 2] = 1;                            // [ -25      -55       1 ]
            variantFigure[2, 0] = -35;                          // [  60      -20       1 ]
            variantFigure[2, 1] = 50;
            variantFigure[2, 2] = 1;
            variantFigure[3, 0] = -25;
            variantFigure[3, 1] = -55;
            variantFigure[3, 2] = 1;
            variantFigure[4, 0] = 60;
            variantFigure[4, 1] = -20;
            variantFigure[4, 2] = 1;
        }

        /// <summary>
        /// Рисование фигуры из варианта
        /// </summary>
        private void VariantFigureDraw()
        {
            int[,] newSquareMatrix;
            if (!matrixExist)
            {
                VariantFigureInitializing();
                ShiftMatrixInitializing(k, l);
                newSquareMatrix = MatrixMuptiply(variantFigure, shiftMatrix);
                DrawingVariantFigure(newSquareMatrix);
                k = 0;
                l = 0;
                matrixExist = true;
            }
            else
            {
                ShiftMatrixInitializing(k, l);
                newSquareMatrix = MatrixMuptiply(variantFigure, shiftMatrix);
                DrawingVariantFigure(newSquareMatrix);
            }
        }

        /// <summary>
        /// Рисование фигуры из варианта на холсте
        /// </summary>
        /// <param name="newSquareMatrix">Преобразованная матрица</param>
        private void DrawingVariantFigure(int[,] newSquareMatrix)
        {
            RefreshCoordinateVariantFigure(newSquareMatrix);
            string comboBoxColor = comboBox1.Text;
            Color color = (Color)TypeDescriptor.GetConverter(typeof(Color)).ConvertFromString(comboBoxColor);
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height); ;
            Graphics cg = Graphics.FromImage(bmp);
            Pen myPen = new Pen(color, thickness);
            cg.DrawLine(myPen, variantFigure[0, 0], variantFigure[0, 1], variantFigure[1, 0], variantFigure[1, 1]);
            cg.DrawLine(myPen, variantFigure[1, 0], variantFigure[1, 1], variantFigure[2, 0], variantFigure[2, 1]);
            cg.DrawLine(myPen, variantFigure[2, 0], variantFigure[2, 1], variantFigure[3, 0], variantFigure[3, 1]);    
            cg.DrawLine(myPen, variantFigure[3, 0], variantFigure[3, 1], variantFigure[4, 0], variantFigure[4, 1]);
            cg.DrawLine(myPen, variantFigure[4, 0], variantFigure[4, 1], variantFigure[0, 0], variantFigure[0, 1]);
            pictureBox1.Image = bmp;
            cg.Dispose();
            myPen.Dispose();
        }

        /// <summary>
        /// Обновление матрицы тела
        /// </summary>
        /// <param name="a">Преобразованная матрица</param>
        private void RefreshCoordinateVariantFigure(int[,] a)
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    variantFigure[i, j] = a[i, j];
                }
            }
        }

        /// <summary>
        /// Построение осей координат
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            CoordinateAxesDraw();
        }

        /// <summary>
        /// Построение квадрата
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            button12.Enabled = false;
            variantFigureExist = false;
            squareExist = true;
            if (matrixExist)
            {
                MessageBox.Show("Фигура уже создана");
                return;
            }
            k = pictureBox1.Width / 2;
            l = pictureBox1.Height / 2;            
            SquareDraw();
        }

        /// <summary>
        /// Очистка экрана
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.BackgroundImage = null;
            direction = Direction.Null;
            matrixExist = false;
            variantFigureExist = false;
            squareExist = false;
            button2.Enabled = true;
            button12.Enabled = true;
        }

        /// <summary>
        /// Дискретный сдвиг вправо
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (!matrixExist)
            {
                MessageBox.Show("Создайте фигуру для взаимодействия с ней");
                return;
            }
            direction = Direction.Right;
            k = 5;
            l = 0;
            if(squareExist)
            {
                SquareDraw();
            }
            else if(variantFigureExist)
            {
                VariantFigureDraw();
            } 
        }

        /// <summary>
        /// Дискретный сдвиг влево
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            if (!matrixExist)
            {
                MessageBox.Show("Создайте фигуру для взаимодействия с ней");
                return;
            }
            direction = Direction.Left;
            k = -5;
            l = 0;
            if (squareExist)
            {
                SquareDraw();
            }
            else if (variantFigureExist)
            {
                VariantFigureDraw();
            }
        }

        /// <summary>
        /// Дискретный сдвиг вниз
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            if (!matrixExist)
            {
                MessageBox.Show("Создайте фигуру для взаимодействия с ней");
                return;
            }
            direction = Direction.Down;
            l = 5;
            k = 0;
            if (squareExist)
            {
                SquareDraw();
            }
            else if (variantFigureExist)
            {
                VariantFigureDraw();
            }
        }

        /// <summary>
        /// Дискретный сдвиг вверх
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            if (!matrixExist)
            {
                MessageBox.Show("Создайте фигуру для взаимодействия с ней");
                return;
            }
            direction = Direction.Up;
            l = -5;
            k = 0;
            if (squareExist)
            {
                SquareDraw();
            }
            else if (variantFigureExist)
            {
                VariantFigureDraw();
            }
        }

        /// <summary>
        /// Непрерывное движение тела по последнему направлению
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            if (direction== Direction.Null)
            {
                MessageBox.Show("У фигуры нет направления");
                return;
            }
            timer1.Interval = 100;
            button8.Text = "Стоп";
            if (continuousMovement == true)
            {
                timer1.Start();
            }
            else
            {
                timer1.Stop();
                button8.Text = "Старт";
            }
            continuousMovement = !continuousMovement;
        }

        /// <summary>
        /// Масштабирование тела
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            if (!matrixExist)
            {
                MessageBox.Show("Создайте фигуру для взаимодействия с ней");
                return;
            }
            if (textBox1.Text == "")
            {
                MessageBox.Show("Введите, во сколько раз хотите увеличить масштаб");
                return;
            }
            s = Convert.ToDouble(textBox1.Text);
            s = 1 / s;
            ScalingMatrixInitializing();
            if (squareExist)
            {
                int[,] newSquareMatrix = ScalingMatrixMuptiply(squareMatrix, scalingMatrix);
                DrawingSquare(newSquareMatrix);
            }
            else if (variantFigureExist)
            {
                int[,] newSquareMatrix = ScalingMatrixMuptiply(variantFigure, scalingMatrix);
                DrawingVariantFigure(newSquareMatrix);
            }
        }

        /// <summary>
        /// Поворот тела на угол
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            if(!matrixExist)
            {
                MessageBox.Show("Создайте фигуру для взаимодействия с ней");
                return;
            }
            if (textBox2.Text == "")
            {
                MessageBox.Show("Введите угол для поворота");
                return;
            }
            double alpha = Convert.ToDouble(textBox2.Text);
            alpha = alpha * Math.PI / 180;
            RotationMatrixInitializing(alpha);
            if (squareExist)
            {
                int[,] newSquareMatrix = MatrixMuptiply(squareMatrix, rotationMatrix);
                DrawingSquare(newSquareMatrix);
            }
            else if (variantFigureExist)
            {
                int[,] newSquareMatrix = MatrixMuptiply(variantFigure, rotationMatrix);
                DrawingVariantFigure(newSquareMatrix);
            }
        }

        /// <summary>
        /// Отражения тела относительно ОY
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            if (!matrixExist)
            {
                MessageBox.Show("Создайте фигуру для взаимодействия с ней");
                return;
            }
            ReflectionMatrixInitializing();
            if (squareExist)
            {
                int[,] newSquareMatrix = MatrixMuptiply(squareMatrix, reflectionMatrix);
                DrawingSquare(newSquareMatrix);
            }
            else if (variantFigureExist)
            {
                int[,] newSquareMatrix = MatrixMuptiply(variantFigure, reflectionMatrix);
                DrawingVariantFigure(newSquareMatrix);
            }
        }

        /// <summary>
        /// Построение фигуры из варианта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            if (matrixExist)
            {
                MessageBox.Show("Фигура уже создана");
                return;
            }
            if (radioButton2.Checked == true)
            {
                if (textBox6.Text == "")
                {
                    MessageBox.Show("Введите толщину");
                    return;
                }
                thickness = int.Parse(textBox6.Text);
            }
            else
            {
                thickness = 1;
            }
            button2.Enabled = false;
            variantFigureExist = true;
            squareExist = false;
            k = pictureBox1.Width / 2;
            l = pictureBox1.Height / 2;
            VariantFigureDraw();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            k = 0;
            l = 0;
            switch(direction)
            {                
                case Direction.Right:
                    {
                        k=5;
                        break;
                    }
                case Direction.Left:
                    {
                        k=-5;
                        break;
                    }
                case Direction.Down:
                    {
                        l=5;
                        break;
                    }
                case Direction.Up:
                    {
                        l=-5;
                        break;
                    }
            }
            if (squareExist)
            {
                SquareDraw();
            }
            else if (variantFigureExist)
            {
                VariantFigureDraw();
            }
            Thread.Sleep(100);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_TextUpdate(object sender, EventArgs e)
        {
            MessageBox.Show("Вам нельзя пытаться изменить имя цвета");
            comboBox1.SelectedIndex = 0;
            return;
        }

        public Form1()
        {
            InitializeComponent();
        }
    }
}
