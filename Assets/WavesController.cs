using UnityEngine;
using System.Collections;

#region waveengine-imports
using System;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.Threading;
//using System.Windows;
//using System.Windows.Forms;
#endregion

public class WavesController : MonoBehaviour {

    public Texture2D texture;
    // Use this for initialization
    int size = 256; // Size of the wave pool. It indicates both the width and height since the pool will always be a square.


    Camera cam;
    Renderer render;
    public Waves w;

    void Start() {
        cam = Camera.main;
        //setPool();

        //TEXTURE
        this.texture = new Texture2D(size, size);

        render = GetComponent<Renderer>();

       // stretch();

        w = new Waves(render, texture);
    }

    private Vector2 getObjectCoordinates(GameObject obj)
    {
        Vector3 pos = cam.WorldToScreenPoint(obj.transform.position);

        Vector2 particleCoord = screenToParticleCoords(pos);
        

        Debug.Log("pos " + pos + " particleCoord" + particleCoord);

        w.Oscillator2Position = particleCoord;
        w.Oscillator2Active = true;

        return particleCoord;

    }

    public class Waves
    {

        public Waves(Renderer render, Texture2D texture)
        {
            setPool();
            this.texture = texture;
            //GameObject.Find("Your_Name_Here").transform.position;
            
            render.material.mainTexture = texture;



        }

    

        ////set obstacles
        public void setObstacles(int rectX, int rectY, int rectWidth, int rectHeight)
        {
            SetParticles(rectX, rectY, rectWidth, rectHeight, Convert.ToSingle(true), ParticleAttribute.Fixity);
        }
 
    

    

    private static void applyBlackToTexture(Texture2D texture) {

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();
    }

    private static void applyFractalToTexture(Texture2D texture)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color color = ((x & y) != 0 ? Color.black : Color.gray);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
    }


    #region wavesimulator-code

    //Bitmap bmp; // This will be our canvas.
    Texture2D texture;

    //REPLACE BitmapData bd; // This will be used to modify the RGB pixel array of the "bmp".

    // "vd" means "vertex data"
    float[] vd; // Height map
    float[] vdv; // Velocity map
    float[] vda; // Acceleration map
    float[] vds; // Sustainability map. Sustainability can be thought as anti-damping.
    bool[] vd_static; // Static particle map. Particles which will act like a obstacle or wall.



    //float mass = 0.05f; // Mass of each particle. It is the same for all particles.
    float mass = 0.1f; // Mass of each particle. It is the same for all particles.
    public float limit = 500f; // Maximum absolute height a particle can reach.
    float action_resolution = 20f; // Resolution of movement of particles.
    //float sustain = 1000f; // Anti-damping. Propagation range increases by increasing this variable. Minimum is 1f.
    float sustain = 80f; // Anti-damping. Propagation range increases by increasing this variable. Minimum is 1f.
    public float phase1 = 0f; // Current phase value of oscillator1.
    public float phase2 = 0f; // Current phase value of oscillator2.
    float freq1 = 0.2f; // Phase changing rate of oscillator1 per calculation. Frequency increases by increasing this variable.
    float freq2 = 0.2f; // Phase changing rate of oscillator2 per calculation. Frequency increases by increasing this variable.
    float power = 1.0f; // Power of the output force exerted on each particle. Natural value is 1.0f

    //REPLACE BufferedGraphics bufgraph; // Double-buffered graphics for rendering. It minimizes flickering.
    //REPLACE BufferedGraphicsContext bufgcont; // Will be used to initialize bufgraph.

    Thread ForceCalcT; // Worker thread that will do force calculations.


    bool work_now = false; // True = Thread must make calculations now, False = Thread must sleep now.

    bool highcont = false; // High contrast drawing.

    bool disposing = false; // It will be true once the termination starts.

    bool osc1active = false; // Is oscillator1 is turned on?
    bool osc2active = false; // Is oscillator2 is turned on?

    int osc1point = 0; // Location of the oscillator1 in the wave pool. It is an index value.
    int osc2point = 0; // Location of the oscillator2 in the wave pool. It is an index value.

    
    Color color1 = Color.black; // Color of the crest or trough.
    Color color2 = Color.cyan; // Color of the crest or trough. 

    Color colorstatic = Color.black; // Color of the static particles.

    int size = 256;

        // These variables are used for edge absorbtion. It is used for eliminating reflection from window boundaries.
    int absorb_offset = 10; // Offset from each window boundary where the sustainability starts to decrease.
    float min_sustain = 2f; // The lowest sustainability value. They are located at the boundaries.
    bool edge_absorbtion = true; // If true, the particles near the boundaries will have low sustainability.
    public float[] vd_previous;

