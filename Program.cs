using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;
        private static GL Gl;
        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
        layout (location = 1) in vec4 vCol;
        out vec4 outCol;
        void main()
        {
            outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";

        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        in vec4 outCol;
        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "1. szeminárium - háromszög";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);
            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();
            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
        }

        private static void GraphicWindow_Update(double deltaTime) { }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            float[] vertexArray = new float[]
            {
                 // teto
                 0.0f,  0.50f, 0.0f,   // v0
                -0.4f,  0.25f, 0.0f,   // v1
                 0.0f,  0.00f, 0.0f,   // v2 
                 0.4f,  0.25f, 0.0f,   // v3

                // bal top-left, bot-left, bottom, center
                -0.4f,  0.25f, 0.0f,   // v4
                -0.4f, -0.25f, 0.0f,   // v5
                 0.0f, -0.50f, 0.0f,   // v6
                 0.0f,  0.00f, 0.0f,   // v7 (center)

                // jobb top-right, center, bot-right, bottom
                 0.4f,  0.25f, 0.0f,   // v8
                 0.0f,  0.00f, 0.0f,   // v9
                 0.4f, -0.25f, 0.0f,   // v10
                 0.0f, -0.50f, 0.0f,   // v11
            };

            float[] colorArray = new float[]
            {
                // teto - lila (v0..v3)
                0.6f, 0.2f, 0.8f, 1.0f,
                0.6f, 0.2f, 0.8f, 1.0f,
                0.6f, 0.2f, 0.8f, 1.0f,
                0.6f, 0.2f, 0.8f, 1.0f,

                // bal - rozsaszin (v4..v7)
                1.0f, 0.4f, 0.7f, 1.0f,
                1.0f, 0.4f, 0.7f, 1.0f,
                1.0f, 0.4f, 0.7f, 1.0f,
                1.0f, 0.4f, 0.7f, 1.0f,

                // jobb - barna (v8..v11)
                0.55f, 0.27f, 0.07f, 1.0f,
                0.55f, 0.27f, 0.07f, 1.0f,
                0.55f, 0.27f, 0.07f, 1.0f,
                0.55f, 0.27f, 0.07f, 1.0f,
            };

            uint[] indexArray = new uint[]
            {
                // teto
                0, 1, 2,
                0, 2, 3,
                // bal
                4, 5, 7,
                5, 6, 7,
                // jobb
                8, 9, 10,
                9, 11, 10,
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);
            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null);

            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
        }
    }
}