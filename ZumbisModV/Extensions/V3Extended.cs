using System;
using GTA;
using GTA.Math;

namespace ZumbisModV.Extensions
{
    public static class V3Extended
    {
        public static bool IsOnScreen(this Vector3 worldPosition)
        {
            Vector3 cameraPosition = GameplayCamera.Position;
            Vector3 cameraDirection = GameplayCamera.Direction;

            // Vetor da câmera para a posição do mundo
            Vector3 toWorld = worldPosition - cameraPosition;

            // Produto escalar entre a direção da câmera e o vetor até o ponto
            float dotProduct = Vector3.Dot(cameraDirection, toWorld);

            // Se o produto escalar for negativo, o ponto está atrás da câmera
            if (dotProduct <= 0)
            {
                return false;
            }

            // Calcula o ângulo usando o produto escalar (mais eficiente que Vector3.Angle)
            float angle = (float)Math.Acos(dotProduct / toWorld.Length());

            // Converte o ângulo para graus
            angle = MathHelper.ToDegrees(angle);

            return angle < GameplayCamera.FieldOfView / 2; // Dividir o campo de visão por 2
        }
    }

    public static class MathHelper
    {
        public const float Pi = 3.14159265358979323846f;
        public const float TwoPi = Pi * 2f;
        public const float PiOver2 = Pi / 2f;

        public static float ToDegrees(float radians)
        {
            return radians * (180f / Pi);
        }

        public static float ToRadians(float degrees)
        {
            return degrees * (Pi / 180f);
        }

        // Outras funções...
    }
}
