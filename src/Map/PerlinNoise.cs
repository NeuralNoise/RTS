/*
 *  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 *  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 *  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 *  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
 *  REMAINS UNCHANGED.
 *
 *  REPO: http://www.github.com/tomwilsoncoder/RTS
*/
/*
 * Translated from C++ to C#
 * 
 * Thanks to Nick Banks on Stack Overflow
 * https://stackoverflow.com/posts/4753123/revisions
*/


public sealed class PerlinNoise {
    private double p_Persistance,
                   p_Frequency,
                   p_Amplitude;
    private int p_Octaves,
                p_Seed;
    

    public PerlinNoise() {
        p_Persistance =
        p_Frequency =
        p_Amplitude = 0;

        p_Octaves =
        p_Seed = 0;
    }

    public PerlinNoise(double persistance, double frequency, double amplitude, int octaves, int seed) {
        Set(persistance, frequency, amplitude, octaves, seed);
    }
    public void Set(double persistance, double frequency, double amplitude, int octaves, int seed) {
        p_Persistance = persistance;
        p_Frequency = frequency;
        p_Amplitude = amplitude;
        p_Octaves = octaves;
        p_Seed = seed;
    }

    public double GetHeight(int x, int y) {
        return p_Amplitude * total(x, y);
    }

    public double Persistance {
        get { return p_Persistance; }
        set { p_Persistance = value; }
    }
    public double Frequency {
        get { return p_Frequency; }
        set { p_Frequency = value; }
    }
    public double Amplitude {
        get { return p_Amplitude; }
        set { p_Amplitude = value; }
    }
    public int Octaves {
        get { return p_Octaves; }
        set { p_Octaves = value; }
    }
    public int Seed {
        get { return p_Seed; }
        set { p_Seed = value; }
    }

    private double total(double i, double j) {
        double buffer = 0.0f;
        double amp = 1;
        double freq = p_Frequency;

        for (int c = 0; c < p_Octaves; c++) {
            buffer += getValue(
                j * freq + p_Seed,
                i * freq + p_Seed) * amp;
            amp *= p_Persistance;
            freq *= 2;
        }
        return buffer;
    }
    private double getValue(double x, double y) {
        int Xint = (int)x;
        int Yint = (int)y;
        double Xfrac = x - Xint;
        double Yfrac = y - Yint;

        //noise values
        double[] n = {
            noise(Xint - 1, Yint - 1),
            noise(Xint + 1, Yint - 1),
            noise(Xint - 1, Yint + 1),
            noise(Xint + 1, Yint + 1),
            noise(Xint - 1, Yint),
            noise(Xint + 1, Yint),
            noise(Xint, Yint - 1),
            noise(Xint, Yint + 1),
            noise(Xint, Yint),

            //n12 [index: 9]
            noise(Xint + 2, Yint - 1),
            noise(Xint + 2, Yint + 1),
            noise(Xint + 2, Yint),

            //n23-28 [index: 12] 
            noise(Xint - 1, Yint + 2),
            noise(Xint + 1, Yint + 2),
            noise(Xint, Yint + 2),

            //n34 [index: 15]
            noise(Xint + 2, Yint + 2)
        };

        //find the noise values of the four corners
        double x0y0 = 0.0625 * (n[00] + n[01] + n[02] + n[03]) + 0.125 * (n[04] + n[05] + n[06] + n[07]) + 0.25 * (n[08]);
        double x1y0 = 0.0625 * (n[06] + n[09] + n[07] + n[10]) + 0.125 * (n[08] + n[11] + n[01] + n[03]) + 0.25 * (n[05]);
        double x0y1 = 0.0625 * (n[04] + n[05] + n[12] + n[13]) + 0.125 * (n[02] + n[03] + n[08] + n[14]) + 0.25 * (n[07]);
        double x1y1 = 0.0625 * (n[08] + n[11] + n[14] + n[15]) + 0.125 * (n[07] + n[10] + n[05] + n[13]) + 0.25 * (n[03]);

        //interpolate between those values according to the x and y fractions
        double v1 = interpolate(x0y0, x1y0, Xfrac); //interpolate in x direction (y)
        double v2 = interpolate(x0y1, x1y1, Xfrac); //interpolate in x direction (y+1)
        double fin = interpolate(v1, v2, Yfrac);  //interpolate in y direction

        return fin;
    }
    private double interpolate(double x, double y, double a) {
        double negA = 1.0f - a;
        double negAS = negA * negA;

        double fac1 = 3.0f * negAS - 2.0 * (negAS * negA);
        double aS = a * a;
        double fac2 = 3.0f * aS - 2.0 * (aS * a);

        return (x * fac1) + (y * fac2);
    }
    private double noise(int x, int y) {
        int n = x + y * 57;
        n = (n << 13) ^ n;

        int t = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
        return 1.0f - (double)t * 0.931322574615478515625e-9;
    }
}