        //REPLACE Control control; // This will be the control where the engine runs and renders on.


        public float Mass
    {
        get { return mass; }
        set
        {
            if (value > 0f)
            {
                mass = value;
            }
        }
    }

    public float Limit
    {
        get { return limit; }
        set
        {
            if (value > 0f)
            {
                limit = value;
            }
        }
    }

    public float ActionResolution
    {
        get { return action_resolution; }
        set
        {
            if (value >= 1f)
            {
                action_resolution = value;
            }
        }
    }

    public float Sustainability
    {
        get { return sustain; }
        set
        {
            if (value >= 1f)
            {
                sustain = value;
                setSustain();
            }
        }
    }
    public float PhaseRate1
    {
        get { return freq1; }
        set
        {
            if (value > 0f && value < Math.PI * 2f)
            {
                freq1 = value;
            }
        }
    }
    public float PhaseRate2
    {
        get { return freq2; }
        set
        {
            if (value > 0f && value < Math.PI * 2f)
            {
                freq2 = value;
            }
        }
    }
    public float Power
    {
        get { return power; }
        set
        {
            if (power > 0f)
            {
                power = value;
            }
        }
    }
    public int Size
    {
        get { return size; }
        set
        {
            if (size >= 1f)
            {
                size = value;
                setPool();
            }
        }
    }
    public float EdgeSustainability
    {
        get { return min_sustain; }
        set
        {
            if (value >= 1f)
            {
                min_sustain = value;
                setSustain();
            }
        }
    }
    public int AbsorbtionOffset
    {
        get { return absorb_offset; }
        set
        {
            if (value > 0 && value < size / 2)
            {
                absorb_offset = value;
                setSustain();
            }
        }
    }
    public Color Color1
    {
        get { return color1; }
        set
        {
            color1 = value;
        }
    }
    public Color Color2
    {
        get { return color2; }
        set
        {
            color2 = value;
        }
    }
    public Color ColorStatic
    {
        get { return colorstatic; }
        set
        {
            colorstatic = value;
        }
    }
    public bool HighContrast
    {
        get { return highcont; }
        set
        {
            highcont = value;
        }
    }
    public bool EdgeAbsorbtion
    {
        get { return edge_absorbtion; }
        set
        {
            edge_absorbtion = value;
            setSustain();
        }
    }

    public bool Oscillator1Active
    {
        get { return osc1active; }
        set
        {
            osc1active = value;
            setSustain();
        }
    }

    public bool Oscillator2Active
    {
        get { return osc2active; }
        set
        {
            osc2active = value;
            setSustain();
        }
    }

    public Vector2 Oscillator1Position
    {
        get { return new Vector2(osc1point % size, (int)Math.Floor((float)osc1point / (float)size)); }
        set
        {

            if (value.x + value.y * size < size * size)
            {
                osc1point = ((int)value.x + (int)value.y * size);
                setSustain();
            }
        }
    }

    public Vector2 Oscillator2Position
    {
        get { return new Vector2(osc2point % size, (int)Math.Floor((float)osc2point / (float)size)); }
        set
        {
            if (value.x + value.y * size < size * size)
            {
                osc2point = ((int)value.x + (int)value.y * size);
                //Debug.Log("SettingOscillator to: (x=" + (int)value.x + ") + (y=" + (int)value.y + ") * (size=" + size + ") = " + osc2point );
                setSustain();
            }
        }
    }

    public float getHeightAt(int x, int y)
    {
        return vd[x + y * size];
    }
    public float getVelocityAt(int x, int y)
    {
        return vdv[x + y * size];
    }
    public float getPreviousHeightAt(int x, int y)
    {
        return vd_previous[x + y * size];
    }


