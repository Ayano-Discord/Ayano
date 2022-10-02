namespace Ayano.Core.Utilities;

public static class MathHelpers
{

    #region Trigonometry
    public static double findSine(double angle)
    {
        return Math.Sin(angle);
    }
    public static double findCosine(double angle)
    {
        return Math.Cos(angle);
    }
    public static double findTangent(double angle)
    {
        return Math.Tan(angle);
    }
    public static double findHyperbolicSine(double angle)
    {
        return Math.Sinh(angle);
    }
    public static double findHyperbolicCosine(double angle)
    {
        return Math.Cosh(angle);
    }
    public static double findHyperbolicTangent(double angle)
    {
        return Math.Tanh(angle);
    }
    public static double findAngleOfSine(double angle)
    {
        return Math.Asin(angle);
    }
    public static double findAngleOfCosine(double angle)
    {
        return Math.Acos(angle);
    }
    public static double findAngleOfTan(double angle)
    {
        return Math.Atan(angle);
    }
    public static decimal findAreaTriangle(decimal bbase, decimal height)
    {
        return (height * bbase) / 2;
    }
    public static decimal findPerimeterTriangle(decimal bbase, decimal side1, decimal side2)
    {
        return bbase + side1 + side2;
    }
    #endregion

    #region Geometry
    public static decimal findAreaCircle(decimal radius)
    {
        const decimal PI = 3.14M;
        decimal finalArea = PI * radius * radius;
        return finalArea;
    }
    public static decimal findPerimeterCircle(decimal radius)
    {
        const decimal PI = 3.14M;
        decimal finalPerim = 2.0M * PI * radius;
        return finalPerim;
    }
    public static decimal findDiameterCircle(decimal radius)
    {
        decimal finalRadius = 2.0M * radius;
        return finalRadius;
    }
    public static decimal findAreaRectangle(decimal length, decimal width)
    {
        decimal finalArea = length * width;
        return finalArea;
    }
    public static decimal findPerimeterRectangle(decimal length, decimal width)
    {
        decimal finalPerim = 2 * (length + width);
        return finalPerim;
    }
    public static decimal findDiagonalRectangle(decimal length, decimal width)
    {
        decimal temp;
        decimal finalDiag;
        temp = (width * width) + (length * length);
        finalDiag = squareRoot(temp);
        return finalDiag;
    }
    public static decimal findAreaRhombus(decimal diag1, decimal diag2)
    {
        decimal finalArea = (diag1 * diag2) / 2;
        return finalArea;
    }
    public static decimal findPerimeterRhombus(decimal sideLength)
    {
        decimal finalPerim = 4 * sideLength;
        return finalPerim;
    }
    public static decimal findAreaTrapezoid(decimal base1, decimal base2, decimal height)
    {
        decimal finalArea = ((base1 + base2) / 2) * height;
        return finalArea;
    }
    public static decimal findPerimeterTrapezoid(decimal base1, decimal base2, decimal sideLength1, decimal sideLength2)
    {
        decimal finalPerim = base1 + base2 + sideLength1 + sideLength2;
        return finalPerim;
    }
    public static decimal findAreaPentagon(decimal sideLength)
    {
        decimal finalArea;
        finalArea = (squareRoot(5 * (5 + 2 * (squareRoot(5)))) * sideLength * sideLength) / 4;
        return finalArea;
    }
    public static decimal findPerimeterPentagon(decimal sideLength)
    {
        decimal finalPerim = 5 * sideLength;
        return finalPerim;
    }
    public static decimal findAreaHexagon(decimal sideLength)
    {
        decimal finalArea = ((3 * squareRoot(3)) / 2) * sideLength * sideLength;
        return finalArea;
    }
    public static decimal findPerimeterHexagon(decimal sideLength)
    {
        decimal finalPerimeter = 6M * sideLength;
        return finalPerimeter;
    }
    public static decimal findAreaOctagon(decimal sideLength)
    {
        decimal finalArea = (2 * (1 + squareRoot(2))) * sideLength * sideLength;
        return finalArea;
    }
    public static decimal findPerimeterOctagon(decimal sideLength)
    {
        decimal finalPerimeter = 8 * sideLength;
        return finalPerimeter;
    }
    public static decimal findAreaNonagon(decimal sideLength)
    {
        decimal finalArea = 6.18182M * power(sideLength, 2);
        return finalArea;
    }
    public static decimal findPerimeterNonagon(decimal sideLength)
    {
        decimal finalPerim = 9 * sideLength;
        return finalPerim;
    }
    public static decimal findAreaHeptagon(decimal sideLength)
    {
        decimal finalArea = 3.633912444M * power(sideLength, 2);
        return finalArea;
    }
    public static decimal findPerimeterHeptagon(decimal sideLength)
    {
        decimal finalPerim = 7 * sideLength;
        return finalPerim;
    }
    public static decimal findAreaSphere(decimal radius)
    {
        decimal finalArea = 4 * 3.14159265359M * radius * radius;
        return finalArea;
    }
    public static decimal findVolumeSphere(decimal radius)
    {
        decimal temp = 1.333333333333333M * 3.14159265359M;
        decimal finalVolume = temp * (radius * radius * radius);
        return finalVolume;
    }
    public static decimal findAreaRecPrism(decimal length, decimal width, decimal height)
    {
        decimal finalArea = 2 * ((width * length) + (height * length) + (height * width));
        return finalArea;
    }
    public static decimal findVolumeRecPrism(decimal length, decimal width, decimal height)
    {
        decimal finalVolume = width * length * height;
        return finalVolume;
    }
    public static decimal findAreaCylinder(decimal radius, decimal height)
    {
        decimal finalArea = (2 * 3.14159265359M * radius * height) + (2 * 3.14159265359M * radius * radius);
        return finalArea;
    }
    public static decimal findVolumeCylinder(decimal radius, decimal height)
    {
        decimal finalVolume = 3.14159265359M * radius * radius * height;
        return finalVolume;
    }
    public static decimal findAreaCone(decimal radius, decimal height)
    {
        decimal finalArea = 3.14159265359M * radius * (radius + squareRoot((height * height) + (radius * radius)));
        return finalArea;
    }
    public static decimal findVolumeCone(decimal radius, decimal height)
    {
        decimal finalVolume = 3.14159265359M * radius * radius * (height / 3);
        return finalVolume;
    }
    public static decimal findSlantHeightCone(decimal radius, decimal height)
    {
        decimal finalSlant = squareRoot((radius * radius) + (height * height));
        return finalSlant;
    }
    public static decimal findAreaTetrahedron(decimal edge)
    {
        decimal finalArea = 4 * ((squareRoot(3) / 4) * edge * edge);
        return finalArea;
    }
    public static decimal findVolumeTetrahedron(decimal edge)
    {
        decimal finalVolume = (edge * edge * edge) / (6 * (squareRoot(2)));
        return finalVolume;
    }
    public static decimal findAreaTorus(decimal majorRadius, decimal minorRadius)
    {
        const decimal PI = 3.14159265359M;
        decimal finalArea = (2 * PI * majorRadius) * (2 * PI * minorRadius);
        return finalArea;
    }
    public static decimal findVolumeTorus(decimal majorRadius, decimal minorRadius)
    {
        const decimal PI = 3.14159265359M; ;
        decimal finalVolume = (PI * minorRadius * minorRadius) * (2 * PI * majorRadius);
        return finalVolume;
    }
    public static decimal findAreaRecPyramid(decimal length, decimal width, decimal height)
    {
        decimal finalArea = (length * width) + length * (squareRoot((width / 2) * (width / 2) + (height * height) +
            width) * squareRoot((0.5M * 0.5M) + (height * height)));
        return finalArea;
    }
    public static decimal findVolumeRecPyramid(decimal length, decimal width, decimal height)
    {
        decimal finalVolume = (length * width * height) / 3;
        return finalVolume;
    }

    //Extra helps
    public static decimal squareRoot(decimal _d)
    {
        decimal x = 0;
        decimal y = 2;
        decimal z = 1;
        decimal w = _d;
        decimal h = 1;
        decimal t = 0;
        decimal px = 0;
        int itr = 0;
        while (true)
        {
            w = (w / y);
            h *= y;
            if (h > w)
            {
                t = w;
                w = h;
                h = t;
                z *= 0.5M;
                y = (1 + z);
            }
            x = ((w + h) * 0.5M);
            if (itr >= 100 || w == h || px == x)
            {
                return (x);
            }
            px = x;
            itr++;
        }
    }
    public static decimal power(decimal num1, decimal num2)
    {
        int count = 1;
        decimal pow = num1;
        while (count < num2)
        {
            pow *= num1;
            count++;
        }
        return pow;
    }
    #endregion
}