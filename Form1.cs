using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;


namespace LR4_AKG
{
    public partial class Form1 : Form
    {
        private Device device = null; // Девайс
        private VertexBuffer VB = null; // Вертексный буфер
        private CustomVertex.PositionColored[] verts = null; // Массив точек

        float angleStepY = 0.005f; // Переменная приращения поворота по Y

        private Microsoft.DirectX.Direct3D.Font font = null; // Шрифт
        private string TextLabel = String.Empty; // Надпись в окне

        // Инициализируем панель
        public Form1()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            InitializeComponent();
            InitialGraphics();
            font = new Microsoft.DirectX.Direct3D.Font(device, new System.Drawing.Font("Areal", 10.0f, FontStyle.Regular));  // Определяем шрифт
        }

        // Инициализируем графику
        private void InitialGraphics()
        {
            PresentParameters pp = new PresentParameters();
            pp.Windowed = true;
            pp.SwapEffect = SwapEffect.Discard;

            pp.EnableAutoDepthStencil = true;
            pp.AutoDepthStencilFormat = DepthFormat.D24X8;

            device = new Device(0, DeviceType.Hardware, this.panel1, CreateFlags.HardwareVertexProcessing, pp); // Создаем девайс

            VB = new VertexBuffer(typeof(CustomVertex.PositionColored), 5, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            VB.Created += new EventHandler(this.OnVertexBufferCreate);
            OnVertexBufferCreate(VB, null);

            VB.SetData(verts, 0, LockFlags.None);
        }

        // Заполняем вертексный буфер
        private void OnVertexBufferCreate(object sender, EventArgs e)
        {
            VertexBuffer buffer = (VertexBuffer)sender;
            verts = new CustomVertex.PositionColored[5];

            // Координаты треугольника
            verts[0] = new CustomVertex.PositionColored(-3, -3, 0, Color.Blue.ToArgb());
            verts[1] = new CustomVertex.PositionColored(0, 3, 0, Color.Blue.ToArgb());
            verts[2] = new CustomVertex.PositionColored(3, -3, 0, Color.Blue.ToArgb());
            // Координаты линии
            verts[3] = new CustomVertex.PositionColored(3, 3, 3,  Color.Red.ToArgb());
            verts[4] = new CustomVertex.PositionColored(-3, -4, -2, Color.Red.ToArgb());
            buffer.SetData(verts, 0, LockFlags.None);
        }

        // Инициализация камеры
        private void SetupCamera()
        {
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 1.0f, 100.0f); // Установить настройки проекции
            device.Transform.View = Matrix.LookAtLH(new Vector3(0, 0, 16), new Vector3(), new Vector3(0, 1, 0)); // Установить настройки позиции камеры   

            device.RenderState.Lighting = false;
            device.RenderState.CullMode = Cull.None;
        }

        // Глобальный метод перерисовки
        private void Form1Form_Paint(object sender, PaintEventArgs e)
        {
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1, 0); // Очищаем область рисования, Z-Буффер

            SetupCamera(); // Вызываем метод инициализации камеры
            device.BeginScene(); // Запускаем отрисовку сцены
            device.VertexFormat = CustomVertex.PositionColored.Format; // Вызываем метод вывода вершин
            device.SetStreamSource(0, VB, 0); // Устанавливаем буфер вершин

            
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            device.DrawPrimitives(PrimitiveType.LineStrip, 3, 1);

            TextLabel =
                        verts[0].Position.ToString() + "\n" +
                        verts[1].Position.ToString() + "\n" +
                        verts[2].Position.ToString() + "\n" +
                        verts[3].Position.ToString() + "\n" +
                        verts[4].Position.ToString() + "\n\n\n\n\n\n\n\n\n\n\n\n\n\n" +
                        RayIntersectsTriangle(verts[3].Position, verts[4].Position, verts[0].Position, verts[1].Position, verts[2].Position);

            font.DrawText(null, string.Format(TextLabel), new Rectangle(10, 10, 0, 0), DrawTextFormat.NoClip, Color.BlanchedAlmond);
            RotateFunc();
            device.EndScene(); // Конец отрисовки сцены
            device.Present(); // Показать сцену
            Update(); // Обновляем элементы интерфейса
            this.Invalidate(); // Перерисовать интерфейс
        }
        
        // Показать / скрыть сетку
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            { device.RenderState.FillMode = FillMode.WireFrame; }
            else
            { device.RenderState.FillMode = FillMode.Solid; }
        }
        // Вращать фигуру
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox2.Checked)
            { this.angleStepY = 0; }
            else
            { this.angleStepY = 0.01f; }
        }


        // orig и dir задают начало и направление луча. v0, v1, v2 - вершины треугольника.
        // Функция возвращает расстояние от начала луча до точки пересечения или 0.
        private string RayIntersectsTriangle(Vector3 origin, Vector3 dir, Vector3 V0, Vector3 V1, Vector3 V2)
        {
            Vector3 e1 = V1 - V0;
            Vector3 e2 = V2 - V0;
            dir.Normalize();

            // Вычисление вектора нормали к плоскости
            Vector3 pvec = Vector3.Cross(dir, e2);
            float det = Vector3.Dot(e1, pvec);

            // Луч параллелен плоскости
            if (det < 1*Math.E-8 && det > -1*Math.E-8)
            {
                return "Паралелен";
            }

            float inv_det = 1 / det;
            Vector3 tvec = origin - V0;
            float u = Vector3.Dot(tvec, pvec) * inv_det;
            if (u < 0 || u > 1)
            {
                return "Не пересекает";
            }

            Vector3 qvec = Vector3.Cross(tvec, e1);
            float v = Vector3.Dot(dir, qvec) * inv_det;
            if (v < 0 || u + v > 1)
            {
                return "Не пересекает";
            }
            return "Расстояние от начала вектора до треугольника около: " + (Vector3.Dot(e2, qvec) * inv_det).ToString();
        }

        // Метод который поворачивает треугольник по Y
        public void RotateFunc()
        {
            verts[0].Position = Vector3.TransformCoordinate(verts[0].Position, Matrix.RotationY(angleStepY));
            verts[1].Position = Vector3.TransformCoordinate(verts[1].Position, Matrix.RotationY(angleStepY));
            verts[2].Position = Vector3.TransformCoordinate(verts[2].Position, Matrix.RotationY(angleStepY));
            verts[3].Position = Vector3.TransformCoordinate(verts[3].Position, Matrix.RotationY(-angleStepY));
            verts[4].Position = Vector3.TransformCoordinate(verts[4].Position, Matrix.RotationY(-angleStepY));
            VB.SetData(verts, 0, LockFlags.None);
        }
    }
}