    /// <summary>
    /// Sets particles' specified attribute(s) to a specified value in a specified rectangular area.
    /// </summary>
    /// <param name="rectX">Together with `rectY`: origin for a rectangular area which contains particles.</param>
    /// <param name="rectY"></param>
    /// <param name="rectWidth">Width of that area</param>
    /// <param name="rectHeight">Height of that area</param>
    /// <param name="value">Value to set the particles to.</param>
    /// <param name="partatt">Attribute(s) that will be set.</param>
    public void SetParticles(int rectX, int rectY, int rectWidth, int rectHeight, float value, ParticleAttribute partatt)
    {

        if (rectX < 0)
            rectX = 0;

        if (rectY < 0)
            rectY = 0;

        if (rectWidth + rectX > size)
            rectWidth -= (rectX + rectWidth) - size;

        if (rectHeight + rectY > size)
            rectHeight -= (rectY + rectHeight) - size;

        bool xh = false, xv = false, xa = false, xs = false, xf = false;
        // Let's see which attributes we are gonna deal with.
        if ((ParticleAttribute.All & partatt) == ParticleAttribute.All)
        {
            xh = true; xv = true; xa = true; xs = true; xf = true;
        }
        else
        {
            if ((ParticleAttribute.Height & partatt) == ParticleAttribute.Height)
                xh = true;
            if ((ParticleAttribute.Velocity & partatt) == ParticleAttribute.Velocity)
                xv = true;
            if ((ParticleAttribute.Acceleration & partatt) == ParticleAttribute.Acceleration)
                xa = true;
            if ((ParticleAttribute.Sustainability & partatt) == ParticleAttribute.Sustainability)
                xs = true;
            if ((ParticleAttribute.Fixity & partatt) == ParticleAttribute.Fixity)
                xf = true;
        }

        for (int y = rectY * size; y < rectY * size + rectHeight * size; y += size)
        {
            for (int x = rectX; x < rectX + rectWidth; x++)
            {
                if (xh)
                    vd[x + y] = value;
                if (xv)
                    vdv[x + y] = value;
                if (xa)
                    vda[x + y] = value;
                if (xs)
                    vds[x + y] = value;
                if (xf)
                    vd_static[x + y] = Convert.ToBoolean(value);
            }
        }
    }

    /// <summary>
    /// Gives a float array of specified attribute of particles in a specified rectangular area.
    /// </summary>
    /// <param name="rectX">Together with `rectY`: origin for a rectangular area which contains particles.</param>
    /// <param name="rectY"></param>
    /// <param name="rectWidth">Width of that area</param>
    /// <param name="rectHeight">Height of that area</param>
    /// <param name="partatt">Attribute whose array will be given. Only one attribute can be specified and "All" cannot be specified.</param>
    public float[] GetParticles(int rectX, int rectY, int rectWidth, int rectHeight, ParticleAttribute partatt)
    {

        float[] result = new float[1];

        bool xh = false, xv = false, xa = false, xs = false, xf = false;

        if ((int)partatt == 1 || (int)partatt == 2 || (int)partatt == 4 || (int)partatt == 8 || (int)partatt == 16)
        {

            if (rectX < 0)
                rectX = 0;

            if (rectY < 0)
                rectY = 0;

            if (rectWidth + rectX > size)
                rectWidth -= (int)(rectX + rectWidth) - size;

            if (rectHeight + rectY > size)
                rectHeight -= (int)(rectY + rectHeight) - size;

            result = new float[rectWidth * rectHeight];

            if (partatt == ParticleAttribute.Height)
                xh = true;
            if (partatt == ParticleAttribute.Velocity)
                xv = true;
            if (partatt == ParticleAttribute.Acceleration)
                xa = true;
            if (partatt == ParticleAttribute.Sustainability)
                xs = true;
            if (partatt == ParticleAttribute.Fixity)
                xf = true;

            int r = 0;
            for (int y = rectY * size; y < rectY * size + rectHeight * size; y += size)
            {
                for (int x = rectX; x < rectX + rectWidth; x++)
                {
                    if (xh)
                        result[r] = vd[x + y];
                    if (xv)
                        result[r] = vdv[x + y];
                    if (xa)
                        result[r] = vda[x + y];
                    if (xs)
                        result[r] = vds[x + y];
                    if (xf)
                        result[r] = Convert.ToSingle(vd_static[x + y]);
                    r += 1;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Starts the force calculation.
    /// </summary>
    public void Run()
    {
        work_now = true;
    }

    /// <summary>
    /// Suspends the force calculation indefinitely.
    /// </summary>
    public void Stop()
    {
        work_now = false;
    }


    public void Dispose()
    {
        work_now = false;
        disposing = true;
        ThreadPool.QueueUserWorkItem((System.Object arg1) =>
        {
                //REPLACE bmp.Dispose();
            });
    }

    public void CalculateForces()
    {

        this.vd_previous = (float[]) vd.Clone(); // backup for force calculations

        float total_height = 0;// This will be used to shift the height center of the whole particle system to the origin.

        // This loop calculates the forces exerted on the particles.
        for (int index = 0; index < vd.Length; index += 1)
        {
            // If this is a static particle, it will not move at all. Continue with the next particle.
            if (vd_static[index])
            {
                vd[index] = 0;
                vdv[index] = 0;
                vda[index] = 0;
                continue;
            }


            if (index == osc1point && osc1active)
            {
                // This is where the oscillator1 is located. It is currently active.
                // So this particle only serves as an oscillator for neighbor particles.
                // It will not be affected by any forces. It will just move up and down.
                vdv[index] = 0;
                vda[index] = 0;
                vd[index] = limit * (float)Math.Sin(phase1);
                phase1 += freq1;
                if (phase1 >= 2f * (float)Math.PI)
                    phase1 -= (float)Math.PI * 2f;

                continue;
            }

            if (index == osc2point && osc2active)
            {
                vdv[index] = 0;
                vda[index] = 0;
                vd[index] = limit * (float)Math.Sin(phase2);
                phase2 += freq2;
                if (phase2 >= 2f * (float)Math.PI)
                    phase2 -= (float)Math.PI * 2f;

                continue;
            }

            // So this particle is neither an oscillator nor static. So let's calculate the force.

            // Reset the acceleration. We do this because acceleration dynamically changes with the force.
            vda[index] = 0;

            // Sum up all the height values so we will find the average height of the system.
            // This doesn't contribute to the force calculation. It is immaterial.
            total_height += vd[index];

            // Now we will find out the average height of the 8 neighbor particles.
            // So we will know where the current particle will be attracted to.

            // "heights" is the sum of all the height values of neighbor particles.
            float heights = 0;
            // "num_of_part" is the number of particles which contributed to the "heights".
            int num_of_part = 0;


            //// UP
            if (!(index >= 0 && index < size)) // Make sure that this point is not on a boundary.
            {
                if (!vd_static[index - size]) // Make sure that the neighbor particle is not static.
                {
                    heights += vd[index - size];

                    num_of_part += 1;
                }
            }


            //// UPPER-RIGHT
            if (!((index + 1) % size == 0 || (index >= 0 && index < size)))
            {
                if (!vd_static[index - size + 1])
                {
                    heights += vd[index - size + 1];

                    num_of_part += 1;
                }
            }

            //// RIGHT
            if (!((index + 1) % size == 0))
            {
                if (!vd_static[index + 1])
                {
                    heights += vd[index + 1];

                    num_of_part += 1;
                }
            }

            //// LOWER-RIGHT
            if (!((index + 1) % size == 0 || (index >= (size * size) - size && index < (size * size))))
            {
                if (!vd_static[index + size + 1])
                {
                    heights += vd[index + size + 1];

                    num_of_part += 1;
                }
            }

            //// DOWN
            if (!(index >= (size * size) - size && index < (size * size)))
            {
                if (!vd_static[index + size])
                {
                    heights += vd[index + size];

                    num_of_part += 1;
                }
            }

            //// LOWER-LEFT
            if (!(index % size == 0 || (index >= (size * size) - size && index < (size * size))))
            {
                if (!vd_static[index + size - 1])
                {
                    heights += vd[index + size - 1];

                    num_of_part += 1;
                }
            }


            //// LEFT
            if (!(index % size == 0))
            {
                if (!vd_static[index - 1])
                {
                    heights += vd[index - 1];

                    num_of_part += 1;
                }
            }


            // UPPER-LEFT

            if (!(index % size == 0 || (index >= 0 && index < size)))
            {
                if (!vd_static[index - size - 1])
                {
                    heights += vd[index - size - 1];

                    num_of_part += 1;
                }
            }


            if (num_of_part != 0)
            {
                heights /= (float)num_of_part;

                if (power != 1.0f)
                    vda[index] += Math.Sign(heights - vd[index]) * (float)Math.Pow(Math.Abs(vd[index] - heights), power) / mass;
                else
                    vda[index] += -(vd[index] - heights) / mass;
            }


            // Damping takes place.
            vda[index] -= vdv[index] / vds[index];

            // Don't let things go beyond their limit.
            // This makes sense. It eliminates a critic uncertainty.
            if (vda[index] > limit)
                vda[index] = limit;
            else if (vda[index] < -limit)
                vda[index] = -limit;


        }
        // Now we have finished with the force calculation.

        // Origin height is zero. So "shifting" is the distance between the system average height and the origin.
        float shifting = -total_height / (float)vd.Length;



        // We are taking the final steps in this loop
        for (int index = 0; index < vd.Length; index += 1)
        {

            // Acceleration feeds velocity. Don't forget that we took care of the damping before.
            vdv[index] += vda[index];


            // Here is the purpose of "action_resolution":
            // It is used to divide movements.
            // If the particle goes along the road at once, a chaos is most likely unavoidable.
            if (vd[index] + vdv[index] / action_resolution > limit)
                vd[index] = limit;
            else if (vd[index] + vdv[index] / action_resolution <= limit && vd[index] + vdv[index] / action_resolution >= -limit)
                vd[index] += vdv[index] / action_resolution; // Velocity feeds height.
            else
                vd[index] = -limit;


            // Here is the last step on shifting the whole system to the origin point.
            vd[index] += shifting;


        }

    }

    public void drawToTexture()
    {

        // Get the bitmap data of "bmp".
        //REPLACE? bd = bmp.LockBits(new Rectangle(0, 0, size, size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

        //IntPtr ptr = bd.Scan0; // Get the address of the first line in "bd"
        //int bytes = bd.Stride * bd.Height; // "Stride" gives the size of a line in bytes.
        //byte[] rgbdata = new byte[bytes];
        Color[] colors = new Color[texture.width * texture.height];



        // It's time for the coloration of the height.
        for (int index = 0; index < vd.Length; index++)
        {
            colors[index] = new Color();

            // Brightness. This value is the 'brightness' of the height.
            // Now we see why "limit" makes sense.
            //byte bright = (byte)(((float)vd[index] + limit) / (float)((limit * 2f) / 255f));
            float bright = ((vd[index] + limit) / ((limit * 2f) / 255f)); //TODO not sure about the /255f here. How does our color-space work? 0..1? 0..255? ..


            if (vd_static[index])
            {
                //rgbdata[index * 3] = ColorStatic.B;
                //rgbdata[index * 3 + 1] = ColorStatic.G;
                //rgbdata[index * 3 + 2] = ColorStatic.R;
                colors[index] = ColorStatic;
            }
            else
            {


                if (highcont)
                {
                    float red_average = ((color1.r + color2.r) / 2f);
                    float green_average = ((color1.g + color2.g) / 2f);
                    float blue_average = ((color1.b + color2.b) / 2f);

                    if (vd[index] > 0)
                    {
                        // rgbdata[index * 3] = color1.b;
                        // rgbdata[index * 3 + 1] = color1.g;
                        // rgbdata[index * 3 + 2] = color1.r;
                        colors[index].r = color1.r;
                        colors[index].g = color1.g;
                        colors[index].b = color1.b;
                    }

                    else if (vd[index] < 0)
                    {
                        //rgbdata[index * 3] = color2.b;
                        //rgbdata[index * 3 + 1] = color2.g;
                        //rgbdata[index * 3 + 2] = color2.r;
                        colors[index].r = color2.r;
                        colors[index].g = color2.g;
                        colors[index].b = color2.b;
                    }
                    else if (vd[index] == 0)
                    {
                        // rgbdata[index * 3] = blue_average;
                        // rgbdata[index * 3 + 1] = green_average;
                        // rgbdata[index * 3 + 2] = red_average;
                        colors[index].r = red_average;
                        colors[index].g = green_average;
                        colors[index].b = blue_average;
                    }
                }
                else
                {
                    float brightr1 = bright / 255f;
                    float brightr2 = 1f - bright / 255f;
                    // rgbdata[index * 3] = (byte)((float)color1.b * brightr1 + (float)color2.b * brightr2);
                    // rgbdata[index * 3 + 1] = (byte)((float)color1.g * brightr1 + (float)color2.g * brightr2);
                    // rgbdata[index * 3 + 2] = (byte)((float)color1.r * brightr1 + (float)color2.r * brightr2);
                    colors[index].r = color1.r * brightr1 + color2.r * brightr2;
                    colors[index].g = color1.g * brightr1 + color2.g * brightr2;
                    colors[index].b = color1.b * brightr1 + color2.b * brightr2;
                }


            }
        }

        // At last, we overwrite and release the bitmap data.
        //System.Runtime.InteropServices.Marshal.Copy(rgbdata, 0, ptr, bytes);
        //bmp.UnlockBits(bd);
        texture.SetPixels(colors);

        texture.SetPixel(0, 0, Color.red); //TODO 4dbg remove me
        texture.Apply();
    }













    /// <summary>
    /// Sets sustainability of each particle.
    /// </summary>
    void setSustain()
    {
        if (edge_absorbtion)
        {
            // We will fill "vds" array with "sustain" then we will deal with elements near to window boundaries.

            // Since we want the sustainability to decrease towards the edges, "min_sustain" can't be bigger than "sustain".
            if (min_sustain > sustain)
            {
                min_sustain = 1.0f; // even "sustain" can't be less than 1.0f so this is a reliable value.
            }

            // Sustainability reduction fields should not mix with each other. So the maximum offset is the middle-screen.
            if (absorb_offset >= size / 2)
            {
                absorb_offset = size / 2 - 1;
            }

            // This value is sustainability decreasion rate per row/column. The decreasion is linear.
            float dec = (sustain - min_sustain) / (float)absorb_offset;
            // This one stores the current sustainability.
            float cur = min_sustain;

            // First, we fill "vds" array with "sustain".
            for (int i = 0; i < vds.Length - 1; i++)
                vds[i] = sustain;

            // This loop sets up the sustainability values for the top.
            for (int off = 0; off <= absorb_offset; off++)
            {
                // Process each row/column from the edge to the offset.
                for (int x = off; x < size - off; x++)
                {
                    // Process each sustainability element in the current row/column
                    vds[x + off * size] = cur;
                }
                cur += dec;
            }

            cur = sustain; // Reset the current sustainability.


            // This loop sets up the sustainability values for the bottom.
            for (int off = 0; off <= absorb_offset; off++)
            {
                for (int x = absorb_offset - off; x < size - (absorb_offset - off); x++)
                {
                    vds[x + off * size + size * (size - absorb_offset - 1)] = cur;
                }
                cur -= dec;
            }


            cur = sustain;

            // This loop sets up the sustainability values for the left.
            for (int off = 0; off <= absorb_offset; off++)
            {
                for (int x = absorb_offset - off; x < size - (absorb_offset - off); x++)
                {
                    vds[x * size + (absorb_offset - off)] = cur;
                }
                cur -= dec;
            }

            cur = sustain;

            // This loop sets up the sustainability values for the right.
            for (int off = 0; off <= absorb_offset; off++)
            {
                for (int x = absorb_offset - off; x < size - (absorb_offset - off); x++)
                {
                    vds[x * size + off + size - absorb_offset - 1] = cur;
                }
                cur -= dec;
            }
        }
        else
        {
            // The only thing to do is to fill "vds" array with "sustain" in this case.
            for (int i = 0; i < vds.Length; i++)
                vds[i] = sustain;
        }
    }


    /// <summary>
    /// Initializes the wave pool system.
    /// </summary>
    void setPool()
    {
        /* REPLACE

        if (bufgraph != null)
            bufgraph.Dispose();

        if (bufgcont != null)
            bufgcont.Dispose();


        bufgcont = new BufferedGraphicsContext();

        bufgraph = bufgcont.Allocate(control.CreateGraphics(), control.ClientRectangle);

        */

        vd = new float[size * size];

        vdv = new float[size * size];

        vda = new float[size * size];

        vd_static = new bool[size * size];

        vds = new float[size * size];

        setSustain();

    }

    #endregion
}

    //void OnMouseDown(UnityEngine.EventSystems.PointerEventData eventData) {
    void OnMouseDown()
    {
       
        Debug.Log("OnMouseDown: " + Input.mousePosition + " to texture coord " + screenToParticleCoords(Input.mousePosition));
    }

    Vector2 screenToParticleCoords(Vector3 screenCoordinate)
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(screenCoordinate), out hit))
            throw new Exception("@mousePositionToUVCoordinates: Missed canvas plane with click somehow.");

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (rend == null || rend.material == null || rend.material.mainTexture == null || meshCollider == null)
            throw new Exception("@mousePositionToUVCoordinates: some variable was null." +
                "Renderer: " + rend +
                ", material: " + rend.material +
                ", maintexture: " + rend.material.mainTexture +
                ", meshCollider: " + meshCollider
                );

        Vector2 pixelUV = hit.textureCoord;
        return new Vector2(pixelUV.x * texture.width, pixelUV.y * texture.height);             
        //TODO return (int,int)

    }

    Vector2 mousePositionToParticleCoords()
    {
        return screenToParticleCoords(Input.mousePosition);
    }

    public static readonly int[][] EIGHT_NEIGHBOURS = new int[][]{
        new int[]{ -1, -1 },
        new int[]{  0, -1 },
        new int[]{  1, -1 },
        new int[]{ -1,  0 },
        new int[]{  1,  0 },
        new int[]{ -1,  1 },
        new int[]{  0,  1 },
        new int[]{  1,  1 }
    };
    

    void fitCanvasOverViewport()
    {
       
        // STRETCH / RESIZE CANVAS-PLANE... 
        float defaultSizeOfPlane = 10f;
        float worldHeight = 2 * cam.orthographicSize;
        float worldWidth = worldHeight * cam.aspect;
        //...to fit cam-dimensions
        //this.transform.localScale = new Vector3(worldWidth / defaultSizeOfPlane, 1, worldHeight / defaultSizeOfPlane);
        //...to cover cam-dimensions while staying square
        float scale = Math.Max(worldHeight, worldWidth) / defaultSizeOfPlane;
        this.transform.localScale = new Vector3(scale, 1, scale);
    }

    //create wall from object with tag "Wall"
    private void createWall(String wallName) {

        GameObject[] wall = GameObject.FindGameObjectsWithTag(wallName);

        foreach (GameObject wal in wall)
        {

            Vector2 coord = screenToParticleCoords(cam.WorldToScreenPoint(wal.transform.position));

            BoxCollider2D boxCollid = wal.GetComponent<Collider2D>() as BoxCollider2D;
            Vector3 boxColliderL = new Vector3(boxCollid.bounds.center.x, boxCollid.bounds.center.y, 0);
            boxColliderL -= boxCollid.bounds.extents;
            Vector3 boxColliderR = new Vector3(boxCollid.bounds.center.x + boxCollid.bounds.extents.x, boxCollid.bounds.center.y + boxCollid.bounds.extents.y, 0);

            Vector2 coordBox = screenToParticleCoords(cam.WorldToScreenPoint(boxColliderL));
            Vector2 coordBox2 = screenToParticleCoords(cam.WorldToScreenPoint(boxColliderR));

            w.setObstacles((int)coordBox.x, (int)coordBox.y, (int)(coordBox2.x - coordBox.x), (int)(coordBox2.y - coordBox.y));
        }
    }

    // Update is called once per frame
    void Update()
    {
        fitCanvasOverViewport();

        createWall("Wall");

        updateOsc2Timeout();
        handleInput();
        updateWavePhysics();
        updateWave2RigidBodyForces();
        w.drawToTexture();
    }

    public float waveBurstDuration = 0.5f; //limits how long oscillators emit (if >0). in seconds.
    float osc2timer = -1; // how long osc2 has been emitting. -1 if not running.
    void startOsc2Timeout()
    {
        osc2timer = 0; //start counter
    }
    void updateOsc2Timeout()
    {
        if (osc2timer >= 0)
        {
            osc2timer += Time.deltaTime;
        }
        if(waveBurstDuration > 0 && osc2timer > waveBurstDuration)
        {
            w.Oscillator2Active = false;
            osc2timer = -1;
        }
    }
    void handleInput()
    {
        // CLICK/TOUCH DETECTION
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began ||
            Input.GetMouseButtonDown(0))
        {

            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;

            try
            {

                Vector2 textureCoords = mousePositionToParticleCoords();
                //Vector2 pixelUV = mousePositionToParticleCoords();
                //Vector2 textureCoords = new Vector2(pixelUV.x * texture.width, pixelUV.y * texture.height);             
                if(waveBurstDuration < 0 || (waveBurstDuration > 0 && osc2timer < 0)) //not activated or blocking timer not running yet / run out already
                {
                    Debug.Log("placing osc, starting counter");
                    startOsc2Timeout();
                    w.Oscillator2Position = textureCoords;
                    w.Oscillator2Active = true;
                    w.phase2 = (float)Math.PI; // to start with wave hill
                } 
            }
            catch (Exception e)
            {
                Debug.LogError("Incurred an exception but swallowed it. " + e);
            }
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began ||
          Input.GetMouseButtonDown(1))
        {

            try
            {
                //remove?
                Vector2 mouseCoordPart = screenToParticleCoords(Input.mousePosition);
                w.setObstacles((int)mouseCoordPart.x, (int)mouseCoordPart.y, 10, 10);
              
            }
            catch (Exception e)
            {
                Debug.LogError("Incurred an exception but swallowed it2. " + e);
            }
        }

    }

    const float PHYSICS_UPDATES_PER_SECOND = 20;
    float physicsTimePool = 0;

    void updateWavePhysics() {
        //@times two: only one update at 30fps wasn't enough, the physics rate needs to stay below the actual frame-rate though, to avoid stuttering
        physicsTimePool += Time.deltaTime * 2;
        while(physicsTimePool > 1 / PHYSICS_UPDATES_PER_SECOND ) {
            physicsTimePool -= 1 / PHYSICS_UPDATES_PER_SECOND;
            w.CalculateForces(); 
        }

    }

    void updateWave2RigidBodyForces() {
        GameObject[] pushables = GameObject.FindGameObjectsWithTag("Pushable");
        foreach (GameObject pushable in pushables) {
            try {

                Rigidbody2D rb = pushable.GetComponent<Rigidbody2D>();
                if (rb.isKinematic) continue;
                Vector3 pos = pushable.transform.position;

                Vector3 screenPos = cam.WorldToScreenPoint(pos);
                Vector2 particlePos = screenToParticleCoords(screenPos);

                float ownVelocity = w.getVelocityAt((int)particlePos.x, (int)particlePos.y);
                float ownHeight = w.getHeightAt((int)particlePos.x, (int)particlePos.y);
                float ownPrevHeight = w.getPreviousHeightAt((int)particlePos.x, (int)particlePos.y);
                float ownGain = ownHeight - ownPrevHeight;

                Vector2 grd = gradient(particlePos, 5, (x, y) => w.getHeightAt(x, y));
                Vector2 forceOrigin = new Vector2(pos.x + 0.0f, pos.y + 0.0f); // for debugging
                Vector2 forceDirection = grd / 15; //the constant is a magic number/factor here. Trial and error showed that objects tended to stay on top of the wave using that.
                rb.AddForceAtPosition(forceDirection, forceOrigin);

            }
            catch (Exception e) {
                Debug.LogError("game object left screen probably. swallowing exception. " + e);
            }
        }
    }

    public enum ParticleAttribute
    {
        Height = 1,
        Velocity = 2,
        Acceleration = 4,
        Sustainability = 8,
        Fixity = 16,
        All = 32,
    }

    public Vector2 gradient(Vector2 center, int sizeBelowCenter, Func<int,int,float> lookupFn) {
        int sideLength = sizeBelowCenter * 2 + 1;
        int kernelArea = sideLength * sideLength;

        int kernelLeftBound = (int)Math.Max(0, center.x - sizeBelowCenter);
        int kernelRightBound = (int)Math.Min(size - 1, center.x + sizeBelowCenter);
        int kernelUpperBound = (int)Math.Max(0, center.y - sizeBelowCenter);
        int kernelLowerBound = (int)Math.Min(size - 1, center.y + sizeBelowCenter);

        // e.g. sizebelowcenter==2: kernel = [[-2 -1 0 1 2],[-2 1 0 1 2],...]
        Vector2 gradient = new Vector2();
        for(int y = kernelUpperBound; y <= kernelLowerBound; y++) {
            float yOffset = y - center.y;
            for(int x = kernelLeftBound; x <= kernelRightBound; x++) {
                float xOffset = x - center.x;
                float val = lookupFn(x, y);
                gradient.x += val * xOffset;
                gradient.y += val * yOffset;
            }
        }

        int normalizer = 2 * //apmplification on left and right
            ((sizeBelowCenter - 1) * sizeBelowCenter / 2) * //sum of numbers 1..sizeBelowCenter
            sideLength; // height

        return gradient / normalizer;
    }

    public Vector2 centerOfGravity(Vector2 center, int sizeBelowCenter, Func<int,int,float> lookupFn)
    {
        int sideLength = sizeBelowCenter * 2;
        int kernelArea = sideLength * sideLength;

        int kernelLeftBound = (int)Math.Max(0, center.x - sizeBelowCenter);
        int kernelRightBound = (int)Math.Min(size - 1, center.x + sizeBelowCenter);
        int kernelUpperBound = (int)Math.Max(0, center.y - sizeBelowCenter);
        int kernelLowerBound = (int)Math.Min(size - 1, center.y + sizeBelowCenter);

        float min = float.MaxValue;
        for(int y = kernelUpperBound; y <= kernelLowerBound; y++) {
            for(int x = kernelLeftBound; x <= kernelRightBound; x++) {
                float val = lookupFn(x, y);
                if (val < min) min = val;
            }
        }
        // add this to all values so we don't have sub-zero values.
        float compensation = -min;

        Vector2 gravCenter = new Vector2(0,0);
        float sum = 0;
        for(int y = kernelUpperBound; y <= kernelLowerBound; y++) {
            for(int x = kernelLeftBound; x <= kernelRightBound; x++) {
                float val = lookupFn(x, y) + compensation;
                sum += val;
                gravCenter.x += x * val;
                gravCenter.y += y * val;
            }
        }
        if (sum == 0) return new Vector2(center.x, center.y);
        gravCenter /= sum;
        return gravCenter;

    }

}
